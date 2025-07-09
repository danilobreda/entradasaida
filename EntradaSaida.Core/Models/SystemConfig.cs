namespace EntradaSaida.Core.Models;

/// <summary>
/// Configurações do sistema
/// </summary>
public class SystemConfig
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public ConfigType Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Configurações padrão
    public static class Keys
    {
        public const string CameraUrl = "camera_url";
        public const string ModelPath = "model_path";
        public const string ConfidenceThreshold = "confidence_threshold";
        public const string MaxTrackingDistance = "max_tracking_distance";
        public const string TrackingTimeout = "tracking_timeout";
        public const string VideoWidth = "video_width";
        public const string VideoHeight = "video_height";
        public const string FrameRate = "frame_rate";
        public const string EnableRecording = "enable_recording";
        public const string RecordingPath = "recording_path";
    }
}

/// <summary>
/// Tipo de configuração
/// </summary>
public enum ConfigType
{
    String,
    Integer,
    Float,
    Boolean,
    Json
}
