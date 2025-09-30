using System;
using System.IO;
using FluentAssertions;
using Xunit;

namespace XTinyLog.Tests.Unit
{
    /// <summary>
    /// Unit tests for LogConfigurationBuilder functionality
    /// </summary>
    public class LogConfigurationBuilderTests : IDisposable
    {
        private readonly string _testLogDirectory;

        public LogConfigurationBuilderTests()
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
        public void Constructor_ShouldCreateBuilderWithDefaultConfiguration()
        {
            // Act
            var builder = new LogConfigurationBuilder();

            // Assert
            builder.Should().NotBeNull();
        }

        [Fact]
        public void WithLogDirectory_WithValidPath_ShouldSetLogDirectory()
        {
            // Arrange
            var builder = new LogConfigurationBuilder();

            // Act
            var result = builder.WithLogDirectory(_testLogDirectory);

            // Assert
            result.Should().BeSameAs(builder, "Should return the same builder instance for chaining");
            
            var config = result.BuildWithoutValidation();
            config.LogDirectory.Should().Be(_testLogDirectory);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void WithLogDirectory_WithInvalidPath_ShouldThrowArgumentException(string logDirectory)
        {
            // Arrange
            var builder = new LogConfigurationBuilder();

            // Act & Assert
            var act = () => builder.WithLogDirectory(logDirectory);
            act.Should().Throw<ArgumentException>()
                .WithMessage("*Log directory cannot be null or empty*");
        }

        [Fact]
        public void WithApplicationName_WithValidName_ShouldSetApplicationName()
        {
            // Arrange
            var builder = new LogConfigurationBuilder();
            var appName = "TestApplication";

            // Act
            var result = builder.WithApplicationName(appName);

            // Assert
            result.Should().BeSameAs(builder, "Should return the same builder instance for chaining");
            
            var config = result.BuildWithoutValidation();
            config.ApplicationName.Should().Be(appName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void WithApplicationName_WithInvalidName_ShouldThrowArgumentException(string applicationName)
        {
            // Arrange
            var builder = new LogConfigurationBuilder();

            // Act & Assert
            var act = () => builder.WithApplicationName(applicationName);
            act.Should().Throw<ArgumentException>()
                .WithMessage("*Application name cannot be null or empty*");
        }

        [Fact]
        public void WithFileName_WithValidFunction_ShouldSetFileNameGenerator()
        {
            // Arrange
            var builder = new LogConfigurationBuilder();
            Func<string> fileNameGenerator = () => "custom_log_file";

            // Act
            var result = builder.WithFileName(fileNameGenerator);

            // Assert
            result.Should().BeSameAs(builder, "Should return the same builder instance for chaining");
            
            var config = result.BuildWithoutValidation();
            config.FileName.Should().BeSameAs(fileNameGenerator);
            config.FileName().Should().Be("custom_log_file");
        }

        [Fact]
        public void WithFileName_WithNullFunction_ShouldThrowArgumentNullException()
        {
            // Arrange
            var builder = new LogConfigurationBuilder();

            // Act & Assert
            var act = () => builder.WithFileName((Func<string>)null);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WithFileName_WithValidString_ShouldSetStaticFileName()
        {
            // Arrange
            var builder = new LogConfigurationBuilder();
            var fileName = "static_log_file";

            // Act
            var result = builder.WithFileName(fileName);

            // Assert
            result.Should().BeSameAs(builder, "Should return the same builder instance for chaining");
            
            var config = result.BuildWithoutValidation();
            config.FileName().Should().Be(fileName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void WithFileName_WithInvalidString_ShouldThrowArgumentException(string fileName)
        {
            // Arrange
            var builder = new LogConfigurationBuilder();

            // Act & Assert
            var act = () => builder.WithFileName(fileName);
            act.Should().Throw<ArgumentException>()
                .WithMessage("*File name cannot be null or empty*");
        }

        [Fact]
        public void WithMaxFileSize_WithValidSize_ShouldSetMaxFileSize()
        {
            // Arrange
            var builder = new LogConfigurationBuilder();
            var maxFileSize = 5000L;

            // Act
            var result = builder.WithMaxFileSize(maxFileSize);

            // Assert
            result.Should().BeSameAs(builder, "Should return the same builder instance for chaining");
            
            var config = result.BuildWithoutValidation();
            config.MaxFileSize.Should().Be(maxFileSize);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void WithMaxFileSize_WithInvalidSize_ShouldThrowArgumentException(long maxFileSize)
        {
            // Arrange
            var builder = new LogConfigurationBuilder();

            // Act & Assert
            var act = () => builder.WithMaxFileSize(maxFileSize);
            act.Should().Throw<ArgumentException>()
                .WithMessage("*Max file size must be greater than 0*");
        }

        [Fact]
        public void WithDateBasedFileName_WithDefaultFormat_ShouldSetDateBasedFileName()
        {
            // Arrange
            var builder = new LogConfigurationBuilder();

            // Act
            var result = builder.WithDateBasedFileName();

            // Assert
            result.Should().BeSameAs(builder, "Should return the same builder instance for chaining");
            
            var config = result.BuildWithoutValidation();
            var fileName = config.FileName();
            
            // Should match the default format "dd MM yyyy"
            fileName.Should().MatchRegex(@"\d{2} \d{2} \d{4}");
        }

        [Fact]
        public void WithDateBasedFileName_WithCustomFormat_ShouldSetCustomDateBasedFileName()
        {
            // Arrange
            var builder = new LogConfigurationBuilder();
            var customFormat = "yyyy-MM-dd";

            // Act
            var result = builder.WithDateBasedFileName(customFormat);

            // Assert
            result.Should().BeSameAs(builder, "Should return the same builder instance for chaining");
            
            var config = result.BuildWithoutValidation();
            var fileName = config.FileName();
            
            // Should match the custom format "yyyy-MM-dd"
            fileName.Should().MatchRegex(@"\d{4}-\d{2}-\d{2}");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void WithDateBasedFileName_WithInvalidFormat_ShouldThrowArgumentException(string dateFormat)
        {
            // Arrange
            var builder = new LogConfigurationBuilder();

            // Act & Assert
            var act = () => builder.WithDateBasedFileName(dateFormat);
            act.Should().Throw<ArgumentException>()
                .WithMessage("*Date format cannot be null or empty*");
        }

        [Fact]
        public void Build_WithValidConfiguration_ShouldReturnValidConfiguration()
        {
            // Arrange
            var builder = new LogConfigurationBuilder()
                .WithLogDirectory(_testLogDirectory)
                .WithApplicationName("TestApp")
                .WithFileName("test_log")
                .WithMaxFileSize(2000);

            // Act
            var config = builder.Build();

            // Assert
            config.Should().NotBeNull();
            config.LogDirectory.Should().Be(_testLogDirectory);
            config.ApplicationName.Should().Be("TestApp");
            config.FileName().Should().Be("test_log");
            config.MaxFileSize.Should().Be(2000);
        }

        [Fact]
        public void Build_WithInvalidConfiguration_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var builder = new LogConfigurationBuilder()
                .WithLogDirectory(_testLogDirectory)
                .WithApplicationName("TestApp")
                .WithMaxFileSize(-1); // Invalid

            // Act & Assert
            var act = () => builder.Build();
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*Configuration validation failed*");
        }

        [Fact]
        public void BuildWithoutValidation_ShouldReturnConfigurationWithoutValidation()
        {
            // Arrange
            var builder = new LogConfigurationBuilder()
                .WithLogDirectory(_testLogDirectory)
                .WithApplicationName("TestApp")
                .WithMaxFileSize(-1); // Invalid but should not throw

            // Act
            var config = builder.BuildWithoutValidation();

            // Assert
            config.Should().NotBeNull();
            config.MaxFileSize.Should().Be(-1); // Invalid value should be preserved
        }

        [Fact]
        public void Validate_WithValidConfiguration_ShouldReturnValidResult()
        {
            // Arrange
            var builder = new LogConfigurationBuilder()
                .WithLogDirectory(_testLogDirectory)
                .WithApplicationName("TestApp")
                .WithFileName("test_log")
                .WithMaxFileSize(2000);

            // Act
            var result = builder.Validate();

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_WithInvalidConfiguration_ShouldReturnInvalidResult()
        {
            // Arrange
            var builder = new LogConfigurationBuilder()
                .WithLogDirectory(_testLogDirectory)
                .WithApplicationName("TestApp")
                .WithMaxFileSize(-1); // Invalid

            // Act
            var result = builder.Validate();

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public void FluentChaining_ShouldAllowMethodChaining()
        {
            // Act
            var config = new LogConfigurationBuilder()
                .WithLogDirectory(_testLogDirectory)
                .WithApplicationName("ChainedApp")
                .WithFileName("chained_log")
                .WithMaxFileSize(3000)
                .WithDateBasedFileName("yyyy-MM-dd")
                .Build();

            // Assert
            config.Should().NotBeNull();
            config.LogDirectory.Should().Be(_testLogDirectory);
            config.ApplicationName.Should().Be("ChainedApp");
            config.MaxFileSize.Should().Be(3000);
            
            // The last WithDateBasedFileName should override the previous WithFileName
            var fileName = config.FileName();
            fileName.Should().MatchRegex(@"\d{4}-\d{2}-\d{2}");
        }

        [Fact]
        public void CreateXLogWithBuilder_ShouldWorkCorrectly()
        {
            // Act
            var log = XLog.Create(builder => builder
                .WithLogDirectory(_testLogDirectory)
                .WithApplicationName("BuilderTestApp")
                .WithFileName("builder_test")
                .WithMaxFileSize(1500));

            // Assert
            log.Should().NotBeNull();
            log.Config.LogDirectory.Should().Be(_testLogDirectory);
            log.Config.ApplicationName.Should().Be("BuilderTestApp");
            log.Config.FileName().Should().Be("builder_test");
            log.Config.MaxFileSize.Should().Be(1500);
        }

        [Fact]
        public void CreateXLogWithInvalidBuilder_ShouldThrowException()
        {
            // Act & Assert
            var act = () => XLog.Create(builder => builder
                .WithLogDirectory(_testLogDirectory)
                .WithApplicationName("BuilderTestApp")
                .WithMaxFileSize(-1)); // Invalid

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*Configuration validation failed*");
        }
    }
}