using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace XTinyLog.Tests.Unit
{
    /// <summary>
    /// Unit tests for XLog class functionality
    /// </summary>
    public class XLogTests : IDisposable
    {
        private readonly string _testLogDirectory;

        public XLogTests()
        {
            // Create a unique test directory for each test run
            _testLogDirectory = Path.Combine(Path.GetTempPath(), "XTinyLogTests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testLogDirectory);
        }

        public void Dispose()
        {
            // Clean up test directory
            if (Directory.Exists(_testLogDirectory))
            {
                try
                {
                    Directory.Delete(_testLogDirectory, true);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }

        [Fact]
        public void Log_StaticInstance_ShouldNotBeNull()
        {
            // Act & Assert
            XLog.Log.Should().NotBeNull();
        }

        [Fact]
        public void Create_WithoutConfiguration_ShouldReturnNewInstance()
        {
            // Act
            var log1 = XLog.Create();
            var log2 = XLog.Create();

            // Assert
            log1.Should().NotBeNull();
            log2.Should().NotBeNull();
            log1.Should().NotBeSameAs(log2);
        }

        [Fact]
        public void Create_WithValidConfiguration_ShouldReturnConfiguredInstance()
        {
            // Arrange
            var testAppName = "TestApp";

            // Act
            var log = XLog.Create(config =>
            {
                config.LogDirectory = _testLogDirectory;
                config.ApplicationName = testAppName;
                config.MaxFileSize = 2000;
            });

            // Assert
            log.Should().NotBeNull();
            log.Config.LogDirectory.Should().Be(_testLogDirectory);
            log.Config.ApplicationName.Should().Be(testAppName);
            log.Config.MaxFileSize.Should().Be(2000);
        }

        [Fact]
        public void Create_WithInvalidConfiguration_ShouldThrowInvalidOperationException()
        {
            // Act & Assert
            var act = () => XLog.Create(config =>
            {
                config.LogDirectory = null; // Invalid
                config.ApplicationName = "TestApp";
            });

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*Configuration validation failed*");
        }

        [Fact]
        public void Info_WithValidMessage_ShouldCreateLogFile()
        {
            // Arrange
            var log = XLog.Create(config =>
            {
                config.LogDirectory = _testLogDirectory;
                config.ApplicationName = "TestApp";
            });
            var message = "Test info message";

            // Act
            log.Info(message);

            // Assert
            var logFiles = Directory.GetFiles(Path.Combine(_testLogDirectory, "TestApp"), "*.html");
            logFiles.Should().HaveCount(1);
            
            var logContent = File.ReadAllText(logFiles[0]);
            logContent.Should().Contain(message);
            logContent.Should().Contain("'type': 'info'");
        }

        [Fact]
        public void Debug_WithValidMessage_ShouldCreateLogFile()
        {
            // Arrange
            var log = XLog.Create(config =>
            {
                config.LogDirectory = _testLogDirectory;
                config.ApplicationName = "TestApp";
            });
            var message = "Test debug message";

            // Act
            log.Debug(message);

            // Assert
            var logFiles = Directory.GetFiles(Path.Combine(_testLogDirectory, "TestApp"), "*.html");
            logFiles.Should().HaveCount(1);
            
            var logContent = File.ReadAllText(logFiles[0]);
            logContent.Should().Contain(message);
            logContent.Should().Contain("'type': 'debug'");
        }

        [Fact]
        public void Error_WithValidMessage_ShouldCreateLogFile()
        {
            // Arrange
            var log = XLog.Create(config =>
            {
                config.LogDirectory = _testLogDirectory;
                config.ApplicationName = "TestApp";
            });
            var message = "Test error message";

            // Act
            log.Error(message);

            // Assert
            var logFiles = Directory.GetFiles(Path.Combine(_testLogDirectory, "TestApp"), "*.html");
            logFiles.Should().HaveCount(1);
            
            var logContent = File.ReadAllText(logFiles[0]);
            logContent.Should().Contain(message);
            logContent.Should().Contain("'type': 'error'");
        }

        [Fact]
        public void Error_WithException_ShouldLogExceptionMessage()
        {
            // Arrange
            var log = XLog.Create(config =>
            {
                config.LogDirectory = _testLogDirectory;
                config.ApplicationName = "TestApp";
            });
            var exception = new InvalidOperationException("Test exception message");

            // Act
            log.Error(exception);

            // Assert
            var logFiles = Directory.GetFiles(Path.Combine(_testLogDirectory, "TestApp"), "*.html");
            logFiles.Should().HaveCount(1);
            
            var logContent = File.ReadAllText(logFiles[0]);
            logContent.Should().Contain("Test exception message");
            logContent.Should().Contain("'type': 'error'");
        }

        [Fact]
        public void Warn_WithValidMessage_ShouldCreateLogFile()
        {
            // Arrange
            var log = XLog.Create(config =>
            {
                config.LogDirectory = _testLogDirectory;
                config.ApplicationName = "TestApp";
            });
            var message = "Test warning message";

            // Act
            log.Warn(message);

            // Assert
            var logFiles = Directory.GetFiles(Path.Combine(_testLogDirectory, "TestApp"), "*.html");
            logFiles.Should().HaveCount(1);
            
            var logContent = File.ReadAllText(logFiles[0]);
            logContent.Should().Contain(message);
            logContent.Should().Contain("'type': 'warning'");
        }

        [Fact]
        public async Task InfoAsync_WithValidMessage_ShouldCreateLogFile()
        {
            // Arrange
            var log = XLog.Create(config =>
            {
                config.LogDirectory = _testLogDirectory;
                config.ApplicationName = "TestApp";
            });
            var message = "Test async info message";

            // Act
            await log.InfoAsync(message);

            // Assert
            var logFiles = Directory.GetFiles(Path.Combine(_testLogDirectory, "TestApp"), "*.html");
            logFiles.Should().HaveCount(1);
            
            var logContent = File.ReadAllText(logFiles[0]);
            logContent.Should().Contain(message);
            logContent.Should().Contain("'type': 'info'");
        }

        [Fact]
        public async Task DebugAsync_WithValidMessage_ShouldCreateLogFile()
        {
            // Arrange
            var log = XLog.Create(config =>
            {
                config.LogDirectory = _testLogDirectory;
                config.ApplicationName = "TestApp";
            });
            var message = "Test async debug message";

            // Act
            await log.DebugAsync(message);

            // Assert
            var logFiles = Directory.GetFiles(Path.Combine(_testLogDirectory, "TestApp"), "*.html");
            logFiles.Should().HaveCount(1);
            
            var logContent = File.ReadAllText(logFiles[0]);
            logContent.Should().Contain(message);
            logContent.Should().Contain("'type': 'debug'");
        }

        [Fact]
        public async Task ErrorAsync_WithValidMessage_ShouldCreateLogFile()
        {
            // Arrange
            var log = XLog.Create(config =>
            {
                config.LogDirectory = _testLogDirectory;
                config.ApplicationName = "TestApp";
            });
            var message = "Test async error message";

            // Act
            await log.ErrorAsync(message);

            // Assert
            var logFiles = Directory.GetFiles(Path.Combine(_testLogDirectory, "TestApp"), "*.html");
            logFiles.Should().HaveCount(1);
            
            var logContent = File.ReadAllText(logFiles[0]);
            logContent.Should().Contain(message);
            logContent.Should().Contain("'type': 'error'");
        }

        [Fact]
        public async Task ErrorAsync_WithException_ShouldLogExceptionMessage()
        {
            // Arrange
            var log = XLog.Create(config =>
            {
                config.LogDirectory = _testLogDirectory;
                config.ApplicationName = "TestApp";
            });
            var exception = new InvalidOperationException("Test async exception message");

            // Act
            await log.ErrorAsync(exception);

            // Assert
            var logFiles = Directory.GetFiles(Path.Combine(_testLogDirectory, "TestApp"), "*.html");
            logFiles.Should().HaveCount(1);
            
            var logContent = File.ReadAllText(logFiles[0]);
            logContent.Should().Contain("Test async exception message");
            logContent.Should().Contain("'type': 'error'");
        }

        [Fact]
        public async Task WarnAsync_WithValidMessage_ShouldCreateLogFile()
        {
            // Arrange
            var log = XLog.Create(config =>
            {
                config.LogDirectory = _testLogDirectory;
                config.ApplicationName = "TestApp";
            });
            var message = "Test async warning message";

            // Act
            await log.WarnAsync(message);

            // Assert
            var logFiles = Directory.GetFiles(Path.Combine(_testLogDirectory, "TestApp"), "*.html");
            logFiles.Should().HaveCount(1);
            
            var logContent = File.ReadAllText(logFiles[0]);
            logContent.Should().Contain(message);
            logContent.Should().Contain("'type': 'warning'");
        }

        [Fact]
        public async Task AsyncMethods_WithCancellationToken_ShouldRespectCancellation()
        {
            // Arrange
            var log = XLog.Create(config =>
            {
                config.LogDirectory = _testLogDirectory;
                config.ApplicationName = "TestApp";
            });
            var cts = new CancellationTokenSource();
            cts.Cancel(); // Cancel immediately

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() => 
                log.InfoAsync("Test message", cts.Token));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void LogMethods_WithNullOrEmptyMessage_ShouldNotThrow(string message)
        {
            // Arrange
            var log = XLog.Create(config =>
            {
                config.LogDirectory = _testLogDirectory;
                config.ApplicationName = "TestApp";
            });

            // Act & Assert - Should not throw exceptions
            var act1 = () => log.Info(message);
            var act2 = () => log.Debug(message);
            var act3 = () => log.Error(message);
            var act4 = () => log.Warn(message);

            act1.Should().NotThrow();
            act2.Should().NotThrow();
            act3.Should().NotThrow();
            act4.Should().NotThrow();
        }

        [Fact]
        public void MultipleLogCalls_ShouldAppendToSameFile()
        {
            // Arrange
            var log = XLog.Create(config =>
            {
                config.LogDirectory = _testLogDirectory;
                config.ApplicationName = "TestApp";
            });

            // Act
            log.Info("First message");
            log.Debug("Second message");
            log.Error("Third message");

            // Assert
            var logFiles = Directory.GetFiles(Path.Combine(_testLogDirectory, "TestApp"), "*.html");
            logFiles.Should().HaveCount(1);
            
            var logContent = File.ReadAllText(logFiles[0]);
            logContent.Should().Contain("First message");
            logContent.Should().Contain("Second message");
            logContent.Should().Contain("Third message");
        }
    }
}