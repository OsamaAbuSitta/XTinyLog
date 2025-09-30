using System;
using System.IO;
using FluentAssertions;
using Xunit;

namespace XTinyLog.Tests.Unit
{
    /// <summary>
    /// Unit tests for configuration validation functionality through XLog.Create
    /// </summary>
    public class ConfigurationValidationTests : IDisposable
    {
        private readonly string testLogDirectory;

        public ConfigurationValidationTests()
        {
            this.testLogDirectory = Path.Combine(Path.GetTempPath(), "XTinyLogTests", Guid.NewGuid().ToString());
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
        public void XLogCreate_WithValidConfiguration_ShouldNotThrow()
        {
            // Act & Assert - Should not throw
            var act = () => XLog.Create(config =>
            {
                config.LogDirectory = this.testLogDirectory;
                config.ApplicationName = "TestApp";
                config.MaxFileSize = 1000;
                config.FileName = () => "test_log";
            });

            act.Should().NotThrow();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void XLogCreate_WithInvalidLogDirectory_ShouldThrowInvalidOperationException(string logDirectory)
        {
            // Act & Assert
            var act = () => XLog.Create(config =>
            {
                config.LogDirectory = logDirectory;
                config.ApplicationName = "TestApp";
                config.MaxFileSize = 1000;
                config.FileName = () => "test_log";
            });

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*Configuration validation failed*");
        }

        [Theory]
        [InlineData("C:\\Invalid<Path")]
        [InlineData("C:\\Invalid|Path")]
        [InlineData("C:\\Invalid?Path")]
        public void XLogCreate_WithInvalidLogDirectoryCharacters_ShouldThrowInvalidOperationException(string logDirectory)
        {
            // Act & Assert
            var act = () => XLog.Create(config =>
            {
                config.LogDirectory = logDirectory;
                config.ApplicationName = "TestApp";
                config.MaxFileSize = 1000;
                config.FileName = () => "test_log";
            });

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*Configuration validation failed*");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void XLogCreate_WithInvalidApplicationName_ShouldThrowInvalidOperationException(string applicationName)
        {
            // Act & Assert
            var act = () => XLog.Create(config =>
            {
                config.LogDirectory = this.testLogDirectory;
                config.ApplicationName = applicationName;
                config.MaxFileSize = 1000;
                config.FileName = () => "test_log";
            });

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*Configuration validation failed*");
        }

        [Theory]
        [InlineData("App<Name")]
        [InlineData("App|Name")]
        [InlineData("App?Name")]
        [InlineData("App*Name")]
        public void XLogCreate_WithInvalidApplicationNameCharacters_ShouldThrowInvalidOperationException(string applicationName)
        {
            // Act & Assert
            var act = () => XLog.Create(config =>
            {
                config.LogDirectory = this.testLogDirectory;
                config.ApplicationName = applicationName;
                config.MaxFileSize = 1000;
                config.FileName = () => "test_log";
            });

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*Configuration validation failed*");
        }

        [Fact]
        public void XLogCreate_WithTooLongApplicationName_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var longName = new string('A', 256); // Exceeds 255 character limit

            // Act & Assert
            var act = () => XLog.Create(config =>
            {
                config.LogDirectory = this.testLogDirectory;
                config.ApplicationName = longName;
                config.MaxFileSize = 1000;
                config.FileName = () => "test_log";
            });

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*Configuration validation failed*");
        }

        [Fact]
        public void XLogCreate_WithNullFileName_ShouldThrowInvalidOperationException()
        {
            // Act & Assert
            var act = () => XLog.Create(config =>
            {
                config.LogDirectory = this.testLogDirectory;
                config.ApplicationName = "TestApp";
                config.MaxFileSize = 1000;
                config.FileName = null;
            });

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*Configuration validation failed*");
        }

        [Fact]
        public void XLogCreate_WithFileNameReturningNull_ShouldThrowInvalidOperationException()
        {
            // Act & Assert
            var act = () => XLog.Create(config =>
            {
                config.LogDirectory = this.testLogDirectory;
                config.ApplicationName = "TestApp";
                config.MaxFileSize = 1000;
                config.FileName = () => null;
            });

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*Configuration validation failed*");
        }

        [Fact]
        public void XLogCreate_WithFileNameReturningEmpty_ShouldThrowInvalidOperationException()
        {
            // Act & Assert
            var act = () => XLog.Create(config =>
            {
                config.LogDirectory = this.testLogDirectory;
                config.ApplicationName = "TestApp";
                config.MaxFileSize = 1000;
                config.FileName = () => string.Empty;
            });

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*Configuration validation failed*");
        }

        [Theory]
        [InlineData("file<name")]
        [InlineData("file|name")]
        [InlineData("file?name")]
        [InlineData("file*name")]
        public void XLogCreate_WithInvalidFileNameCharacters_ShouldThrowInvalidOperationException(string fileName)
        {
            // Act & Assert
            var act = () => XLog.Create(config =>
            {
                config.LogDirectory = this.testLogDirectory;
                config.ApplicationName = "TestApp";
                config.MaxFileSize = 1000;
                config.FileName = () => fileName;
            });

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*Configuration validation failed*");
        }

        [Fact]
        public void XLogCreate_WithTooLongFileName_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var longFileName = new string('A', 256); // Exceeds 255 character limit

            // Act & Assert
            var act = () => XLog.Create(config =>
            {
                config.LogDirectory = this.testLogDirectory;
                config.ApplicationName = "TestApp";
                config.MaxFileSize = 1000;
                config.FileName = () => longFileName;
            });

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*Configuration validation failed*");
        }

        [Fact]
        public void XLogCreate_WithFileNameThrowingException_ShouldThrowInvalidOperationException()
        {
            // Act & Assert
            var act = () => XLog.Create(config =>
            {
                config.LogDirectory = this.testLogDirectory;
                config.ApplicationName = "TestApp";
                config.MaxFileSize = 1000;
                config.FileName = () => throw new InvalidOperationException("Test exception");
            });

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*Configuration validation failed*");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void XLogCreate_WithInvalidMaxFileSize_ShouldThrowInvalidOperationException(long maxFileSize)
        {
            // Act & Assert
            var act = () => XLog.Create(config =>
            {
                config.LogDirectory = this.testLogDirectory;
                config.ApplicationName = "TestApp";
                config.MaxFileSize = maxFileSize;
                config.FileName = () => "test_log";
            });

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*Configuration validation failed*");
        }

        [Fact]
        public void XLogCreate_WithTooLargeMaxFileSize_ShouldThrowInvalidOperationException()
        {
            // Act & Assert
            var act = () => XLog.Create(config =>
            {
                config.LogDirectory = this.testLogDirectory;
                config.ApplicationName = "TestApp";
                config.MaxFileSize = long.MaxValue / 1024 + 1; // Too large
                config.FileName = () => "test_log";
            });

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*Configuration validation failed*");
        }
    }
}