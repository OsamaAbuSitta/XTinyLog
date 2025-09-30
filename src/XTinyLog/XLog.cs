using System;
using System.Threading;
using System.Threading.Tasks;

namespace XTinyLog
{
    /// <summary>
    /// Wrapper class to manage instance creation.
    /// </summary>
    public class XLog
    {
        private static readonly Lazy<XLog> _xTinyLogLazy = new Lazy<XLog>(() => new XLog());
        private readonly HtmlLog _logger;

        /// <summary>
        /// Gets the configuration for this log instance.
        /// </summary>
        public ILogConfiguration Config { get; }

        /// <summary>
        /// Gets the single log instance.
        /// </summary>
        public static XLog Log => _xTinyLogLazy.Value;

        /// <summary>
        /// Returns a new log instance if multiple instances are needed.
        /// </summary>
        /// <returns>A new XLog instance.</returns>
        public static XLog Create()
        {
            return new XLog();
        }

        /// <summary>
        /// Returns a new log instance if multiple instances and different configuration are needed.
        /// </summary>
        /// <param name="configuration">Action to configure the log settings.</param>
        /// <returns>A new XLog instance with the specified configuration.</returns>
        /// <exception cref="InvalidOperationException">Thrown when configuration validation fails</exception>
        public static XLog Create(Action<ILogConfiguration> configuration)
        {
            var config = new DefaultLogConfiguration();
            configuration.Invoke(config);

            // Validate configuration
            var validationResult = ConfigurationValidator.Validate(config);
            if (!validationResult.IsValid)
            {
                throw new InvalidOperationException($"Configuration validation failed:{Environment.NewLine}{validationResult.GetErrorsAsString()}");
            }

            return new XLog(config);
        }

        /// <summary>/// Creates a new log instance using the fluent configuration builder
        /// .</summary>
        /// <param name="Action to configure the log settings using the builder">Action to configure the log settings using the builder.</param>
        /// <returns>A new XLog instance with the specified configuration.</returns>
        /// <exception cref="InvalidOperationException">Thrown when configuration validation fails</exception>
        public static XLog Create(Action<LogConfigurationBuilder> builderAction)
        {
            var builder = new LogConfigurationBuilder();
            builderAction.Invoke(builder);
            var config = builder.Build(); // This will validate automatically

            return new XLog(config);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XLog"/> class with the specified configuration.
        /// </summary>
        /// <param name="configuration">The log configuration to use.</param>
        protected XLog(ILogConfiguration configuration)
        {
            Config = configuration;
            _logger = new HtmlLog(configuration);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XLog"/> class with default configuration.
        /// </summary>
        protected XLog()
        {
            var defaultConfig = new DefaultLogConfiguration();

            // Validate default configuration
            var validationResult = ConfigurationValidator.Validate(defaultConfig);
            if (!validationResult.IsValid)
            {
                throw new InvalidOperationException($"Default configuration validation failed:{Environment.NewLine}{validationResult.GetErrorsAsString()}");
            }

            _logger = new HtmlLog(defaultConfig);
            Config = defaultConfig;
        }

        /// <summary>/// Logs an informational message.
        /// .</summary>
        /// <param name="message">The message to log.</param>
        /// <example>
        /// <code>
        /// XLog.Log.Info("Application started successfully");
        /// </code>
        /// </example>
        public virtual void Info(string message)
        {
            _logger.SafetyLog(message, LogTypeEnum.Info);
        }

        /// <summary>/// Logs a debug message.
        /// .</summary>
        /// <param name="message">The debug message to log.</param>
        /// <example>
        /// <code>
        /// XLog.Log.Debug("Processing user request with ID: 12345");
        /// </code>
        /// </example>
        public virtual void Debug(string message)
        {
            _logger.SafetyLog(message, LogTypeEnum.Debug);
        }

        /// <summary>/// Logs an error message.
        /// .</summary>
        /// <param name="message">The error message to log.</param>
        /// <example>
        /// <code>
        /// XLog.Log.Error("Failed to connect to database");
        /// </code>
        /// </example>
        public virtual void Error(string message)
        {
            _logger.SafetyLog(message, LogTypeEnum.Error);
        }

        /// <summary>/// Logs an error message from an exception.
        /// .</summary>
        /// <param name="exception">The exception to log.</param>
        /// <example>
        /// <code>
        /// try
        /// {
        ///     // Some operation
        /// }
        /// catch (Exception ex)
        /// {
        ///     XLog.Log.Error(ex);
        /// }
        /// </code>
        /// </example>
        public virtual void Error(Exception exception)
        {
            _logger.SafetyLog(exception.Message, LogTypeEnum.Error);
        }

        /// <summary>/// Logs a warning message.
        /// .</summary>
        /// <param name="message">The warning message to log.</param>
        /// <example>
        /// <code>
        /// XLog.Log.Warn("Configuration file not found, using defaults");
        /// </code>
        /// </example>
        public virtual void Warn(string message)
        {
            _logger.SafetyLog(message, LogTypeEnum.Warning);
        }

        /// <summary>/// Asynchronously logs an informational message.
        /// .</summary>
        /// <param name="message">The message to log.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous logging operation.</returns>
        /// <example>
        /// <code>
        /// await XLog.Log.InfoAsync("Application started successfully");
        /// </code>
        /// </example>
        public virtual async Task InfoAsync(string message, CancellationToken cancellationToken = default)
        {
            await _logger.SafetyLogAsync(message, LogTypeEnum.Info, cancellationToken);
        }

        /// <summary>/// Asynchronously logs a debug message.
        /// .</summary>
        /// <param name="message">The debug message to log.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous logging operation.</returns>
        /// <example>
        /// <code>
        /// await XLog.Log.DebugAsync("Processing user request with ID: 12345");
        /// </code>
        /// </example>
        public virtual async Task DebugAsync(string message, CancellationToken cancellationToken = default)
        {
            await _logger.SafetyLogAsync(message, LogTypeEnum.Debug, cancellationToken);
        }

        /// <summary>/// Asynchronously logs an error message.
        /// .</summary>
        /// <param name="message">The error message to log.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous logging operation.</returns>
        /// <example>
        /// <code>
        /// await XLog.Log.ErrorAsync("Failed to connect to database");
        /// </code>
        /// </example>
        public virtual async Task ErrorAsync(string message, CancellationToken cancellationToken = default)
        {
            await _logger.SafetyLogAsync(message, LogTypeEnum.Error, cancellationToken);
        }

        /// <summary>/// Asynchronously logs an error message from an exception.
        /// .</summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous logging operation.</returns>
        /// <example>
        /// <code>
        /// try
        /// {
        ///     // Some operation
        /// }
        /// catch (Exception ex)
        /// {
        ///     await XLog.Log.ErrorAsync(ex);
        /// }
        /// </code>
        /// </example>
        public virtual async Task ErrorAsync(Exception exception, CancellationToken cancellationToken = default)
        {
            await _logger.SafetyLogAsync(exception.Message, LogTypeEnum.Error, cancellationToken);
        }

        /// <summary>/// Asynchronously logs a warning message.
        /// .</summary>
        /// <param name="message">The warning message to log.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous logging operation.</returns>
        /// <example>
        /// <code>
        /// await XLog.Log.WarnAsync("Configuration file not found, using defaults");
        /// </code>
        /// </example>
        public virtual async Task WarnAsync(string message, CancellationToken cancellationToken = default)
        {
            await _logger.SafetyLogAsync(message, LogTypeEnum.Warning, cancellationToken);
        }
    }
}
