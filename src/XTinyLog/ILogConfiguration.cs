using System;

namespace XTinyLog
{
    /// <summary>
    /// Defines the configuration interface for XTinyLog logging settings.
    /// </summary>
    public interface ILogConfiguration
    {
        /// <summary>
        /// Gets or sets the log folder directory where log files will be stored.
        /// </summary>
        /// <value>Default value is 'C:\Log'.</value>
        /// <example>
        /// <code>
        /// config.LogDirectory = @"C:\MyApp\Logs";
        /// </code>
        /// </example>
        string LogDirectory { get; set; }

        /// <summary>
        /// Gets or sets the application name used to create log sub-folder organization.
        /// </summary>
        /// <value>Default value is DateTime.Now.ToString("MM yyyy").</value>
        /// <example>
        /// <code>
        /// config.ApplicationName = "MyApplication";
        /// </code>
        /// </example>
        string ApplicationName { get; set; }

        /// <summary>
        /// Gets or sets the maximum file size per log file in kilobytes.
        /// When this size is exceeded, the current log file is archived and a new one is created.
        /// </summary>
        /// <value>Default value is 1,000 kilobytes.</value>
        /// <example>
        /// <code>
        /// config.MaxFileSize = 5000; // 5MB
        /// </code>
        /// </example>
        long MaxFileSize { get; set; }

        /// <summary>
        /// Gets or sets the function that generates the log file name.
        /// This allows for dynamic file naming based on date, time, or other criteria.
        /// </summary>
        /// <value>Default value returns DateTime.Now.ToString("dd MM yyyy").</value>
        /// <example>
        /// <code>
        /// config.FileName = () => $"log_{DateTime.Now:yyyyMMdd}";
        /// </code>
        /// </example>
        Func<string> FileName { get; set; }
    }
}