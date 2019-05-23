using System;

namespace XTinyLog
{
    internal class DefaultLogConfiguration : ILogConfiguration
    {
        public DefaultLogConfiguration()
        {
            LogDirectory = @"c:\Log";
            MaxFileSize = 1_000;
            FileName =() => DateTime.Now.ToString("dd MM yyyy");
            ApplicationName = DateTime.Now.ToString("MM yyyy");
        }

        public string LogDirectory { get; set; }
        public string ApplicationName { get; set; } 
        public long MaxFileSize { get; set; }
        public Func<string> FileName { get; set; } 
    }
}