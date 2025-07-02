Console.WriteLine("Object Detection Benchmark");
Console.WriteLine("Comparing YoloDotNet, YoloSharp, and OnnxRuntime");
Console.WriteLine(new string('=', 60));

// Check for model and image files
var modelPath = Path.Combine("assets", "yolo11n.onnx");
var imagePath = Path.Combine("assets", "test_image.jpg");

if (!File.Exists(modelPath))
{
    Console.WriteLine($"❌ Model file not found: {modelPath}");
    Console.WriteLine("\nPlease download a YOLO model and place it in the assets folder:");
    Console.WriteLine("1. Download yolo11n.onnx from: https://github.com/ultralytics/assets/releases");
    Console.WriteLine("2. Place the file in: assets/yolo11n.onnx");
    Console.WriteLine("\nPress any key to exit...");
    Console.ReadKey();
    return;
}

if (!File.Exists(imagePath))
{
    Console.WriteLine($"❌ Test image not found: {imagePath}");
    Console.WriteLine("\nPlease place a test image from: https://www.ultralytics.com/images/bus.jpg) in the assets folder:");
    Console.WriteLine("- Supported formats: JPG, PNG, BMP");
    Console.WriteLine("- File name: test_image.jpg");
    Console.WriteLine("- Location: assets/test_image.jpg");
    Console.WriteLine("\nPress any key to exit...");
    Console.ReadKey();
    return;
}

Console.WriteLine($"✅ Model file: {modelPath}");
Console.WriteLine($"✅ Test image: {imagePath}");
Console.WriteLine($"Image size: {GetImageDimensions(imagePath)}");
Console.WriteLine();

try
{
    var runner = new BenchmarkRunner(modelPath, imagePath);

    Console.WriteLine("Starting benchmark tests...");
    Console.WriteLine("This may take a few minutes to complete.\n");

    var results = await runner.RunBenchmarksAsync();

    ResultsDisplay.DisplayResults(results);
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Benchmark failed: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
}

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();
return;

static string GetImageDimensions(string imagePath)
{
    try
    {
        using var image = SixLabors.ImageSharp.Image.Load(imagePath);
        return $"{image.Width}x{image.Height}";
    }
    catch
    {
        return "Unknown";
    }
}