using EntradaSaida.Core.Interfaces;
using EntradaSaida.Core.Models;
using EntradaSaida.ML.Detection;
using EntradaSaida.ML.Tracking;
using EntradaSaida.ML.Processing;
using Emgu.CV;
using Emgu.CV.CvEnum;

namespace EntradaSaida.ML.Processing;

/// <summary>
/// Processador principal de vídeo
/// </summary>
public class VideoProcessor : IVideoProcessor, IDisposable
{
    private readonly YoloV8Detector _detector;
    private readonly PersonTracker _tracker;
    private readonly LineCounter _lineCounter;
    private readonly FrameProcessor _frameProcessor;
    
    private VideoCapture? _capture;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _processingTask;
    private bool _disposed;
    
    // Eventos
    public event EventHandler<FrameProcessedEventArgs>? FrameProcessed;
    public event EventHandler<CounterEventsEventArgs>? CounterEvents;
    
    // Estatísticas
    private DateTime _startTime;
    private int _totalFramesProcessed;
    private int _totalDetections;
    private readonly List<double> _processingTimes = new();
    private byte[]? _currentFrame;
    
    public bool IsRunning => _processingTask?.Status == TaskStatus.Running;
    
    public VideoProcessor()
    {
        _detector = new YoloV8Detector();
        _tracker = new PersonTracker();
        _lineCounter = new LineCounter();
        _frameProcessor = new FrameProcessor(_detector, _tracker, _lineCounter);
    }
    
    public async Task<bool> SetVideoSourceAsync(string source)
    {
        try
        {
            _capture?.Dispose();
            
            // Tentar parsear como número (webcam)
            if (int.TryParse(source, out var cameraIndex))
            {
                _capture = new VideoCapture(cameraIndex);
            }
            else
            {
                // Arquivo ou URL
                _capture = new VideoCapture(source);
            }
            
            // Verificar se a captura foi bem-sucedida
            if (_capture.IsOpened)
            {
                // Configurar resolução padrão
                _capture.Set(CapProp.FrameWidth, 640);
                _capture.Set(CapProp.FrameHeight, 480);
                _capture.Set(CapProp.Fps, 30);
                
                return await Task.FromResult(true);
            }
            
            return await Task.FromResult(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao configurar fonte de vídeo: {ex.Message}");
            return await Task.FromResult(false);
        }
    }
    
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (IsRunning)
            return;
        
        if (_capture == null || !_capture.IsOpened)
            throw new InvalidOperationException("Fonte de vídeo não configurada");
        
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _startTime = DateTime.UtcNow;
        _totalFramesProcessed = 0;
        _totalDetections = 0;
        _processingTimes.Clear();
        
        _processingTask = ProcessVideoAsync(_cancellationTokenSource.Token);
        await Task.CompletedTask;
    }
    
    public async Task StopAsync()
    {
        if (!IsRunning)
            return;
        
        _cancellationTokenSource?.Cancel();
        
        if (_processingTask != null)
        {
            try
            {
                await _processingTask;
            }
            catch (OperationCanceledException)
            {
                // Esperado quando cancelamos
            }
        }
        
        _processingTask = null;
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
    }
    
    public async Task<byte[]?> GetCurrentFrameAsync()
    {
        return await Task.FromResult(_currentFrame);
    }
    
    public async Task<ProcessingStats> GetProcessingStatsAsync()
    {
        var uptime = DateTime.UtcNow - _startTime;
        var avgProcessingTime = _processingTimes.Count > 0 ? _processingTimes.Average() : 0;
        var fps = _totalFramesProcessed > 0 ? _totalFramesProcessed / uptime.TotalSeconds : 0;
        
        return await Task.FromResult(new ProcessingStats
        {
            FramesPerSecond = fps,
            AverageProcessingTime = avgProcessingTime,
            TotalFramesProcessed = _totalFramesProcessed,
            TotalDetections = _totalDetections,
            StartTime = _startTime,
            Uptime = uptime
        });
    }
    
    /// <summary>
    /// Loop principal de processamento de vídeo
    /// </summary>
    private async Task ProcessVideoAsync(CancellationToken cancellationToken)
    {
        if (_capture == null) return;
        
        using var frame = new Mat();
        var frameNumber = 0;
        
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var startTime = DateTime.UtcNow;
                
                // Capturar frame
                if (!_capture.Read(frame) || frame.IsEmpty)
                {
                    await Task.Delay(10, cancellationToken);
                    continue;
                }
                
                // Converter para bytes
                var frameBytes = frame.ToImage<Emgu.CV.Structure.Bgr, byte>().ToJpegData();
                _currentFrame = frameBytes;
                
                // Processar frame
                var result = await _frameProcessor.ProcessFrameAsync(frameBytes, DateTime.UtcNow);
                
                if (result.Success)
                {
                    _totalFramesProcessed++;
                    _totalDetections += result.DetectionCount;
                    
                    var processingTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                    _processingTimes.Add(processingTime);
                    
                    // Manter apenas as últimas 100 medições
                    if (_processingTimes.Count > 100)
                    {
                        _processingTimes.RemoveAt(0);
                    }
                    
                    // Disparar eventos
                    FrameProcessed?.Invoke(this, new FrameProcessedEventArgs
                    {
                        FrameData = result.AnnotatedFrame ?? frameBytes,
                        Detections = result.CounterEvents.Select(e => new Detection
                        {
                            Timestamp = e.Timestamp,
                            X = e.PositionX,
                            Y = e.PositionY,
                            Confidence = e.Confidence
                        }).ToList(),
                        Timestamp = result.Timestamp,
                        FrameNumber = frameNumber++
                    });
                    
                    if (result.CounterEvents.Any())
                    {
                        CounterEvents?.Invoke(this, new CounterEventsEventArgs
                        {
                            Events = result.CounterEvents,
                            Timestamp = result.Timestamp
                        });
                    }
                }
                
                // Controle de FPS (30 FPS = ~33ms por frame)
                var elapsed = (DateTime.UtcNow - startTime).TotalMilliseconds;
                var delay = Math.Max(0, 33 - (int)elapsed);
                
                if (delay > 0)
                {
                    await Task.Delay(delay, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro no processamento de vídeo: {ex.Message}");
                await Task.Delay(1000, cancellationToken);
            }
        }
    }
    
    /// <summary>
    /// Adiciona uma linha de contagem
    /// </summary>
    public void AddCountingLine(CountingLine line)
    {
        _lineCounter.AddLine(line);
    }
    
    /// <summary>
    /// Remove uma linha de contagem
    /// </summary>
    public bool RemoveCountingLine(int lineId)
    {
        return _lineCounter.RemoveLine(lineId);
    }
    
    /// <summary>
    /// Carrega modelo YOLO
    /// </summary>
    public async Task<bool> LoadModelAsync(string modelPath)
    {
        return await _detector.LoadModelAsync(modelPath);
    }
    
    public void Dispose()
    {
        if (!_disposed)
        {
            StopAsync().Wait();
            _capture?.Dispose();
            _frameProcessor?.Dispose();
            _cancellationTokenSource?.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
