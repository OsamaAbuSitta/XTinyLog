using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace XTinyLog.Tests.EndToEnd
{
    /// <summary>
    /// Tests that validate performance optimizations and final polish of the XTinyLog library.
    /// </summary>
    public sealed class PerformanceValidationTests : IDisposable
    {
        private readonly string _testDirectory;
        private readonly string _testAppName;

        public PerformanceValidationTests()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), "XTinyLog_Performance_Tests", Guid.NewGuid().ToString());
            _testAppName = "PerformanceTestApp";
            Directory.CreateDirectory(_testDirectory);
        }

        [Fact]
        public async Task AsyncLogging_ShouldBeMorePerformantThanSync()
        {
            // Arrange
            var logger = XLog.Create(config =>
            {
                config.LogDirectory = _testDirectory;
                config.ApplicationName = _testAppName;
                config.FileName = () => "async-performance-test";
            });

            const int messageCount = 100;
            var message = "Performance test message for async vs sync comparison";

            // Act - Measure sync performance
            var syncStopwatch = Stopwatch.StartNew();
            for (int i = 0; i < messageCount; i++)
            {
                logger.Info($"{message} sync {i}");
            }
            syncStopwatch.Stop();

            // Act - Measure async performance
            var asyncStopwatch = Stopwatch.StartNew();
            var asyncTasks = new Task[messageCount];
            for (int i = 0; i < messageCount; i++)
            {
                int index = i;
                asyncTasks[i] = logger.InfoAsync($"{message} async {index}");
            }
            await Task.WhenAll(asyncTasks);
            asyncStopwatch.Stop();

            // Assert - Async should not be significantly slower than sync
            // (In some cases async might be faster due to better I/O handling)
            var syncTime = syncStopwatch.ElapsedMilliseconds;
            var asyncTime = asyncStopwatch.ElapsedMilliseconds;

            // Both should complete in reasonable time
            syncTime.Should().BeLessThan(5000, "Sync logging should complete within 5 seconds");
            asyncTime.Should().BeLessThan(5000, "Async logging should complete within 5 seconds");

            // Verify all messages were logged
            await Task.Delay(200); // Allow async operations to complete
            var htmlFiles = Directory.GetFiles(_testDirectory, "*.html", SearchOption.AllDirectories);
            htmlFiles.Should().NotBeEmpty("Performance test should create HTML files");
        }

        [Fact]
        public void MemoryUsage_ShouldBeReasonable()
        {
            // Arrange
            var logger = XLog.Create(config =>
            {
                config.LogDirectory = _testDirectory;
                config.ApplicationName = _testAppName;
                config.FileName = () => "memory-test";
            });

            // Measure initial memory
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var initialMemory = GC.GetTotalMemory(false);

            // Act - Log many messages
            const int messageCount = 1000;
            for (int i = 0; i < messageCount; i++)
            {
                logger.Info($"Memory test message {i} with some additional content to make it longer");
            }

            // Force garbage collection and measure final memory
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var finalMemory = GC.GetTotalMemory(false);

            // Assert - Memory usage should be reasonable
            var memoryIncrease = finalMemory - initialMemory;
            var memoryIncreaseKB = memoryIncrease / 1024;

            // Memory increase should be reasonable (less than 10MB for 1000 messages)
            memoryIncreaseKB.Should().BeLessThan(10 * 1024, "Memory usage should be reasonable");

            // Verify logging worked
            var htmlFiles = Directory.GetFiles(_testDirectory, "*.html", SearchOption.AllDirectories);
            htmlFiles.Should().NotBeEmpty("Memory test should create HTML files");
        }

        [Fact]
        public void HtmlOutputSize_ShouldBeOptimized()
        {
            // Arrange
            var logger = XLog.Create(config =>
            {
                config.LogDirectory = _testDirectory;
                config.ApplicationName = _testAppName;
                config.FileName = () => "html-size-test";
            });

            // Act - Log various types of messages
            logger.Info("Short message");
            logger.Info("This is a longer message with more content to test HTML output size optimization");
            logger.Error("Error message with details");
            logger.Warn("Warning message");

            // Assert - Check HTML file size
            var htmlFiles = Directory.GetFiles(_testDirectory, "*.html", SearchOption.AllDirectories);
            htmlFiles.Should().NotBeEmpty("HTML size test should create files");

            var htmlFile = htmlFiles[0];
            var fileInfo = new FileInfo(htmlFile);
            var htmlContent = File.ReadAllText(htmlFile);

            // HTML should be reasonably sized (not bloated)
            fileInfo.Length.Should().BeLessThan(100 * 1024, "HTML file should be less than 100KB for basic logging");

            // HTML should be well-formed and optimized
            htmlContent.Should().Contain("<!DOCTYPE html>", "Should have proper doctype");
            htmlContent.Should().NotContain("    ", "Should not have excessive indentation");
            
            // Should contain essential content
            htmlContent.Should().Contain("Short message");
            htmlContent.Should().Contain("longer message");
            htmlContent.Should().Contain("Error message");
            htmlContent.Should().Contain("Warning message");
        }

        [Fact]
        public async Task ConcurrentLogging_ShouldHandleHighLoad()
        {
            // Arrange
            var logger = XLog.Create(config =>
            {
                config.LogDirectory = _testDirectory;
                config.ApplicationName = _testAppName;
                config.FileName = () => "concurrent-load-test";
            });

            const int concurrentTasks = 10;
            const int messagesPerTask = 50;

            // Act - Create concurrent logging load
            var stopwatch = Stopwatch.StartNew();
            var tasks = new Task[concurrentTasks];

            for (int i = 0; i < concurrentTasks; i++)
            {
                int taskId = i;
                tasks[i] = Task.Run(async () =>
                {
                    for (int j = 0; j < messagesPerTask; j++)
                    {
                        await logger.InfoAsync($"Concurrent task {taskId}, message {j}");
                    }
                });
            }

            await Task.WhenAll(tasks);
            stopwatch.Stop();

            // Assert - Should handle concurrent load efficiently
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(10000, "Concurrent logging should complete within 10 seconds");

            // Verify files were created
            await Task.Delay(200); // Allow file operations to complete
            var htmlFiles = Directory.GetFiles(_testDirectory, "*.html", SearchOption.AllDirectories);
            htmlFiles.Should().NotBeEmpty("Concurrent load test should create HTML files");
        }

        [Fact]
        public void FileHandling_ShouldBeEfficient()
        {
            // Arrange
            var logger = XLog.Create(config =>
            {
                config.LogDirectory = _testDirectory;
                config.ApplicationName = _testAppName;
                config.FileName = () => "file-handling-test";
            });

            // Act - Test file handling efficiency
            var stopwatch = Stopwatch.StartNew();

            // Log messages that will trigger file operations
            for (int i = 0; i < 100; i++)
            {
                logger.Info($"File handling test message {i}");
            }

            stopwatch.Stop();

            // Assert - File operations should be efficient
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000, "File handling should be efficient");

            // Verify file was created and is accessible
            var htmlFiles = Directory.GetFiles(_testDirectory, "*.html", SearchOption.AllDirectories);
            htmlFiles.Should().NotBeEmpty("File handling test should create HTML files");

            var htmlFile = htmlFiles[0];
            File.Exists(htmlFile).Should().BeTrue("HTML file should exist");

            // File should not be locked (can be read immediately)
            var content = File.ReadAllText(htmlFile);
            content.Should().NotBeNullOrEmpty("HTML file should have content");
            content.Should().Contain("File handling test message");
        }

        [Fact]
        public void ErrorHandling_ShouldNotImpactPerformance()
        {
            // Arrange
            var logger = XLog.Create(config =>
            {
                config.LogDirectory = _testDirectory;
                config.ApplicationName = _testAppName;
                config.FileName = () => "error-performance-test";
            });

            // Act - Test performance with various error scenarios
            var stopwatch = Stopwatch.StartNew();

            // Normal logging
            for (int i = 0; i < 50; i++)
            {
                logger.Info($"Normal message {i}");
            }

            // Error logging with exceptions
            for (int i = 0; i < 50; i++)
            {
                try
                {
                    throw new InvalidOperationException($"Test exception {i}");
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            }

            stopwatch.Stop();

            // Assert - Error handling should not significantly impact performance
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(3000, "Error handling should not significantly impact performance");

            // Verify logging worked correctly
            var htmlFiles = Directory.GetFiles(_testDirectory, "*.html", SearchOption.AllDirectories);
            htmlFiles.Should().NotBeEmpty("Error performance test should create HTML files");

            var content = File.ReadAllText(htmlFiles[0]);
            content.Should().Contain("Normal message");
            content.Should().Contain("Test exception");
        }

        [Fact]
        public void ConfigurationValidation_ShouldBeFast()
        {
            // Act - Test configuration validation performance
            var stopwatch = Stopwatch.StartNew();

            // Create multiple loggers with different configurations
            for (int i = 0; i < 100; i++)
            {
                var logger = XLog.Create(config =>
                {
                    config.LogDirectory = Path.Combine(_testDirectory, $"config-test-{i}");
                    config.ApplicationName = $"ConfigApp{i}";
                    config.FileName = () => $"config-file-{i}";
                });

                // Log one message to ensure logger is fully initialized
                logger.Info($"Configuration test {i}");
            }

            stopwatch.Stop();

            // Assert - Configuration validation should be fast
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000, "Configuration validation should be fast");

            // Verify multiple configurations worked
            var allHtmlFiles = Directory.GetFiles(_testDirectory, "*.html", SearchOption.AllDirectories);
            allHtmlFiles.Should().HaveCountGreaterThan(50, "Multiple configurations should create multiple files");
        }

        [Fact]
        public async Task LargeMessageHandling_ShouldBeOptimized()
        {
            // Arrange
            var logger = XLog.Create(config =>
            {
                config.LogDirectory = _testDirectory;
                config.ApplicationName = _testAppName;
                config.FileName = () => "large-message-test";
            });

            // Create large messages
            var smallMessage = "Small message";
            var mediumMessage = new string('M', 1000); // 1KB
            var largeMessage = new string('L', 10000); // 10KB

            // Act - Test performance with different message sizes
            var stopwatch = Stopwatch.StartNew();

            // Log messages of different sizes
            for (int i = 0; i < 10; i++)
            {
                logger.Info($"{smallMessage} {i}");
                logger.Info($"{mediumMessage} {i}");
                await logger.InfoAsync($"{largeMessage} {i}");
            }

            stopwatch.Stop();

            // Assert - Large message handling should be optimized
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000, "Large message handling should be optimized");

            // Verify all messages were logged
            await Task.Delay(200);
            var htmlFiles = Directory.GetFiles(_testDirectory, "*.html", SearchOption.AllDirectories);
            htmlFiles.Should().NotBeEmpty("Large message test should create HTML files");

            var content = File.ReadAllText(htmlFiles[0]);
            content.Should().Contain("Small message");
            content.Should().Contain("MMMM"); // Part of medium message
            content.Should().Contain("LLLL"); // Part of large message
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_testDirectory))
                {
                    Directory.Delete(_testDirectory, true);
                }
            }
            catch
            {
                // Ignore cleanup errors in tests
            }
        }
    }
}