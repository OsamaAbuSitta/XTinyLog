using FluentAssertions;
using Serilog;
using Serilog.Events;
using XTinyLog.Serilog;

namespace XTinyLog.Serilog.Tests;

public class XTinyLogSinkBuilderIntegrationTests : IDisposable
{
    private readonly string _testLogDirectory;

    public XTinyLogSinkBuilderIntegrationTests()
    {
        _testLogDirectory = Path.Combine(Path.GetTempPath(), "XTinyLogSinkBuilderTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testLogDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testLogDirectory))
        {
            Directory.Delete(_testLogDirectory, true);
        }
    }

    [Fact]
    public void XTinyLog_WithBuilderConfiguration_ShouldCreateSinkWithCustomSettings()
    {
        // Arrange & Act
        var logger = new LoggerConfiguration()
            .WriteTo.XTinyLogWithBuilder(builder => builder
                .WithLogDirectory(_testLogDirectory)
                .WithApplicationName("BuilderTestApp")
                .WithMaxFileSize(2048)
                .WithIncludeProperties(false)
                .WithPropertyFormat(PropertyFormatMode.Hidden))
            .CreateLogger();

        logger.Information("Test message from builder configuration");
        logger.Dispose();

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "*.html");
        logFiles.Should().NotBeEmpty();

        var logContent = File.ReadAllText(logFiles[0]);
        logContent.Should().Contain("Test message from builder configuration");
        logContent.Should().Contain("BuilderTestApp");
    }

    [Fact]
    public void XTinyLogAsync_WithBuilderConfiguration_ShouldCreateAsyncSinkWithCustomSettings()
    {
        // Arrange & Act
        var logger = new LoggerConfiguration()
            .WriteTo.XTinyLogAsyncWithBuilder(builder => builder
                .WithLogDirectory(_testLogDirectory)
                .WithApplicationName("AsyncBuilderTestApp")
                .WithMaxFileSize(4096)
                .WithIncludeProperties(true)
                .WithPropertyFormat(PropertyFormatMode.Structured))
            .CreateLogger();

        logger.Information("Test message from async builder configuration");
        logger.Dispose();

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "*.html");
        logFiles.Should().NotBeEmpty();

