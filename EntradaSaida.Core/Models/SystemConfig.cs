namespace EntradaSaida.Core.Models
{
    /// <summary>
    /// Configurações do sistema
    /// </summary>
    public class SystemConfig
    {
        public int Id { get; set; }
        public string CameraUrl { get; set; }
        public string ModelPath { get; set; }
        public float ConfidenceThreshold { get; set; }
        public float MaxTrackingDistance { get; set; }
        public int TrackingTimeout { get; set; }
        public int VideoWidth { get; set; }
        public int VideoHeight { get; set; }
        public int FrameRate { get; set; }
        public bool EnableRecording { get; set; }
        public string RecordingPath { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public SystemConfig()
        {
            CameraUrl = string.Empty;
            ModelPath = "C:\\PROJETOS\\danilobreda\\entradasaida\\models\\yolov8n.onnx";
            ConfidenceThreshold = 0.5f;
            MaxTrackingDistance = 50.0f;
            TrackingTimeout = 30;
            VideoWidth = 640;
            VideoHeight = 480;
            FrameRate = 30;
            EnableRecording = false;
            RecordingPath = "recordings/";
        }
    }
}
