using Serilog.Configuration;

namespace XTinyLog.Serilog;

/// <summary>
/// Extension methods for configuring XTinyLog sinks in Serilog.
/// </summary>
public static class XTinyLogSinkExtensions
{
    /// <summary>
    /// Adds a synchronous XTinyLog sink to the logger configuration with default settings.
    /// </summary>
    /// <param name="loggerSinkConfiguration">The logger sink configuration.</param>
    /// <returns>The logger configuration for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when loggerSinkConfiguration is null.</exception>
    public static LoggerConfiguration XTinyLog(
        this LoggerSinkConfiguration loggerSinkConfiguration)
    {
        if (loggerSinkConfiguration == null)
        {
            throw new ArgumentNullException(nameof(loggerSinkConfiguration));
        }

        var configuration = new XTinyLogSinkConfiguration();
        return loggerSinkConfiguration.Sink(new XTinyLogSink(configuration));
    }

    /// <summary>
    /// Adds a synchronous XTinyLog sink to the logger configuration with custom configuration.
    /// </summary>
    /// <param name="loggerSinkConfiguration">The logger sink configuration.</param>
    /// <param name="configureAction">Action to configure the XTinyLog sink.</param>
    /// <returns>The logger configuration for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when loggerSinkConfiguration or configureAction is null.</exception>
    public static LoggerConfiguration XTinyLog(
        this LoggerSinkConfiguration loggerSinkConfiguration,
        Action<XTinyLogSinkConfiguration> configureAction)
    {
        if (loggerSinkConfiguration == null)
        {
            throw new ArgumentNullException(nameof(loggerSinkConfiguration));
        }

        if (configureAction == null)
        {
            throw new ArgumentNullException(nameof(configureAction));
        }

        var configuration = new XTinyLogSinkConfiguration();
        configureAction(configuration);

        return loggerSinkConfiguration.Sink(new XTinyLogSink(configuration));
    }

    /// <summary>
    /// Adds an asynchronous XTinyLog sink to the logger configuration with default settings.
    /// </summary>
    /// <param name="loggerSinkConfiguration">The logger sink configuration.</param>
    /// <returns>The logger configuration for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when loggerSinkConfiguration is null.</exception>
    public static LoggerConfiguration XTinyLogAsync(
        this LoggerSinkConfiguration loggerSinkConfiguration)
    {
        if (loggerSinkConfiguration == null)
        {
            throw new ArgumentNullException(nameof(loggerSinkConfiguration));
        }

        var configuration = new XTinyLogSinkConfiguration();
        return loggerSinkConfiguration.Sink(new XTinyLogAsyncSink(configuration));
    }

    /// <summary>
    /// Adds an asynchronous XTinyLog sink to the logger configuration with custom configuration.
    /// </summary>
    /// <param name="loggerSinkConfiguration">The logger sink configuration.</param>
    /// <param name="configureAction">Action to configure the XTinyLog sink.</param>
    /// <returns>The logger configuration for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when loggerSinkConfiguration or configureAction is null.</exception>
    public static LoggerConfiguration XTinyLogAsync(
        this LoggerSinkConfiguration loggerSinkConfiguration,
        Action<XTinyLogSinkConfiguration> configureAction)
    {
        if (loggerSinkConfiguration == null)
        {
            throw new ArgumentNullException(nameof(loggerSinkConfiguration));
        }

        if (configureAction == null)
        {
            throw new ArgumentNullException(nameof(configureAction));
        }

        var configuration = new XTinyLogSinkConfiguration();
        configureAction(configuration);

        return loggerSinkConfiguration.Sink(new XTinyLogAsyncSink(configuration));
    }

    /// <summary>
    /// Adds a synchronous XTinyLog sink to the logger configuration with parameters suitable for appsettings.json configuration.
    /// </summary>
    /// <param name="loggerSinkConfiguration">The logger sink configuration.</param>
    /// <param name="logDirectory">The directory where log files will be stored. Defaults to "c:\Log".</param>
    /// <param name="applicationName">The application name used for log organization. Defaults to "Application".</param>
    /// <param name="maxFileSize">The maximum file size in KB before archiving. Defaults to 1024.</param>
    /// <param name="includeProperties">Whether to include structured properties in the output. Defaults to true.</param>
    /// <param name="propertyFormat">How structured properties should be formatted. Defaults to Inline.</param>
    /// <returns>The logger configuration for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when loggerSinkConfiguration is null.</exception>
    /// <exception cref="ArgumentException">Thrown when configuration parameters are invalid.</exception>
    public static LoggerConfiguration XTinyLog(
        this LoggerSinkConfiguration loggerSinkConfiguration,
        string logDirectory = @"c:\Log",
        string applicationName = "Application",
        long maxFileSize = 1024,
        bool includeProperties = true,
        PropertyFormatMode propertyFormat = PropertyFormatMode.Inline)
    {
        if (loggerSinkConfiguration == null)
        {
            throw new ArgumentNullException(nameof(loggerSinkConfiguration));
        }

        // Validate parameters before creating configuration
        ValidateConfigurationParameters(logDirectory, applicationName, maxFileSize);

        var configuration = new XTinyLogSinkConfiguration
        {
            LogDirectory = logDirectory ?? @"c:\Log",
            ApplicationName = applicationName ?? "Application",
            MaxFileSize = maxFileSize,
            IncludeProperties = includeProperties,
            PropertyFormat = propertyFormat
        };

        return loggerSinkConfiguration.Sink(new XTinyLogSink(configuration));
    }

