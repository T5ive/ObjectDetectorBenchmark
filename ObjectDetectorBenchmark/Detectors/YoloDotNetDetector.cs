using SkiaSharp;
using YoloDotNet;
using YoloDotNet.Enums;
using YoloDotNet.Models;

namespace ObjectDetectorBenchmark.Detectors;

public class YoloDotNetDetector : BaseDetector
{
    private Yolo? _yolo;

    public override string Name => "YoloDotNet";

    public override async Task InitializeAsync(string modelPath)
    {
        await Task.Run(() =>
        {
            _yolo = new Yolo(new YoloOptions
            {
                OnnxModel = modelPath,
                ModelType = ModelType.ObjectDetection,
                Cuda = false
            });
        });
    }

    public override async Task<DetectionResult[]> DetectAsync(string imagePath)
    {
        if (_yolo == null)
            throw new InvalidOperationException("Detector not initialized");

        return await Task.Run(() =>
        {
            using var image = SKImage.FromEncodedData(imagePath);
            var results = _yolo.RunObjectDetection(image, 0.5f);
            return results.Select(r => new DetectionResult(
                r.Label.Name,
                r.Confidence,
                r.BoundingBox.Left,
                r.BoundingBox.Top,
                r.BoundingBox.Width,
                r.BoundingBox.Height
            )).ToArray();
        });
    }

    public override void Dispose()
    {
        _yolo?.Dispose();
    }
}
