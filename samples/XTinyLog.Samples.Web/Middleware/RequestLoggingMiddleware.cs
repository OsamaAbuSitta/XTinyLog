using System.Diagnostics;

namespace XTinyLog.Samples.Web;

/// <summary>
/// Middleware for logging HTTP requests and responses with performance metrics
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ApiLogger _apiLogger;
    private readonly PerformanceLogger _performanceLogger;

    public RequestLoggingMiddleware(RequestDelegate next, ApiLogger apiLogger, PerformanceLogger performanceLogger)
    {
        _next = next;
        _apiLogger = apiLogger;
        _performanceLogger = performanceLogger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString("N")[..8];
        
        // Log incoming request
        await _apiLogger.Logger.InfoAsync($"[{requestId}] {context.Request.Method} {context.Request.Path} - Request started");

        try
        {
            await _next(context);
            
            stopwatch.Stop();
            var duration = stopwatch.ElapsedMilliseconds;
            
            // Log successful response
            await _apiLogger.Logger.InfoAsync($"[{requestId}] {context.Request.Method} {context.Request.Path} - Response: {context.Response.StatusCode} ({duration}ms)");
            
            // Log performance metrics
            if (duration > 1000) // Log slow requests (>1 second)
            {
                await _performanceLogger.Logger.WarnAsync($"[{requestId}] Slow request: {context.Request.Method} {context.Request.Path} took {duration}ms");
            }
            else if (duration > 100) // Log moderately slow requests (>100ms)
            {
                await _performanceLogger.Logger.InfoAsync($"[{requestId}] Request duration: {context.Request.Method} {context.Request.Path} took {duration}ms");
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            var duration = stopwatch.ElapsedMilliseconds;
            
            // Log error
            await _apiLogger.Logger.ErrorAsync($"[{requestId}] {context.Request.Method} {context.Request.Path} - Error after {duration}ms: {ex.Message}");
            
            throw; // Re-throw to maintain normal error handling
        }
    }
}