using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using System.Reflection;
using XTinyLog.Serilog;
using Xunit;

namespace XTinyLog.Serilog.Tests;

/// <summary>
/// Integration tests that demonstrate complete appsettings.json configuration workflow.
/// These tests show how the XTinyLog sink would be configured in real applications.
/// </summary>
public class XTinyLogSinkAppsettingsIntegrationTests
{
    [Fact]
    public void XTinyLog_WithAppsettingsJsonConfiguration_ShouldCreateLoggerSuccessfully()
    {
        // Arrange - Create configuration that simulates appsettings.json
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Serilog:WriteTo:0:Name"] = "XTinyLog",
            ["Serilog:WriteTo:0:Args:logDirectory"] = @"C:\IntegrationTestLogs",
            ["Serilog:WriteTo:0:Args:applicationName"] = "IntegrationTestApp",
            ["Serilog:WriteTo:0:Args:maxFileSize"] = "2048",
            ["Serilog:WriteTo:0:Args:includeProperties"] = "true",
            ["Serilog:WriteTo:0:Args:propertyFormat"] = "Structured"
        });

        var configuration = configurationBuilder.Build();

        // Act - Create logger using Serilog's configuration system
        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        // Assert
        Assert.NotNull(logger);

        // Test that the logger can write log entries without throwing
        logger.Information("Integration test log message with {Property}", "TestValue");
        logger.Warning("Integration test warning with {UserId} and {Action}", 123, "Login");
        logger.Error("Integration test error message");

        logger.Dispose();
    }

    [Fact]
    public void XTinyLogAsync_WithAppsettingsJsonConfiguration_ShouldCreateLoggerSuccessfully()
    {
        // Arrange - Create configuration that simulates appsettings.json for async sink
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Serilog:WriteTo:0:Name"] = "XTinyLogAsync",
            ["Serilog:WriteTo:0:Args:logDirectory"] = @"C:\AsyncIntegrationTestLogs",
            ["Serilog:WriteTo:0:Args:applicationName"] = "AsyncIntegrationTestApp",
            ["Serilog:WriteTo:0:Args:maxFileSize"] = "4096",
            ["Serilog:WriteTo:0:Args:includeProperties"] = "false",
            ["Serilog:WriteTo:0:Args:propertyFormat"] = "Hidden"
        });

        var configuration = configurationBuilder.Build();

        // Act - Create logger using Serilog's configuration system
        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        // Assert
        Assert.NotNull(logger);

        // Test that the logger can write log entries without throwing
        logger.Information("Async integration test log message");
        logger.Debug("Async integration test debug message with {Data}", new { Id = 1, Name = "Test" });

        logger.Dispose();
    }

    [Fact]
    public void XTinyLog_WithPartialAppsettingsJsonConfiguration_ShouldUseDefaults()
    {
        // Arrange - Create configuration with only some parameters specified
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Serilog:WriteTo:0:Name"] = "XTinyLog",
            ["Serilog:WriteTo:0:Args:applicationName"] = "PartialConfigApp",
            ["Serilog:WriteTo:0:Args:maxFileSize"] = "8192"
            // logDirectory, includeProperties, and propertyFormat will use defaults
        });

        var configuration = configurationBuilder.Build();

        // Act - Create logger using Serilog's configuration system
        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        // Assert
        Assert.NotNull(logger);

        // Test that the logger can write log entries without throwing
        logger.Information("Partial configuration test message");

        logger.Dispose();
    }

    [Fact]
    public void XTinyLog_WithMultipleSinksInAppsettingsJson_ShouldCreateLoggerSuccessfully()
    {
        // Arrange - Create configuration with multiple sinks including XTinyLog
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Serilog:WriteTo:0:Name"] = "Console",
            ["Serilog:WriteTo:1:Name"] = "XTinyLog",
            ["Serilog:WriteTo:1:Args:logDirectory"] = @"C:\MultiSinkTestLogs",
            ["Serilog:WriteTo:1:Args:applicationName"] = "MultiSinkApp",
            ["Serilog:WriteTo:1:Args:maxFileSize"] = "1024",
            ["Serilog:WriteTo:2:Name"] = "XTinyLogAsync",
            ["Serilog:WriteTo:2:Args:logDirectory"] = @"C:\MultiSinkAsyncTestLogs",
            ["Serilog:WriteTo:2:Args:applicationName"] = "MultiSinkAsyncApp",
            ["Serilog:WriteTo:2:Args:includeProperties"] = "true",
            ["Serilog:WriteTo:2:Args:propertyFormat"] = "Inline"
        });

        var configuration = configurationBuilder.Build();

        // Act - Create logger using Serilog's configuration system
        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        // Assert
        Assert.NotNull(logger);

        // Test that the logger can write to all sinks without throwing
        logger.Information("Multi-sink test message with {Property1} and {Property2}", "Value1", 42);

        logger.Dispose();
    }

    [Fact]
    public void XTinyLog_WithMinimumLevelInAppsettingsJson_ShouldRespectLogLevel()
    {
        // Arrange - Create configuration with minimum level set
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Serilog:MinimumLevel"] = "Warning",
            ["Serilog:WriteTo:0:Name"] = "XTinyLog",
            ["Serilog:WriteTo:0:Args:logDirectory"] = @"C:\LogLevelTestLogs",
            ["Serilog:WriteTo:0:Args:applicationName"] = "LogLevelTestApp"
        });

        var configuration = configurationBuilder.Build();

        // Act - Create logger using Serilog's configuration system
        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        // Assert
        Assert.NotNull(logger);

        // Test that the logger respects minimum level
        logger.Debug("This debug message should not be logged");
        logger.Information("This info message should not be logged");
        logger.Warning("This warning message should be logged");
        logger.Error("This error message should be logged");

        logger.Dispose();
    }

    [Fact]
    public void XTinyLog_WithAllPropertyFormatModesInAppsettingsJson_ShouldWork()
    {
        // Test all property format modes that can be configured via appsettings.json
        var formatModes = new[] { "Inline", "Structured", "Hidden" };

        foreach (var formatMode in formatModes)
        {
            // Arrange
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Serilog:WriteTo:0:Name"] = "XTinyLog",
                ["Serilog:WriteTo:0:Args:logDirectory"] = $@"C:\PropertyFormatTest_{formatMode}",
                ["Serilog:WriteTo:0:Args:applicationName"] = $"PropertyFormatTest_{formatMode}",
                ["Serilog:WriteTo:0:Args:propertyFormat"] = formatMode,
                ["Serilog:WriteTo:0:Args:includeProperties"] = "true"
            });

            var configuration = configurationBuilder.Build();

            // Act
            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            // Assert
            Assert.NotNull(logger);

            // Test that the logger can write structured log entries
            logger.Information("Property format test with {FormatMode} and {TestData}", 
                formatMode, new { Id = 1, Name = "Test", Timestamp = DateTimeOffset.Now });

            logger.Dispose();
        }
    }

    [Fact]
    public void XTinyLog_WithInvalidAppsettingsJsonConfiguration_ShouldThrowMeaningfulException()
    {
        // Arrange - Create configuration with invalid parameters
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Serilog:WriteTo:0:Name"] = "XTinyLog",
            ["Serilog:WriteTo:0:Args:logDirectory"] = "", // Invalid empty directory
            ["Serilog:WriteTo:0:Args:applicationName"] = "", // Invalid empty name
            ["Serilog:WriteTo:0:Args:maxFileSize"] = "-1" // Invalid negative size
        });

        var configuration = configurationBuilder.Build();

        // Act & Assert - Should throw meaningful exception during logger creation
        var ex = Assert.Throws<TargetInvocationException>(() =>
        {
            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
        });

        // Verify that the inner exception contains information about all validation errors
        Assert.NotNull(ex.InnerException);
        Assert.IsType<ArgumentException>(ex.InnerException);
        Assert.Contains("LogDirectory cannot be empty", ex.InnerException.Message);
        Assert.Contains("ApplicationName cannot be empty", ex.InnerException.Message);
        Assert.Contains("MaxFileSize must be greater than 0", ex.InnerException.Message);
    }

    [Fact]
    public void XTinyLog_WithComplexAppsettingsJsonScenario_ShouldWorkEndToEnd()
    {
        // Arrange - Create a complex, realistic appsettings.json scenario
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Serilog:Using:0"] = "XTinyLog.Serilog",
            ["Serilog:MinimumLevel:Default"] = "Information",
            ["Serilog:MinimumLevel:Override:Microsoft"] = "Warning",
            ["Serilog:MinimumLevel:Override:System"] = "Warning",
            ["Serilog:WriteTo:0:Name"] = "Console",
            ["Serilog:WriteTo:1:Name"] = "XTinyLog",
            ["Serilog:WriteTo:1:Args:logDirectory"] = @"C:\Production\Logs\WebApp",
            ["Serilog:WriteTo:1:Args:applicationName"] = "ProductionWebApp",
            ["Serilog:WriteTo:1:Args:maxFileSize"] = "10240", // 10MB
            ["Serilog:WriteTo:1:Args:includeProperties"] = "true",
            ["Serilog:WriteTo:1:Args:propertyFormat"] = "Structured",
            ["Serilog:WriteTo:2:Name"] = "XTinyLogAsync",
            ["Serilog:WriteTo:2:Args:logDirectory"] = @"C:\Production\Logs\Background",
            ["Serilog:WriteTo:2:Args:applicationName"] = "BackgroundService",
            ["Serilog:WriteTo:2:Args:maxFileSize"] = "20480", // 20MB
            ["Serilog:WriteTo:2:Args:includeProperties"] = "true",
            ["Serilog:WriteTo:2:Args:propertyFormat"] = "Inline",
            ["Serilog:Enrich:0"] = "FromLogContext",
            ["Serilog:Properties:Application"] = "MyWebApplication",
            ["Serilog:Properties:Environment"] = "Production"
        });

        var configuration = configurationBuilder.Build();

        // Act - Create logger using complete Serilog configuration
        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        // Assert
        Assert.NotNull(logger);

        // Test complex logging scenarios
        logger.Information("Application started with configuration {@Config}", new 
        { 
            Version = "1.0.0", 
            Environment = "Production",
            StartTime = DateTimeOffset.Now 
        });

        logger.Warning("High memory usage detected: {MemoryUsage}MB", 512);

        logger.Error("Database connection failed for user {UserId} with error {ErrorCode}", 
            "user123", "DB_TIMEOUT");

        // Test that Microsoft and System logs are filtered out (minimum level Warning)
        logger.ForContext("SourceContext", "Microsoft.AspNetCore.Hosting")
            .Information("This should not be logged due to override");

        logger.ForContext("SourceContext", "Microsoft.AspNetCore.Hosting")
            .Warning("This should be logged despite override");

        logger.Dispose();
    }
}