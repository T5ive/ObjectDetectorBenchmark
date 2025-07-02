namespace ObjectDetectorBenchmark;

public class BenchmarkRunner(string modelPath, string imagePath)
{
    private const int RunCount = 5;

    public async Task<List<BenchmarkResult>> RunBenchmarksAsync()
    {
        var results = new List<BenchmarkResult>();
        var libraryDetectionResults = new Dictionary<string, DetectionResult[]>();
        var detectors = new BaseDetector[]
        {
             new OnnxRuntimeDetector(),
            new YoloSharpDetector(),
            new YoloDotNetDetector(),
        };

        foreach (var detector in detectors)
        {
            Console.WriteLine($"Testing {detector.Name}...");
            var result = await BenchmarkDetectorAsync(detector);
            results.Add(result);

            // Store detection results for visualization
            if (result is { Success: true, DetectionResults: not null })
            {
                libraryDetectionResults[detector.Name] = result.DetectionResults;
            }

            detector.Dispose();

            // Add small delay between tests
            await Task.Delay(1000);
        }

        // Create visualization results
        Console.WriteLine("\nSaving detection results...");
        await SaveDetectionResultsAsync(libraryDetectionResults);

        return results;
    }

    private async Task SaveDetectionResultsAsync(Dictionary<string, DetectionResult[]> libraryDetectionResults)
    {
        try
        {
            // Save individual results for each library
            foreach (var kvp in libraryDetectionResults)
            {
                await ResultVisualizer.SaveDetectionResultsAsync(imagePath, kvp.Value, kvp.Key);
            }

            // Create comparison image
            await ResultVisualizer.CreateComparisonImageAsync(imagePath, libraryDetectionResults);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving detection results: {ex.Message}");
        }
    }

    private async Task<BenchmarkResult> BenchmarkDetectorAsync(BaseDetector detector)
    {
        var result = new BenchmarkResult
        {
            LibraryName = detector.Name
        };

        using var resourceMonitor = new ResourceMonitor();

        try
        {
            // Measure model loading time with resource monitoring
            resourceMonitor.StartMonitoring();
            var stopwatch = Stopwatch.StartNew();
            await detector.InitializeAsync(modelPath);
            stopwatch.Stop();
            resourceMonitor.StopMonitoring();

            result.ModelLoadTime = stopwatch.Elapsed.TotalMilliseconds;

            // Get resource usage during model loading
            var (peakMemoryLoad, avgMemoryLoad) = resourceMonitor.GetMemoryStats();
            var (peakCpuLoad, avgCpuLoad) = resourceMonitor.GetCpuStats();

            // Warm-up run
            await detector.DetectAsync(imagePath);

            // Reset resource monitor for prediction benchmarking
            resourceMonitor.ResetStats();
            resourceMonitor.StartMonitoring();

            // Measure single prediction time
            stopwatch.Restart();
            var singleResult = await detector.DetectAsync(imagePath);
            stopwatch.Stop();
            result.SinglePredictionTime = stopwatch.Elapsed.TotalMilliseconds;
            result.TotalDetections = singleResult.Length;

            // Store detection results for visualization
            result.DetectionResults = singleResult;

            // Measure average prediction time over 100 runs
            var times = new List<double>();
            for (var i = 0; i < RunCount; i++)
            {
                stopwatch.Restart();
                await detector.DetectAsync(imagePath);
                stopwatch.Stop();
                times.Add(stopwatch.Elapsed.TotalMilliseconds);
            }

            resourceMonitor.StopMonitoring();
            result.AveragePredictionTime = times.Average();

            // Get resource usage during predictions
            var (peakMemoryPred, avgMemoryPred) = resourceMonitor.GetMemoryStats();
            var (peakCpuPred, avgCpuPred) = resourceMonitor.GetCpuStats();

            // Use the higher values from either loading or prediction phase
            result.PeakMemoryUsageMB = Math.Max(peakMemoryLoad, peakMemoryPred);
            result.AverageMemoryUsageMB = Math.Max(avgMemoryLoad, avgMemoryPred);
            result.PeakCpuUsagePercent = Math.Max(peakCpuLoad, peakCpuPred);
            result.AverageCpuUsagePercent = Math.Max(avgCpuLoad, avgCpuPred);

            result.Success = true;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            Console.WriteLine($"Error with {detector.Name}: {ex.Message}");
        }

        return result;
    }
}