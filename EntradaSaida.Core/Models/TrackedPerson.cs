namespace EntradaSaida.Core.Models
{
    /// <summary>
    /// Representa uma pessoa sendo rastreada pelo sistema
    /// </summary>
    public class TrackedPerson
    {
        public int Id { get; set; }
        public DateTime FirstSeen { get; set; }
        public DateTime LastSeen { get; set; }
        public List<Detection> Detections { get; set; } = new();
        public TrackingStatus Status { get; set; }
        public float CurrentX { get; set; }
        public float CurrentY { get; set; }
        public float VelocityX { get; set; }
        public float VelocityY { get; set; }
        public int ConsecutiveMisses { get; set; }
    
        /// <summary>
        /// Adiciona uma nova detecção ao tracking
        /// </summary>
        public void AddDetection(Detection detection)
        {
            if (Detections.Count > 0)
            {
                var lastDetection = Detections.Last();
                var deltaTime = (detection.Timestamp - lastDetection.Timestamp).TotalSeconds;
            
                if (deltaTime > 0)
                {
                    VelocityX = (detection.CenterX - lastDetection.CenterX) / (float)deltaTime;
                    VelocityY = (detection.CenterY - lastDetection.CenterY) / (float)deltaTime;
                }
            }
        
            Detections.Add(detection);
            CurrentX = detection.CenterX;
            CurrentY = detection.CenterY;
            LastSeen = detection.Timestamp;
            ConsecutiveMisses = 0;
            Status = TrackingStatus.Active;
        }
    
        /// <summary>
        /// Marca que a pessoa não foi detectada neste frame
        /// </summary>
        public void Miss()
        {
            ConsecutiveMisses++;
            if (ConsecutiveMisses > 5)
            {
                Status = TrackingStatus.Lost;
            }
        }
    }

    /// <summary>
    /// Status do rastreamento de uma pessoa
    /// </summary>
    public enum TrackingStatus
    {
        Active,     // Ativo
        Lost,       // Perdido
        Completed   // Completado
    }
}
