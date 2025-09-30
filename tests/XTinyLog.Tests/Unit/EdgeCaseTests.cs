using System;
using System.IO;
using System.Text;
using FluentAssertions;
using Xunit;

namespace XTinyLog.Tests.Unit
{
    /// <summary>
    /// Tests for edge cases and error scenarios
    /// </summary>
    public class EdgeCaseTests : IDisposable
    {
        private readonly string _testLogDirectory;

        public EdgeCaseTests()
        {
            _testLogDirectory = Path.Combine(Path.GetTempPath(), "XTinyLogTests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testLogDirectory);
        }

        public void Dispose()
        {
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
        public void LogWithVeryLongMessage_ShouldHandleGracefully()
        {
            // Arrange
            var log = XLog.Create(config =>
            {
                config.LogDirectory = _testLogDirectory;
                config.ApplicationName = "LongMessageTest";
            });

            var longMessage = new string('A', 10000); // 10KB message

            // Act & Assert - Should not throw
            var act = () => log.Info(longMessage);
            act.Should().NotThrow();

            // Verify the message was logged
            var logFiles = Directory.GetFiles(Path.Combine(_testLogDirectory, "LongMessageTest"), "*.html");
            logFiles.Should().HaveCount(1);
            
            var logContent = File.ReadAllText(logFiles[0]);
            logContent.Should().Contain(longMessage);
        }

        [Fact]
        public void LogWithSpecialCharacters_ShouldHandleCorrectly()
        {
            // Arrange
            var log = XLog.Create(config =>
            {
                config.LogDirectory = _testLogDirectory;
                config.ApplicationName = "SpecialCharsTest";
            });

            var specialMessage = "Test with special chars: <>&\"'{}[]()@#$%^&*+=|\\:;?/~`";

            // Act
            log.Info(specialMessage);

            // Assert
            var logFiles = Directory.GetFiles(Path.Combine(_testLogDirectory, "SpecialCharsTest"), "*.html");
            logFiles.Should().HaveCount(1);
            
            var logContent = File.ReadAllText(logFiles[0]);
            logContent.Should().Contain(specialMessage);
        }

        [Fact]
        public void LogWithUnicodeCharacters_ShouldHandleCorrectly()
        {
            // Arrange
            var log = XLog.Create(config =>
            {
                config.LogDirectory = _testLogDirectory;
                config.ApplicationName = "UnicodeTest";
            });

            var unicodeMessage = "Unicode test: 你好世界 🌍 Здравствуй мир ñáéíóú";

            // Act
            log.Info(unicodeMessage);

            // Assert
            var logFiles = Directory.GetFiles(Path.Combine(_testLogDirectory, "UnicodeTest"), "*.html");
            logFiles.Should().HaveCount(1);
            
            var logContent = File.ReadAllText(logFiles[0], Encoding.UTF8);
            logContent.Should().Contain(unicodeMessage);
        }

        [Fact]
        public void LogWithNewlinesAndTabs_ShouldHandleCorrectly()
        {
            // Arrange
            var log = XLog.Create(config =>
            {
                config.LogDirectory = _testLogDirectory;
                config.ApplicationName = "NewlineTest";
            });

            var messageWithNewlines = "Line 1\nLine 2\r\nLine 3\tTabbed content";

            // Act
            log.Info(messageWithNewlines);

            // Assert
            var logFiles = Directory.GetFiles(Path.Combine(_testLogDirectory, "NewlineTest"), "*.html");
            logFiles.Should().HaveCount(1);
            
            var logContent = File.ReadAllText(logFiles[0]);
            logContent.Should().Contain("Line 1");
            logContent.Should().Contain("Line 2");
            logContent.Should().Contain("Line 3");
            logContent.Should().Contain("Tabbed content");
        }

        [Fact]
        public void FileNameWithDynamicContent_ShouldUpdateCorrectly()
        {
            // Arrange
            var counter = 0;
            var log = XLog.Create(config =>
            {
                config.LogDirectory = _testLogDirectory;
                config.ApplicationName = "DynamicFileTest";
                config.FileName = () => $"dynamic_{++counter}";
                config.MaxFileSize = 1; // Force file rotation
            });

            // Act
            log.Info("First message");
            log.Info("Second message"); // Should create new file due to small MaxFileSize

            // Assert
            var logFiles = Directory.GetFiles(Path.Combine(_testLogDirectory, "DynamicFileTest"), "*.html");
            logFiles.Should().HaveCountGreaterThan(1, "Multiple files should be created due to file rotation");
        }

        [Fact]
        public void VerySmallMaxFileSize_ShouldCreateMultipleFiles()
        {
            // Arrange
            var log = XLog.Create(config =>
            {
                config.LogDirectory = _testLogDirectory;
                config.ApplicationName = "SmallFileTest";
                config.MaxFileSize = 1; // 1KB - very small
            });

            // Act
            for (int i = 0; i < 10; i++)
            {
                log.Info($"Message {i} with some content to make it larger than 1KB when combined with HTML structure");
            }

            // Assert
            var logFiles = Directory.GetFiles(Path.Combine(_testLogDirectory, "SmallFileTest"), "*.html");
            logFiles.Should().HaveCountGreaterThan(1, "Multiple files should be created due to small MaxFileSize");

            // Check that archive directory exists
            var archiveDir = Path.Combine(_testLogDirectory, "SmallFileTest", "Archive");
            if (Directory.Exists(archiveDir))
            {
                var archivedFiles = Directory.GetFiles(archiveDir, "*.html");
                archivedFiles.Should().NotBeEmpty("Some files should be archived");
            }
        }

        [Fact]
        public void LogDirectoryWithLongPath_ShouldHandleGracefully()
        {
            // Arrange - Create a very long path (but within Windows limits)
            var longPath = _testLogDirectory;
            for (int i = 0; i < 5; i++)
            {
                longPath = Path.Combine(longPath, "VeryLongDirectoryNameThatMakesThePathLonger");
            }

            var log = XLog.Create(config =>
            {
                config.LogDirectory = longPath;
                config.ApplicationName = "LongPathTest";
            });

            // Act & Assert - Should handle gracefully
            var act = () => log.Info("Test message with long path");
            
            if (longPath.Length < 260) // Within Windows path limits
            {
                act.Should().NotThrow();
            }
            else
            {
                // On systems with path length limitations, this might throw or use fallback
                // The important thing is that it doesn't crash the application
                try
                {
                    act();
                }
                catch (Exception ex)
                {
                    // Should be a handled exception, not an unhandled crash
                    (ex is InvalidOperationException || 
                     ex is DirectoryNotFoundException || 
                     ex is PathTooLongException).Should().BeTrue();
                }
            }
        }

        [Fact]
        public void ApplicationNameWithInvalidCharacters_ShouldBeValidatedInConfiguration()
        {
            // Act & Assert
            var act = () => XLog.Create(config =>
            {
                config.LogDirectory = _testLogDirectory;
                config.ApplicationName = "Invalid<App>Name"; // Contains invalid characters
            });

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*Configuration validation failed*");
        }

        [Fact]
        public void FileNameFunctionThrowingException_ShouldBeHandledGracefully()
        {
            // Act & Assert
            var act = () => XLog.Create(config =>
            {
                config.LogDirectory = _testLogDirectory;
                config.ApplicationName = "ExceptionTest";
                config.FileName = () => throw new InvalidOperationException("Test exception in filename");
            });

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*Configuration validation failed*");
        }

        [Fact]
        public void LogToReadOnlyDirectory_ShouldUseInternalErrorHandling()
        {
            // This test is platform-specific and might not work on all systems
            // We'll create a scenario where we can't write to the directory
            
            // Arrange
            var readOnlyDir = Path.Combine(_testLogDirectory, "ReadOnlyTest");
            Directory.CreateDirectory(readOnlyDir);
            
            try
            {
                // Try to make directory read-only (this might not work on all systems)
                var dirInfo = new DirectoryInfo(readOnlyDir);
                dirInfo.Attributes |= FileAttributes.ReadOnly;

                var log = XLog.Create(config =>
                {
                    config.LogDirectory = readOnlyDir;
                    config.ApplicationName = "ReadOnlyTest";
                });

                // Act & Assert - Should not crash, might use fallback logging
                var act = () => log.Info("Test message to read-only directory");
                act.Should().NotThrow("Should handle read-only directory gracefully");
            }
            finally
            {
                // Clean up - remove read-only attribute
                try
                {
                    var dirInfo = new DirectoryInfo(readOnlyDir);
                    dirInfo.Attributes &= ~FileAttributes.ReadOnly;
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }

        [Fact]
        public void MultipleInstancesWithSameConfiguration_ShouldNotInterfere()
        {
            // Arrange
            var config1 = XLog.Create(config =>
            {
                config.LogDirectory = _testLogDirectory;
                config.ApplicationName = "SharedConfigTest";
            });

            var config2 = XLog.Create(config =>
            {
                config.LogDirectory = _testLogDirectory;
                config.ApplicationName = "SharedConfigTest"; // Same as config1
            });

            // Act
            config1.Info("Message from instance 1");
            config2.Info("Message from instance 2");

            // Assert
            var logFiles = Directory.GetFiles(Path.Combine(_testLogDirectory, "SharedConfigTest"), "*.html");
            logFiles.Should().HaveCount(1, "Both instances should write to the same file");
            
            var logContent = File.ReadAllText(logFiles[0]);
            logContent.Should().Contain("Message from instance 1");
            logContent.Should().Contain("Message from instance 2");
        }

        [Fact]
        public void LogAfterDirectoryDeleted_ShouldRecreateDirectory()
        {
            // Arrange
            var log = XLog.Create(config =>
            {
                config.LogDirectory = _testLogDirectory;
                config.ApplicationName = "RecreateTest";
            });

            // Log first message to create directory
            log.Info("First message");

            // Delete the directory
            var appDir = Path.Combine(_testLogDirectory, "RecreateTest");
            Directory.Delete(appDir, true);

            // Act - Log after directory is deleted
            var act = () => log.Info("Second message after directory deleted");

            // Assert - Should recreate directory and continue logging
            act.Should().NotThrow("Should recreate directory and continue logging");

            Directory.Exists(appDir).Should().BeTrue("Directory should be recreated");
            var logFiles = Directory.GetFiles(appDir, "*.html");
            logFiles.Should().HaveCount(1);
            
            var logContent = File.ReadAllText(logFiles[0]);
            logContent.Should().Contain("Second message after directory deleted");
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(1000)]
        public void DifferentMaxFileSizes_ShouldBehavePredictably(long maxFileSize)
        {
            // Arrange
            var log = XLog.Create(config =>
            {
                config.LogDirectory = _testLogDirectory;
                config.ApplicationName = $"MaxSizeTest_{maxFileSize}";
                config.MaxFileSize = maxFileSize;
            });

            // Act - Log multiple messages
            for (int i = 0; i < 20; i++)
            {
                log.Info($"Test message {i} with some additional content to reach the file size limit");
            }

            // Assert
            var logDir = Path.Combine(_testLogDirectory, $"MaxSizeTest_{maxFileSize}");
            var logFiles = Directory.GetFiles(logDir, "*.html");
            
            if (maxFileSize < 10) // Very small files should create multiple files
            {
                logFiles.Should().HaveCountGreaterThan(1, "Small max file size should create multiple files");
            }
            else
            {
                logFiles.Should().HaveCountGreaterOrEqualTo(1, "Should create at least one log file");
            }

            // Verify all messages are logged somewhere
            var totalContent = string.Empty;
            foreach (var file in logFiles)
            {
                totalContent += File.ReadAllText(file);
            }

            // Check for archived files too
            var archiveDir = Path.Combine(logDir, "Archive");
            if (Directory.Exists(archiveDir))
            {
                var archivedFiles = Directory.GetFiles(archiveDir, "*.html");
                foreach (var file in archivedFiles)
                {
                    totalContent += File.ReadAllText(file);
                }
            }

            for (int i = 0; i < 20; i++)
            {
                totalContent.Should().Contain($"Test message {i}", $"Message {i} should be logged somewhere");
            }
        }
    }
}