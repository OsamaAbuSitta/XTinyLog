using System.IO;

namespace XTinyLog.Serilog.Tests;

/// <summary>
/// Unit tests for the XTinyLogSinkConfiguration class.
/// </summary>
public class XTinyLogSinkConfigurationTests
{
    /// <summary>
    /// Tests that default configuration values are set correctly.
    /// </summary>
    [Fact]
    public void Constructor_ShouldSetDefaultValues()
    {
        // Act
        var config = new XTinyLogSinkConfiguration();

        // Assert
        config.LogDirectory.Should().Be(@"c:\Log");
        config.ApplicationName.Should().Be("Application");
        config.MaxFileSize.Should().Be(1024);
        config.FileName.Should().BeNull();
        config.IncludeProperties.Should().BeTrue();
        config.PropertyFormat.Should().Be(PropertyFormatMode.Inline);
    }

    /// <summary>
    /// Tests that properties can be set and retrieved correctly.
    /// </summary>
    [Fact]
    public void Properties_ShouldBeSettable()
    {
        // Arrange
        var config = new XTinyLogSinkConfiguration();
        var customFileName = () => "custom.log";

        // Act
        config.LogDirectory = @"d:\CustomLogs";
        config.ApplicationName = "TestApp";
        config.MaxFileSize = 2048;
        config.FileName = customFileName;
        config.IncludeProperties = false;
        config.PropertyFormat = PropertyFormatMode.Structured;

        // Assert
        config.LogDirectory.Should().Be(@"d:\CustomLogs");
        config.ApplicationName.Should().Be("TestApp");
        config.MaxFileSize.Should().Be(2048);
        config.FileName.Should().BeSameAs(customFileName);
        config.IncludeProperties.Should().BeFalse();
        config.PropertyFormat.Should().Be(PropertyFormatMode.Structured);
    }

    /// <summary>
    /// Tests that ToLogConfiguration creates a valid ILogConfiguration.
    /// </summary>
    [Fact]
    public void ToLogConfiguration_ShouldCreateValidConfiguration()
    {
        // Arrange
        var tempDir = Path.GetTempPath();
        var sinkConfig = new XTinyLogSinkConfiguration
        {
            LogDirectory = tempDir,
            ApplicationName = "TestApplication",
            MaxFileSize = 512,
        };

        // Act
        var logConfig = sinkConfig.ToLogConfiguration();

        // Assert
        logConfig.Should().NotBeNull();
        logConfig.LogDirectory.Should().Be(tempDir);
        logConfig.ApplicationName.Should().Be("TestApplication");
        logConfig.MaxFileSize.Should().Be(512);
    }

    /// <summary>
    /// Tests that ToLogConfiguration preserves custom FileName function.
    /// </summary>
    [Fact]
    public void ToLogConfiguration_WithCustomFileName_ShouldPreserveFunction()
    {
        // Arrange
        var customFileName = () => "custom-log.html";
        var sinkConfig = new XTinyLogSinkConfiguration
        {
            FileName = customFileName,
        };

        // Act
        var logConfig = sinkConfig.ToLogConfiguration();

        // Assert
        logConfig.FileName.Should().BeSameAs(customFileName);
    }

    /// <summary>
    /// Tests that Clone creates an independent copy.
    /// </summary>
    [Fact]
    public void Clone_ShouldCreateIndependentCopy()
    {
        // Arrange
        var original = new XTinyLogSinkConfiguration
        {
            LogDirectory = @"d:\OriginalLogs",
            ApplicationName = "OriginalApp",
            MaxFileSize = 1024,
            IncludeProperties = false,
            PropertyFormat = PropertyFormatMode.Hidden,
        };

        // Act
        var clone = original.Clone();

        // Assert
        clone.Should().NotBeSameAs(original);
        clone.LogDirectory.Should().Be(original.LogDirectory);
        clone.ApplicationName.Should().Be(original.ApplicationName);
        clone.MaxFileSize.Should().Be(original.MaxFileSize);
        clone.IncludeProperties.Should().Be(original.IncludeProperties);
        clone.PropertyFormat.Should().Be(original.PropertyFormat);

        // Verify independence
        clone.LogDirectory = @"d:\ClonedLogs";
        original.LogDirectory.Should().Be(@"d:\OriginalLogs");
    }

    /// <summary>
    /// Tests validation with valid configuration.
    /// </summary>
    [Fact]
    public void Validate_WithValidConfiguration_ShouldReturnNoErrors()
    {
        // Arrange
        var config = new XTinyLogSinkConfiguration
        {
            LogDirectory = @"d:\ValidLogs",
            ApplicationName = "ValidApp",
            MaxFileSize = 1024,
        };

        // Act
        var errors = config.Validate();

        // Assert
        errors.Should().BeEmpty();
    }

    /// <summary>
    /// Tests validation with null or empty LogDirectory.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithInvalidLogDirectory_ShouldReturnError(string? logDirectory)
    {
        // Arrange
        var config = new XTinyLogSinkConfiguration
        {
            LogDirectory = logDirectory!,
        };

        // Act
        var errors = config.Validate();

        // Assert
        errors.Should().Contain("LogDirectory cannot be null or empty.");
    }

    /// <summary>
    /// Tests validation with null or empty ApplicationName.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithInvalidApplicationName_ShouldReturnError(string? applicationName)
    {
        // Arrange
        var config = new XTinyLogSinkConfiguration
        {
            ApplicationName = applicationName!,
        };

        // Act
        var errors = config.Validate();

        // Assert
        errors.Should().Contain("ApplicationName cannot be null or empty.");
    }

    /// <summary>
    /// Tests validation with invalid MaxFileSize.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_WithInvalidMaxFileSize_ShouldReturnError(long maxFileSize)
    {
        // Arrange
        var config = new XTinyLogSinkConfiguration
        {
            MaxFileSize = maxFileSize,
        };

        // Act
        var errors = config.Validate();

        // Assert
        errors.Should().Contain("MaxFileSize must be greater than 0.");
    }

    /// <summary>
    /// Tests validation with multiple invalid properties.
    /// </summary>
    [Fact]
    public void Validate_WithMultipleInvalidProperties_ShouldReturnMultipleErrors()
    {
        // Arrange
        var config = new XTinyLogSinkConfiguration
        {
            LogDirectory = "",
            ApplicationName = null!,
            MaxFileSize = -1,
        };

        // Act
        var errors = config.Validate();

        // Assert
        errors.Should().HaveCount(3);
        errors.Should().Contain("LogDirectory cannot be null or empty.");
        errors.Should().Contain("ApplicationName cannot be null or empty.");
        errors.Should().Contain("MaxFileSize must be greater than 0.");
    }
}