        var logContent = File.ReadAllText(logFiles[0]);
        logContent.Should().Contain("Test message from async builder configuration");
        logContent.Should().Contain("AsyncBuilderTestApp");
    }

    [Fact]
    public void XTinyLog_WithBuilderAndCustomFileName_ShouldUseCustomFileName()
    {
        // Arrange
        var customFileName = "custom-builder-log.html";

        // Act
        var logger = new LoggerConfiguration()
            .WriteTo.XTinyLogWithBuilder(builder => builder
                .WithLogDirectory(_testLogDirectory)
                .WithApplicationName("CustomFileNameApp")
                .WithFileName(() => customFileName))
            .CreateLogger();

        logger.Information("Test message with custom file name");
        logger.Dispose();

        // Assert
        var expectedFilePath = Path.Combine(_testLogDirectory, customFileName);
        File.Exists(expectedFilePath).Should().BeTrue();

        var logContent = File.ReadAllText(expectedFilePath);
        logContent.Should().Contain("Test message with custom file name");
    }

    [Fact]
    public void XTinyLog_WithBuilderNullAction_ShouldThrowArgumentNullException()
    {
        // Arrange
        Action<XTinyLogSinkConfigurationBuilder> nullAction = null!;

        // Act & Assert
        var action = () => new LoggerConfiguration()
            .WriteTo.XTinyLogWithBuilder(nullAction)
            .CreateLogger();

        action.Should().Throw<ArgumentNullException>()
            .WithMessage("*configureBuilder*");
    }

    [Fact]
    public void XTinyLogAsync_WithBuilderNullAction_ShouldThrowArgumentNullException()
    {
        // Arrange
        Action<XTinyLogSinkConfigurationBuilder> nullAction = null!;

        // Act & Assert
        var action = () => new LoggerConfiguration()
            .WriteTo.XTinyLogAsyncWithBuilder(nullAction)
            .CreateLogger();

        action.Should().Throw<ArgumentNullException>()
            .WithMessage("*configureBuilder*");
    }

    [Fact]
    public void XTinyLog_WithBuilderInvalidConfiguration_ShouldThrowArgumentException()
    {
        // Act & Assert
        var action = () => new LoggerConfiguration()
            .WriteTo.XTinyLogWithBuilder(builder => builder
                .WithLogDirectory("") // Invalid empty directory
                .WithApplicationName("TestApp"))
            .CreateLogger();

        action.Should().Throw<ArgumentException>()
            .WithMessage("Log directory cannot be null or whitespace.*");
    }

    [Fact]
    public void XTinyLogAsync_WithBuilderInvalidConfiguration_ShouldThrowArgumentException()
    {
        // Act & Assert
        var action = () => new LoggerConfiguration()
            .WriteTo.XTinyLogAsyncWithBuilder(builder => builder
                .WithMaxFileSize(-1) // Invalid negative size
                .WithApplicationName("TestApp"))
            .CreateLogger();

        action.Should().Throw<ArgumentException>()
            .WithMessage("Max file size must be greater than 0.*");
    }

    [Fact]
    public void XTinyLog_WithBuilderFluentChaining_ShouldApplyAllSettings()
    {
        // Arrange
        var customDirectory = Path.Combine(_testLogDirectory, "fluent");
        Directory.CreateDirectory(customDirectory);

        // Act
        var logger = new LoggerConfiguration()
            .WriteTo.XTinyLogWithBuilder(builder => builder
                .WithLogDirectory(customDirectory)
                .WithApplicationName("FluentApp")
                .WithMaxFileSize(1024)
                .WithIncludeProperties(true)
                .WithPropertyFormat(PropertyFormatMode.Inline)
                .WithFileName(() => "fluent-test.html"))
            .CreateLogger();

        logger.Information("Fluent configuration test with {Property}", "TestValue");
        logger.Dispose();

        // Assert
        var expectedFilePath = Path.Combine(customDirectory, "fluent-test.html");
        File.Exists(expectedFilePath).Should().BeTrue();

        var logContent = File.ReadAllText(expectedFilePath);
        logContent.Should().Contain("Fluent configuration test");
        logContent.Should().Contain("FluentApp");
        logContent.Should().Contain("TestValue"); // Property should be included
    }

    [Fact]
    public void XTinyLog_WithBuilderReset_ShouldUseDefaultValues()
    {
        // Act
        var logger = new LoggerConfiguration()
            .WriteTo.XTinyLogWithBuilder(builder => builder
                .WithLogDirectory(_testLogDirectory)
                .WithApplicationName("CustomApp")
                .WithMaxFileSize(2048)
                .Reset() // Reset to defaults
                .WithLogDirectory(_testLogDirectory)) // Only set directory again
            .CreateLogger();

        logger.Information("Reset builder test");
        logger.Dispose();

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "*.html");
        logFiles.Should().NotBeEmpty();

        var logContent = File.ReadAllText(logFiles[0]);
        logContent.Should().Contain("Reset builder test");
        logContent.Should().Contain("Application"); // Default application name
    }

    [Fact]
    public void XTinyLog_WithBuilderAndStructuredLogging_ShouldFormatPropertiesCorrectly()
    {
        // Act
        var logger = new LoggerConfiguration()
            .WriteTo.XTinyLogWithBuilder(builder => builder
                .WithLogDirectory(_testLogDirectory)
                .WithApplicationName("StructuredApp")
                .WithIncludeProperties(true)
                .WithPropertyFormat(PropertyFormatMode.Inline))
            .CreateLogger();

        logger.Information("User {UserId} performed {Action} at {Timestamp}", 
            123, "Login", DateTimeOffset.Now);
        logger.Dispose();

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "*.html");
        logFiles.Should().NotBeEmpty();

        var logContent = File.ReadAllText(logFiles[0]);
        logContent.Should().Contain("User 123 performed Login");
        logContent.Should().Contain("StructuredApp");
    }

    [Fact]
    public void XTinyLogAsync_WithBuilderAndException_ShouldLogExceptionDetails()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception message");

        // Act
        var logger = new LoggerConfiguration()
            .WriteTo.XTinyLogAsyncWithBuilder(builder => builder
                .WithLogDirectory(_testLogDirectory)
                .WithApplicationName("ExceptionApp"))
            .CreateLogger();

        logger.Error(exception, "An error occurred while processing {Operation}", "TestOperation");
        logger.Dispose();

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "*.html");
        logFiles.Should().NotBeEmpty();

        var logContent = File.ReadAllText(logFiles[0]);
        logContent.Should().Contain("An error occurred while processing TestOperation");
        logContent.Should().Contain("Test exception message");
        logContent.Should().Contain("InvalidOperationException");
        logContent.Should().Contain("ExceptionApp");
    }
}