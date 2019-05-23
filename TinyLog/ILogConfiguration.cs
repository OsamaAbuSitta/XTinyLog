using System;

namespace XTinyLog
{
    public interface ILogConfiguration
    {
        /// <summary>
        /// gets/sets log folder directory 
        /// <value>Default value 'C:\Log'</value>
        /// </summary>
        string LogDirectory { get; set; }

        /// <summary>
        /// gets/sets Application name used to set log sub folder name
        /// <value>Default value DateTime.Now.ToString("MM yyyy")</value>
        /// </summary>
        string ApplicationName { get; set; }
        /// <summary>
        /// Max file size per log file in KBytes
        /// <value>Default value 1,000 Kilobyte</value>
        /// </summary>
        long MaxFileSize { get; set; }

        /// <summary>
        /// Log file name
        /// <value>DateTime.Now.ToString("dd MM yyyy")</value>
        /// </summary>
        Func<string> FileName { get; set; }
    }
}