using EntradaSaida.ML.Detection;

namespace EntradaSaida.ML.Tracking
{
    /// <summary>
    /// Algoritmo SORT (Simple Online and Realtime Tracking) simplificado
    /// </summary>
    public class TrackingAlgorithm
    {
        private readonly float _maxDistance;
        private readonly float _minIoU;
        private readonly int _maxMissedFrames;
    
        public TrackingAlgorithm(float maxDistance = 50.0f, float minIoU = 0.3f, int maxMissedFrames = 10)
        {
            _maxDistance = maxDistance;
            _minIoU = minIoU;
            _maxMissedFrames = maxMissedFrames;
        }
    
        /// <summary>
        /// Associa detecções a objetos rastreados existentes
        /// </summary>
        public List<(TrackedObject track, DetectionResult? detection)> Associate(
            List<TrackedObject> tracks, 
            List<DetectionResult> detections,
            DateTime timestamp)
        {
            var associations = new List<(TrackedObject track, DetectionResult? detection)>();
            var usedDetections = new HashSet<int>();
        
            // Primeiro, tentar associar por IoU (mais confiável)
            foreach (var track in tracks.ToList())
            {
                var bestMatch = -1;
                var bestIoU = _minIoU;
            
                for (int i = 0; i < detections.Count; i++)
                {
                    if (usedDetections.Contains(i)) continue;
                
                    var detection = detections[i];
                    var iou = track.CalculateIoU(detection.X, detection.Y, detection.Width, detection.Height);
                
                    if (iou > bestIoU)
                    {
                        bestIoU = iou;
                        bestMatch = i;
                    }
                }
            
                if (bestMatch >= 0)
                {
                    associations.Add((track, detections[bestMatch]));
                    usedDetections.Add(bestMatch);
                }
            }
        
            // Em seguida, tentar associar por distância para tracks não associados
            var unassociatedTracks = tracks.Where(t => !associations.Any(a => a.track.Id == t.Id)).ToList();
            var unassociatedDetections = detections.Where((d, i) => !usedDetections.Contains(i)).ToList();
        
            foreach (var track in unassociatedTracks)
            {
                var bestMatch = -1;
                var bestDistance = _maxDistance;
            
                // Predizer posição baseada na velocidade
                var deltaTime = (timestamp - track.LastUpdate).TotalSeconds;
                var (predictedX, predictedY) = track.PredictNextPosition(deltaTime);
            
                for (int i = 0; i < unassociatedDetections.Count; i++)
                {
                    var detection = unassociatedDetections[i];
                    var distance = track.DistanceTo(detection.CenterX, detection.CenterY);
                
                    // Considerar também a distância da posição predita
                    var predictedDistance = (float)Math.Sqrt(
                        Math.Pow(predictedX - detection.CenterX, 2) + 
                        Math.Pow(predictedY - detection.CenterY, 2));
                
                    var combinedDistance = Math.Min(distance, predictedDistance);
                
                    if (combinedDistance < bestDistance)
                    {
                        bestDistance = combinedDistance;
                        bestMatch = i;
                    }
                }
            
                if (bestMatch >= 0)
                {
                    associations.Add((track, unassociatedDetections[bestMatch]));
                    unassociatedDetections.RemoveAt(bestMatch);
                }
                else
                {
                    // Track sem detecção associada
                    associations.Add((track, null));
                }
            }
        
            return associations;
        }
    
        /// <summary>
        /// Verifica se um track deve ser removido
        /// </summary>
        public bool ShouldRemoveTrack(TrackedObject track)
        {
            return track.ConsecutiveMisses > _maxMissedFrames;
        }
    
        /// <summary>
        /// Filtra detecções de baixa qualidade
        /// </summary>
        public List<DetectionResult> FilterDetections(List<DetectionResult> detections, float minConfidence = 0.5f)
        {
            return detections
                .Where(d => d.Confidence >= minConfidence)
                .Where(d => d.Width > 10 && d.Height > 10) // Filtrar detecções muito pequenas
                .Where(d => d.Area < 100000) // Filtrar detecções muito grandes
                .ToList();
        }
    
        /// <summary>
        /// Calcula matriz de custos para associação Hungarian (implementação simplificada)
        /// </summary>
        private float[,] CalculateCostMatrix(List<TrackedObject> tracks, List<DetectionResult> detections)
        {
            var costs = new float[tracks.Count, detections.Count];
        
            for (int i = 0; i < tracks.Count; i++)
            {
                for (int j = 0; j < detections.Count; j++)
                {
                    var track = tracks[i];
                    var detection = detections[j];
                
                    // Custo baseado em distância e IoU
                    var distance = track.DistanceTo(detection.CenterX, detection.CenterY);
                    var iou = track.CalculateIoU(detection.X, detection.Y, detection.Width, detection.Height);
                
                    // Combinar métricas (menor custo = melhor match)
                    costs[i, j] = distance * (1 - iou);
                }
            }
        
            return costs;
        }
    }
}
