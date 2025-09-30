using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace XTinyLog.Tests.EndToEnd
{
    /// <summary>
    /// Tests that verify cross-platform compatibility across Windows, Linux, and macOS.
    /// </summary>
    public sealed class CrossPlatformTests : IDisposable
    {
        private readonly string _testDirectory;
        private readonly string _testAppName;

        public CrossPlatformTests()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), "XTinyLog_CrossPlatform_Tests", Guid.NewGuid().ToString());
            _testAppName = "CrossPlatformTestApp";
            Directory.CreateDirectory(_testDirectory);
        }

        [Fact]
        public void FilePathHandling_ShouldWorkOnCurrentPlatform()
        {
            // Arrange & Act
            var logger = XLog.Create(config =>
            {
                config.LogDirectory = _testDirectory;
                config.ApplicationName = _testAppName;
                config.FileName = () => "cross-platform-test";
            });

            logger.Info("Testing cross-platform file path handling");

            // Assert - Verify file was created with correct path separators
            var htmlFiles = Directory.GetFiles(_testDirectory, "*.html", SearchOption.AllDirectories);
            htmlFiles.Should().NotBeEmpty("HTML log file should be created on current platform");

            var htmlFile = htmlFiles[0];
            File.Exists(htmlFile).Should().BeTrue("Log file should exist on current platform");

            // Verify path uses correct separators for current platform
            var expectedSeparator = Path.DirectorySeparatorChar;
            htmlFile.Should().Contain(expectedSeparator.ToString(), "File path should use platform-appropriate separators");
        }

        [Fact]
        public void DirectoryCreation_ShouldHandlePlatformSpecificPaths()
        {
            // Test with various path formats that should work cross-platform
            var testPaths = new[]
            {
                Path.Combine(_testDirectory, "subdir1"),
                Path.Combine(_testDirectory, "sub", "dir2"),
                Path.Combine(_testDirectory, "test_dir_with_underscores"),
                Path.Combine(_testDirectory, "test-dir-with-hyphens")
            };

            foreach (var testPath in testPaths)
            {
                // Arrange & Act
                var logger = XLog.Create(config =>
                {
                    config.LogDirectory = testPath;
                    config.ApplicationName = "TestApp";
                    config.FileName = () => "path-test";
                });

                logger.Info($"Testing path: {testPath}");

                // Assert
                Directory.Exists(testPath).Should().BeTrue($"Directory should be created: {testPath}");
                var htmlFiles = Directory.GetFiles(testPath, "*.html", SearchOption.AllDirectories);
                htmlFiles.Should().NotBeEmpty($"HTML file should be created in: {testPath}");
            }
        }

        [Fact]
        public async Task HtmlOutput_ShouldBeValidAcrossPlatforms()
        {
            // Arrange
            var logger = XLog.Create(config =>
            {
                config.LogDirectory = _testDirectory;
                config.ApplicationName = _testAppName;
                config.FileName = () => "html-validation-test";
            });

            // Act - Log various types of content that might be platform-sensitive
            logger.Info("Testing HTML output validation");
            logger.Info($"Current platform: {RuntimeInformation.OSDescription}");
            logger.Info($"Framework: {RuntimeInformation.FrameworkDescription}");
            logger.Info($"Architecture: {RuntimeInformation.OSArchitecture}");

            // Test with special characters that might be handled differently
            logger.Info("Testing special characters: àáâãäåæçèéêë");
            logger.Info("Testing symbols: !@#$%^&*()_+-=[]{}|;':\",./<>?");

            await Task.Delay(100);

            // Assert
            var htmlFiles = Directory.GetFiles(_testDirectory, "*.html", SearchOption.AllDirectories);
            var htmlContent = File.ReadAllText(htmlFiles[0]);

            // Verify HTML structure is valid
            htmlContent.Should().Contain("<!DOCTYPE html>", "Should have valid HTML doctype");
            htmlContent.Should().Contain("<html", "Should have html tag");
            htmlContent.Should().Contain("</html>", "Should have closing html tag");
            htmlContent.Should().Contain("<head>", "Should have head section");
            htmlContent.Should().Contain("</head>", "Should have closing head tag");
            htmlContent.Should().Contain("<body>", "Should have body section");
            htmlContent.Should().Contain("</body>", "Should have closing body tag");

            // Verify content is properly encoded
            htmlContent.Should().Contain("Testing HTML output validation");
            htmlContent.Should().Contain(RuntimeInformation.OSDescription);
            htmlContent.Should().Contain("Testing special characters");
            htmlContent.Should().Contain("Testing symbols");

            // Verify no platform-specific line ending issues
            htmlContent.Should().NotContain("\r\r\n", "Should not have double carriage returns");
        }

        [Fact]
        public void PlatformInfo_ShouldBeDetectedCorrectly()
        {
            // This test verifies that we can detect the current platform correctly
            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            var isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            var isMacOS = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

            // At least one platform should be detected
            (isWindows || isLinux || isMacOS).Should().BeTrue("Should detect at least one platform");

            // Only one platform should be detected
            var platformCount = (isWindows ? 1 : 0) + (isLinux ? 1 : 0) + (isMacOS ? 1 : 0);
            platformCount.Should().Be(1, "Should detect exactly one platform");

            // Log the detected platform for verification
            var logger = XLog.Create(config =>
            {
                config.LogDirectory = _testDirectory;
                config.ApplicationName = _testAppName;
                config.FileName = () => "platform-detection-test";
            });

            if (isWindows)
            {
                logger.Info("Detected platform: Windows");
            }
            else if (isLinux)
            {
                logger.Info("Detected platform: Linux");
            }
            else if (isMacOS)
            {
                logger.Info("Detected platform: macOS");
            }

            logger.Info($"OS Description: {RuntimeInformation.OSDescription}");
            logger.Info($"Framework: {RuntimeInformation.FrameworkDescription}");
        }

        [Fact]
        public void FileSystemCaseSensitivity_ShouldBeHandledCorrectly()
        {
            // Test file system case sensitivity handling
            var logger1 = XLog.Create(config =>
            {
                config.LogDirectory = _testDirectory;
                config.ApplicationName = "TestApp";
                config.FileName = () => "case-test";
            });

            var logger2 = XLog.Create(config =>
            {
                config.LogDirectory = _testDirectory;
                config.ApplicationName = "testapp"; // Different case
                config.FileName = () => "case-test";
            });

            logger1.Info("Message from TestApp");
            logger2.Info("Message from testapp");

            // On case-sensitive file systems (Linux/macOS), these should create separate directories
            // On case-insensitive file systems (Windows), they might share the same directory
            var htmlFiles = Directory.GetFiles(_testDirectory, "*.html", SearchOption.AllDirectories);
            htmlFiles.Should().NotBeEmpty("Should create at least one HTML file");

            // The behavior will differ by platform, but the library should handle it gracefully
            foreach (var file in htmlFiles)
            {
                var content = File.ReadAllText(file);
                content.Should().Contain("Message from", "Should contain logged messages");
            }
        }

        [Fact]
        public void SpecialCharactersInPaths_ShouldBeHandledSafely()
        {
            // Test handling of special characters in file paths
            // Use characters that are valid on most platforms
            var safeSpecialChars = new[] { "_", "-", "." };

            foreach (var specialChar in safeSpecialChars)
            {
                var testDir = Path.Combine(_testDirectory, $"test{specialChar}dir");
                var logger = XLog.Create(config =>
                {
                    config.LogDirectory = testDir;
                    config.ApplicationName = $"App{specialChar}Name";
                    config.FileName = () => $"file{specialChar}name";
                });

                logger.Info($"Testing special character: {specialChar}");

                // Verify directory and file creation
                Directory.Exists(testDir).Should().BeTrue($"Directory with '{specialChar}' should be created");
                var htmlFiles = Directory.GetFiles(testDir, "*.html", SearchOption.AllDirectories);
                htmlFiles.Should().NotBeEmpty($"HTML file with '{specialChar}' should be created");
            }
        }

        [Fact]
        public async Task AsyncOperations_ShouldWorkAcrossPlatforms()
        {
            // Test async operations on different platforms
            var logger = XLog.Create(config =>
            {
                config.LogDirectory = _testDirectory;
                config.ApplicationName = _testAppName;
                config.FileName = () => "async-cross-platform-test";
            });

            // Perform multiple async operations concurrently
            var tasks = new Task[10];
            for (int i = 0; i < 10; i++)
            {
                int taskId = i;
                tasks[i] = Task.Run(async () =>
                {
                    await logger.InfoAsync($"Async message {taskId} on {RuntimeInformation.OSDescription}");
                });
            }

            await Task.WhenAll(tasks);
            await Task.Delay(200); // Allow file operations to complete

            // Verify results
            var htmlFiles = Directory.GetFiles(_testDirectory, "*.html", SearchOption.AllDirectories);
            htmlFiles.Should().NotBeEmpty("Async operations should create HTML files");

            var htmlContent = File.ReadAllText(htmlFiles[0]);
            htmlContent.Should().Contain("Async message", "Should contain async messages");
            htmlContent.Should().Contain(RuntimeInformation.OSDescription, "Should contain platform information");
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
                // Ignore cleanup errors in tests - some platforms may have different cleanup behavior
            }
        }
    }
}