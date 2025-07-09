namespace EntradaSaida.ML.Detection;

/// <summary>
/// Resultado de detecção do YOLO
/// </summary>
public class DetectionResult
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    public float Confidence { get; set; }
    public int ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    
    /// <summary>
    /// Centro X da detecção
    /// </summary>
    public float CenterX => X + Width / 2;
    
    /// <summary>
    /// Centro Y da detecção
    /// </summary>
    public float CenterY => Y + Height / 2;
    
    /// <summary>
    /// Área da bounding box
    /// </summary>
    public float Area => Width * Height;
    
    /// <summary>
    /// Calcula IoU (Intersection over Union) com outra detecção
    /// </summary>
    public float CalculateIoU(DetectionResult other)
    {
        var x1 = Math.Max(X, other.X);
        var y1 = Math.Max(Y, other.Y);
        var x2 = Math.Min(X + Width, other.X + other.Width);
        var y2 = Math.Min(Y + Height, other.Y + other.Height);
        
        if (x2 <= x1 || y2 <= y1) return 0;
        
        var intersection = (x2 - x1) * (y2 - y1);
        var union = Area + other.Area - intersection;
        
        return intersection / union;
    }
}
