using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace EntradaSaida.ML.Detection
{
    /// <summary>
    /// Carregador e gerenciador de modelos ONNX
    /// </summary>
    public class ModelLoader : IDisposable
    {
        private InferenceSession? _session;
        private bool _disposed;
    
        public bool IsLoaded => _session != null;
        public string? ModelPath { get; private set; }
        public DateTime? LoadedAt { get; private set; }
    
        /// <summary>
        /// Carrega um modelo ONNX
        /// </summary>
        public async Task<bool> LoadModelAsync(string modelPath)
        {
            try
            {
                if (!File.Exists(modelPath))
                {
                    throw new FileNotFoundException($"Modelo não encontrado: {modelPath}");
                }
            
                // Configurações de sessão para otimizar performance
                var sessionOptions = new SessionOptions
                {
                    EnableCpuMemArena = true,
                    EnableMemoryPattern = true,
                    GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL
                };
            
                // Usar GPU se disponível
                if (IsGpuAvailable())
                {
                    sessionOptions.AppendExecutionProvider_CUDA();
                }
            
                _session?.Dispose();
                _session = new InferenceSession(modelPath, sessionOptions);
            
                ModelPath = modelPath;
                LoadedAt = DateTime.UtcNow;
            
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar modelo: {ex.Message}");
                return await Task.FromResult(false);
            }
        }
    
        /// <summary>
        /// Executa inferência no modelo
        /// </summary>
        public async Task<float[][]> RunInferenceAsync(float[] inputData, int batchSize, int channels, int height, int width)
        {
            if (_session == null)
                throw new InvalidOperationException("Modelo não carregado");
        
            var inputTensor = new DenseTensor<float>(inputData, new[] { batchSize, channels, height, width });
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor(_session.InputMetadata.Keys.First(), inputTensor)
            };
        
            using var results = _session.Run(inputs);
            var output = results.First().AsEnumerable<float>().ToArray();
        
            // Reshape output para formato conveniente [num_detections, 85]
            // 85 = 4 (bbox) + 1 (confidence) + 80 (classes COCO)
            var numDetections = output.Length / 85;
            var reshapedOutput = new float[numDetections][];
        
            for (int i = 0; i < numDetections; i++)
            {
                reshapedOutput[i] = new float[85];
                Array.Copy(output, i * 85, reshapedOutput[i], 0, 85);
            }
        
            return await Task.FromResult(reshapedOutput);
        }
    
        /// <summary>
        /// Obtém metadados do modelo
        /// </summary>
        public ModelMetadata GetModelMetadata()
        {
            if (_session == null)
                throw new InvalidOperationException("Modelo não carregado");
        
            var inputMetadata = _session.InputMetadata.First();
            var outputMetadata = _session.OutputMetadata.First();
        
            return new ModelMetadata
            {
                InputName = inputMetadata.Key,
                InputShape = inputMetadata.Value.Dimensions.ToArray(),
                OutputName = outputMetadata.Key,
                OutputShape = outputMetadata.Value.Dimensions.ToArray(),
                ModelPath = ModelPath ?? string.Empty,
                LoadedAt = LoadedAt ?? DateTime.MinValue
            };
        }
    
        private static bool IsGpuAvailable()
        {
            try
            {
                // Verificação simples se CUDA está disponível
                var providers = OrtEnv.Instance().GetAvailableProviders();
                return providers.Contains("CUDAExecutionProvider");
            }
            catch
            {
                return false;
            }
        }
    
        public void Dispose()
        {
            if (!_disposed)
            {
                _session?.Dispose();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Metadados do modelo carregado
    /// </summary>
    public class ModelMetadata
    {
        public string InputName { get; set; } = string.Empty;
        public int[] InputShape { get; set; } = Array.Empty<int>();
        public string OutputName { get; set; } = string.Empty;
        public int[] OutputShape { get; set; } = Array.Empty<int>();
        public string ModelPath { get; set; } = string.Empty;
        public DateTime LoadedAt { get; set; }
    }
}
