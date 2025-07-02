namespace ObjectDetectorBenchmark.Models;

public class BenchmarkResult
{
    public string LibraryName { get; set; } = string.Empty;
    public double ModelLoadTime { get; set; }
    public double SinglePredictionTime { get; set; }
    public double AveragePredictionTime { get; set; }
    public int TotalDetections { get; set; }
    public bool Success { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;

    // Resource usage metrics
    public double PeakMemoryUsageMB { get; set; }

    public double AverageMemoryUsageMB { get; set; }
    public double PeakCpuUsagePercent { get; set; }
    public double AverageCpuUsagePercent { get; set; }

    // Detection results for visualization
    public DetectionResult[]? DetectionResults { get; set; }
}