# Object Detector Benchmark

This project benchmarks three popular .NET object detection libraries:
- **YoloDotNet** - YOLO implementation for .NET
- **YoloSharp** - Another YOLO wrapper for .NET  
- **OnnxRuntime** - Microsoft's ONNX Runtime for .NET

## Setup Instructions

### 1. Download YOLO Model
Download a YOLO ONNX model file and place it in the `assets` folder:
```
assets/yolo11n.onnx
```

You can download from:
- [Ultralytics YOLOv11](https://github.com/ultralytics/assets/releases)
- [YOLO Official Models](https://docs.ultralytics.com/models/yolo11/)

### 2. Add Test Image
Place a test image in the `assets` folder:
```
assets/test_image.jpg
```

Recommended: 640x640 pixels for optimal performance

## Running the Benchmark

```bash
dotnet run
```

## What It Measures

- **Model Load Time**: Time to initialize each library with the ONNX model
- **Single Prediction**: Time for one object detection inference  
- **Average 100 Runs**: Average inference time over 100 iterations
- **Detection Count**: Number of objects detected in the test image

## Expected Output

```
==============================================================================================
OBJECT DETECTION BENCHMARK RESULTS  
==============================================================================================
Library         Load Time    Single Pred  Avg 100 Runs Detections Status    
Name            (ms)         (ms)         (ms)         Count      
----------------------------------------------------------------------------------------------
YoloDotNet      1250.45      125.30       98.75        15         ✓ Success  
YoloSharp       980.22       110.45       87.32        14         ✓ Success  
OnnxRuntime     2100.88      145.67       112.43       16         ✓ Success  
----------------------------------------------------------------------------------------------

PERFORMANCE ANALYSIS:
--------------------------------------------------
Fastest Model Loading: YoloSharp (980.22 ms)
Fastest Single Prediction: YoloSharp (110.45 ms)  
Fastest Average Prediction: YoloSharp (87.32 ms)
Most Detections Found: OnnxRuntime (16 objects)

RECOMMENDATIONS:
• For fastest inference: YoloSharp
• For fastest startup: YoloSharp  
• For most detections: OnnxRuntime
==============================================================================================
```

## Project Structure

```
ObjectDetectorBenchmark/
├── Program.cs                 # Main entry point
├── Models.cs                  # Data models and interfaces
├── BenchmarkRunner.cs         # Benchmark orchestration
├── ResultsDisplay.cs          # Console output formatting
├── Detectors/
│   ├── YoloDotNetDetector.cs  # YoloDotNet implementation
│   ├── YoloSharpDetector.cs   # YoloSharp implementation
│   └── OnnxRuntimeDetector.cs # OnnxRuntime implementation
└── assets/
    ├── yolo11n.onnx          # YOLO model file (download required)
    └── test_image.jpg        # Test image (provide your own)
```

## Dependencies

- .NET 8.0
- YoloDotNet 2.0.3
- YoloSharp 1.2.0  
- Microsoft.ML.OnnxRuntime 1.16.3
- SixLabors.ImageSharp 3.0.2

## Notes

- No external benchmarking libraries (like BenchmarkDotNet) are used
- Pure console output with formatted tables
- Each library is tested independently with proper cleanup
- Includes warm-up runs to ensure fair comparison
- Error handling for missing files or failed initializations
