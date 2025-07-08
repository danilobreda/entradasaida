using EntradaSaida.Core.Interfaces;
using EntradaSaida.Core.Models;
using EntradaSaida.ML.Detection;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace EntradaSaida.ML.Detection;

/// <summary>
/// Detector YOLOv8 usando ONNX Runtime
/// </summary>
public class YoloV8Detector : IDetectionService, IDisposable
{
    private readonly ModelLoader _modelLoader;
    private bool _disposed;
    
    // Configurações do modelo
    private const int ModelInputSize = 640;
    private const float DefaultConfidenceThreshold = 0.5f;
    private const float NmsThreshold = 0.4f;
    
    // Classes COCO - índice 0 é "person"
    private static readonly string[] CocoClasses = {
        "person", "bicycle", "car", "motorcycle", "airplane", "bus", "train", "truck", "boat",
        "traffic light", "fire hydrant", "stop sign", "parking meter", "bench", "bird", "cat",
        "dog", "horse", "sheep", "cow", "elephant", "bear", "zebra", "giraffe", "backpack",
        "umbrella", "handbag", "tie", "suitcase", "frisbee", "skis", "snowboard", "sports ball",
        "kite", "baseball bat", "baseball glove", "skateboard", "surfboard", "tennis racket",
        "bottle", "wine glass", "cup", "fork", "knife", "spoon", "bowl", "banana", "apple",
        "sandwich", "orange", "broccoli", "carrot", "hot dog", "pizza", "donut", "cake",
        "chair", "couch", "potted plant", "bed", "dining table", "toilet", "tv", "laptop",
        "mouse", "remote", "keyboard", "cell phone", "microwave", "oven", "toaster", "sink",
        "refrigerator", "book", "clock", "vase", "scissors", "teddy bear", "hair drier", "toothbrush"
    };
    
    public YoloV8Detector()
    {
        _modelLoader = new ModelLoader();
    }
    
    public bool IsModelLoaded => _modelLoader.IsLoaded;
    
    public async Task<bool> LoadModelAsync(string modelPath)
    {
        return await _modelLoader.LoadModelAsync(modelPath);
    }
    
    public async Task<List<Detection>> DetectPersonsAsync(byte[] imageData)
    {
        return await DetectPersonsAsync(imageData, DefaultConfidenceThreshold);
    }
    
    public async Task<List<Detection>> DetectPersonsAsync(byte[] imageData, float confidenceThreshold)
    {
        if (!IsModelLoaded)
            throw new InvalidOperationException("Modelo não carregado. Chame LoadModelAsync primeiro.");
        
        try
        {
            // Carregar e preprocessar imagem
            using var mat = new Mat();
            CvInvoke.Imdecode(imageData, ImreadModes.Color, mat);
            
            var preprocessed = PreprocessImage(mat);
            
            // Executar inferência
            var outputs = await _modelLoader.RunInferenceAsync(preprocessed.inputData, 1, 3, ModelInputSize, ModelInputSize);
            
            // Pós-processar resultados
            var detectionResults = PostprocessOutputs(outputs, confidenceThreshold, preprocessed.scaleX, preprocessed.scaleY);
            
            // Filtrar apenas pessoas e converter para o formato do domínio
            var personDetections = detectionResults
                .Where(d => d.ClassId == 0 && d.ClassName == "person") // person é classe 0 no COCO
                .Select(d => new Detection
                {
                    Timestamp = DateTime.UtcNow,
                    X = d.X,
                    Y = d.Y,
                    Width = d.Width,
                    Height = d.Height,
                    Confidence = d.Confidence,
                    Class = d.ClassName
                })
                .ToList();
            
            return personDetections;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro na detecção: {ex.Message}");
            return new List<Detection>();
        }
    }
    
    public async Task<ModelInfo> GetModelInfoAsync()
    {
        if (!IsModelLoaded)
            throw new InvalidOperationException("Modelo não carregado");
        
        var metadata = _modelLoader.GetModelMetadata();
        
        return await Task.FromResult(new ModelInfo
        {
            Name = "YOLOv8",
            Version = "8.0",
            InputWidth = ModelInputSize,
            InputHeight = ModelInputSize,
            LoadedAt = metadata.LoadedAt
        });
    }
    
