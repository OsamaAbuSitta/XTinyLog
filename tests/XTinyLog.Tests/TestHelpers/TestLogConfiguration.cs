using System;

namespace XTinyLog.Tests.TestHelpers
{
    /// <summary>
    /// Test implementation of ILogConfiguration for unit testing
    /// </summary>
    public class TestLogConfiguration : ILogConfiguration
    {
        public TestLogConfiguration()
        {
            LogDirectory = @"c:\Log";
            MaxFileSize = 1_000;
            FileName = () => DateTime.Now.ToString("dd MM yyyy");
            ApplicationName = DateTime.Now.ToString("MM yyyy");
        }

        public string LogDirectory { get; set; }
        public string ApplicationName { get; set; }
        public long MaxFileSize { get; set; }
        public Func<string> FileName { get; set; }
    }
}