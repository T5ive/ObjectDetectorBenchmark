namespace ObjectDetectorBenchmark.Models;

public record DetectionResult(string Label, double Confidence, float X, float Y, float Width, float Height);