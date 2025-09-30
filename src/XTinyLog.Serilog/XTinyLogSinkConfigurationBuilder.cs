namespace XTinyLog.Serilog;

/// <summary>
/// Builder class for creating XTinyLogSinkConfiguration instances using a fluent API.
/// </summary>
public class XTinyLogSinkConfigurationBuilder
{
    private string _logDirectory = @"c:\Log";
    private string _applicationName = "Application";
    private long _maxFileSize = 1024; // 1MB default
    private Func<string>? _fileName;
    private bool _includeProperties = true;
    private PropertyFormatMode _propertyFormat = PropertyFormatMode.Inline;

    /// <summary>
    /// Sets the directory where log files will be stored.
    /// </summary>
    /// <param name="logDirectory">The log directory path.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when logDirectory is null or whitespace.</exception>
    public XTinyLogSinkConfigurationBuilder WithLogDirectory(string logDirectory)
    {
        if (string.IsNullOrWhiteSpace(logDirectory))
        {
            throw new ArgumentException("Log directory cannot be null or whitespace.", nameof(logDirectory));
        }

        _logDirectory = logDirectory;
        return this;
    }

    /// <summary>
    /// Sets the application name used for log organization.
    /// </summary>
    /// <param name="applicationName">The application name.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when applicationName is null or whitespace.</exception>
    public XTinyLogSinkConfigurationBuilder WithApplicationName(string applicationName)
    {
        if (string.IsNullOrWhiteSpace(applicationName))
        {
            throw new ArgumentException("Application name cannot be null or whitespace.", nameof(applicationName));
        }

        _applicationName = applicationName;
        return this;
    }

    /// <summary>
    /// Sets the maximum file size in KB before archiving.
    /// </summary>
    /// <param name="maxFileSize">The maximum file size in KB.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when maxFileSize is less than or equal to 0.</exception>
    public XTinyLogSinkConfigurationBuilder WithMaxFileSize(long maxFileSize)
    {
        if (maxFileSize <= 0)
        {
            throw new ArgumentException("Max file size must be greater than 0.", nameof(maxFileSize));
        }

        _maxFileSize = maxFileSize;
        return this;
    }

    /// <summary>
    /// Sets the file name generator function.
    /// </summary>
    /// <param name="fileName">The file name generator function.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public XTinyLogSinkConfigurationBuilder WithFileName(Func<string> fileName)
    {
        _fileName = fileName;
        return this;
    }

    /// <summary>
    /// Sets whether to include structured properties in the output.
    /// </summary>
    /// <param name="includeProperties">True to include properties, false otherwise.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public XTinyLogSinkConfigurationBuilder WithIncludeProperties(bool includeProperties)
    {
        _includeProperties = includeProperties;
        return this;
    }

    /// <summary>
    /// Sets how structured properties should be formatted.
    /// </summary>
    /// <param name="propertyFormat">The property format mode.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public XTinyLogSinkConfigurationBuilder WithPropertyFormat(PropertyFormatMode propertyFormat)
    {
        _propertyFormat = propertyFormat;
        return this;
    }

    /// <summary>
    /// Builds and validates the configuration.
    /// </summary>
    /// <returns>A new XTinyLogSinkConfiguration instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the configuration is invalid.</exception>
    public XTinyLogSinkConfiguration Build()
    {
        var configuration = new XTinyLogSinkConfiguration
        {
            LogDirectory = _logDirectory,
            ApplicationName = _applicationName,
            MaxFileSize = _maxFileSize,
            FileName = _fileName,
            IncludeProperties = _includeProperties,
            PropertyFormat = _propertyFormat
        };

        // Validate the configuration
        var validationErrors = configuration.Validate();
        if (validationErrors.Count > 0)
        {
            throw new InvalidOperationException($"Configuration validation failed: {string.Join(", ", validationErrors)}");
        }

        return configuration;
    }

    /// <summary>
    /// Resets the builder to default values.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
    public XTinyLogSinkConfigurationBuilder Reset()
    {
        _logDirectory = @"c:\Log";
        _applicationName = "Application";
        _maxFileSize = 1024;
        _fileName = null;
        _includeProperties = true;
        _propertyFormat = PropertyFormatMode.Inline;
        return this;
    }
}