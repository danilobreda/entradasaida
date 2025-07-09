namespace EntradaSaida.ML.Models
{
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
}
