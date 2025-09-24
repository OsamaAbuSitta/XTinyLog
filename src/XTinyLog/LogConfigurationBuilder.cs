using System;

namespace XTinyLog
{
    /// <summary>
    /// Provides a fluent API for building log configuration
    /// </summary>
    public class LogConfigurationBuilder
    {
        private readonly DefaultLogConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogConfigurationBuilder"/> class
        /// </summary>
        public LogConfigurationBuilder()
        {
            _configuration = new DefaultLogConfiguration();
        }

        /// <summary>
        /// Sets the log directory path
        /// </summary>
        /// <param name="logDirectory">The directory where log files will be stored</param>
        /// <returns>The configuration builder for method chaining</returns>
        /// <exception cref="ArgumentException">Thrown when logDirectory is null or empty</exception>
        public LogConfigurationBuilder WithLogDirectory(string logDirectory)
        {
            if (string.IsNullOrWhiteSpace(logDirectory))
                throw new ArgumentException("Log directory cannot be null or empty", nameof(logDirectory));

            _configuration.LogDirectory = logDirectory;
            return this;
        }

        /// <summary>
        /// Sets the application name used for organizing log files
        /// </summary>
        /// <param name="applicationName">The name of the application</param>
        /// <returns>The configuration builder for method chaining</returns>
        /// <exception cref="ArgumentException">Thrown when applicationName is null or empty</exception>
        public LogConfigurationBuilder WithApplicationName(string applicationName)
        {
            if (string.IsNullOrWhiteSpace(applicationName))
                throw new ArgumentException("Application name cannot be null or empty", nameof(applicationName));

            _configuration.ApplicationName = applicationName;
            return this;
        }

        /// <summary>
        /// Sets the file name generator function
        /// </summary>
        /// <param name="fileNameGenerator">A function that generates the log file name</param>
        /// <returns>The configuration builder for method chaining</returns>
        /// <exception cref="ArgumentNullException">Thrown when fileNameGenerator is null</exception>
        public LogConfigurationBuilder WithFileName(Func<string> fileNameGenerator)
        {
            _configuration.FileName = fileNameGenerator ?? throw new ArgumentNullException(nameof(fileNameGenerator));
            return this;
        }

        /// <summary>
        /// Sets the file name using a static string
        /// </summary>
        /// <param name="fileName">The static file name to use</param>
        /// <returns>The configuration builder for method chaining</returns>
        /// <exception cref="ArgumentException">Thrown when fileName is null or empty</exception>
        public LogConfigurationBuilder WithFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name cannot be null or empty", nameof(fileName));

            _configuration.FileName = () => fileName;
            return this;
        }

        /// <summary>
        /// Sets the maximum file size in kilobytes
        /// </summary>
        /// <param name="maxFileSizeKB">The maximum file size in kilobytes</param>
        /// <returns>The configuration builder for method chaining</returns>
        /// <exception cref="ArgumentException">Thrown when maxFileSizeKB is less than or equal to 0</exception>
        public LogConfigurationBuilder WithMaxFileSize(long maxFileSizeKB)
        {
            if (maxFileSizeKB <= 0)
                throw new ArgumentException("Max file size must be greater than 0", nameof(maxFileSizeKB));

            _configuration.MaxFileSize = maxFileSizeKB;
            return this;
        }

        /// <summary>
        /// Sets the file name to use the current date in the specified format
        /// </summary>
        /// <param name="dateFormat">The date format string (default: "dd MM yyyy")</param>
        /// <returns>The configuration builder for method chaining</returns>
        /// <exception cref="ArgumentException">Thrown when dateFormat is null or empty</exception>
        public LogConfigurationBuilder WithDateBasedFileName(string dateFormat = "dd MM yyyy")
        {
            if (string.IsNullOrWhiteSpace(dateFormat))
                throw new ArgumentException("Date format cannot be null or empty", nameof(dateFormat));

            _configuration.FileName = () => DateTime.Now.ToString(dateFormat);
            return this;
        }

        /// <summary>
        /// Builds and validates the configuration
        /// </summary>
        /// <returns>The built configuration</returns>
        /// <exception cref="InvalidOperationException">Thrown when the configuration is invalid</exception>
        public ILogConfiguration Build()
        {
            var validationResult = ConfigurationValidator.Validate(_configuration);
            
            if (!validationResult.IsValid)
            {
                throw new InvalidOperationException($"Configuration validation failed:{Environment.NewLine}{validationResult.GetErrorsAsString()}");
            }

            return _configuration;
        }

        /// <summary>
        /// Builds the configuration without validation (for advanced scenarios)
        /// </summary>
        /// <returns>The built configuration</returns>
        public ILogConfiguration BuildWithoutValidation()
        {
            return _configuration;
        }

        /// <summary>
        /// Validates the current configuration without building it
        /// </summary>
        /// <returns>The validation result</returns>
        public ValidationResult Validate()
        {
            return ConfigurationValidator.Validate(_configuration);
        }
    }
}