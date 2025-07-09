namespace EntradaSaida.Core.Models
{
    /// <summary>
    /// Estatísticas de contagem
    /// </summary>
    public class CounterStats
    {
        public DateTime Date { get; set; }
        public int TotalEntries { get; set; }
        public int TotalExits { get; set; }
        public int CurrentOccupancy { get; set; }
        public int MaxOccupancy { get; set; }
        public double AverageStayTime { get; set; }
        public List<HourlyStats> HourlyBreakdown { get; set; } = new();
    
        /// <summary>
        /// Saldo de pessoas (entradas - saídas)
        /// </summary>
        public int Balance => TotalEntries - TotalExits;
    }
}
