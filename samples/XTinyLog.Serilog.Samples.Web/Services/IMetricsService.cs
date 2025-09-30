namespace XTinyLog.Serilog.Samples.Web.Services;

public interface IMetricsService
{
    void IncrementCounter(string name, int value = 1);
    void RecordDuration(string name, TimeSpan duration);
    Dictionary<string, object> GetMetrics();
}