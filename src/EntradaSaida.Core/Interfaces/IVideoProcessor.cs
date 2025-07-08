using EntradaSaida.Core.Models;

namespace EntradaSaida.Core.Interfaces;

/// <summary>
/// Interface para processamento de vídeo
/// </summary>
public interface IVideoProcessor
{
    /// <summary>
    /// Inicia o processamento de vídeo
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Para o processamento de vídeo
    /// </summary>
    Task StopAsync();
    
    /// <summary>
    /// Verifica se o processamento está ativo
    /// </summary>
    bool IsRunning { get; }
    
    /// <summary>
    /// Configura a fonte de vídeo
    /// </summary>
    Task<bool> SetVideoSourceAsync(string source);
    
    /// <summary>
    /// Obtém o frame atual
    /// </summary>
    Task<byte[]?> GetCurrentFrameAsync();
    
    /// <summary>
    /// Evento disparado quando um novo frame é processado
    /// </summary>
    event EventHandler<FrameProcessedEventArgs>? FrameProcessed;
    
    /// <summary>
    /// Evento disparado quando há novos eventos de contagem
    /// </summary>
    event EventHandler<CounterEventsEventArgs>? CounterEvents;
    
    /// <summary>
    /// Obtém estatísticas de performance
    /// </summary>
    Task<ProcessingStats> GetProcessingStatsAsync();
}

/// <summary>
/// Argumentos do evento de frame processado
/// </summary>
public class FrameProcessedEventArgs : EventArgs
{
    public byte[] FrameData { get; set; } = Array.Empty<byte>();
    public List<Detection> Detections { get; set; } = new();
    public DateTime Timestamp { get; set; }
    public int FrameNumber { get; set; }
}

/// <summary>
/// Argumentos do evento de contagem
/// </summary>
public class CounterEventsEventArgs : EventArgs
{
    public List<CounterEvent> Events { get; set; } = new();
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Estatísticas de processamento
/// </summary>
public class ProcessingStats
{
    public double FramesPerSecond { get; set; }
    public double AverageProcessingTime { get; set; }
    public int TotalFramesProcessed { get; set; }
    public int TotalDetections { get; set; }
    public DateTime StartTime { get; set; }
    public TimeSpan Uptime { get; set; }
}
