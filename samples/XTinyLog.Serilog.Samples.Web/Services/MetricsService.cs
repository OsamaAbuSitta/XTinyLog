using System.Collections.Concurrent;

namespace XTinyLog.Serilog.Samples.Web.Services;

public class MetricsService : IMetricsService
{
    private readonly ILogger<MetricsService> _logger;
    private readonly ConcurrentDictionary<string, long> _counters = new();
    private readonly ConcurrentDictionary<string, List<double>> _durations = new();

    public MetricsService(ILogger<MetricsService> logger)
    {
        _logger = logger;
    }

    public void IncrementCounter(string name, int value = 1)
    {
        _counters.AddOrUpdate(name, value, (key, current) => current + value);
        _logger.LogTrace("Incremented counter {CounterName} by {Value} (new total: {Total})", 
            name, value, _counters[name]);
    }

    public void RecordDuration(string name, TimeSpan duration)
    {
        var durationMs = duration.TotalMilliseconds;
        _durations.AddOrUpdate(name, 
            new List<double> { durationMs }, 
            (key, current) => 
            {
                lock (current)
                {
                    current.Add(durationMs);
                    // Keep only the last 1000 measurements to prevent memory issues
                    if (current.Count > 1000)
                    {
                        current.RemoveAt(0);
                    }
                    return current;
                }
            });
        
        _logger.LogTrace("Recorded duration for {MetricName}: {Duration}ms", name, durationMs);
    }

    public Dictionary<string, object> GetMetrics()
    {
        _logger.LogDebug("Generating metrics summary");
        
        var metrics = new Dictionary<string, object>();
        
        // Add counters
        foreach (var counter in _counters)
        {
            metrics[counter.Key] = counter.Value;
        }
        
        // Add duration statistics
        foreach (var duration in _durations)
        {
            List<double> values;
            lock (duration.Value)
            {
                values = new List<double>(duration.Value);
            }
            
            if (values.Any())
            {
                var stats = new
                {
                    Count = values.Count,
                    Average = values.Average(),
                    Min = values.Min(),
                    Max = values.Max(),
                    P50 = GetPercentile(values, 0.5),
                    P95 = GetPercentile(values, 0.95),
                    P99 = GetPercentile(values, 0.99)
                };
                
                metrics[$"{duration.Key}_stats"] = stats;
            }
        }
        
        metrics["generated_at"] = DateTime.UtcNow;
        
        _logger.LogDebug("Generated metrics summary with {MetricCount} entries", metrics.Count);
        
        return metrics;
    }

    private static double GetPercentile(List<double> values, double percentile)
    {
        if (!values.Any()) return 0;
        
        var sorted = values.OrderBy(x => x).ToList();
        var index = (int)Math.Ceiling(percentile * sorted.Count) - 1;
        index = Math.Max(0, Math.Min(index, sorted.Count - 1));
        
        return sorted[index];
    }
}