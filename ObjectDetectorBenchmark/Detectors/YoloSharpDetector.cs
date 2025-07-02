using Compunet.YoloSharp;

namespace ObjectDetectorBenchmark.Detectors;

public class YoloSharpDetector : BaseDetector
{
    private YoloPredictor? _predictor;

    public override string Name => "YoloSharp";

    public override async Task InitializeAsync(string modelPath)
    {
        await Task.Run(() =>
        {
            _predictor = new YoloPredictor(modelPath, new YoloPredictorOptions
            {
                UseCuda = false
            });
        });
    }

    public override async Task<DetectionResult[]> DetectAsync(string imagePath)
    {
        if (_predictor == null)
            throw new InvalidOperationException("Detector not initialized");

        return await Task.Run(async () =>
        {
            var results = await _predictor.DetectAsync(imagePath);
            return results.Select(r => new DetectionResult(
                r.Name.Name,
                r.Confidence,
                r.Bounds.X,
                r.Bounds.Y,
                r.Bounds.Width,
                r.Bounds.Height
            )).ToArray();
        });
    }

    public override void Dispose()
    {
        _predictor?.Dispose();
    }
}
