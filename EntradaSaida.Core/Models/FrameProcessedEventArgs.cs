namespace EntradaSaida.Core.Models
{
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
}
