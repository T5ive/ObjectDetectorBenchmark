namespace ObjectDetectorBenchmark.Results;

public static class ResultsDisplay
{
    public static void DisplayResults(List<BenchmarkResult> results)
    {
        Console.WriteLine("\n" + new string('=', 120));
        Console.WriteLine("OBJECT DETECTION BENCHMARK RESULTS");
        Console.WriteLine(new string('=', 120));

        // Performance Table
        Console.WriteLine("\nPERFORMANCE METRICS:");
        Console.WriteLine($"{"Library",-15} {"Load Time",-10} {"Single Pred",-11} {"Avg 100 Runs",-11} {"Detections",-10} {"Status",-10}");
        Console.WriteLine($"{"Name",-15} {"(ms)",-10} {"(ms)",-11} {"(ms)",-11} {"Count",-10} {"",-10}");
        Console.WriteLine(new string('-', 85));

        foreach (var result in results)
        {
            var loadTime = result.Success ? $"{result.ModelLoadTime:F1}" : "N/A";
            var singleTime = result.Success ? $"{result.SinglePredictionTime:F1}" : "N/A";
            var avgTime = result.Success ? $"{result.AveragePredictionTime:F1}" : "N/A";
            var detections = result.Success ? result.TotalDetections.ToString() : "N/A";
            var status = result.Success ? "✓ Success" : "✗ Failed";

            Console.WriteLine($"{result.LibraryName,-15} {loadTime,-10} {singleTime,-11} {avgTime,-11} {detections,-10} {status,-10}");
        }

        // Resource Usage Table
        Console.WriteLine("\nRESOURCE USAGE:");
        Console.WriteLine($"{"Library",-15} {"Peak RAM",-10} {"Avg RAM",-10} {"Peak CPU",-10} {"Avg CPU",-10}");
        Console.WriteLine($"{"Name",-15} {"(MB)",-10} {"(MB)",-10} {"(%)",-10} {"(%)",-10}");
        Console.WriteLine(new string('-', 70));

        foreach (var result in results)
        {
            if (result.Success)
            {
                Console.WriteLine($"{result.LibraryName,-15} {result.PeakMemoryUsageMB,-10:F1} {result.AverageMemoryUsageMB,-10:F1} {result.PeakCpuUsagePercent,-10:F1} {result.AverageCpuUsagePercent,-10:F1}");
            }
            else
            {
                Console.WriteLine($"{result.LibraryName,-15} {"N/A",-10} {"N/A",-10} {"N/A",-10} {"N/A",-10}");
            }
        }

        Console.WriteLine(new string('-', 120));

        // Analysis
        var successfulResults = results.Where(r => r.Success).ToList();
        if (successfulResults.Any())
        {
            Console.WriteLine("\nPERFORMANCE ANALYSIS:");
            Console.WriteLine(new string('-', 60));

            // Performance winners
            var fastestLoad = successfulResults.OrderBy(r => r.ModelLoadTime).First();
            var fastestSingle = successfulResults.OrderBy(r => r.SinglePredictionTime).First();
            var fastestAvg = successfulResults.OrderBy(r => r.AveragePredictionTime).First();
            var mostDetections = successfulResults.OrderByDescending(r => r.TotalDetections).First();

            Console.WriteLine($"🚀 Fastest Model Loading: {fastestLoad.LibraryName} ({fastestLoad.ModelLoadTime:F1} ms)");
            Console.WriteLine($"⚡ Fastest Single Prediction: {fastestSingle.LibraryName} ({fastestSingle.SinglePredictionTime:F1} ms)");
            Console.WriteLine($"🏎️  Fastest Average Prediction: {fastestAvg.LibraryName} ({fastestAvg.AveragePredictionTime:F1} ms)");
            Console.WriteLine($"🎯 Most Detections Found: {mostDetections.LibraryName} ({mostDetections.TotalDetections} objects)");

            Console.WriteLine("\nRESOURCE USAGE ANALYSIS:");
            Console.WriteLine(new string('-', 60));

            // Resource usage analysis
            var lowestPeakRam = successfulResults.OrderBy(r => r.PeakMemoryUsageMB).First();
            var lowestAvgRam = successfulResults.OrderBy(r => r.AverageMemoryUsageMB).First();
            var lowestPeakCpu = successfulResults.OrderBy(r => r.PeakCpuUsagePercent).First();
            var lowestAvgCpu = successfulResults.OrderBy(r => r.AverageCpuUsagePercent).First();

            var highestPeakRam = successfulResults.OrderByDescending(r => r.PeakMemoryUsageMB).First();
            var highestAvgRam = successfulResults.OrderByDescending(r => r.AverageMemoryUsageMB).First();
            var highestPeakCpu = successfulResults.OrderByDescending(r => r.PeakCpuUsagePercent).First();
            var highestAvgCpu = successfulResults.OrderByDescending(r => r.AverageCpuUsagePercent).First();

            Console.WriteLine($"💚 Lowest Peak RAM: {lowestPeakRam.LibraryName} ({lowestPeakRam.PeakMemoryUsageMB:F1} MB)");
            Console.WriteLine($"💛 Highest Peak RAM: {highestPeakRam.LibraryName} ({highestPeakRam.PeakMemoryUsageMB:F1} MB)");
            Console.WriteLine($"🔋 Lowest Peak CPU: {lowestPeakCpu.LibraryName} ({lowestPeakCpu.PeakCpuUsagePercent:F1}%)");
            Console.WriteLine($"🔥 Highest Peak CPU: {highestPeakCpu.LibraryName} ({highestPeakCpu.PeakCpuUsagePercent:F1}%)");

            Console.WriteLine("\nRECOMMENDATIONS:");
            Console.WriteLine($"🏆 Best Overall Performance: {fastestAvg.LibraryName}");
            Console.WriteLine($"🔧 Best for Resource-Constrained Environments: {lowestPeakRam.LibraryName}");
            Console.WriteLine($"⚙️  Best for Quick Startup: {fastestLoad.LibraryName}");
            Console.WriteLine($"🎪 Best for Detection Accuracy: {mostDetections.LibraryName}");
        }

        // Show errors if any
        var failedResults = results.Where(r => !r.Success).ToList();
        if (failedResults.Count != 0)
        {
            Console.WriteLine("\nERRORS:");
            Console.WriteLine(new string('-', 60));
            foreach (var failed in failedResults)
            {
                Console.WriteLine($"❌ {failed.LibraryName}: {failed.ErrorMessage}");
            }
        }

        Console.WriteLine(new string('=', 120));
    }
}