namespace EntradaSaida.Core.Models;

/// <summary>
/// Representa uma linha virtual para contagem de entrada/saída
/// </summary>
public class CountingLine
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public float StartX { get; set; }
    public float StartY { get; set; }
    public float EndX { get; set; }
    public float EndY { get; set; }
    public LineDirection Direction { get; set; }
    public bool IsActive { get; set; } = true;
    public string? CameraId { get; set; }
    
    /// <summary>
    /// Verifica se uma pessoa cruzou a linha
    /// </summary>
    public bool HasPersonCrossed(float previousX, float previousY, float currentX, float currentY)
    {
        // Implementação do algoritmo de intersecção de linha
        return DoLinesIntersect(previousX, previousY, currentX, currentY, StartX, StartY, EndX, EndY);
    }
    
    /// <summary>
    /// Determina a direção do cruzamento
    /// </summary>
    public CounterEventType GetCrossingDirection(float previousX, float previousY, float currentX, float currentY)
    {
        // Produto vetorial para determinar direção
        var lineVector = (EndX - StartX, EndY - StartY);
        var movementVector = (currentX - previousX, currentY - previousY);
        
        var crossProduct = lineVector.Item1 * movementVector.Item2 - lineVector.Item2 * movementVector.Item1;
        
        return Direction switch
        {
            LineDirection.LeftToRight => crossProduct > 0 ? CounterEventType.Entry : CounterEventType.Exit,
            LineDirection.RightToLeft => crossProduct < 0 ? CounterEventType.Entry : CounterEventType.Exit,
            LineDirection.TopToBottom => crossProduct > 0 ? CounterEventType.Entry : CounterEventType.Exit,
            LineDirection.BottomToTop => crossProduct < 0 ? CounterEventType.Entry : CounterEventType.Exit,
            _ => CounterEventType.Entry
        };
    }
    
    private static bool DoLinesIntersect(float p1x, float p1y, float p2x, float p2y, 
                                       float p3x, float p3y, float p4x, float p4y)
    {
        var denom = (p1x - p2x) * (p3y - p4y) - (p1y - p2y) * (p3x - p4x);
        if (Math.Abs(denom) < 1e-10) return false;
        
        var t = ((p1x - p3x) * (p3y - p4y) - (p1y - p3y) * (p3x - p4x)) / denom;
        var u = -((p1x - p2x) * (p1y - p3y) - (p1y - p2y) * (p1x - p3x)) / denom;
        
        return t >= 0 && t <= 1 && u >= 0 && u <= 1;
    }
}

/// <summary>
/// Direção da linha de contagem
/// </summary>
public enum LineDirection
{
    LeftToRight,
    RightToLeft,
    TopToBottom,
    BottomToTop
}
