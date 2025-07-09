namespace EntradaSaida.ML.Tracking
{
    /// <summary>
    /// Representa um objeto sendo rastreado
    /// </summary>
    public class TrackedObject
    {
        public int Id { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float VelocityX { get; set; }
        public float VelocityY { get; set; }
        public DateTime LastUpdate { get; set; }
        public int ConsecutiveMisses { get; set; }
        public float Confidence { get; set; }
        public List<(float x, float y, DateTime timestamp)> TrackHistory { get; set; } = new();
    
        /// <summary>
        /// Centro X do objeto
        /// </summary>
        public float CenterX => X + Width / 2;
    
        /// <summary>
        /// Centro Y do objeto
        /// </summary>
        public float CenterY => Y + Height / 2;
    
        /// <summary>
        /// Atualiza a posição do objeto
        /// </summary>
        public void Update(float x, float y, float width, float height, float confidence, DateTime timestamp)
        {
            // Calcular velocidade baseada na posição anterior
            if (TrackHistory.Count > 0)
            {
                var lastPosition = TrackHistory.Last();
                var deltaTime = (timestamp - lastPosition.timestamp).TotalSeconds;
            
                if (deltaTime > 0)
                {
                    VelocityX = (x + width / 2 - lastPosition.x) / (float)deltaTime;
                    VelocityY = (y + height / 2 - lastPosition.y) / (float)deltaTime;
                }
            }
        
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Confidence = confidence;
            LastUpdate = timestamp;
            ConsecutiveMisses = 0;
        
            // Adicionar ao histórico
            TrackHistory.Add((CenterX, CenterY, timestamp));
        
            // Manter apenas os últimos 30 pontos
            if (TrackHistory.Count > 30)
            {
                TrackHistory.RemoveAt(0);
            }
        }
    
        /// <summary>
        /// Prediz a próxima posição baseada na velocidade
        /// </summary>
        public (float x, float y) PredictNextPosition(double deltaTime)
        {
            var predictedX = CenterX + VelocityX * (float)deltaTime;
            var predictedY = CenterY + VelocityY * (float)deltaTime;
            return (predictedX, predictedY);
        }
    
        /// <summary>
        /// Calcula a distância até um ponto
        /// </summary>
        public float DistanceTo(float x, float y)
        {
            var dx = CenterX - x;
            var dy = CenterY - y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }
    
        /// <summary>
        /// Calcula IoU (Intersection over Union) com uma bounding box
        /// </summary>
        public float CalculateIoU(float x, float y, float width, float height)
        {
            var x1 = Math.Max(X, x);
            var y1 = Math.Max(Y, y);
            var x2 = Math.Min(X + Width, x + width);
            var y2 = Math.Min(Y + Height, y + height);
        
            if (x2 <= x1 || y2 <= y1) return 0;
        
            var intersection = (x2 - x1) * (y2 - y1);
            var thisArea = Width * Height;
            var otherArea = width * height;
            var union = thisArea + otherArea - intersection;
        
            return intersection / union;
        }
    }
}
