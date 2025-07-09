using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace EntradaSaida.ML.Detection
{
    public class YoloML : IDisposable
    {
        private int scaleX = 640;
        private int scaleY = 640;

        internal async Task<List<Core.Models.Detection>> DetectPersonsAsync(byte[] frameData)
        {
            // Read paths
            string modelFilePath = @"C:\PROJETOS\danilobreda\entradasaida\models\yolov8n.onnx";

            var input = GetImageTensorFromPath(frameData);
            var result = GetPredictions(input, modelFilePath);

            return new List<Core.Models.Detection>();
        }

        private Tensor<float> GetImageTensorFromPath(byte[] frameData)
        {
            // Read image
            using Image<Rgb24> image = Image.Load<Rgb24>(frameData);

            // Resize image
            image.Mutate(x =>
            {
                x.Resize(new ResizeOptions
                {
                    Size = new Size(scaleX, scaleY),
                    Mode = ResizeMode.Crop
                });
            });

            // Preprocess image
            Tensor<float> input = new DenseTensor<float>(new[] { 1, 3, scaleX, scaleY });
            var mean = new[] { 0.485f, 0.456f, 0.406f };
            var stddev = new[] { 0.229f, 0.224f, 0.225f };
            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    Span<Rgb24> pixelSpan = accessor.GetRowSpan(y);
                    for (int x = 0; x < accessor.Width; x++)
                    {
                        input[0, 0, y, x] = ((pixelSpan[x].R / 255f) - mean[0]) / stddev[0];
                        input[0, 1, y, x] = ((pixelSpan[x].G / 255f) - mean[1]) / stddev[1];
                        input[0, 2, y, x] = ((pixelSpan[x].B / 255f) - mean[2]) / stddev[2];
                    }
                }
            });

            return input;
        }

        private List<DetectionResult> GetPredictions(Tensor<float> input, string modelFilePath)
        {
            var session = new InferenceSession(modelFilePath, new SessionOptions()
            {
                EnableCpuMemArena = true,
                EnableMemoryPattern = true,
                GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL
            });

            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor(session.InputMetadata.Keys.First(), input)
            };

            // Run inference
            var result = session.Run(inputs).First().AsEnumerable<float>().ToArray();

            //FASE 1
            var numDetections = result.Length / 85;
            var reshapedOutput = new float[numDetections][];

            for (int i = 0; i < numDetections; i++)
            {
                reshapedOutput[i] = new float[85];
                Array.Copy(result, i * 85, reshapedOutput[i], 0, 85);
            }

            //FASE 2

            //verificando boxes
            var confidenceThreshold = 100;
            List<DetectionResult> detections = new();
            foreach (var output in reshapedOutput)
            {
                // output format: [x_center, y_center, width, height, confidence, class_scores...]
                var confidence = output[4];

                if (confidence < confidenceThreshold) continue;

                // Encontrar classe com maior score
                var classScores = output.Skip(5).ToArray();
                var classIndex = Array.IndexOf(classScores, classScores.Max());
                var classConfidence = classScores[classIndex];
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
                    ClassId = classIndex,
                    //ClassName = maxClassIndex < CocoClasses.Length ? CocoClasses[maxClassIndex] : "unknown"
                });
            }
            return detections;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
