namespace XTinyLog.Serilog;

/// <summary>
/// A Serilog sink that writes log events to XTinyLog's HTML format.
/// </summary>
public sealed class XTinyLogSink : ILogEventSink
{
    private readonly XTinyLogSinkConfiguration _configuration;
    private readonly XLog _xLog;

    /// <summary>
    /// Initializes a new instance of the <see cref="XTinyLogSink"/> class.
    /// </summary>
    /// <param name="configuration">The sink configuration.</param>
    /// <exception cref="ArgumentNullException">Thrown when configuration is null.</exception>
    /// <exception cref="ArgumentException">Thrown when configuration is invalid.</exception>
    public XTinyLogSink(XTinyLogSinkConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        try
        {
            // Validate configuration
            var validationErrors = _configuration.Validate();
            if (validationErrors.Count > 0)
            {
                var errorMessage = $"Invalid XTinyLogSink configuration: {string.Join(", ", validationErrors)}";
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
                _xLog.Info($"XTinyLogSink initialized successfully. LogDirectory: {logConfiguration.LogDirectory}, ApplicationName: {logConfiguration.ApplicationName}");
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
            var errorMessage = $"Failed to initialize XTinyLogSink with the provided configuration. LogDirectory: {configuration?.LogDirectory ?? "null"}, ApplicationName: {configuration?.ApplicationName ?? "null"}. Error: {ex.Message}";
            throw new InvalidOperationException(errorMessage, ex);
        }
    }

    /// <summary>
    /// Emit the provided log event to XTinyLog.
    /// </summary>
    /// <param name="logEvent">The log event to emit.</param>
    public void Emit(LogEvent logEvent)
    {
        if (logEvent == null)
        {
            // Log diagnostic information about null events
            try
            {
                _xLog?.Warn("XTinyLogSink received null log event");
            }
            catch
            {
                // Ignore diagnostic logging errors
            }
            return;
        }

        if (_xLog == null)
        {
            // This should not happen in normal operation, but handle gracefully
            return;
        }

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
                    _xLog.Warn($"XTinyLogSink: Log event formatting failed. Error: {formatEx.Message}");
                }
                catch
                {
                    // Ignore diagnostic logging errors
                }
            }

            // Use the level mapper to log with the appropriate XTinyLog method
            LogLevelMapper.LogMessage(_xLog, logEvent.Level, message);
        }
        catch (Exception ex)
        {
            // Never throw exceptions from the sink - use XTinyLog's internal logging for diagnostics
            try
            {
                var diagnosticMessage = $"XTinyLogSink error: Failed to emit log event. Level: {logEvent.Level}, Timestamp: {logEvent.Timestamp:yyyy-MM-dd HH:mm:ss}, Error: {ex.Message}";
                _xLog.Error(diagnosticMessage);
            }
            catch
            {
                // If even the error logging fails, silently ignore to prevent application crashes
                // This follows Serilog sink best practices
            }
        }
    }
}