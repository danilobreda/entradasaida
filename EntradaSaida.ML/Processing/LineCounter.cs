using EntradaSaida.Core.Models;
using EntradaSaida.ML.Detection;
using EntradaSaida.ML.Tracking;

namespace EntradaSaida.ML.Processing
{
    /// <summary>
    /// Contador de linha para detectar entradas e saídas
    /// </summary>
    public class LineCounter
    {
        private readonly List<CountingLine> _lines = new();
        private readonly Dictionary<int, (float x, float y, DateTime timestamp)> _lastPositions = new();
    
        /// <summary>
        /// Adiciona uma linha de contagem
        /// </summary>
        public void AddLine(CountingLine line)
        {
            _lines.Add(line);
        }
    
        /// <summary>
        /// Remove uma linha de contagem
        /// </summary>
        public bool RemoveLine(int lineId)
        {
            var line = _lines.FirstOrDefault(l => l.Id == lineId);
            if (line != null)
            {
                _lines.Remove(line);
                return true;
            }
            return false;
        }
    
        /// <summary>
        /// Obtém todas as linhas ativas
        /// </summary>
        public List<CountingLine> GetActiveLines()
        {
            return _lines.Where(l => l.IsActive).ToList();
        }
    
        /// <summary>
        /// Verifica cruzamentos e gera eventos de contagem
        /// </summary>
        public List<CounterEvent> CheckCrossings(List<TrackedObject> tracks, DateTime timestamp)
        {
            var events = new List<CounterEvent>();
            var activeLines = GetActiveLines();
        
            foreach (var track in tracks)
            {
                var currentPos = (track.CenterX, track.CenterY);
            
                // Verificar se temos posição anterior para este track
                if (_lastPositions.TryGetValue(track.Id, out var lastPos))
                {
                    foreach (var line in activeLines)
                    {
                        if (HasCrossedLine(lastPos.x, lastPos.y, currentPos.Item1, currentPos.Item2, line))
                        {
                            var direction = GetCrossingDirection(lastPos.x, lastPos.y, currentPos.Item1, currentPos.Item2, line);
                        
                            var counterEvent = new CounterEvent
                            {
                                Timestamp = timestamp,
                                Type = direction,
                                PersonId = track.Id,
                                PositionX = currentPos.Item1,
                                PositionY = currentPos.Item2,
                                CameraId = line.CameraId,
                                Confidence = track.Confidence
                            };
                        
                            events.Add(counterEvent);
                        
                            // Log do evento
                            Console.WriteLine($"[{timestamp:HH:mm:ss}] {direction} detectada - Pessoa {track.Id} na linha {line.Name}");
                        }
                    }
                }
            
                // Atualizar posição anterior
                _lastPositions[track.Id] = (currentPos.Item1, currentPos.Item2, timestamp);
            }
        
            // Limpar posições antigas (tracks que não estão mais ativos)
            var activeTrackIds = tracks.Select(t => t.Id).ToHashSet();
            var keysToRemove = _lastPositions.Keys.Where(id => !activeTrackIds.Contains(id)).ToList();
            foreach (var key in keysToRemove)
            {
                _lastPositions.Remove(key);
            }
        
            return events;
        }
    
        /// <summary>
        /// Verifica se uma linha foi cruzada
        /// </summary>
        private static bool HasCrossedLine(float x1, float y1, float x2, float y2, CountingLine line)
        {
            return DoLinesIntersect(x1, y1, x2, y2, line.StartX, line.StartY, line.EndX, line.EndY);
        }
    
        /// <summary>
        /// Determina a direção do cruzamento
        /// </summary>
        private static CounterEventType GetCrossingDirection(float x1, float y1, float x2, float y2, CountingLine line)
        {
            // Vetor da linha
            var lineVectorX = line.EndX - line.StartX;
            var lineVectorY = line.EndY - line.StartY;
        
            // Vetor do movimento
            var movementVectorX = x2 - x1;
            var movementVectorY = y2 - y1;
        
            // Produto vetorial para determinar direção
            var crossProduct = lineVectorX * movementVectorY - lineVectorY * movementVectorX;
        
            return line.Direction switch
            {
                LineDirection.LeftToRight => crossProduct > 0 ? CounterEventType.Entry : CounterEventType.Exit,
                LineDirection.RightToLeft => crossProduct < 0 ? CounterEventType.Entry : CounterEventType.Exit,
                LineDirection.TopToBottom => crossProduct > 0 ? CounterEventType.Entry : CounterEventType.Exit,
                LineDirection.BottomToTop => crossProduct < 0 ? CounterEventType.Entry : CounterEventType.Exit,
                _ => CounterEventType.Entry
            };
        }
    
        /// <summary>
        /// Verifica se duas linhas se intersectam
        /// </summary>
        private static bool DoLinesIntersect(float p1x, float p1y, float p2x, float p2y, float p3x, float p3y, float p4x, float p4y)
        {
            var denom = (p1x - p2x) * (p3y - p4y) - (p1y - p2y) * (p3x - p4x);
            if (Math.Abs(denom) < 1e-10) return false;
        
            var t = ((p1x - p3x) * (p3y - p4y) - (p1y - p3y) * (p3x - p4x)) / denom;
            var u = -((p1x - p2x) * (p1y - p3y) - (p1y - p2y) * (p1x - p3x)) / denom;
        
            return t >= 0 && t <= 1 && u >= 0 && u <= 1;
        }
    
        /// <summary>
        /// Limpa dados de rastreamento
        /// </summary>
        public void Reset()
        {
            _lastPositions.Clear();
        }
    
        /// <summary>
        /// Obtém estatísticas do contador
        /// </summary>
        public LineCounterStats GetStats()
        {
            return new LineCounterStats
            {
                ActiveLines = _lines.Count(l => l.IsActive),
                TotalLines = _lines.Count,
                TrackedObjects = _lastPositions.Count
            };
        }
    }

    /// <summary>
    /// Estatísticas do contador de linha
    /// </summary>
    public class LineCounterStats
    {
        public int ActiveLines { get; set; }
        public int TotalLines { get; set; }
        public int TrackedObjects { get; set; }
    }
}
