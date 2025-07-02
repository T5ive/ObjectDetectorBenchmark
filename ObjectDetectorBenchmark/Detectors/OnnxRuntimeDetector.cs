using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace ObjectDetectorBenchmark.Detectors;

public class OnnxRuntimeDetector : BaseDetector
{
    private InferenceSession? _session;
    private readonly string[] _labels =
    [
        "person", "bicycle", "car", "motorcycle", "airplane", "bus", "train", "truck", "boat", "traffic light",
        "fire hydrant", "stop sign", "parking meter", "bench", "bird", "cat", "dog", "horse", "sheep", "cow",
        "elephant", "bear", "zebra", "giraffe", "backpack", "umbrella", "handbag", "tie", "suitcase", "frisbee",
        "skis", "snowboard", "sports ball", "kite", "baseball bat", "baseball glove", "skateboard", "surfboard",
        "tennis racket", "bottle", "wine glass", "cup", "fork", "knife", "spoon", "bowl", "banana", "apple",
        "sandwich", "orange", "broccoli", "carrot", "hot dog", "pizza", "donut", "cake", "chair", "couch",
        "potted plant", "bed", "dining table", "toilet", "tv", "laptop", "mouse", "remote", "keyboard", "cell phone",
        "microwave", "oven", "toaster", "sink", "refrigerator", "book", "clock", "vase", "scissors", "teddy bear",
        "hair drier", "toothbrush"
    ];

    public override string Name => "OnnxRuntime";

    public override async Task InitializeAsync(string modelPath)
    {
        await Task.Run(() =>
        {
            var options = new SessionOptions();
            options.LogSeverityLevel = OrtLoggingLevel.ORT_LOGGING_LEVEL_ERROR;
            _session = new InferenceSession(modelPath, options);
        });
    }

    public override async Task<DetectionResult[]> DetectAsync(string imagePath)
    {
        if (_session == null)
            throw new InvalidOperationException("Detector not initialized");

        return await Task.Run(() =>
        {
            using var image = Image.Load<Rgb24>(imagePath);
            image.Mutate(x => x.Resize(640, 640));

            // Convert image to tensor
            var tensor = new DenseTensor<float>(new[] { 1, 3, 640, 640 });
            for (var y = 0; y < 640; y++)
            {
                for (var x = 0; x < 640; x++)
                {
                    var pixel = image[x, y];
                    tensor[0, 0, y, x] = pixel.R / 255.0f;
                    tensor[0, 1, y, x] = pixel.G / 255.0f;
                    tensor[0, 2, y, x] = pixel.B / 255.0f;
                }
            }

            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("images", tensor)
            };

            using var results = _session.Run(inputs);
            var output = results[0].AsTensor<float>();

            return ParseDetections(output);
        });
    }

    private DetectionResult[] ParseDetections(Tensor<float> output)
    {
        var detections = new List<DetectionResult>();

        // YOLO output format: [batch, 84, 8400] where 84 = 4 (bbox) + 80 (classes)
        var numDetections = output.Dimensions[2];

        for (var i = 0; i < numDetections; i++)
        {
            var centerX = output[0, 0, i];
            var centerY = output[0, 1, i];
            var width = output[0, 2, i];
            var height = output[0, 3, i];

            // Get class scores
            float maxScore = 0;
            var classId = 0;
            for (var j = 4; j < 84; j++)
            {
                var score = output[0, j, i];
                if (score > maxScore)
                {
                    maxScore = score;
                    classId = j - 4;
                }
            }

            if (maxScore > 0.5f) // Confidence threshold
            {
                var x = centerX - width / 2;
                var y = centerY - height / 2;

                var label = classId < _labels.Length ? _labels[classId] : $"class_{classId}";

                detections.Add(new DetectionResult(label, maxScore, x, y, width, height));
            }
        }

        return detections.ToArray();
    }

    public override void Dispose()
    {
        _session?.Dispose();
    }
}
