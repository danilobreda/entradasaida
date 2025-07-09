using EntradaSaida.Core.Models;

namespace EntradaSaida.Core.Interfaces
{
    /// <summary>
    /// Interface para serviços de detecção de pessoas
    /// </summary>
    public interface IDetectionService
    {
        /// <summary>
        /// Detecta pessoas em um frame de vídeo
        /// </summary>
        Task<List<Detection>> DetectPersonsAsync(byte[] imageData);
    
        /// <summary>
        /// Detecta pessoas em um frame com configurações específicas
        /// </summary>
        Task<List<Detection>> DetectPersonsAsync(byte[] imageData, float confidenceThreshold);
    
        /// <summary>
        /// Carrega o modelo YOLO
        /// </summary>
        Task<bool> LoadModelAsync(string modelPath);
    
        /// <summary>
        /// Verifica se o modelo está carregado
        /// </summary>
        bool IsModelLoaded { get; }
    
        /// <summary>
        /// Obtém informações sobre o modelo carregado
        /// </summary>
        Task<ModelInfo> GetModelInfoAsync();
    }

    /// <summary>
    /// Informações sobre o modelo carregado
    /// </summary>
    public class ModelInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public int InputWidth { get; set; }
        public int InputHeight { get; set; }
        public DateTime LoadedAt { get; set; }
    }
}
