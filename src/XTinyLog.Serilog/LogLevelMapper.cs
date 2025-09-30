namespace XTinyLog.Serilog;

/// <summary>
/// Maps Serilog log levels to XTinyLog log method names and provides logging functionality.
/// </summary>
/// <remarks>
/// This internal class handles the mapping between Serilog's LogEventLevel enumeration
/// and XTinyLog's logging methods. It provides both synchronous and asynchronous logging
/// capabilities with proper error handling and fallback mechanisms.
/// 
/// The mapping follows these conventions:
/// <list type="table">
/// <listheader>
/// <term>Serilog Level</term>
/// <description>XTinyLog Method</description>
/// </listheader>
/// <item>
/// <term>Verbose, Debug</term>
/// <description>Debug</description>
/// </item>
/// <item>
/// <term>Information</term>
/// <description>Info</description>
/// </item>
/// <item>
/// <term>Warning</term>
/// <description>Warn</description>
/// </item>
/// <item>
/// <term>Error, Fatal</term>
/// <description>Error</description>
/// </item>
/// </list>
/// </remarks>
internal static class LogLevelMapper
{
    /// <summary>
    /// Maps a Serilog LogEventLevel to the corresponding XTinyLog method name.
    /// </summary>
    /// <param name="logEventLevel">The Serilog log event level to map.</param>
    /// <returns>The corresponding XTinyLog method name as a string.</returns>
    /// <remarks>
    /// This method provides a simple string mapping for diagnostic purposes.
    /// For actual logging operations, use <see cref="LogMessage"/> or <see cref="LogMessageAsync"/>
    /// which handle the method invocation directly.
    /// </remarks>
    /// <example>
    /// <code>
    /// string methodName = LogLevelMapper.MapLogLevel(LogEventLevel.Information);
    /// // Returns: "Info"
    /// 
    /// string debugMethod = LogLevelMapper.MapLogLevel(LogEventLevel.Debug);
    /// // Returns: "Debug"
    /// </code>
    /// </example>
    public static string MapLogLevel(LogEventLevel logEventLevel)
    {
        return logEventLevel switch
        {
            LogEventLevel.Verbose => "Debug",
            LogEventLevel.Debug => "Debug",
            LogEventLevel.Information => "Info",
            LogEventLevel.Warning => "Warn",
            LogEventLevel.Error => "Error",
            LogEventLevel.Fatal => "Error",
            _ => "Info", // Default fallback
        };
    }

    /// <summary>
    /// Logs a message using the appropriate XTinyLog method based on the log level.
    /// </summary>
    /// <param name="xLog">The XTinyLog instance to log to. Must not be null.</param>
    /// <param name="logEventLevel">The Serilog log event level that determines which XTinyLog method to call.</param>
    /// <param name="message">The message to log. Empty or null messages are handled gracefully.</param>
    /// <remarks>
    /// This method provides synchronous logging with comprehensive error handling.
    /// If the primary logging operation fails, it attempts to log the error using
    /// XTinyLog's error method. If that also fails, the error is silently ignored
    /// to prevent application crashes, following Serilog sink best practices.
    /// 
    /// The method handles null or empty messages by substituting a placeholder message.
    /// </remarks>
    /// <example>
    /// <code>
    /// var xLog = XLog.Create();
    /// LogLevelMapper.LogMessage(xLog, LogEventLevel.Information, "User logged in");
    /// LogLevelMapper.LogMessage(xLog, LogEventLevel.Error, "Database connection failed");
    /// </code>
    /// </example>
    public static void LogMessage(XLog xLog, LogEventLevel logEventLevel, string message)
    {
        if (xLog == null)
        {
            // Cannot log if XLog instance is null - this should not happen in normal operation
            return;
        }

        if (string.IsNullOrEmpty(message))
        {
            message = "[Empty message]";
        }

        try
        {
            switch (logEventLevel)
            {
                case LogEventLevel.Verbose:
                case LogEventLevel.Debug:
                    xLog.Debug(message);
                    break;
                case LogEventLevel.Information:
                    xLog.Info(message);
                    break;
                case LogEventLevel.Warning:
                    xLog.Warn(message);
                    break;
                case LogEventLevel.Error:
                case LogEventLevel.Fatal:
                    xLog.Error(message);
                    break;
                default:
                    // Fallback for unknown log levels
                    xLog.Info($"[Unknown level {logEventLevel}] {message}");
                    break;
            }
        }
        catch (Exception ex)
        {
            // If logging fails, try to log the error using XTinyLog's error method
            // This provides a fallback mechanism
            try
            {
                xLog.Error($"LogLevelMapper error: Failed to log message at level {logEventLevel}. Original message: {message}. Error: {ex.Message}");
            }
            catch
            {
                // If even error logging fails, silently ignore to prevent application crashes
                // This follows Serilog sink best practices
            }
        }
    }

