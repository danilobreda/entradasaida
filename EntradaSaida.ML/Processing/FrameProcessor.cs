using EntradaSaida.Core.Models;
using EntradaSaida.ML.Detection;
using EntradaSaida.ML.Tracking;
using Emgu.CV;
using Emgu.CV.CvEnum;

namespace EntradaSaida.ML.Processing;

/// <summary>
/// Processador de frames individuais
/// </summary>
public class FrameProcessor : IDisposable
{
    private readonly YoloV8Detector _detector;
    private readonly PersonTracker _tracker;
    private readonly LineCounter _lineCounter;
    private bool _disposed;
    
    public FrameProcessor(YoloV8Detector detector, PersonTracker tracker, LineCounter lineCounter)
    {
        _detector = detector;
        _tracker = tracker;
        _lineCounter = lineCounter;
    }
    
    /// <summary>
    /// Processa um frame completo
    /// </summary>
    public async Task<FrameProcessingResult> ProcessFrameAsync(byte[] frameData, DateTime timestamp)
    {
        try
        {
            var result = new FrameProcessingResult
            {
                Timestamp = timestamp,
                Success = false
            };
            
            // 1. Detectar pessoas no frame
            var detections = await _detector.DetectPersonsAsync(frameData);
            result.DetectionCount = detections.Count;
            
            // 2. Converter detecções para o formato do tracker
            var detectionResults = detections.Select(d => new DetectionResult
            {
                X = d.X,
                Y = d.Y,
                Width = d.Width,
                Height = d.Height,
                Confidence = d.Confidence,
                ClassId = 0, // person
                ClassName = "person"
            }).ToList();
            
            // 3. Atualizar rastreamento
            var trackedObjects = _tracker.Update(detectionResults, timestamp);
            result.TrackedCount = trackedObjects.Count;
            
            // 4. Verificar cruzamentos de linha
            var counterEvents = _lineCounter.CheckCrossings(trackedObjects, timestamp);
            result.CounterEvents = counterEvents;
            
            // 5. Criar frame anotado (opcional)
            if (frameData.Length > 0)
            {
                result.AnnotatedFrame = await CreateAnnotatedFrameAsync(frameData, detections, trackedObjects);
            }
            
            result.Success = true;
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro no processamento do frame: {ex.Message}");
            return new FrameProcessingResult
            {
                Timestamp = timestamp,
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
    
    /// <summary>
    /// Cria frame anotado com detecções e tracks
    /// </summary>
    private async Task<byte[]> CreateAnnotatedFrameAsync(byte[] frameData, List<Detection> detections, List<TrackedObject> tracks)
    {
        try
        {
            using var mat = new Mat();
            CvInvoke.Imdecode(frameData, ImreadModes.Color, mat);
            
            // Desenhar detecções
            foreach (var detection in detections)
            {
                var rect = new System.Drawing.Rectangle(
                    (int)detection.X, 
                    (int)detection.Y, 
                    (int)detection.Width, 
                    (int)detection.Height);
                
                // Bounding box verde para detecções
                CvInvoke.Rectangle(mat, rect, new Emgu.CV.Structure.MCvScalar(0, 255, 0), 2);
                
                // Confidence score
                var label = $"{detection.Confidence:F2}";
                CvInvoke.PutText(mat, label, 
                    new System.Drawing.Point((int)detection.X, (int)detection.Y - 10),
                    FontFace.HersheySimplex, 0.5, new Emgu.CV.Structure.MCvScalar(0, 255, 0), 1);
            }
            
            // Desenhar tracks
            foreach (var track in tracks)
            {
                var rect = new System.Drawing.Rectangle(
                    (int)track.X, 
                    (int)track.Y, 
                    (int)track.Width, 
                    (int)track.Height);
                
                // Bounding box azul para tracks
                CvInvoke.Rectangle(mat, rect, new Emgu.CV.Structure.MCvScalar(255, 0, 0), 2);
                
                // ID do track
                var label = $"ID: {track.Id}";
                CvInvoke.PutText(mat, label, 
                    new System.Drawing.Point((int)track.X, (int)track.Y - 30),
                    FontFace.HersheySimplex, 0.6, new Emgu.CV.Structure.MCvScalar(255, 0, 0), 2);
                
                // Desenhar histórico de movimento
                if (track.TrackHistory.Count > 1)
                {
                    for (int i = 1; i < track.TrackHistory.Count; i++)
                    {
                        var prev = track.TrackHistory[i - 1];
                        var curr = track.TrackHistory[i];
                        
                        CvInvoke.Line(mat, 
                            new System.Drawing.Point((int)prev.x, (int)prev.y),
                            new System.Drawing.Point((int)curr.x, (int)curr.y),
                            new Emgu.CV.Structure.MCvScalar(0, 0, 255), 2);
                    }
                }
            }
            
            // Desenhar linhas de contagem
            var lines = _lineCounter.GetActiveLines();
            foreach (var line in lines)
            {
                CvInvoke.Line(mat,
                    new System.Drawing.Point((int)line.StartX, (int)line.StartY),
                    new System.Drawing.Point((int)line.EndX, (int)line.EndY),
                    new Emgu.CV.Structure.MCvScalar(0, 255, 255), 3);
                
                // Nome da linha
                CvInvoke.PutText(mat, line.Name,
                    new System.Drawing.Point((int)line.StartX, (int)line.StartY - 10),
                    FontFace.HersheySimplex, 0.7, new Emgu.CV.Structure.MCvScalar(0, 255, 255), 2);
            }
            
            // Converter de volta para bytes
            return mat.ToImage<Emgu.CV.Structure.Bgr, byte>().ToJpegData();
        }
        catch
        {
            return frameData; // Retornar frame original em caso de erro
        }
    }
    
    /// <summary>
    /// Obtém estatísticas de processamento
    /// </summary>
    public ProcessingStats GetProcessingStats()
    {
        var trackingStats = _tracker.GetStats();
        var lineStats = _lineCounter.GetStats();
        
        return new ProcessingStats
        {
            ActiveTracks = trackingStats.ActiveTracks,
            TotalTracksCreated = trackingStats.TotalTracksCreated,
            ActiveLines = lineStats.ActiveLines,
            TrackedObjects = lineStats.TrackedObjects
        };
    }
    
    public void Dispose()
    {
        if (!_disposed)
        {
            _detector?.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Resultado do processamento de um frame
/// </summary>
public class FrameProcessingResult
{
    public DateTime Timestamp { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int DetectionCount { get; set; }
    public int TrackedCount { get; set; }
    public List<CounterEvent> CounterEvents { get; set; } = new();
    public byte[]? AnnotatedFrame { get; set; }
}

/// <summary>
/// Estatísticas de processamento detalhadas
/// </summary>
public class ProcessingStats
{
    public int ActiveTracks { get; set; }
    public int TotalTracksCreated { get; set; }
    public int ActiveLines { get; set; }
    public int TrackedObjects { get; set; }
}
