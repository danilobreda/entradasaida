using EntradaSaida.Core.Interfaces;
using EntradaSaida.Core.Models;

namespace EntradaSaida.Core.Services
{
    /// <summary>
    /// Implementação do serviço de contagem
    /// </summary>
    public class CounterService : ICounterService
    {
        private readonly List<CountingLine> _countingLines = new();
        private readonly List<TrackedPerson> _trackedPersons = new();
        private readonly List<CounterEvent> _events = new();
        private int _nextPersonId = 1;
        private int _nextEventId = 1;

        public async Task<List<CounterEvent>> ProcessDetectionsAsync(List<Detection> detections)
        {
            var events = new List<CounterEvent>();
        
            // Atualizar pessoas rastreadas
            await UpdateTrackedPersonsAsync(detections);
        
            // Verificar cruzamentos de linha
            var activeLines = await GetActiveCountingLinesAsync();
        
            foreach (var person in _trackedPersons.Where(p => p.Status == TrackingStatus.Active))
            {
                if (person.Detections.Count < 2) continue;
            
                var previous = person.Detections[^2];
                var current = person.Detections[^1];
            
                foreach (var line in activeLines)
                {
                    if (line.HasPersonCrossed(previous.CenterX, previous.CenterY, current.CenterX, current.CenterY))
                    {
                        var eventType = line.GetCrossingDirection(previous.CenterX, previous.CenterY, current.CenterX, current.CenterY);
                    
                        var counterEvent = new CounterEvent
                        {
                            Id = _nextEventId++,
                            Timestamp = current.Timestamp,
                            Type = eventType,
                            PersonId = person.Id,
                            PositionX = current.CenterX,
                            PositionY = current.CenterY,
                            CameraId = line.CameraId,
                            Confidence = current.Confidence
                        };
                    
                        _events.Add(counterEvent);
                        events.Add(counterEvent);
                    }
                }
            }
        
            return events;
        }

        public async Task<CountingLine> AddCountingLineAsync(CountingLine line)
        {
            line.Id = _countingLines.Count + 1;
            _countingLines.Add(line);
            return await Task.FromResult(line);
        }

        public async Task<bool> RemoveCountingLineAsync(int lineId)
        {
            var line = _countingLines.FirstOrDefault(l => l.Id == lineId);
            if (line != null)
            {
                _countingLines.Remove(line);
                return await Task.FromResult(true);
            }
            return await Task.FromResult(false);
        }

        public async Task<List<CountingLine>> GetActiveCountingLinesAsync()
        {
            return await Task.FromResult(_countingLines.Where(l => l.IsActive).ToList());
        }

        public async Task<CounterStats> GetStatsAsync(DateTime date)
        {
            var dayEvents = _events.Where(e => e.Timestamp.Date == date.Date).ToList();
        
            var stats = new CounterStats
            {
                Date = date.Date,
                TotalEntries = dayEvents.Count(e => e.Type == CounterEventType.Entry),
                TotalExits = dayEvents.Count(e => e.Type == CounterEventType.Exit)
            };
        
            stats.CurrentOccupancy = stats.Balance;
        
            // Calcular estatísticas por hora
            for (int hour = 0; hour < 24; hour++)
            {
                var hourEvents = dayEvents.Where(e => e.Timestamp.Hour == hour).ToList();
                stats.HourlyBreakdown.Add(new HourlyStats
                {
                    Hour = hour,
                    Entries = hourEvents.Count(e => e.Type == CounterEventType.Entry),
                    Exits = hourEvents.Count(e => e.Type == CounterEventType.Exit)
                });
            }
        
            return await Task.FromResult(stats);
        }

        public async Task<List<CounterStats>> GetStatsRangeAsync(DateTime startDate, DateTime endDate)
        {
            var stats = new List<CounterStats>();
            var currentDate = startDate.Date;
        
            while (currentDate <= endDate.Date)
            {
                stats.Add(await GetStatsAsync(currentDate));
                currentDate = currentDate.AddDays(1);
            }
        
            return stats;
        }

        public async Task<int> GetCurrentOccupancyAsync()
        {
            var todayStats = await GetStatsAsync(DateTime.Today);
            return await Task.FromResult(Math.Max(0, todayStats.Balance));
        }

        public async Task<List<CounterEvent>> GetTodayEventsAsync()
        {
            return await Task.FromResult(_events.Where(e => e.Timestamp.Date == DateTime.Today).ToList());
        }

        public async Task ResetCountersAsync()
        {
            _events.Clear();
            _trackedPersons.Clear();
            _nextPersonId = 1;
            _nextEventId = 1;
            await Task.CompletedTask;
        }

        private async Task UpdateTrackedPersonsAsync(List<Detection> detections)
        {
            const float maxTrackingDistance = 50.0f;
        
            // Marcar todas as pessoas como perdidas inicialmente
            foreach (var person in _trackedPersons)
            {
                person.Miss();
            }
        
            // Associar detecções a pessoas rastreadas
            foreach (var detection in detections)
            {
                var closestPerson = _trackedPersons
                    .Where(p => p.Status == TrackingStatus.Active)
                    .OrderBy(p => CalculateDistance(p.CurrentX, p.CurrentY, detection.CenterX, detection.CenterY))
                    .FirstOrDefault();
            
                if (closestPerson != null && 
                    CalculateDistance(closestPerson.CurrentX, closestPerson.CurrentY, detection.CenterX, detection.CenterY) < maxTrackingDistance)
                {
                    closestPerson.AddDetection(detection);
                }
                else
                {
                    // Nova pessoa
                    var newPerson = new TrackedPerson
                    {
                        Id = _nextPersonId++,
                        FirstSeen = detection.Timestamp,
                        Status = TrackingStatus.Active
                    };
                    newPerson.AddDetection(detection);
                    _trackedPersons.Add(newPerson);
                }
            }
        
            // Remover pessoas perdidas há muito tempo
            _trackedPersons.RemoveAll(p => p.Status == TrackingStatus.Lost && p.ConsecutiveMisses > 30);
        
            await Task.CompletedTask;
        }
    
        private static float CalculateDistance(float x1, float y1, float x2, float y2)
        {
            return (float)Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
        }
    }
}
