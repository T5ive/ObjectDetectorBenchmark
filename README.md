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

## Notes

- No external benchmarking libraries (like BenchmarkDotNet) are used
- Pure console output with formatted tables
- Each library is tested independently with proper cleanup
- Includes warm-up runs to ensure fair comparison
- Error handling for missing files or failed initializations
