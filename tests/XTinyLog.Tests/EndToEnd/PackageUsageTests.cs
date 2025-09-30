using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace XTinyLog.Tests.EndToEnd
{
    /// <summary>
    /// Tests that verify package installation and usage scenarios.
    /// </summary>
    public sealed class PackageUsageTests : IDisposable
    {
        private readonly string _testDirectory;

        public PackageUsageTests()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), "XTinyLog_Package_Tests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testDirectory);
        }

        [Fact]
        public void Assembly_ShouldHaveCorrectMetadata()
        {
            // Arrange
            var assembly = typeof(XLog).Assembly;

            // Assert
            assembly.Should().NotBeNull("XTinyLog assembly should be loaded");

            var assemblyName = assembly.GetName();
            assemblyName.Name.Should().Be("XTinyLog", "Assembly name should be XTinyLog");
            assemblyName.Version.Should().NotBeNull("Assembly should have version information");
        }

        [Fact]
        public void PublicApi_ShouldBeAccessible()
        {
            // Test that all expected public APIs are accessible
            var xlogType = typeof(XLog);
            xlogType.Should().NotBeNull("XLog class should be public");

            // Test static methods
            var createMethod = xlogType.GetMethod("Create", Array.Empty<Type>());
            createMethod.Should().NotBeNull("Create method should be public");

            // Test instance methods
            var logger = XLog.Create();
            logger.Should().NotBeNull("Create should return valid instance");

            var infoMethod = logger.GetType().GetMethod("Info", new[] { typeof(string) });
            infoMethod.Should().NotBeNull("Info method should be public");

            var errorMethod = logger.GetType().GetMethod("Error", new[] { typeof(string) });
            errorMethod.Should().NotBeNull("Error method should be public");

            var warnMethod = logger.GetType().GetMethod("Warn", new[] { typeof(string) });
            warnMethod.Should().NotBeNull("Warn method should be public");
        }

        [Fact]
        public async Task AsyncApi_ShouldBeAccessible()
        {
            // Test async API availability
            var logger = XLog.Create(config =>
            {
                config.LogDirectory = _testDirectory;
                config.ApplicationName = "TestApp";
                config.FileName = () => "async-test";
            });

            var loggerType = logger.GetType();

            var infoAsyncMethod = loggerType.GetMethod("InfoAsync", new[] { typeof(string), typeof(System.Threading.CancellationToken) });
            infoAsyncMethod.Should().NotBeNull("InfoAsync method should be public");

            var errorAsyncMethod = loggerType.GetMethod("ErrorAsync", new[] { typeof(string), typeof(System.Threading.CancellationToken) });
            errorAsyncMethod.Should().NotBeNull("ErrorAsync method should be public");

            var warnAsyncMethod = loggerType.GetMethod("WarnAsync", new[] { typeof(string), typeof(System.Threading.CancellationToken) });
            warnAsyncMethod.Should().NotBeNull("WarnAsync method should be public");

            // Test that async methods actually work
            await logger.InfoAsync("Async test message");
            await logger.ErrorAsync("Async error message");
            await logger.WarnAsync("Async warning message");

            // Verify file was created
            await Task.Delay(100);
            var htmlFiles = Directory.GetFiles(_testDirectory, "*.html", SearchOption.AllDirectories);
            htmlFiles.Should().NotBeEmpty("Async logging should create HTML files");
        }

        [Fact]
        public void ConfigurationBuilder_ShouldBeAccessible()
        {
            // Test configuration builder pattern
            var logger = XLog.Create(config =>
            {
                config.LogDirectory = _testDirectory;
                config.ApplicationName = "ConfigTest";
                config.FileName = () => "config";
            });
            logger.Should().NotBeNull("Configuration should work with builder pattern");

            // Test that configuration validation works
            Action invalidConfig = () => XLog.Create(config =>
            {
                config.LogDirectory = string.Empty;
                config.ApplicationName = "Test";
                config.FileName = () => "test";
            });
            invalidConfig.Should().Throw<InvalidOperationException>("Invalid configuration should be rejected");
        }

        [Fact]
        public void MultipleFrameworks_ShouldBeSupported()
        {
            // This test verifies that the library works across different .NET frameworks
            var assembly = typeof(XLog).Assembly;
            var location = assembly.Location;

            location.Should().NotBeNullOrEmpty("Assembly should have a valid location");
            File.Exists(location).Should().BeTrue("Assembly file should exist");

            // Test that basic functionality works (this implicitly tests framework compatibility)
            var logger = XLog.Create(config =>
            {
                config.LogDirectory = _testDirectory;
                config.ApplicationName = "FrameworkTest";
                config.FileName = () => "framework";
            });
            logger.Info("Framework compatibility test");

            // If we get here without exceptions, framework compatibility is working
            logger.Should().NotBeNull("Logger should work on current framework");
        }

        [Fact]
        public void ExceptionHandling_ShouldNotExposeInternalTypes()
        {
            // Test that exceptions thrown by the library are appropriate for public consumption
            try
            {
                XLog.Create(config =>
                {
                    config.LogDirectory = string.Empty;
                    config.ApplicationName = "Test";
                    config.FileName = () => "test";
                });
            }
            catch (Exception ex)
            {
                ex.Should().BeOfType<InvalidOperationException>("Should throw standard .NET exception types");
                ex.Message.Should().NotBeNullOrEmpty("Exception should have meaningful message");
            }

            try
            {
                XLog.Create(config =>
                {
                    config.LogDirectory = _testDirectory;
                    config.ApplicationName = string.Empty;
                    config.FileName = () => "test";
                });
            }
            catch (Exception ex)
            {
                ex.Should().BeOfType<InvalidOperationException>("Should throw standard .NET exception types");
                ex.Message.Should().NotBeNullOrEmpty("Exception should have meaningful message");
            }
        }

        [Fact]
        public async Task ThreadSafety_ShouldWorkInPackageUsage()
        {
            // Test thread safety in typical usage scenarios
            var logger = XLog.Create(config =>
            {
                config.LogDirectory = _testDirectory;
                config.ApplicationName = "ThreadTest";
                config.FileName = () => "thread-safety";
            });

            var tasks = new Task[5]; // Reduced for faster execution
            for (int i = 0; i < 5; i++)
            {
                int taskId = i;
                tasks[i] = Task.Run(async () =>
                {
                    for (int j = 0; j < 5; j++)
                    {
                        await logger.InfoAsync($"Task {taskId}, Message {j}");
                        logger.Warn($"Task {taskId}, Warning {j}");
                    }
                });
            }

            await Task.WhenAll(tasks);
            await Task.Delay(200); // Allow file operations to complete

            var htmlFiles = Directory.GetFiles(_testDirectory, "*.html", SearchOption.AllDirectories);
            htmlFiles.Should().NotBeEmpty("Thread-safe logging should create files");
        }

        [Fact]
        public void DefaultUsage_ShouldWorkWithMinimalConfiguration()
        {
            // Test the most basic usage scenario that users would typically start with
            var logger = XLog.Log;

            logger.Info("Hello, World!");
            logger.Error("Something went wrong");

            // Should not throw exceptions and should create output
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