    /// <summary>
    /// Preprocessa a imagem para o formato esperado pelo modelo
    /// </summary>
    private (float[] inputData, float scaleX, float scaleY) PreprocessImage(Mat image)
    {
        // Calcular escalas para redimensionamento
        var scaleX = (float)ModelInputSize / image.Width;
        var scaleY = (float)ModelInputSize / image.Height;
        
        // Redimensionar mantendo aspect ratio
        using var resized = new Mat();
        CvInvoke.Resize(image, resized, new System.Drawing.Size(ModelInputSize, ModelInputSize));
        
        // Converter BGR para RGB
        using var rgb = new Mat();
        CvInvoke.CvtColor(resized, rgb, ColorConversion.Bgr2Rgb);
        
        // Normalizar para [0,1] e converter para formato NCHW
        var inputData = new float[3 * ModelInputSize * ModelInputSize];
        var rgbBytes = rgb.GetData();
        
        for (int c = 0; c < 3; c++)
        {
            for (int h = 0; h < ModelInputSize; h++)
            {
                for (int w = 0; w < ModelInputSize; w++)
                {
                    var pixelIndex = (h * ModelInputSize + w) * 3 + c;
                    var outputIndex = c * ModelInputSize * ModelInputSize + h * ModelInputSize + w;
                    inputData[outputIndex] = rgbBytes[pixelIndex] / 255.0f;
                }
            }
        }
        
        return (inputData, scaleX, scaleY);
    }
    
    /// <summary>
    /// Pós-processa as saídas do modelo YOLO
    /// </summary>
    private List<DetectionResult> PostprocessOutputs(float[][] outputs, float confidenceThreshold, float scaleX, float scaleY)
    {
        var detections = new List<DetectionResult>();
        
        foreach (var output in outputs)
        {
            // output format: [x_center, y_center, width, height, confidence, class_scores...]
            var confidence = output[4];
            
            if (confidence < confidenceThreshold) continue;
            
            // Encontrar classe com maior score
            var classScores = output.Skip(5).ToArray();
            var maxClassIndex = Array.IndexOf(classScores, classScores.Max());
            var classConfidence = classScores[maxClassIndex];
            var finalConfidence = confidence * classConfidence;
            
            if (finalConfidence < confidenceThreshold) continue;
            
            // Converter coordenadas do centro para canto superior esquerdo
            var centerX = output[0];
            var centerY = output[1];
            var width = output[2];
            var height = output[3];
            
            var x = (centerX - width / 2) / scaleX;
            var y = (centerY - height / 2) / scaleY;
            
            detections.Add(new DetectionResult
            {
                X = x,
                Y = y,
                Width = width / scaleX,
                Height = height / scaleY,
                Confidence = finalConfidence,
                ClassId = maxClassIndex,
                ClassName = maxClassIndex < CocoClasses.Length ? CocoClasses[maxClassIndex] : "unknown"
            });
        }
        
        // Aplicar Non-Maximum Suppression
        return ApplyNMS(detections, NmsThreshold);
    }
    
    /// <summary>
    /// Aplica Non-Maximum Suppression para remover detecções duplicadas
    /// </summary>
    private static List<DetectionResult> ApplyNMS(List<DetectionResult> detections, float nmsThreshold)
    {
        var sortedDetections = detections.OrderByDescending(d => d.Confidence).ToList();
        var keep = new List<DetectionResult>();
        
        while (sortedDetections.Count > 0)
        {
            var current = sortedDetections[0];
            keep.Add(current);
            sortedDetections.RemoveAt(0);
            
            // Remover detecções com IoU alto
            sortedDetections.RemoveAll(d => current.CalculateIoU(d) > nmsThreshold);
        }
        
        return keep;
    }
    
    public void Dispose()
    {
        if (!_disposed)
        {
            _modelLoader?.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
