namespace ObjectDetectorBenchmark.Models;

public abstract class BaseDetector
{
    public abstract string Name { get; }

    public abstract Task InitializeAsync(string modelPath);

    public abstract Task<DetectionResult[]> DetectAsync(string imagePath);

    public abstract void Dispose();
}