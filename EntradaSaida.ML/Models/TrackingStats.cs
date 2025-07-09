namespace EntradaSaida.ML.Models
{
    /// <summary>
    /// Estatísticas do rastreamento
    /// </summary>
    public class TrackingStats
    {
        public int ActiveTracks { get; set; }
        public int TotalTracksCreated { get; set; }
        public double AverageTrackAge { get; set; }
        public int OldestTrack { get; set; }
    }
}
