using FluentAssertions;
using XTinyLog.Serilog;

namespace XTinyLog.Serilog.Tests;

public class XTinyLogSinkConfigurationBuilderTests
{
    [Fact]
    public void Build_WithDefaultValues_ShouldCreateValidConfiguration()
    {
        // Arrange
        var builder = new XTinyLogSinkConfigurationBuilder();

        // Act
        var configuration = builder.Build();

        // Assert
        configuration.Should().NotBeNull();
        configuration.LogDirectory.Should().Be(@"c:\Log");
        configuration.ApplicationName.Should().Be("Application");
        configuration.MaxFileSize.Should().Be(1024);
        configuration.FileName.Should().BeNull();
        configuration.IncludeProperties.Should().BeTrue();
        configuration.PropertyFormat.Should().Be(PropertyFormatMode.Inline);
    }

    [Fact]
    public void WithLogDirectory_WithValidDirectory_ShouldSetLogDirectory()
    {
        // Arrange
        var builder = new XTinyLogSinkConfigurationBuilder();
        var logDirectory = @"c:\MyLogs";

        // Act
        var result = builder.WithLogDirectory(logDirectory);
        var configuration = result.Build();

        // Assert
        result.Should().BeSameAs(builder); // Fluent interface
        configuration.LogDirectory.Should().Be(logDirectory);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void WithLogDirectory_WithInvalidDirectory_ShouldThrowArgumentException(string? invalidDirectory)
    {
        // Arrange
        var builder = new XTinyLogSinkConfigurationBuilder();

        // Act & Assert
        var action = () => builder.WithLogDirectory(invalidDirectory!);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Log directory cannot be null or whitespace.*");
    }

    [Fact]
    public void WithApplicationName_WithValidName_ShouldSetApplicationName()
    {
        // Arrange
        var builder = new XTinyLogSinkConfigurationBuilder();
        var applicationName = "MyApp";

        // Act
        var result = builder.WithApplicationName(applicationName);
        var configuration = result.Build();

        // Assert
        result.Should().BeSameAs(builder); // Fluent interface
        configuration.ApplicationName.Should().Be(applicationName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void WithApplicationName_WithInvalidName_ShouldThrowArgumentException(string? invalidName)
    {
        // Arrange
        var builder = new XTinyLogSinkConfigurationBuilder();

        // Act & Assert
        var action = () => builder.WithApplicationName(invalidName!);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Application name cannot be null or whitespace.*");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(512)]
    [InlineData(2048)]
    [InlineData(1073741824)] // 1GB in KB, which is reasonable
    public void WithMaxFileSize_WithValidSize_ShouldSetMaxFileSize(long validSize)
    {
        // Arrange
        var builder = new XTinyLogSinkConfigurationBuilder();

        // Act
        var result = builder.WithMaxFileSize(validSize);
        var configuration = result.Build();

        // Assert
        result.Should().BeSameAs(builder); // Fluent interface
        configuration.MaxFileSize.Should().Be(validSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void WithMaxFileSize_WithInvalidSize_ShouldThrowArgumentException(long invalidSize)
    {
        // Arrange
        var builder = new XTinyLogSinkConfigurationBuilder();

        // Act & Assert
        var action = () => builder.WithMaxFileSize(invalidSize);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Max file size must be greater than 0.*");
    }

    [Fact]
    public void WithFileName_WithValidFunction_ShouldSetFileName()
    {
        // Arrange
        var builder = new XTinyLogSinkConfigurationBuilder();
        Func<string> fileNameFunc = () => "custom-log.html";

        // Act
        var result = builder.WithFileName(fileNameFunc);
        var configuration = result.Build();

        // Assert
        result.Should().BeSameAs(builder); // Fluent interface
        configuration.FileName.Should().BeSameAs(fileNameFunc);
        configuration.FileName!().Should().Be("custom-log.html");
    }

    [Fact]
    public void WithFileName_WithNull_ShouldSetFileNameToNull()
    {
        // Arrange
        var builder = new XTinyLogSinkConfigurationBuilder();

        // Act
        var result = builder.WithFileName(null!);
        var configuration = result.Build();

        // Assert
        result.Should().BeSameAs(builder); // Fluent interface
        configuration.FileName.Should().BeNull();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void WithIncludeProperties_ShouldSetIncludeProperties(bool includeProperties)
    {
        // Arrange
        var builder = new XTinyLogSinkConfigurationBuilder();

        // Act
        var result = builder.WithIncludeProperties(includeProperties);
        var configuration = result.Build();

        // Assert
        result.Should().BeSameAs(builder); // Fluent interface
        configuration.IncludeProperties.Should().Be(includeProperties);
    }

    [Theory]
    [InlineData(PropertyFormatMode.Inline)]
    [InlineData(PropertyFormatMode.Structured)]
    [InlineData(PropertyFormatMode.Hidden)]
    public void WithPropertyFormat_ShouldSetPropertyFormat(PropertyFormatMode propertyFormat)
    {
        // Arrange
        var builder = new XTinyLogSinkConfigurationBuilder();

        // Act
        var result = builder.WithPropertyFormat(propertyFormat);
        var configuration = result.Build();

        // Assert
        result.Should().BeSameAs(builder); // Fluent interface
        configuration.PropertyFormat.Should().Be(propertyFormat);
    }

    [Fact]
    public void Build_WithFluentChaining_ShouldSetAllProperties()
    {
        // Arrange
        var builder = new XTinyLogSinkConfigurationBuilder();
        var logDirectory = @"c:\CustomLogs";
        var applicationName = "TestApp";
        var maxFileSize = 2048L;
        Func<string> fileName = () => "test.html";
        var includeProperties = false;
        var propertyFormat = PropertyFormatMode.Structured;

        // Act
        var configuration = builder
            .WithLogDirectory(logDirectory)
            .WithApplicationName(applicationName)
            .WithMaxFileSize(maxFileSize)
            .WithFileName(fileName)
            .WithIncludeProperties(includeProperties)
            .WithPropertyFormat(propertyFormat)
            .Build();

        // Assert
        configuration.LogDirectory.Should().Be(logDirectory);
        configuration.ApplicationName.Should().Be(applicationName);
        configuration.MaxFileSize.Should().Be(maxFileSize);
        configuration.FileName.Should().BeSameAs(fileName);
        configuration.IncludeProperties.Should().Be(includeProperties);
        configuration.PropertyFormat.Should().Be(propertyFormat);
    }

    [Fact]
    public void Reset_ShouldRestoreDefaultValues()
    {
        // Arrange
        var builder = new XTinyLogSinkConfigurationBuilder();
        
        // Modify all values
        builder
            .WithLogDirectory(@"c:\CustomLogs")
            .WithApplicationName("TestApp")
            .WithMaxFileSize(2048)
            .WithFileName(() => "test.html")
            .WithIncludeProperties(false)
            .WithPropertyFormat(PropertyFormatMode.Structured);

        // Act
        var result = builder.Reset();
        var configuration = result.Build();

        // Assert
        result.Should().BeSameAs(builder); // Fluent interface
        configuration.LogDirectory.Should().Be(@"c:\Log");
        configuration.ApplicationName.Should().Be("Application");
        configuration.MaxFileSize.Should().Be(1024);
        configuration.FileName.Should().BeNull();
        configuration.IncludeProperties.Should().BeTrue();
        configuration.PropertyFormat.Should().Be(PropertyFormatMode.Inline);
    }

    [Fact]
    public void Build_WithInvalidConfiguration_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var builder = new XTinyLogSinkConfigurationBuilder();
        
        // Create invalid configuration by setting invalid values through reflection
        // since the builder validates inputs, we need to test the Build validation
        var builderType = typeof(XTinyLogSinkConfigurationBuilder);
        var logDirectoryField = builderType.GetField("_logDirectory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        logDirectoryField!.SetValue(builder, ""); // Set empty string directly

        // Act & Assert
        var action = () => builder.Build();
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Configuration validation failed:*");
    }

    [Fact]
    public void MultipleBuilds_ShouldCreateIndependentConfigurations()
    {
        // Arrange
        var builder = new XTinyLogSinkConfigurationBuilder();

        // Act
        var config1 = builder.WithLogDirectory(@"c:\Logs1").Build();
        var config2 = builder.WithLogDirectory(@"c:\Logs2").Build();

        // Assert
        config1.Should().NotBeSameAs(config2);
        config1.LogDirectory.Should().Be(@"c:\Logs1");
        config2.LogDirectory.Should().Be(@"c:\Logs2");
    }
}