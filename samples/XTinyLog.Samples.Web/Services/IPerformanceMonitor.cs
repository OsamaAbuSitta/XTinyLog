namespace XTinyLog.Samples.Web.Services;

public interface IPerformanceMonitor
{
    Task<IPerformanceOperation> StartOperationAsync(string operationName);
    object GetMetrics();
}

public interface IPerformanceOperation : IDisposable
{
    void AddMetric(string key, object value);
    void SetError(Exception exception);
}