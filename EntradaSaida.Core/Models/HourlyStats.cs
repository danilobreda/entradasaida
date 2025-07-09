namespace EntradaSaida.Core.Models
{
    /// <summary>
    /// Estatísticas por hora
    /// </summary>
    public class HourlyStats
    {
        public int Hour { get; set; }
        public int Entries { get; set; }
        public int Exits { get; set; }
        public int PeakOccupancy { get; set; }
    
        public int Balance => Entries - Exits;
    }
}