    /// <summary>
    /// Asynchronously logs a message using the appropriate XTinyLog method based on the log level.
    /// </summary>
    /// <param name="xLog">The XTinyLog instance to log to. Must not be null.</param>
    /// <param name="logEventLevel">The Serilog log event level that determines which XTinyLog async method to call.</param>
    /// <param name="message">The message to log. Empty or null messages are handled gracefully.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation. Default is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task representing the asynchronous logging operation.</returns>
    /// <remarks>
    /// This method provides asynchronous logging with comprehensive error handling and
    /// proper cancellation token support. If the primary logging operation fails, it
    /// attempts to log the error using XTinyLog's async error method. If that also fails,
    /// the error is silently ignored to prevent application crashes.
    /// 
    /// <see cref="OperationCanceledException"/> is re-thrown as it represents expected
    /// cancellation behavior rather than an error condition.
    /// </remarks>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled via the cancellation token.</exception>
    /// <example>
    /// <code>
    /// var xLog = XLog.Create();
    /// var cts = new CancellationTokenSource();
    /// 
    /// await LogLevelMapper.LogMessageAsync(xLog, LogEventLevel.Information, "User logged in", cts.Token);
    /// await LogLevelMapper.LogMessageAsync(xLog, LogEventLevel.Error, "Database connection failed");
    /// </code>
    /// </example>
    public static async Task LogMessageAsync(XLog xLog, LogEventLevel logEventLevel, string message, CancellationToken cancellationToken = default)
    {
        if (xLog == null)
        {
            // Cannot log if XLog instance is null - this should not happen in normal operation
            return;
        }

        if (string.IsNullOrEmpty(message))
        {
            message = "[Empty message]";
        }

        try
        {
            switch (logEventLevel)
            {
                case LogEventLevel.Verbose:
                case LogEventLevel.Debug:
                    await xLog.DebugAsync(message, cancellationToken);
                    break;
                case LogEventLevel.Information:
                    await xLog.InfoAsync(message, cancellationToken);
                    break;
                case LogEventLevel.Warning:
                    await xLog.WarnAsync(message, cancellationToken);
                    break;
                case LogEventLevel.Error:
                case LogEventLevel.Fatal:
                    await xLog.ErrorAsync(message, cancellationToken);
                    break;
                default:
                    // Fallback for unknown log levels
                    await xLog.InfoAsync($"[Unknown level {logEventLevel}] {message}", cancellationToken);
                    break;
            }
        }
        catch (OperationCanceledException)
        {
            // Cancellation is expected behavior, don't treat as error
            throw;
        }
        catch (Exception ex)
        {
            // If logging fails, try to log the error using XTinyLog's error method
            // This provides a fallback mechanism
            try
            {
                await xLog.ErrorAsync($"LogLevelMapper async error: Failed to log message at level {logEventLevel}. Original message: {message}. Error: {ex.Message}", cancellationToken);
            }
            catch
            {
                // If even error logging fails, silently ignore to prevent application crashes
                // This follows Serilog sink best practices
            }
        }
    }
}