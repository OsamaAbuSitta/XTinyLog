using Serilog;
using Serilog.Events;
using XTinyLog.Serilog;
using Xunit;

namespace XTinyLog.Serilog.Tests;

public class XTinyLogSinkJsonConfigurationTests
{
    [Fact]
    public void XTinyLog_WithJsonStyleParameters_ShouldCreateLogger()
    {
        // Arrange - simulate parameters that would come from appsettings.json
        var logDirectory = @"c:\JsonConfigLogs";
        var applicationName = "JsonConfigApp";
        var maxFileSize = 2048L;
        var includeProperties = false;
        var propertyFormat = PropertyFormatMode.Structured;

        // Act
        var logger = new LoggerConfiguration()
            .WriteTo.XTinyLog(
                logDirectory: logDirectory,
                applicationName: applicationName,
                maxFileSize: maxFileSize,
                includeProperties: includeProperties,
                propertyFormat: propertyFormat)
            .CreateLogger();

        // Assert
        Assert.NotNull(logger);
        
        // Test that the logger can write a log entry without throwing
        logger.Information("Test log message from JSON configuration");
        
        logger.Dispose();
    }

    [Fact]
    public void XTinyLogAsync_WithJsonStyleParameters_ShouldCreateLogger()
    {
        // Arrange - simulate parameters that would come from appsettings.json
        var logDirectory = @"c:\AsyncJsonConfigLogs";
        var applicationName = "AsyncJsonConfigApp";
        var maxFileSize = 4096L;
        var includeProperties = true;
        var propertyFormat = PropertyFormatMode.Hidden;

        // Act
        var logger = new LoggerConfiguration()
            .WriteTo.XTinyLogAsync(
                logDirectory: logDirectory,
                applicationName: applicationName,
                maxFileSize: maxFileSize,
                includeProperties: includeProperties,
                propertyFormat: propertyFormat)
            .CreateLogger();

        // Assert
        Assert.NotNull(logger);
        
        // Test that the logger can write a log entry without throwing
        logger.Information("Test async log message from JSON configuration");
        
        logger.Dispose();
    }

    [Fact]
    public void XTinyLog_WithDefaultJsonParameters_ShouldCreateLogger()
    {
        // Arrange & Act - test with default parameters (as would be used when not specified in appsettings.json)
        var logger = new LoggerConfiguration()
            .WriteTo.XTinyLog()
            .CreateLogger();

        // Assert
        Assert.NotNull(logger);
        
        // Test that the logger can write a log entry without throwing
        logger.Information("Test log message with default JSON configuration");
        
        logger.Dispose();
    }

    [Fact]
    public void XTinyLogAsync_WithDefaultJsonParameters_ShouldCreateLogger()
    {
        // Arrange & Act - test with default parameters (as would be used when not specified in appsettings.json)
        var logger = new LoggerConfiguration()
            .WriteTo.XTinyLogAsync()
            .CreateLogger();

        // Assert
        Assert.NotNull(logger);
        
        // Test that the logger can write a log entry without throwing
        logger.Information("Test async log message with default JSON configuration");
        
        logger.Dispose();
    }

    [Fact]
    public void XTinyLog_WithPartialJsonParameters_ShouldUseDefaults()
    {
        // Arrange - simulate partial configuration from appsettings.json
        var applicationName = "PartialConfigApp";
        var maxFileSize = 8192L;

        // Act
        var logger = new LoggerConfiguration()
            .WriteTo.XTinyLog(
                applicationName: applicationName,
                maxFileSize: maxFileSize)
            .CreateLogger();

        // Assert
        Assert.NotNull(logger);
        
        // Test that the logger can write a log entry without throwing
        logger.Information("Test log message with partial JSON configuration");
        
        logger.Dispose();
    }

    [Fact]
    public void XTinyLog_WithNullParameters_ShouldUseDefaults()
    {
        // Arrange & Act - test null parameter handling
        var logger = new LoggerConfiguration()
            .WriteTo.XTinyLog(
                logDirectory: null,
                applicationName: null)
            .CreateLogger();

        // Assert
        Assert.NotNull(logger);
        
        // Test that the logger can write a log entry without throwing
        logger.Information("Test log message with null parameters");
        
        logger.Dispose();
    }

    [Fact]
    public void XTinyLog_WithAllPropertyFormatModes_ShouldWork()
    {
        // Test all property format modes that could be configured via JSON
        var formatModes = new[]
        {
            PropertyFormatMode.Inline,
            PropertyFormatMode.Structured,
            PropertyFormatMode.Hidden
        };

        foreach (var formatMode in formatModes)
        {
            // Arrange
            var logger = new LoggerConfiguration()
                .WriteTo.XTinyLog(
                    applicationName: $"PropertyFormatTest_{formatMode}",
                    propertyFormat: formatMode)
                .CreateLogger();

            // Act & Assert
            Assert.NotNull(logger);
            
            // Test that the logger can write a log entry without throwing
            logger.Information("Test log message with {PropertyFormat} format mode", formatMode);
            
            logger.Dispose();
        }
    }

