namespace EntradaSaida.Core.Models;

/// <summary>
/// Argumentos do evento de contagem
/// </summary>
public class CounterEventsEventArgs : EventArgs
{
    public List<CounterEvent> Events { get; set; } = new();
    public DateTime Timestamp { get; set; }
}
