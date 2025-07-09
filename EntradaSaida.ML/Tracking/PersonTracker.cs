using EntradaSaida.ML.Detection;
using EntradaSaida.ML.Tracking;

namespace EntradaSaida.ML.Tracking;

/// <summary>
/// Rastreador principal de pessoas
/// </summary>
public class PersonTracker
{
    private readonly TrackingAlgorithm _algorithm;
    private readonly List<TrackedObject> _activeTracks = new();
    private int _nextTrackId = 1;
    
    public PersonTracker(float maxDistance = 50.0f, float minIoU = 0.3f, int maxMissedFrames = 10)
    {
        _algorithm = new TrackingAlgorithm(maxDistance, minIoU, maxMissedFrames);
    }
    
    /// <summary>
    /// Atualiza o rastreamento com novas detecções
    /// </summary>
    public List<TrackedObject> Update(List<DetectionResult> detections, DateTime timestamp)
    {
        // Filtrar detecções de baixa qualidade
        var filteredDetections = _algorithm.FilterDetections(detections);
        
        // Associar detecções a tracks existentes
        var associations = _algorithm.Associate(_activeTracks, filteredDetections, timestamp);
        
        // Atualizar tracks existentes
        var updatedTracks = new List<TrackedObject>();
        var usedDetections = new HashSet<DetectionResult>();
        
        foreach (var (track, detection) in associations)
        {
            if (detection != null)
            {
                // Atualizar track com nova detecção
                track.Update(detection.X, detection.Y, detection.Width, detection.Height, detection.Confidence, timestamp);
                usedDetections.Add(detection);
            }
            else
            {
                // Track sem detecção - incrementar misses
                track.ConsecutiveMisses++;
            }
            
            // Manter track se ainda é válido
            if (!_algorithm.ShouldRemoveTrack(track))
            {
                updatedTracks.Add(track);
            }
        }
        
        // Criar novos tracks para detecções não associadas
        var unassociatedDetections = filteredDetections.Where(d => !usedDetections.Contains(d)).ToList();
        
        foreach (var detection in unassociatedDetections)
        {
            var newTrack = new TrackedObject
            {
                Id = _nextTrackId++,
                X = detection.X,
                Y = detection.Y,
                Width = detection.Width,
                Height = detection.Height,
                Confidence = detection.Confidence,
                LastUpdate = timestamp,
                ConsecutiveMisses = 0
            };
            
            // Adicionar posição inicial ao histórico
            newTrack.TrackHistory.Add((newTrack.CenterX, newTrack.CenterY, timestamp));
            updatedTracks.Add(newTrack);
        }
        
        _activeTracks.Clear();
        _activeTracks.AddRange(updatedTracks);
        
        return new List<TrackedObject>(_activeTracks);
    }
    
    /// <summary>
    /// Obtém todos os tracks ativos
    /// </summary>
    public List<TrackedObject> GetActiveTracks()
    {
        return new List<TrackedObject>(_activeTracks);
    }
    
    /// <summary>
    /// Obtém track por ID
    /// </summary>
    public TrackedObject? GetTrack(int trackId)
    {
        return _activeTracks.FirstOrDefault(t => t.Id == trackId);
    }
    
    /// <summary>
    /// Remove todos os tracks
    /// </summary>
    public void Reset()
    {
        _activeTracks.Clear();
        _nextTrackId = 1;
    }
    
    /// <summary>
    /// Obtém estatísticas do rastreamento
    /// </summary>
    public TrackingStats GetStats()
    {
        return new TrackingStats
        {
            ActiveTracks = _activeTracks.Count,
            TotalTracksCreated = _nextTrackId - 1,
            AverageTrackAge = _activeTracks.Count > 0 
                ? _activeTracks.Average(t => t.TrackHistory.Count) 
                : 0,
            OldestTrack = _activeTracks.Count > 0 
                ? _activeTracks.Max(t => t.TrackHistory.Count) 
                : 0
        };
    }
}

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
