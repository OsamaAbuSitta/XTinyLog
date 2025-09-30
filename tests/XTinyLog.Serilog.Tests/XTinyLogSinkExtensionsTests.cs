using Serilog;
using Serilog.Events;
using XTinyLog.Serilog;
using Xunit;

namespace XTinyLog.Serilog.Tests;

public class XTinyLogSinkExtensionsTests
{
    [Fact]
    public void XTinyLog_WithDefaultConfiguration_ShouldCreateLogger()
    {
        // Arrange & Act
        var logger = new LoggerConfiguration()
            .WriteTo.XTinyLog()
            .CreateLogger();

        // Assert
        Assert.NotNull(logger);
        logger.Dispose();
    }

    [Fact]
    public void XTinyLog_WithCustomConfiguration_ShouldCreateLogger()
    {
        // Arrange & Act
        var logger = new LoggerConfiguration()
            .WriteTo.XTinyLog(config =>
            {
                config.ApplicationName = "TestApp";
                config.LogDirectory = @"c:\TestLogs";
                config.MaxFileSize = 2048;
                config.IncludeProperties = false;
                config.PropertyFormat = PropertyFormatMode.Structured;
            })
            .CreateLogger();

        // Assert
        Assert.NotNull(logger);
        logger.Dispose();
    }

    [Fact]
    public void XTinyLogAsync_WithDefaultConfiguration_ShouldCreateLogger()
    {
        // Arrange & Act
        var logger = new LoggerConfiguration()
            .WriteTo.XTinyLogAsync()
            .CreateLogger();

        // Assert
        Assert.NotNull(logger);
        logger.Dispose();
    }

    [Fact]
    public void XTinyLogAsync_WithCustomConfiguration_ShouldCreateLogger()
    {
        // Arrange & Act
        var logger = new LoggerConfiguration()
            .WriteTo.XTinyLogAsync(config =>
            {
                config.ApplicationName = "AsyncTestApp";
                config.LogDirectory = @"c:\AsyncTestLogs";
                config.MaxFileSize = 4096;
                config.IncludeProperties = true;
                config.PropertyFormat = PropertyFormatMode.Hidden;
            })
            .CreateLogger();

        // Assert
        Assert.NotNull(logger);
        logger.Dispose();
    }

