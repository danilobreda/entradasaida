namespace EntradaSaida.Core.Models
{
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
}
