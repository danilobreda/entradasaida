using EntradaSaida.Core.Models;

namespace EntradaSaida.ML.Models
{
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
}