    /// <summary>
    /// Adds an asynchronous XTinyLog sink to the logger configuration with parameters suitable for appsettings.json configuration.
    /// </summary>
    /// <param name="loggerSinkConfiguration">The logger sink configuration.</param>
    /// <param name="logDirectory">The directory where log files will be stored. Defaults to "c:\Log".</param>
    /// <param name="applicationName">The application name used for log organization. Defaults to "Application".</param>
    /// <param name="maxFileSize">The maximum file size in KB before archiving. Defaults to 1024.</param>
    /// <param name="includeProperties">Whether to include structured properties in the output. Defaults to true.</param>
    /// <param name="propertyFormat">How structured properties should be formatted. Defaults to Inline.</param>
    /// <returns>The logger configuration for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when loggerSinkConfiguration is null.</exception>
    /// <exception cref="ArgumentException">Thrown when configuration parameters are invalid.</exception>
    public static LoggerConfiguration XTinyLogAsync(
        this LoggerSinkConfiguration loggerSinkConfiguration,
        string logDirectory = @"c:\Log",
        string applicationName = "Application",
        long maxFileSize = 1024,
        bool includeProperties = true,
        PropertyFormatMode propertyFormat = PropertyFormatMode.Inline)
    {
        if (loggerSinkConfiguration == null)
        {
            throw new ArgumentNullException(nameof(loggerSinkConfiguration));
        }

        // Validate parameters before creating configuration
        ValidateConfigurationParameters(logDirectory, applicationName, maxFileSize);

        var configuration = new XTinyLogSinkConfiguration
        {
            LogDirectory = logDirectory ?? @"c:\Log",
            ApplicationName = applicationName ?? "Application",
            MaxFileSize = maxFileSize,
            IncludeProperties = includeProperties,
            PropertyFormat = propertyFormat
        };

        return loggerSinkConfiguration.Sink(new XTinyLogAsyncSink(configuration));
    }

    /// <summary>
    /// Adds a synchronous XTinyLog sink to the logger configuration using a configuration builder.
    /// </summary>
    /// <param name="loggerSinkConfiguration">The logger sink configuration.</param>
    /// <param name="configureBuilder">Action to configure the XTinyLog sink using the builder pattern.</param>
    /// <returns>The logger configuration for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when loggerSinkConfiguration or configureBuilder is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the builder configuration is invalid.</exception>
    public static LoggerConfiguration XTinyLogWithBuilder(
        this LoggerSinkConfiguration loggerSinkConfiguration,
        Action<XTinyLogSinkConfigurationBuilder> configureBuilder)
    {
        if (loggerSinkConfiguration == null)
        {
            throw new ArgumentNullException(nameof(loggerSinkConfiguration));
        }

        if (configureBuilder == null)
        {
            throw new ArgumentNullException(nameof(configureBuilder));
        }

        var builder = new XTinyLogSinkConfigurationBuilder();
        configureBuilder(builder);
        var configuration = builder.Build();

        return loggerSinkConfiguration.Sink(new XTinyLogSink(configuration));
    }

    /// <summary>
    /// Adds an asynchronous XTinyLog sink to the logger configuration using a configuration builder.
    /// </summary>
    /// <param name="loggerSinkConfiguration">The logger sink configuration.</param>
    /// <param name="configureBuilder">Action to configure the XTinyLog sink using the builder pattern.</param>
    /// <returns>The logger configuration for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when loggerSinkConfiguration or configureBuilder is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the builder configuration is invalid.</exception>
    public static LoggerConfiguration XTinyLogAsyncWithBuilder(
        this LoggerSinkConfiguration loggerSinkConfiguration,
        Action<XTinyLogSinkConfigurationBuilder> configureBuilder)
    {
        if (loggerSinkConfiguration == null)
        {
            throw new ArgumentNullException(nameof(loggerSinkConfiguration));
        }

        if (configureBuilder == null)
        {
            throw new ArgumentNullException(nameof(configureBuilder));
        }

        var builder = new XTinyLogSinkConfigurationBuilder();
        configureBuilder(builder);
        var configuration = builder.Build();

        return loggerSinkConfiguration.Sink(new XTinyLogAsyncSink(configuration));
    }

    /// <summary>
    /// Validates configuration parameters for appsettings.json binding.
    /// </summary>
    /// <param name="logDirectory">The log directory to validate.</param>
    /// <param name="applicationName">The application name to validate.</param>
    /// <param name="maxFileSize">The max file size to validate.</param>
    /// <exception cref="ArgumentException">Thrown when parameters are invalid.</exception>
    private static void ValidateConfigurationParameters(string? logDirectory, string? applicationName, long maxFileSize)
    {
        var errors = new List<string>();

        // Only validate non-null values - null values will be replaced with defaults
        if (logDirectory != null && string.IsNullOrWhiteSpace(logDirectory))
        {
            errors.Add("LogDirectory cannot be empty when specified in configuration.");
        }

        if (applicationName != null && string.IsNullOrWhiteSpace(applicationName))
        {
            errors.Add("ApplicationName cannot be empty when specified in configuration.");
        }

        if (maxFileSize <= 0)
        {
            errors.Add("MaxFileSize must be greater than 0.");
        }

        if (errors.Count > 0)
        {
            throw new ArgumentException($"Invalid configuration parameters: {string.Join(", ", errors)}");
        }
    }
}