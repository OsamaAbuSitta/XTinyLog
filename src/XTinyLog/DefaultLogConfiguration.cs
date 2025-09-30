using System;

namespace XTinyLog
{
    /// <summary>
    /// Provides the default implementation of <see cref="ILogConfiguration"/> with standard logging settings.
    /// </summary>
    internal class DefaultLogConfiguration : ILogConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultLogConfiguration"/> class with default values.
        /// </summary>
        public DefaultLogConfiguration()
        {
            LogDirectory = @"c:\Log";
            MaxFileSize = 1_000;
            FileName = () => DateTime.Now.ToString("dd MM yyyy");
            ApplicationName = DateTime.Now.ToString("MM yyyy");
        }

        /// <inheritdoc />
        public string LogDirectory { get; set; }

        /// <inheritdoc />
        public string ApplicationName { get; set; }

        /// <inheritdoc />
        public long MaxFileSize { get; set; }

        /// <inheritdoc />
        public Func<string> FileName { get; set; }
    }
}