    [Fact]
    public void XTinyLog_JsonConfiguration_ShouldValidateParameters()
    {
        // Test that invalid parameters are handled appropriately
        
        // Test with invalid maxFileSize should throw
        var ex1 = Assert.Throws<ArgumentException>(() =>
        {
            var logger1 = new LoggerConfiguration()
                .WriteTo.XTinyLog(maxFileSize: -1)
                .CreateLogger();
        });
        Assert.Contains("MaxFileSize must be greater than 0", ex1.Message);

        // Test with zero maxFileSize should throw
        var ex2 = Assert.Throws<ArgumentException>(() =>
        {
            var logger2 = new LoggerConfiguration()
                .WriteTo.XTinyLog(maxFileSize: 0)
                .CreateLogger();
        });
        Assert.Contains("MaxFileSize must be greater than 0", ex2.Message);

        // Test with empty logDirectory should throw
        var ex3 = Assert.Throws<ArgumentException>(() =>
        {
            var logger3 = new LoggerConfiguration()
                .WriteTo.XTinyLog(logDirectory: "")
                .CreateLogger();
        });
        Assert.Contains("LogDirectory cannot be empty", ex3.Message);

        // Test with empty applicationName should throw
        var ex4 = Assert.Throws<ArgumentException>(() =>
        {
            var logger4 = new LoggerConfiguration()
                .WriteTo.XTinyLog(applicationName: "")
                .CreateLogger();
        });
        Assert.Contains("ApplicationName cannot be empty", ex4.Message);

        // Test with valid parameters should work
        var logger5 = new LoggerConfiguration()
            .WriteTo.XTinyLog(maxFileSize: 1024)
            .CreateLogger();
        
        Assert.NotNull(logger5);
        logger5.Dispose();
    }

    [Fact]
    public void XTinyLogAsync_JsonConfiguration_ShouldValidateParameters()
    {
        // Test that invalid parameters are handled appropriately for async sink
        
        // Test with invalid maxFileSize should throw
        var ex1 = Assert.Throws<ArgumentException>(() =>
        {
            var logger1 = new LoggerConfiguration()
                .WriteTo.XTinyLogAsync(maxFileSize: -1)
                .CreateLogger();
        });
        Assert.Contains("MaxFileSize must be greater than 0", ex1.Message);

        // Test with zero maxFileSize should throw
        var ex2 = Assert.Throws<ArgumentException>(() =>
        {
            var logger2 = new LoggerConfiguration()
                .WriteTo.XTinyLogAsync(maxFileSize: 0)
                .CreateLogger();
        });
        Assert.Contains("MaxFileSize must be greater than 0", ex2.Message);

        // Test with empty logDirectory should throw
        var ex3 = Assert.Throws<ArgumentException>(() =>
        {
            var logger3 = new LoggerConfiguration()
                .WriteTo.XTinyLogAsync(logDirectory: "")
                .CreateLogger();
        });
        Assert.Contains("LogDirectory cannot be empty", ex3.Message);

        // Test with empty applicationName should throw
        var ex4 = Assert.Throws<ArgumentException>(() =>
        {
            var logger4 = new LoggerConfiguration()
                .WriteTo.XTinyLogAsync(applicationName: "")
                .CreateLogger();
        });
        Assert.Contains("ApplicationName cannot be empty", ex4.Message);

        // Test with valid parameters should work
        var logger5 = new LoggerConfiguration()
            .WriteTo.XTinyLogAsync(maxFileSize: 1024)
            .CreateLogger();
        
        Assert.NotNull(logger5);
        logger5.Dispose();
    }

    [Fact]
    public void XTinyLog_JsonConfiguration_ShouldHandleMultipleValidationErrors()
    {
        // Test that multiple validation errors are reported together
        var ex = Assert.Throws<ArgumentException>(() =>
        {
            var logger = new LoggerConfiguration()
                .WriteTo.XTinyLog(
                    logDirectory: "",
                    applicationName: "",
                    maxFileSize: -1)
                .CreateLogger();
        });

        // Should contain all validation errors
        Assert.Contains("LogDirectory cannot be empty", ex.Message);
        Assert.Contains("ApplicationName cannot be empty", ex.Message);
        Assert.Contains("MaxFileSize must be greater than 0", ex.Message);
    }

    [Fact]
    public void XTinyLogAsync_JsonConfiguration_ShouldHandleMultipleValidationErrors()
    {
        // Test that multiple validation errors are reported together for async sink
        var ex = Assert.Throws<ArgumentException>(() =>
        {
            var logger = new LoggerConfiguration()
                .WriteTo.XTinyLogAsync(
                    logDirectory: "",
                    applicationName: "",
                    maxFileSize: 0)
                .CreateLogger();
        });

        // Should contain all validation errors
        Assert.Contains("LogDirectory cannot be empty", ex.Message);
        Assert.Contains("ApplicationName cannot be empty", ex.Message);
        Assert.Contains("MaxFileSize must be greater than 0", ex.Message);
    }

    [Fact]
    public void XTinyLog_JsonConfiguration_ShouldWorkWithTypicalAppsettingsValues()
    {
        // Test with values that would typically come from appsettings.json
        var logger = new LoggerConfiguration()
            .WriteTo.XTinyLog(
                logDirectory: @"C:\Logs\MyApp",
                applicationName: "MyApplication",
                maxFileSize: 5120, // 5MB
                includeProperties: true,
                propertyFormat: PropertyFormatMode.Structured)
            .CreateLogger();

        Assert.NotNull(logger);
        
        // Test that the logger can write structured log entries
        logger.Information("User {UserId} performed {Action} at {Timestamp}", 
            123, "Login", DateTimeOffset.Now);
        
        logger.Dispose();
    }
}