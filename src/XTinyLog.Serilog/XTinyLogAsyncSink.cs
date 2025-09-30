namespace XTinyLog.Serilog;

/// <summary>
/// An asynchronous Serilog sink that writes log events to XTinyLog's HTML format.
/// This sink implements ILogEventSink but performs asynchronous operations internally.
/// </summary>
public sealed class XTinyLogAsyncSink : ILogEventSink
{
    private readonly XTinyLogSinkConfiguration _configuration;
    private readonly XLog _xLog;

    /// <summary>
    /// Initializes a new instance of the <see cref="XTinyLogAsyncSink"/> class.
    /// </summary>
    /// <param name="configuration">The sink configuration.</param>
    /// <exception cref="ArgumentNullException">Thrown when configuration is null.</exception>
    /// <exception cref="ArgumentException">Thrown when configuration is invalid.</exception>
    public XTinyLogAsyncSink(XTinyLogSinkConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        try
        {
            // Validate configuration
            var validationErrors = _configuration.Validate();
            if (validationErrors.Count > 0)
            {
                var errorMessage = $"Invalid XTinyLogAsyncSink configuration: {string.Join(", ", validationErrors)}";
                throw new ArgumentException(errorMessage, nameof(configuration));
            }

            // Create XTinyLog instance using the configuration
            var logConfiguration = _configuration.ToLogConfiguration();
            _xLog = XLog.Create(builder => 
            {
                builder.WithLogDirectory(logConfiguration.LogDirectory)
                       .WithApplicationName(logConfiguration.ApplicationName)
                       .WithMaxFileSize(logConfiguration.MaxFileSize);
                
                if (logConfiguration.FileName != null)
                {
                    builder.WithFileName(logConfiguration.FileName);
                }
            });

            // Log successful initialization for diagnostics
            try
            {
                _xLog.Info($"XTinyLogAsyncSink initialized successfully. LogDirectory: {logConfiguration.LogDirectory}, ApplicationName: {logConfiguration.ApplicationName}");
            }
            catch
            {
                // Ignore logging errors during initialization
            }
        }
        catch (ArgumentException)
        {
            // Re-throw validation errors
            throw;
        }
        catch (Exception ex)
        {
            // Log configuration error and rethrow - this is during construction so it's acceptable to throw
            var errorMessage = $"Failed to initialize XTinyLogAsyncSink with the provided configuration. LogDirectory: {configuration?.LogDirectory ?? "null"}, ApplicationName: {configuration?.ApplicationName ?? "null"}. Error: {ex.Message}";
            throw new InvalidOperationException(errorMessage, ex);
        }
    }

    /// <summary>
    /// Emit the provided log event to XTinyLog asynchronously.
    /// Note: This method implements the synchronous ILogEventSink interface but performs async operations internally.
    /// </summary>
    /// <param name="logEvent">The log event to emit.</param>
    public void Emit(LogEvent logEvent)
    {
        if (logEvent == null)
        {
            // Log diagnostic information about null events
            _ = Task.Run(async () =>
            {
                try
                {
                    if (_xLog != null)
                    {
                        await _xLog.WarnAsync("XTinyLogAsyncSink received null log event", CancellationToken.None);
                    }
                }
                catch
                {
                    // Ignore diagnostic logging errors
                }
            });
            return;
        }

        if (_xLog == null)
        {
            // This should not happen in normal operation, but handle gracefully
            return;
        }

        // Fire and forget async operation - this is a common pattern for async sinks
        // that implement the synchronous ILogEventSink interface
        _ = Task.Run(async () =>
        {
            try
            {
                // Format the log event into a message suitable for XTinyLog
                string message;
                try
                {
                    message = LogEventFormatter.FormatLogEvent(
                        logEvent,
                        _configuration.IncludeProperties,
                        _configuration.PropertyFormat);
                }
                catch (Exception formatEx)
                {
                    // Fallback message formatting
                    message = $"[Message formatting failed: {formatEx.Message}] {logEvent.MessageTemplate} at {logEvent.Timestamp:yyyy-MM-dd HH:mm:ss}";
                    
                    // Log formatting error for diagnostics
                    try
                    {
                        await _xLog.WarnAsync($"XTinyLogAsyncSink: Log event formatting failed. Error: {formatEx.Message}", CancellationToken.None);
                    }
                    catch
                    {
                        // Ignore diagnostic logging errors
                    }
                }

                // Use the level mapper to log with the appropriate XTinyLog async method
                await LogLevelMapper.LogMessageAsync(_xLog, logEvent.Level, message, CancellationToken.None);
            }
            catch (OperationCanceledException)
            {
                // Cancellation is expected behavior in async operations
                // Don't log this as an error
            }
            catch (Exception ex)
            {
                // Never throw exceptions from the sink - use XTinyLog's internal logging for diagnostics
                try
                {
                    var diagnosticMessage = $"XTinyLogAsyncSink error: Failed to emit log event. Level: {logEvent.Level}, Timestamp: {logEvent.Timestamp:yyyy-MM-dd HH:mm:ss}, Error: {ex.Message}";
                    await _xLog.ErrorAsync(diagnosticMessage, CancellationToken.None);
                }
                catch
                {
                    // If even the error logging fails, silently ignore to prevent application crashes
                    // This follows Serilog sink best practices
                }
            }
        });
    }
}