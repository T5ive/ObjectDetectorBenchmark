namespace ObjectDetectorBenchmark.Results;

public static class ResultVisualizer
{
    private static readonly Dictionary<string, Color> ClassColors = new()
    {
        { "person", Color.Red },
        { "bicycle", Color.Blue },
        { "car", Color.Green },
        { "motorcycle", Color.Yellow },
        { "airplane", Color.Purple },
        { "bus", Color.Orange },
        { "train", Color.Pink },
        { "truck", Color.Cyan },
        { "boat", Color.Magenta },
        { "traffic light", Color.LightBlue },
        { "fire hydrant", Color.LightGreen },
        { "stop sign", Color.LightCoral },
        { "parking meter", Color.LightGray },
        { "bench", Color.DarkBlue },
        { "bird", Color.DarkGreen },
        { "cat", Color.DarkRed },
        { "dog", Color.DarkOrange },
        { "horse", Color.DarkViolet },
        { "sheep", Color.DarkCyan },
        { "cow", Color.DarkMagenta }
    };

    public static async Task SaveDetectionResultsAsync(
        string originalImagePath,
        DetectionResult[] detections,
        string libraryName,
        string outputDirectory = "results")
    {
        try
        {
            // Create output directory structure
            var libraryDir = Path.Combine(outputDirectory, libraryName);
            Directory.CreateDirectory(libraryDir);

            // Load the original image
            using var image = await Image.LoadAsync<Rgba32>(originalImagePath);

            // Create a copy for drawing
            var resultImage = image.Clone();

            // Draw bounding boxes and labels
            await DrawDetectionsAsync(resultImage, detections);

            // Save the result image
            var originalFileName = Path.GetFileNameWithoutExtension(originalImagePath);
            var outputPath = Path.Combine(libraryDir, $"{originalFileName}_detections.jpg");
            await resultImage.SaveAsJpegAsync(outputPath);

            // Save detection details as text
            await SaveDetectionDetailsAsync(detections, libraryDir, originalFileName);

            Console.WriteLine($"✅ Results saved for {libraryName}: {outputPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error saving results for {libraryName}: {ex.Message}");
        }
    }

    private static Task DrawDetectionsAsync(Image<Rgba32> image, DetectionResult[] detections)
    {
        if (detections.Length == 0) return Task.CompletedTask;

        // Try to load a font, fallback to default if not available
        Font font;
        try
        {
            var fontCollection = new FontCollection();
            var fontFamily = fontCollection.Add("fonts/arial.ttf"); // You might need to add a font file
            font = fontFamily.CreateFont(16, FontStyle.Bold);
        }
        catch
        {
            // Fallback to system default
            font = SystemFonts.CreateFont("Arial", 16, FontStyle.Bold);
        }

        image.Mutate(ctx =>
        {
            foreach (var detection in detections)
            {
                // Get color for this class, or use red as default
                var color = ClassColors.GetValueOrDefault(detection.Label.ToLower(), Color.Red);

                // Draw bounding box
                var rect = new RectangleF(detection.X, detection.Y, detection.Width, detection.Height);
                ctx.Draw(color, 3, rect);

                // Draw label background
                var labelText = $"{detection.Label} ({detection.Confidence:P1})";
                var labelSize = TextMeasurer.MeasureSize(labelText, new TextOptions(font));
                var labelBackground = new RectangleF(
                    detection.X,
                    detection.Y - labelSize.Height - 4,
                    labelSize.Width + 8,
                    labelSize.Height + 4);

                ctx.Fill(color, labelBackground);

                // Draw label text
                ctx.DrawText(labelText, font, Color.White, new PointF(detection.X + 4, detection.Y - labelSize.Height));
            }
        });

        return Task.CompletedTask;
    }

    private static async Task SaveDetectionDetailsAsync(DetectionResult[] detections, string outputDir, string fileName)
    {
        var detailsPath = Path.Combine(outputDir, $"{fileName}_details.txt");
        var lines = new List<string>
        {
            $"Detection Results - {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
            $"Total Detections: {detections.Length}",
            "",
            "Detected Objects:",
            "================"
        };

        for (var i = 0; i < detections.Length; i++)
        {
            var detection = detections[i];
            lines.Add($"{i + 1:D2}. {detection.Label}");
            lines.Add($"    Confidence: {detection.Confidence:P1}");
            lines.Add($"    Bounding Box: X={detection.X:F1}, Y={detection.Y:F1}, W={detection.Width:F1}, H={detection.Height:F1}");
            lines.Add("");
        }

        // Group by class and count
        var classCounts = detections.GroupBy(d => d.Label)
                                  .OrderByDescending(g => g.Count())
                                  .ToDictionary(g => g.Key, g => g.Count());

        lines.Add("Object Summary:");
        lines.Add("===============");
        lines.AddRange(classCounts.Select(kvp => $"{kvp.Key}: {kvp.Value} object(s)"));

        await File.WriteAllLinesAsync(detailsPath, lines);
    }

    public static async Task CreateComparisonImageAsync(
        string originalImagePath,
        Dictionary<string, DetectionResult[]> libraryResults,
        string outputDirectory = "results")
    {
        try
        {
            Directory.CreateDirectory(outputDirectory);

            // Load original image
            using var originalImage = await Image.LoadAsync<Rgba32>(originalImagePath);

            var imageWidth = originalImage.Width;
            var imageHeight = originalImage.Height;
            var libraries = libraryResults.Keys.ToList();
            var cols = Math.Min(2, libraries.Count);
            var rows = (int)Math.Ceiling((libraries.Count + 1) / (double)cols); // +1 for original

            // Create comparison image
            var comparisonWidth = imageWidth * cols;
            var comparisonHeight = imageHeight * rows;

            using var comparisonImage = new Image<Rgba32>(comparisonWidth, comparisonHeight);

            comparisonImage.Mutate(ctx =>
            {
                // Add original image
                ctx.DrawImage(originalImage, new Point(0, 0), 1f);

                // Add title for original
                try
                {
                    var font = SystemFonts.CreateFont("Arial", 20, FontStyle.Bold);
                    ctx.DrawText("Original", font, Color.White, new PointF(10, 10));
                }
                catch { }

                // Add library results
                for (var i = 0; i < libraries.Count; i++)
                {
                    var library = libraries[i];
                    var detections = libraryResults[library];

                    var col = (i + 1) % cols;
                    var row = (i + 1) / cols;
                    var x = col * imageWidth;
                    var y = row * imageHeight;

                    // Draw image with detections
                    var libraryImage = originalImage.Clone();
                    DrawDetectionsAsync(libraryImage, detections);

                    ctx.DrawImage(libraryImage, new Point(x, y), 1f);

                    // Add library name
                    try
                    {
                        var font = SystemFonts.CreateFont("Arial", 20, FontStyle.Bold);
                        var titleText = $"{library} ({detections.Length} objects)";
                        ctx.DrawText(titleText, font, Color.White, new PointF(x + 10, y + 10));
                    }
                    catch { }

                    libraryImage.Dispose();
                }
            });

            var originalFileName = Path.GetFileNameWithoutExtension(originalImagePath);
            var comparisonPath = Path.Combine(outputDirectory, $"{originalFileName}_comparison.jpg");
            await comparisonImage.SaveAsJpegAsync(comparisonPath);

            Console.WriteLine($"✅ Comparison image saved: {comparisonPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error creating comparison image: {ex.Message}");
        }
    }
}