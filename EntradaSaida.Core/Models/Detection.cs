namespace EntradaSaida.Core.Models;

/// <summary>
/// Representa uma detecção de pessoa realizada pelo modelo YOLO
/// </summary>
public class Detection
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    public float Confidence { get; set; }
    public string Class { get; set; } = "person";
    
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
}
