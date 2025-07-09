namespace EntradaSaida.Core.Models;

/// <summary>
/// Representa um evento de entrada ou saída detectado
/// </summary>
public class CounterEvent
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public CounterEventType Type { get; set; }
    public int PersonId { get; set; }
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public string? CameraId { get; set; }
    public float Confidence { get; set; }
}

/// <summary>
/// Tipo de evento de contagem
/// </summary>
public enum CounterEventType
{
    Entry,  // Entrada
    Exit    // Saída
}
