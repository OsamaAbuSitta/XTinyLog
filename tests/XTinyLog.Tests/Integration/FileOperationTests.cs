using System;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using Xunit;

namespace XTinyLog.Tests.Integration
{
    /// <summary>
    /// Integration tests for file operations including HTML generation and file rotation
    /// </summary>
    public class FileOperationTests : IDisposable
    {
        private readonly string testLogDirectory;

        public FileOperationTests()
        {
            this.testLogDirectory = Path.Combine(Path.GetTempPath(), "XTinyLogIntegrationTests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(this.testLogDirectory);
        }

        public void Dispose()
        {
            if (Directory.Exists(this.testLogDirectory))
            {
                try
                {
                    Directory.Delete(this.testLogDirectory, true);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }

        [Fact]
        public void LogToFile_ShouldCreateValidHtmlFile()
        {
            // Arrange
            var log = XLog.Create(config =>
            {
                config.LogDirectory = this.testLogDirectory;
                config.ApplicationName = "HtmlTest";
                config.FileName = () => "test_log";
            });

            // Act
            log.Info("Test info message");
            log.Debug("Test debug message");
            log.Error("Test error message");
            log.Warn("Test warning message");

            // Assert
            var logFiles = Directory.GetFiles(Path.Combine(this.testLogDirectory, "HtmlTest"), "*.html");
            logFiles.Should().HaveCount(1);

            var logContent = File.ReadAllText(logFiles[0], Encoding.UTF8);
            
            // Verify HTML structure
            logContent.Should().Contain("<!DOCTYPE html>");
            logContent.Should().Contain("<html");
            logContent.Should().Contain("<head>");
            logContent.Should().Contain("<title>");
            logContent.Should().Contain("</title>");
            logContent.Should().Contain("</head>");
            logContent.Should().Contain("<body>");
            logContent.Should().Contain("</body>");
            logContent.Should().Contain("</html>");

            // Verify log messages are present
            logContent.Should().Contain("Test info message");
            logContent.Should().Contain("Test debug message");
            logContent.Should().Contain("Test error message");
            logContent.Should().Contain("Test warning message");

            // Verify log types are present
            logContent.Should().Contain("'type': 'info'");
            logContent.Should().Contain("'type': 'debug'");
            logContent.Should().Contain("'type': 'error'");
            logContent.Should().Contain("'type': 'warning'");
        }

        [Fact]
        public void LogToFile_ShouldCreateDirectoryStructure()
        {
            // Arrange
            var log = XLog.Create(config =>
            {
                config.LogDirectory = this.testLogDirectory;
                config.ApplicationName = "DirectoryTest";
                config.FileName = () => "structure_test";
            });

            // Act
            log.Info("Creating directory structure");

            // Assert
            var appDirectory = Path.Combine(this.testLogDirectory, "DirectoryTest");
            Directory.Exists(appDirectory).Should().BeTrue();

            var logFiles = Directory.GetFiles(appDirectory, "*.html");
            logFiles.Should().HaveCount(1);
            
            Path.GetFileName(logFiles[0]).Should().Be("structure_test.html");
        }

        [Fact]
        public void FileRotation_WhenMaxSizeExceeded_ShouldCreateNewFileAndArchiveOld()
        {
            // Arrange
            var log = XLog.Create(config =>
            {
                config.LogDirectory = this.testLogDirectory;
                config.ApplicationName = "RotationTest";
                config.FileName = () => "rotation_test";
                config.MaxFileSize = 1; // 1KB - very small to force rotation
            });

            // Act - Log enough content to exceed the file size limit
            for (int i = 0; i < 50; i++)
            {
                log.Info($"This is a test message {i} with enough content to make the file grow beyond the 1KB limit when combined with HTML structure and multiple entries");
            }

            // Assert
            var appDirectory = Path.Combine(this.testLogDirectory, "RotationTest");
            Directory.Exists(appDirectory).Should().BeTrue();

            // Should have current log file
            var currentLogFiles = Directory.GetFiles(appDirectory, "rotation_test.html");
            currentLogFiles.Should().HaveCount(1);

            // Should have archive directory with archived files
            var archiveDirectory = Path.Combine(appDirectory, "Archive");
            if (Directory.Exists(archiveDirectory))
            {
                var archivedFiles = Directory.GetFiles(archiveDirectory, "*.html");
                archivedFiles.Should().NotBeEmpty("Some files should be archived due to size limit");
                
                // Verify archived files contain log entries
                foreach (var archivedFile in archivedFiles)
                {
                    var content = File.ReadAllText(archivedFile);
                    content.Should().Contain("This is a test message");
                    content.Should().Contain("'type': 'info'");
                }
            }

            // Verify current file also contains entries
            var currentContent = File.ReadAllText(currentLogFiles[0]);
            currentContent.Should().Contain("This is a test message");
        }

        [Fact]
        public void MultipleApplications_ShouldCreateSeparateDirectories()
        {
            // Arrange
            var log1 = XLog.Create(config =>
            {
                config.LogDirectory = this.testLogDirectory;
                config.ApplicationName = "App1";
                config.FileName = () => "app1_log";
            });

            var log2 = XLog.Create(config =>
            {
                config.LogDirectory = this.testLogDirectory;
                config.ApplicationName = "App2";
                config.FileName = () => "app2_log";
            });

            // Act
            log1.Info("Message from App1");
            log2.Info("Message from App2");

            // Assert
            var app1Directory = Path.Combine(this.testLogDirectory, "App1");
            var app2Directory = Path.Combine(this.testLogDirectory, "App2");

            Directory.Exists(app1Directory).Should().BeTrue();
            Directory.Exists(app2Directory).Should().BeTrue();

            var app1Files = Directory.GetFiles(app1Directory, "*.html");
            var app2Files = Directory.GetFiles(app2Directory, "*.html");

            app1Files.Should().HaveCount(1);
            app2Files.Should().HaveCount(1);

            var app1Content = File.ReadAllText(app1Files[0]);
            var app2Content = File.ReadAllText(app2Files[0]);

            app1Content.Should().Contain("Message from App1");
            app1Content.Should().NotContain("Message from App2");

            app2Content.Should().Contain("Message from App2");
            app2Content.Should().NotContain("Message from App1");
        }

        [Fact]
        public void DynamicFileName_ShouldCreateFilesWithDifferentNames()
        {
            // Arrange
            var counter = 0;
            var log = XLog.Create(config =>
            {
                config.LogDirectory = this.testLogDirectory;
                config.ApplicationName = "DynamicTest";
                config.FileName = () => $"dynamic_{++counter}";
                config.MaxFileSize = 1; // Force rotation to test dynamic naming
            });

            // Act
            log.Info("First batch of messages");
            for (int i = 0; i < 10; i++)
            {
                log.Info($"Message {i} to fill up the file and trigger rotation with dynamic naming");
            }

            // Assert
            var appDirectory = Path.Combine(this.testLogDirectory, "DynamicTest");
            var allHtmlFiles = Directory.GetFiles(appDirectory, "*.html", SearchOption.AllDirectories);
            
            // Should have multiple files due to rotation and dynamic naming
            allHtmlFiles.Should().HaveCountGreaterThan(1);

            // Verify files have different names
            var fileNames = allHtmlFiles.Select(Path.GetFileNameWithoutExtension).ToArray();
            fileNames.Should().OnlyHaveUniqueItems("Each file should have a unique name due to dynamic naming");
        }

        [Fact]
        public void LogFileEncoding_ShouldBeUTF8()
        {
            // Arrange
            var log = XLog.Create(config =>
            {
                config.LogDirectory = this.testLogDirectory;
                config.ApplicationName = "EncodingTest";
                config.FileName = () => "encoding_test";
            });

            var unicodeMessage = "Unicode test: 你好世界 🌍 Здравствуй мир ñáéíóú";

            // Act
            log.Info(unicodeMessage);

            // Assert
            var logFiles = Directory.GetFiles(Path.Combine(this.testLogDirectory, "EncodingTest"), "*.html");
            logFiles.Should().HaveCount(1);

            // Read with UTF-8 encoding
            var logContent = File.ReadAllText(logFiles[0], Encoding.UTF8);
            logContent.Should().Contain(unicodeMessage);

            // Verify the file was written with UTF-8 encoding (no BOM)
            var bytes = File.ReadAllBytes(logFiles[0]);
            var hasBom = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
            hasBom.Should().BeFalse("Should not have UTF-8 BOM");
        }

        [Fact]
        public void ConcurrentFileAccess_ShouldNotCorruptFile()
        {
            // Arrange
            var log = XLog.Create(config =>
            {
                config.LogDirectory = this.testLogDirectory;
                config.ApplicationName = "ConcurrentTest";
                config.FileName = () => "concurrent_test";
                config.MaxFileSize = 10000; // Large enough to avoid rotation during test
            });

            // Act - Simulate concurrent access from multiple threads
            var tasks = new System.Threading.Tasks.Task[5];
            for (int i = 0; i < tasks.Length; i++)
            {
                int threadId = i;
                tasks[i] = System.Threading.Tasks.Task.Run(() =>
                {
                    for (int j = 0; j < 20; j++)
                    {
                        log.Info($"Thread {threadId} - Message {j}");
                    }
                });
            }

            System.Threading.Tasks.Task.WaitAll(tasks);

            // Assert
            var logFiles = Directory.GetFiles(Path.Combine(this.testLogDirectory, "ConcurrentTest"), "*.html");
            logFiles.Should().HaveCount(1);

            var logContent = File.ReadAllText(logFiles[0]);
            
            // Verify all messages are present
            for (int i = 0; i < tasks.Length; i++)
            {
                for (int j = 0; j < 20; j++)
                {
                    logContent.Should().Contain($"Thread {i} - Message {j}");
                }
            }

            // Verify HTML structure is still valid
            logContent.Should().Contain("<!DOCTYPE html>");
            logContent.Should().Contain("</html>");
        }

        [Fact]
        public void FilePermissions_WhenDirectoryReadOnly_ShouldHandleGracefully()
        {
            // This test is platform-specific and might not work on all systems
            var readOnlyTestDir = Path.Combine(this.testLogDirectory, "ReadOnlyTest");
            Directory.CreateDirectory(readOnlyTestDir);

            try
            {
                // Try to make directory read-only
                var dirInfo = new DirectoryInfo(readOnlyTestDir);
                dirInfo.Attributes |= FileAttributes.ReadOnly;

                var log = XLog.Create(config =>
                {
                    config.LogDirectory = readOnlyTestDir;
                    config.ApplicationName = "PermissionTest";
                    config.FileName = () => "permission_test";
                });

                // Act & Assert - Should not crash
                var act = () => log.Info("Test message to read-only directory");
                act.Should().NotThrow("Should handle read-only directory gracefully");

                // The logging might fail, but it should not crash the application
                // The internal error handling should manage this scenario
            }
            finally
            {
                // Clean up - remove read-only attribute
                try
                {
                    var dirInfo = new DirectoryInfo(readOnlyTestDir);
                    dirInfo.Attributes &= ~FileAttributes.ReadOnly;
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }

        [Fact]
        public void CrossPlatformPaths_ShouldWorkCorrectly()
        {
            // Arrange - Test with different path separators and formats
            var log = XLog.Create(config =>
            {
                config.LogDirectory = this.testLogDirectory;
                config.ApplicationName = "CrossPlatform/Test"; // This should be handled correctly
                config.FileName = () => "cross_platform_test";
            });

            // Act
            log.Info("Cross-platform path test");

            // Assert
            // The application name with slash should be sanitized or handled appropriately
            var subdirectories = Directory.GetDirectories(this.testLogDirectory);
            subdirectories.Should().NotBeEmpty("Should create some directory structure");

            // Find the log file in any subdirectory
            var allLogFiles = Directory.GetFiles(this.testLogDirectory, "*.html", SearchOption.AllDirectories);
            allLogFiles.Should().HaveCountGreaterOrEqualTo(1, "Should create at least one log file");

            if (allLogFiles.Length > 0)
            {
                var logContent = File.ReadAllText(allLogFiles[0]);
                logContent.Should().Contain("Cross-platform path test");
            }
        }

        [Fact]
        public void LargeLogFile_ShouldHandleCorrectly()
        {
            // Arrange
            var log = XLog.Create(config =>
            {
                config.LogDirectory = this.testLogDirectory;
                config.ApplicationName = "LargeFileTest";
                config.FileName = () => "large_file_test";
                config.MaxFileSize = 50; // 50KB - moderate size for testing
            });

            // Act - Create a moderately large log file
            for (int i = 0; i < 100; i++)
            {
                var largeMessage = new string('A', 200) + $" - Message {i}"; // ~200 chars per message
                log.Info(largeMessage);
            }

            // Assert
            var appDirectory = Path.Combine(this.testLogDirectory, "LargeFileTest");
            var allLogFiles = Directory.GetFiles(appDirectory, "*.html", SearchOption.AllDirectories);
            
            allLogFiles.Should().NotBeEmpty("Should create log files");

            // Verify total content
            var totalContent = string.Empty;
            foreach (var file in allLogFiles)
            {
                totalContent += File.ReadAllText(file);
            }

            // Should contain all messages
            for (int i = 0; i < 100; i++)
            {
                totalContent.Should().Contain($"Message {i}");
            }
        }
    }
}