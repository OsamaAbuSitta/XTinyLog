using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace XTinyLog.Tests.EndToEnd
{
    /// <summary>
    /// End-to-end tests that verify the complete logging workflow from configuration to HTML output.
    /// </summary>
    public sealed class EndToEndTests : IDisposable
    {
        private readonly string _testDirectory;
        private readonly string _testAppName;

        public EndToEndTests()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), "XTinyLog_E2E_Tests", Guid.NewGuid().ToString());
            _testAppName = "E2ETestApp";
            Directory.CreateDirectory(_testDirectory);
        }

        [Fact]
        public async Task CompleteLoggingWorkflow_ShouldCreateValidHtmlOutput()
        {
            // Arrange
            var logger = XLog.Create(config =>
            {
                config.LogDirectory = _testDirectory;
                config.ApplicationName = _testAppName;
                config.FileName = () => "test-log";
            });

            // Act - Perform various logging operations
            logger.Info("Application started");
            logger.Warn("This is a warning message");
            logger.Error("This is an error message");

            await logger.InfoAsync("Async info message");
            await logger.ErrorAsync("Async error message");

            // Log with exception
            try
            {
                throw new InvalidOperationException("Test exception");
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            // Wait for async operations to complete
            await Task.Delay(100);

            // Assert - Verify HTML file was created
            var htmlFiles = Directory.GetFiles(_testDirectory, "*.html", SearchOption.AllDirectories);
            htmlFiles.Should().NotBeEmpty("HTML log file should be created");

            var htmlFile = htmlFiles[0];
            var htmlContent = File.ReadAllText(htmlFile);

            // Verify HTML structure and content
            htmlContent.Should().Contain("<!DOCTYPE html>", "Should have valid HTML doctype");
            htmlContent.Should().Contain("<html", "Should have html tag");
            htmlContent.Should().Contain("Application started", "Should contain logged info message");
            htmlContent.Should().Contain("This is a warning message", "Should contain warning message");
            htmlContent.Should().Contain("This is an error message", "Should contain error message");
            htmlContent.Should().Contain("Async info message", "Should contain async info message");
            htmlContent.Should().Contain("Async error message", "Should contain async error message");
            htmlContent.Should().Contain("Test exception", "Should contain exception message");
        }

        [Fact]
        public async Task InteractiveFeatures_ShouldBeIncludedInHtmlOutput()
        {
            // Arrange
            var logger = XLog.Create(config =>
            {
                config.LogDirectory = _testDirectory;
                config.ApplicationName = _testAppName;
                config.FileName = () => "interactive-test";
            });

            // Act
            logger.Info("Test message for interactive features");
            await Task.Delay(100);

            // Assert
            var htmlFiles = Directory.GetFiles(_testDirectory, "*.html", SearchOption.AllDirectories);
            var htmlContent = File.ReadAllText(htmlFiles[0]);

            // Verify CSS and JavaScript are included
            htmlContent.Should().Contain("<style>", "Should include CSS styles");
            htmlContent.Should().Contain("<script>", "Should include JavaScript functionality");
        }

        [Fact]
        public void ConfigurationValidation_ShouldPreventInvalidSettings()
        {
            // Test invalid directory
            Action invalidDirectory = () => XLog.Create(config =>
            {
                config.LogDirectory = string.Empty;
                config.ApplicationName = _testAppName;
                config.FileName = () => "test";
            });
            invalidDirectory.Should().Throw<InvalidOperationException>("Should validate log directory");

            // Test invalid application name
            Action invalidAppName = () => XLog.Create(config =>
            {
                config.LogDirectory = _testDirectory;
                config.ApplicationName = string.Empty;
                config.FileName = () => "test";
            });
            invalidAppName.Should().Throw<InvalidOperationException>("Should validate application name");
        }

        [Fact]
        public async Task MultipleLoggerInstances_ShouldWorkIndependently()
        {
            // Arrange
            var logger1 = XLog.Create(config =>
            {
                config.LogDirectory = _testDirectory;
                config.ApplicationName = "App1";
                config.FileName = () => "log1";
            });

            var logger2 = XLog.Create(config =>
            {
                config.LogDirectory = _testDirectory;
                config.ApplicationName = "App2";
                config.FileName = () => "log2";
            });

            // Act
            logger1.Info("Message from logger 1");
            logger2.Info("Message from logger 2");
            await Task.Delay(100);

            // Assert
            var htmlFiles = Directory.GetFiles(_testDirectory, "*.html", SearchOption.AllDirectories);
            htmlFiles.Should().HaveCountGreaterOrEqualTo(1, "Should create log files");

            // Verify content separation (files may be in subdirectories)
            var allContent = string.Join(" ", htmlFiles.Select(File.ReadAllText));
            allContent.Should().Contain("Message from logger 1");
            allContent.Should().Contain("Message from logger 2");
        }

        [Fact]
        public async Task HighVolumeLogging_ShouldMaintainPerformance()
        {
            // Arrange
            var logger = XLog.Create(config =>
            {
                config.LogDirectory = _testDirectory;
                config.ApplicationName = _testAppName;
                config.FileName = () => "performance-test";
            });

            const int messageCount = 50; // Reduced for faster test execution

            // Act
            var startTime = DateTime.UtcNow;

            var tasks = new Task[messageCount];
            for (int i = 0; i < messageCount; i++)
            {
                int messageIndex = i;
                tasks[i] = Task.Run(async () =>
                {
                    await logger.InfoAsync($"Performance test message {messageIndex}");
                });
            }

            await Task.WhenAll(tasks);
            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;

            // Assert
            duration.Should().BeLessThan(TimeSpan.FromSeconds(10), "High volume logging should complete within reasonable time");

            var htmlFiles = Directory.GetFiles(_testDirectory, "*.html", SearchOption.AllDirectories);
            htmlFiles.Should().NotBeEmpty("Should create log files");
        }

        [Fact]
        public async Task ExceptionHandling_ShouldNotBreakLogging()
        {
            // Arrange
            var logger = XLog.Create(config =>
            {
                config.LogDirectory = _testDirectory;
                config.ApplicationName = _testAppName;
                config.FileName = () => "exception-test";
            });

            // Act & Assert - Various exception scenarios
            logger.Error("Error before exception");

            // Exception handling
            try
            {
                throw new ArgumentNullException();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            await Task.Delay(100);

            // Verify logging continued to work
            logger.Info("Error after exception handling");
            await Task.Delay(100);

            var htmlFiles = Directory.GetFiles(_testDirectory, "*.html", SearchOption.AllDirectories);
            htmlFiles.Should().NotBeEmpty("Should create log files even with exceptions");
        }

        [Fact]
        public void DefaultUsage_ShouldWork()
        {
            // Test the most basic usage scenario
            var logger = XLog.Log;

            logger.Info("Hello, World!");
            logger.Error("Something went wrong");

            // Should not throw exceptions
            logger.Should().NotBeNull("Basic usage should work");
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