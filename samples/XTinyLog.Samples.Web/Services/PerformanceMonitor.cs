using System.Collections.Concurrent;
using System.Diagnostics;

namespace XTinyLog.Samples.Web.Services;

/// <summary>
/// Performance monitoring service that integrates with XTinyLog
/// </summary>
public class PerformanceMonitor : IPerformanceMonitor
{
    private readonly PerformanceLogger _performanceLogger;
    private readonly ConcurrentDictionary<string, OperationMetrics> _metrics = new();

    public PerformanceMonitor(PerformanceLogger performanceLogger)
    {
        _performanceLogger = performanceLogger;
    }

    public async Task<IPerformanceOperation> StartOperationAsync(string operationName)
    {
        await _performanceLogger.Logger.DebugAsync($"Starting operation: {operationName}");
        return new PerformanceOperation(operationName, _performanceLogger, _metrics);
    }

    public object GetMetrics()
    {
        return _metrics.ToDictionary(
            kvp => kvp.Key,
            kvp => new
            {
                kvp.Value.TotalCalls,
                kvp.Value.TotalDurationMs,
                AverageDurationMs = kvp.Value.TotalCalls > 0 ? kvp.Value.TotalDurationMs / kvp.Value.TotalCalls : 0,
                kvp.Value.ErrorCount,
                kvp.Value.LastExecuted,
                CustomMetrics = kvp.Value.CustomMetrics.ToDictionary(m => m.Key, m => m.Value)
            });
    }

    private class PerformanceOperation : IPerformanceOperation
    {
        private readonly string _operationName;
        private readonly PerformanceLogger _logger;
        private readonly ConcurrentDictionary<string, OperationMetrics> _globalMetrics;
        private readonly Stopwatch _stopwatch;
        private readonly Dictionary<string, object> _customMetrics = new();
        private Exception? _error;
        private bool _disposed;

        public PerformanceOperation(
            string operationName, 
            PerformanceLogger logger, 
            ConcurrentDictionary<string, OperationMetrics> globalMetrics)
        {
            _operationName = operationName;
            _logger = logger;
            _globalMetrics = globalMetrics;
            _stopwatch = Stopwatch.StartNew();
        }

        public void AddMetric(string key, object value)
        {
            _customMetrics[key] = value;
        }

        public void SetError(Exception exception)
        {
            _error = exception;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _stopwatch.Stop();
            var durationMs = _stopwatch.ElapsedMilliseconds;

            // Update global metrics
            _globalMetrics.AddOrUpdate(_operationName,
                new OperationMetrics
                {
                    TotalCalls = 1,
                    TotalDurationMs = durationMs,
                    ErrorCount = _error != null ? 1 : 0,
                    LastExecuted = DateTime.UtcNow,
                    CustomMetrics = new ConcurrentDictionary<string, object>(_customMetrics)
                },
                (key, existing) =>
                {
                    existing.TotalCalls++;
                    existing.TotalDurationMs += durationMs;
                    if (_error != null) existing.ErrorCount++;
                    existing.LastExecuted = DateTime.UtcNow;
                    
                    // Merge custom metrics
                    foreach (var metric in _customMetrics)
                    {
                        existing.CustomMetrics[metric.Key] = metric.Value;
                    }
                    
                    return existing;
                });

            // Log completion
            Task.Run(async () =>
            {
                if (_error != null)
                {
                    await _logger.Logger.ErrorAsync($"Operation '{_operationName}' failed after {durationMs}ms: {_error.Message}");
                }
                else
                {
                    var logLevel = durationMs > 1000 ? "Warn" : "Info";
                    var metricsStr = _customMetrics.Any() 
                        ? $" | Metrics: {string.Join(", ", _customMetrics.Select(m => $"{m.Key}={m.Value}"))}"
                        : "";
                    
                    var message = $"Operation '{_operationName}' completed in {durationMs}ms{metricsStr}";
                    
                    if (logLevel == "Warn")
                    {
                        await _logger.Logger.WarnAsync(message);
                    }
                    else
                    {
                        await _logger.Logger.InfoAsync(message);
                    }
                }
            });
        }
    }

    private class OperationMetrics
    {
        public long TotalCalls { get; set; }
        public long TotalDurationMs { get; set; }
        public long ErrorCount { get; set; }
        public DateTime LastExecuted { get; set; }
        public ConcurrentDictionary<string, object> CustomMetrics { get; set; } = new();
    }
}