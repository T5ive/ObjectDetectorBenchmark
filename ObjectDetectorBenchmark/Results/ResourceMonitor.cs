namespace ObjectDetectorBenchmark.Results;

public class ResourceMonitor : IDisposable
{
    private readonly Process _currentProcess;
    private readonly Timer _monitoringTimer;
    private readonly List<double> _memoryReadings = [];
    private readonly List<double> _cpuReadings = [];
    private bool _isMonitoring;
    private bool _disposed;
    private DateTime _lastCpuTime = DateTime.UtcNow;
    private TimeSpan _lastTotalProcessorTime = TimeSpan.Zero;

    public ResourceMonitor()
    {
        _currentProcess = Process.GetCurrentProcess();
        _monitoringTimer = new Timer(RecordMetrics, null, Timeout.Infinite, Timeout.Infinite);
    }

    public void StartMonitoring()
    {
        if (_disposed) return;

        _isMonitoring = true;
        _memoryReadings.Clear();
        _cpuReadings.Clear();

        // Initialize CPU timing
        _lastCpuTime = DateTime.UtcNow;
        _lastTotalProcessorTime = _currentProcess.TotalProcessorTime;

        // Start monitoring every 100ms
        _monitoringTimer.Change(100, 100);
    }

    public void StopMonitoring()
    {
        if (_disposed) return;

        _isMonitoring = false;
        _monitoringTimer.Change(Timeout.Infinite, Timeout.Infinite);
    }

    private void RecordMetrics(object? state)
    {
        if (!_isMonitoring || _disposed) return;

        try
        {
            // Refresh process information
            _currentProcess.Refresh();

            // Record memory usage in MB
            var memoryUsage = _currentProcess.WorkingSet64 / 1024.0 / 1024.0;
            _memoryReadings.Add(memoryUsage);

            // Calculate CPU usage manually
            var currentTime = DateTime.UtcNow;
            var currentTotalProcessorTime = _currentProcess.TotalProcessorTime;

            var timeDiff = (currentTime - _lastCpuTime).TotalMilliseconds;
            var processorTimeDiff = (currentTotalProcessorTime - _lastTotalProcessorTime).TotalMilliseconds;

            if (timeDiff > 0)
            {
                var cpuUsage = processorTimeDiff / timeDiff * 100.0 / Environment.ProcessorCount;
                _cpuReadings.Add(Math.Max(0, Math.Min(100, cpuUsage))); // Clamp between 0-100
            }

            _lastCpuTime = currentTime;
            _lastTotalProcessorTime = currentTotalProcessorTime;
        }
        catch (Exception)
        {
            // Ignore errors during monitoring
        }
    }

    public (double peak, double average) GetMemoryStats()
    {
        return _memoryReadings.Count == 0 ? (0, 0) : (_memoryReadings.Max(), _memoryReadings.Average());
    }

    public (double peak, double average) GetCpuStats()
    {
        return _cpuReadings.Count == 0 ? (0, 0) : (_cpuReadings.Max(), _cpuReadings.Average());
    }

    public void ResetStats()
    {
        _memoryReadings.Clear();
        _cpuReadings.Clear();
    }

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;
        _isMonitoring = false;

        _monitoringTimer?.Dispose();
        _currentProcess?.Dispose();
    }
}