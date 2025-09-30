namespace XTinyLog.Serilog;

/// <summary>
/// Configuration class for the XTinyLog Serilog sink.
/// </summary>
public class XTinyLogSinkConfiguration
{
    /// <summary>
    /// Gets or sets the directory where log files will be stored.
    /// </summary>
    public string LogDirectory { get; set; } = @"c:\Log";

    /// <summary>
    /// Gets or sets the application name used for log organization.
    /// </summary>
    public string ApplicationName { get; set; } = "Application";

    /// <summary>
    /// Gets or sets the maximum file size in KB before archiving.
    /// </summary>
    public long MaxFileSize { get; set; } = 1024; // 1MB default

    /// <summary>
    /// Gets or sets the file name generator function.
    /// </summary>
    public Func<string>? FileName { get; set; }

    /// <summary>
    /// Gets or sets whether to include structured properties in the output.
    /// </summary>
    public bool IncludeProperties { get; set; } = true;

    /// <summary>
    /// Gets or sets how structured properties should be formatted.
    /// </summary>
    public PropertyFormatMode PropertyFormat { get; set; } = PropertyFormatMode.Inline;

    /// <summary>
    /// Converts this configuration to an XTinyLog ILogConfiguration.
    /// </summary>
    /// <returns>An ILogConfiguration instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the configuration is invalid.</exception>
    public ILogConfiguration ToLogConfiguration()
    {
        try
        {
            // Validate configuration before conversion
            var validationErrors = Validate();
            if (validationErrors.Count > 0)
            {
                throw new InvalidOperationException($"Cannot convert invalid configuration to ILogConfiguration. Errors: {string.Join(", ", validationErrors)}");
            }

            var builder = new LogConfigurationBuilder()
                .WithLogDirectory(LogDirectory)
                .WithApplicationName(ApplicationName)
                .WithMaxFileSize(MaxFileSize);

            if (FileName != null)
            {
                try
                {
                    builder.WithFileName(FileName);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to configure FileName function: {ex.Message}", ex);
                }
            }

            return builder.Build();
        }
        catch (InvalidOperationException)
        {
            // Re-throw validation and configuration errors
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create ILogConfiguration: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Creates a copy of this configuration.
    /// </summary>
    /// <returns>A new XTinyLogSinkConfiguration instance with the same values.</returns>
    public XTinyLogSinkConfiguration Clone()
    {
        return new XTinyLogSinkConfiguration
        {
            LogDirectory = LogDirectory,
            ApplicationName = ApplicationName,
            MaxFileSize = MaxFileSize,
            FileName = FileName,
            IncludeProperties = IncludeProperties,
            PropertyFormat = PropertyFormat,
        };
    }

    /// <summary>
    /// Validates the configuration and returns any validation errors.
    /// </summary>
    /// <returns>A list of validation error messages, or empty if valid.</returns>
    public IList<string> Validate()
    {
        var errors = new List<string>();

        try
        {
            // Validate LogDirectory
            if (string.IsNullOrWhiteSpace(LogDirectory))
            {
                errors.Add("LogDirectory cannot be null or empty.");
            }
            else
            {
                try
                {
                    // Check for invalid characters first (before Path.GetFullPath which might normalize them)
                    var invalidChars = Path.GetInvalidPathChars();
                    if (LogDirectory.IndexOfAny(invalidChars) >= 0)
                    {
                        errors.Add("LogDirectory contains invalid characters.");
                    }
                    else
                    {
                        // Check if the directory path is valid by trying to get full path
                        var fullPath = Path.GetFullPath(LogDirectory);
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"LogDirectory is invalid: {ex.Message}");
                }
            }

            // Validate ApplicationName
            if (string.IsNullOrWhiteSpace(ApplicationName))
            {
                errors.Add("ApplicationName cannot be null or empty.");
            }
            else
            {
                try
                {
                    // Check for invalid filename characters in application name
                    var invalidChars = Path.GetInvalidFileNameChars();
                    if (ApplicationName.IndexOfAny(invalidChars) >= 0)
                    {
                        errors.Add("ApplicationName contains invalid filename characters.");
                    }

                    // Check length
                    if (ApplicationName.Length > 255)
                    {
                        errors.Add("ApplicationName is too long (maximum 255 characters).");
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"ApplicationName validation failed: {ex.Message}");
                }
            }

            // Validate MaxFileSize
            if (MaxFileSize <= 0)
            {
                errors.Add("MaxFileSize must be greater than 0.");
            }
            else if (MaxFileSize > long.MaxValue / 1024) // Prevent overflow when converting to bytes
            {
                errors.Add("MaxFileSize is too large.");
            }

            // Validate PropertyFormat enum
            if (!Enum.IsDefined(typeof(PropertyFormatMode), PropertyFormat))
            {
                errors.Add($"PropertyFormat value '{PropertyFormat}' is not valid.");
            }

            // Validate FileName function if provided
            if (FileName != null)
            {
                try
                {
                    var testFileName = FileName();
                    if (string.IsNullOrWhiteSpace(testFileName))
                    {
                        errors.Add("FileName function returns null or empty string.");
                    }
                    else
                    {
                        // Check for invalid filename characters
                        var invalidChars = Path.GetInvalidFileNameChars();
                        if (testFileName.IndexOfAny(invalidChars) >= 0)
                        {
                            errors.Add("FileName function returns a string with invalid filename characters.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"FileName function validation failed: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            // If validation itself fails, add a general error
            errors.Add($"Configuration validation failed: {ex.Message}");
        }

        return errors;
    }
}