    [Fact]
    public void XTinyLog_WithNullSinkConfiguration_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => XTinyLogSinkExtensions.XTinyLog(null!));
    }

    [Fact]
    public void XTinyLog_WithNullConfigureAction_ShouldThrowArgumentNullException()
    {
        // Arrange
        var loggerConfiguration = new LoggerConfiguration();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => loggerConfiguration.WriteTo.XTinyLog(null!));
    }

    [Fact]
    public void XTinyLogAsync_WithNullSinkConfiguration_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => XTinyLogSinkExtensions.XTinyLogAsync(null!));
    }

    [Fact]
    public void XTinyLogAsync_WithNullConfigureAction_ShouldThrowArgumentNullException()
    {
        // Arrange
        var loggerConfiguration = new LoggerConfiguration();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => loggerConfiguration.WriteTo.XTinyLogAsync(null!));
    }

    [Fact]
    public void XTinyLog_ConfigurationAction_ShouldModifyConfiguration()
    {
        // Arrange
        var testAppName = "ConfigurationTestApp";
        var testLogDirectory = @"c:\ConfigTestLogs";
        var testMaxFileSize = 8192L;

        // Act
        var logger = new LoggerConfiguration()
            .WriteTo.XTinyLog(config =>
            {
                config.ApplicationName = testAppName;
                config.LogDirectory = testLogDirectory;
                config.MaxFileSize = testMaxFileSize;
                config.IncludeProperties = false;
                config.PropertyFormat = PropertyFormatMode.Structured;
            })
            .CreateLogger();

        // Assert
        Assert.NotNull(logger);
        
        // Test that the logger can write a log entry without throwing
        logger.Information("Test log message");
        
        logger.Dispose();
    }

    [Fact]
    public void XTinyLogAsync_ConfigurationAction_ShouldModifyConfiguration()
    {
        // Arrange
        var testAppName = "AsyncConfigurationTestApp";
        var testLogDirectory = @"c:\AsyncConfigTestLogs";
        var testMaxFileSize = 16384L;

        // Act
        var logger = new LoggerConfiguration()
            .WriteTo.XTinyLogAsync(config =>
            {
                config.ApplicationName = testAppName;
                config.LogDirectory = testLogDirectory;
                config.MaxFileSize = testMaxFileSize;
                config.IncludeProperties = true;
                config.PropertyFormat = PropertyFormatMode.Inline;
            })
            .CreateLogger();

        // Assert
        Assert.NotNull(logger);
        
        // Test that the logger can write a log entry without throwing
        logger.Information("Test async log message");
        
        logger.Dispose();
    }

    [Fact]
    public void XTinyLog_WithInvalidConfiguration_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() =>
        {
            new LoggerConfiguration()
                .WriteTo.XTinyLog(config =>
                {
                    config.LogDirectory = ""; // Invalid empty directory
                    config.ApplicationName = "TestApp";
                })
                .CreateLogger();
        });
    }

    [Fact]
    public void XTinyLogAsync_WithInvalidConfiguration_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() =>
        {
            new LoggerConfiguration()
                .WriteTo.XTinyLogAsync(config =>
                {
                    config.LogDirectory = "TestDir";
                    config.ApplicationName = ""; // Invalid empty application name
                })
                .CreateLogger();
        });
    }

    [Fact]
    public void XTinyLog_WithInvalidParameterConfiguration_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() =>
        {
            new LoggerConfiguration()
                .WriteTo.XTinyLog(
                    logDirectory: "",  // Invalid empty directory
                    applicationName: "TestApp",
                    maxFileSize: 1024)
                .CreateLogger();
        });
    }

    [Fact]
    public void XTinyLogAsync_WithInvalidParameterConfiguration_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() =>
        {
            new LoggerConfiguration()
                .WriteTo.XTinyLogAsync(
                    logDirectory: @"c:\TestLogs",
                    applicationName: "TestApp",
                    maxFileSize: -1)  // Invalid negative max file size
                .CreateLogger();
        });
    }

    [Fact]
    public void XTinyLog_MultipleConfigurations_ShouldAllowChaining()
    {
        // Arrange & Act
        var logger = new LoggerConfiguration()
            .WriteTo.XTinyLog(config =>
            {
                config.ApplicationName = "ChainedApp";
                config.LogDirectory = @"c:\ChainedLogs";
            })
            .WriteTo.XTinyLogAsync(config =>
            {
                config.ApplicationName = "AsyncChainedApp";
                config.LogDirectory = @"c:\AsyncChainedLogs";
            })
            .CreateLogger();

        // Assert
        Assert.NotNull(logger);

        // Test that the logger can write log entries to multiple sinks
        logger.Information("Test message to multiple sinks");

        logger.Dispose();
    }

    [Fact]
    public void XTinyLogWithBuilder_WithInvalidConfiguration_ShouldThrowInvalidOperationException()
    {
        // Arrange & Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            new LoggerConfiguration()
                .WriteTo.XTinyLogWithBuilder(builder =>
                {
                    builder.WithLogDirectory("")  // Invalid empty directory
                           .WithApplicationName("TestApp");
                })
                .CreateLogger();
        });
    }

    [Fact]
    public void XTinyLogAsyncWithBuilder_WithInvalidConfiguration_ShouldThrowInvalidOperationException()
    {
        // Arrange & Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            new LoggerConfiguration()
                .WriteTo.XTinyLogAsyncWithBuilder(builder =>
                {
                    builder.WithLogDirectory(@"c:\TestLogs")
                           .WithApplicationName("")  // Invalid empty application name
                           .WithMaxFileSize(1024);
                })
                .CreateLogger();
        });
    }

    [Fact]
    public void XTinyLogWithBuilder_WithNullBuilder_ShouldThrowArgumentNullException()
    {
        // Arrange
        var loggerConfiguration = new LoggerConfiguration();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            loggerConfiguration.WriteTo.XTinyLogWithBuilder(null!));
    }

    [Fact]
    public void XTinyLogAsyncWithBuilder_WithNullBuilder_ShouldThrowArgumentNullException()
    {
        // Arrange
        var loggerConfiguration = new LoggerConfiguration();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            loggerConfiguration.WriteTo.XTinyLogAsyncWithBuilder(null!));
    }}
