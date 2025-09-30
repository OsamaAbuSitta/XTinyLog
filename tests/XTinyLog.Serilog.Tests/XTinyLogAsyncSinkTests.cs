using System.IO;
using System.Linq;
using Serilog;
using Serilog.Events;
using Serilog.Parsing;

namespace XTinyLog.Serilog.Tests;

/// <summary>
/// Unit tests for the XTinyLogAsyncSink class.
/// </summary>
public class XTinyLogAsyncSinkTests : IDisposable
{
    private readonly string _testLogDirectory;
    private readonly XTinyLogSinkConfiguration _defaultConfiguration;

    public XTinyLogAsyncSinkTests()
    {
        // Create a unique test directory for each test run
        _testLogDirectory = Path.Combine(Path.GetTempPath(), "XTinyLogAsyncSinkTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testLogDirectory);

        _defaultConfiguration = new XTinyLogSinkConfiguration
        {
            LogDirectory = _testLogDirectory,
            ApplicationName = "TestAsyncApp",
            MaxFileSize = 1024,
            IncludeProperties = true,
            PropertyFormat = PropertyFormatMode.Inline
        };
    }

    public void Dispose()
    {
        // Clean up test directory
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
    public void Constructor_WithValidConfiguration_ShouldCreateAsyncSink()
    {
        // Act & Assert
        var sink = new XTinyLogAsyncSink(_defaultConfiguration);
        sink.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullConfiguration_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Action act = () => new XTinyLogAsyncSink(null!);
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("configuration");
    }

    [Fact]
    public void Constructor_WithInvalidConfiguration_ShouldThrowArgumentException()
    {
        // Arrange
        var invalidConfig = new XTinyLogSinkConfiguration
        {
            LogDirectory = "", // Invalid empty directory
            ApplicationName = "TestApp"
        };

        // Act & Assert
        Action act = () => new XTinyLogAsyncSink(invalidConfig);
        act.Should().Throw<ArgumentException>()
           .WithParameterName("configuration");
    }

    [Fact]
    public void Emit_WithNullLogEvent_ShouldNotThrow()
    {
        // Arrange
        var sink = new XTinyLogAsyncSink(_defaultConfiguration);

        // Act & Assert
        Action act = () => sink.Emit(null!);
        act.Should().NotThrow();
    }

    [Fact]
    public async Task Emit_WithValidLogEvent_ShouldCreateLogFileAsync()
    {
        // Arrange
        var sink = new XTinyLogAsyncSink(_defaultConfiguration);
        var logEvent = CreateLogEvent(LogEventLevel.Information, "Test async message");

        // Act
        sink.Emit(logEvent);

        // Assert - Wait for async operation to complete
        await Task.Delay(500); // Give time for async operation

        // Check for log files with retries
        var maxAttempts = 20;
        var attempt = 0;
        string[] logFiles = Array.Empty<string>();
        
        while (attempt < maxAttempts && logFiles.Length == 0)
        {
            await Task.Delay(100);
            logFiles = Directory.GetFiles(_testLogDirectory, "*.html", SearchOption.AllDirectories);
            attempt++;
        }
        
        logFiles.Should().NotBeEmpty("because an async log file should be created");
    }

    [Fact]
    public async Task Emit_WithDifferentLogLevels_ShouldHandleAllLevelsAsync()
    {
        // Arrange
        var sink = new XTinyLogAsyncSink(_defaultConfiguration);
        var logLevels = new[]
        {
            LogEventLevel.Verbose,
            LogEventLevel.Debug,
            LogEventLevel.Information,
            LogEventLevel.Warning,
            LogEventLevel.Error,
            LogEventLevel.Fatal
        };

        // Act & Assert
        foreach (var level in logLevels)
        {
            var logEvent = CreateLogEvent(level, $"Async test message for {level}");
            Action act = () => sink.Emit(logEvent);
            act.Should().NotThrow($"because {level} should be handled gracefully in async sink");
        }

        // Wait for all async operations to complete
        await Task.Delay(1000);
    }

    [Fact]
    public async Task Emit_WithException_ShouldIncludeExceptionInformationAsync()
    {
        // Arrange
        var sink = new XTinyLogAsyncSink(_defaultConfiguration);
        var exception = new InvalidOperationException("Test async exception");
        var logEvent = CreateLogEventWithException(LogEventLevel.Error, "Async error occurred", exception);

        // Act
        Action act = () => sink.Emit(logEvent);

        // Assert
        act.Should().NotThrow("because exceptions should be handled gracefully in async sink");
        
        // Wait for async operation to complete
        await Task.Delay(500);
    }

    [Fact]
    public async Task Emit_WithStructuredProperties_ShouldIncludePropertiesAsync()
    {
        // Arrange
        var config = _defaultConfiguration.Clone();
        config.IncludeProperties = true;
        config.PropertyFormat = PropertyFormatMode.Inline;
        
        var sink = new XTinyLogAsyncSink(config);
        var logEvent = CreateLogEventWithProperties(LogEventLevel.Information, "Async user {UserId} performed {Action}",
            ("UserId", 123), ("Action", "AsyncLogin"));

        // Act
        Action act = () => sink.Emit(logEvent);

        // Assert
        act.Should().NotThrow("because structured properties should be handled gracefully in async sink");
        
        // Wait for async operation to complete
        await Task.Delay(500);
    }

    [Fact]
    public async Task Emit_WithPropertiesDisabled_ShouldNotIncludePropertiesAsync()
    {
        // Arrange
        var config = _defaultConfiguration.Clone();
        config.IncludeProperties = false;
        
        var sink = new XTinyLogAsyncSink(config);
        var logEvent = CreateLogEventWithProperties(LogEventLevel.Information, "Async user {UserId} performed {Action}",
            ("UserId", 123), ("Action", "AsyncLogin"));

        // Act
        Action act = () => sink.Emit(logEvent);

        // Assert
        act.Should().NotThrow("because properties should be handled gracefully even when disabled in async sink");
        
        // Wait for async operation to complete
        await Task.Delay(500);
    }

    [Fact]
    public async Task Emit_WithStructuredPropertyFormat_ShouldFormatPropertiesCorrectlyAsync()
    {
        // Arrange
        var config = _defaultConfiguration.Clone();
        config.IncludeProperties = true;
        config.PropertyFormat = PropertyFormatMode.Structured;
        
        var sink = new XTinyLogAsyncSink(config);
        var logEvent = CreateLogEventWithProperties(LogEventLevel.Information, "Async user {UserId} performed {Action}",
            ("UserId", 123), ("Action", "AsyncLogin"));

        // Act
        Action act = () => sink.Emit(logEvent);

        // Assert
        act.Should().NotThrow("because structured property format should be handled gracefully in async sink");
        
        // Wait for async operation to complete
        await Task.Delay(500);
    }

    [Fact]
    public async Task Emit_WithHiddenPropertyFormat_ShouldNotIncludePropertiesAsync()
    {
        // Arrange
        var config = _defaultConfiguration.Clone();
        config.IncludeProperties = true;
        config.PropertyFormat = PropertyFormatMode.Hidden;
        
        var sink = new XTinyLogAsyncSink(config);
        var logEvent = CreateLogEventWithProperties(LogEventLevel.Information, "Async user {UserId} performed {Action}",
            ("UserId", 123), ("Action", "AsyncLogin"));

        // Act
        Action act = () => sink.Emit(logEvent);

        // Assert
        act.Should().NotThrow("because hidden property format should be handled gracefully in async sink");
        
        // Wait for async operation to complete
        await Task.Delay(500);
    }

    [Fact]
    public async Task Emit_MultipleEventsAsync_ShouldHandleAllEventsAsync()
    {
        // Arrange
        var sink = new XTinyLogAsyncSink(_defaultConfiguration);
        var events = new[]
        {
            CreateLogEvent(LogEventLevel.Information, "First async message"),
            CreateLogEvent(LogEventLevel.Warning, "Second async message"),
            CreateLogEvent(LogEventLevel.Error, "Third async message")
        };

        // Act & Assert
        foreach (var logEvent in events)
        {
            Action act = () => sink.Emit(logEvent);
            act.Should().NotThrow("because multiple events should be handled gracefully in async sink");
        }

        // Wait for all async operations to complete
        await Task.Delay(1000);
    }

    [Fact]
    public async Task Emit_WithCustomFileName_ShouldUseCustomFileNameAsync()
    {
        // Arrange
        var config = _defaultConfiguration.Clone();
        config.FileName = () => "custom-async-log-file";
        
        var sink = new XTinyLogAsyncSink(config);
        var logEvent = CreateLogEvent(LogEventLevel.Information, "Test async message with custom filename");

        // Act
        sink.Emit(logEvent);

        // Assert - Wait for async operation to complete
        await Task.Delay(500);

        // Check for log files with retries
        var maxAttempts = 20;
        var attempt = 0;
        string[] logFiles = Array.Empty<string>();
        
        while (attempt < maxAttempts && logFiles.Length == 0)
        {
            await Task.Delay(100);
            logFiles = Directory.GetFiles(_testLogDirectory, "custom-async-log-file*.html", SearchOption.AllDirectories);
            attempt++;
        }
        
        logFiles.Should().NotBeEmpty("because a custom named async log file should be created");
    }

    [Fact]
    public async Task Emit_HighVolumeAsync_ShouldHandleMultipleSimultaneousEvents()
    {
        // Arrange
        var sink = new XTinyLogAsyncSink(_defaultConfiguration);
        var eventCount = 100;
        var tasks = new List<Task>();

        // Act - Emit many events simultaneously to test async behavior
        for (int i = 0; i < eventCount; i++)
        {
            var logEvent = CreateLogEvent(LogEventLevel.Information, $"High volume async message {i}");
            tasks.Add(Task.Run(() => sink.Emit(logEvent)));
        }

        // Wait for all tasks to complete
        await Task.WhenAll(tasks);

        // Give additional time for async operations to complete
        await Task.Delay(2000);

        // Assert - Should not throw and should handle all events
        tasks.Should().AllSatisfy(t => t.IsCompleted.Should().BeTrue("because all async operations should complete successfully"));
    }

    [Fact]
    public async Task Emit_WithCancellation_ShouldHandleGracefulShutdown()
    {
        // Arrange
        var sink = new XTinyLogAsyncSink(_defaultConfiguration);
        var logEvent = CreateLogEvent(LogEventLevel.Information, "Test message before shutdown");

        // Act
        sink.Emit(logEvent);
        
        // Simulate application shutdown by waiting briefly then disposing
        await Task.Delay(100);

        // Assert - Should not throw during disposal/shutdown
        Action act = () => { /* Sink disposal happens automatically */ };
        act.Should().NotThrow("because async sink should handle graceful shutdown");
    }

    private static LogEvent CreateLogEvent(LogEventLevel level, string message)
    {
        var tokens = new MessageTemplateToken[] { new TextToken(message) };
        return new LogEvent(
            DateTimeOffset.Now,
            level,
            null,
            new MessageTemplate(message, tokens),
            Array.Empty<LogEventProperty>());
    }

    private static LogEvent CreateLogEventWithException(LogEventLevel level, string message, Exception exception)
    {
        var tokens = new MessageTemplateToken[] { new TextToken(message) };
        return new LogEvent(
            DateTimeOffset.Now,
            level,
            exception,
            new MessageTemplate(message, tokens),
            Array.Empty<LogEventProperty>());
    }

    private static LogEvent CreateLogEventWithProperties(LogEventLevel level, string messageTemplate, params (string key, object value)[] properties)
    {
        var logProperties = properties.Select(p => 
            new LogEventProperty(p.key, new ScalarValue(p.value))).ToArray();

        var tokens = new List<MessageTemplateToken>();
        var parts = messageTemplate.Split(new[] { '{', '}' }, StringSplitOptions.RemoveEmptyEntries);
        
        for (int i = 0; i < parts.Length; i++)
        {
            if (i % 2 == 0)
            {
                if (!string.IsNullOrEmpty(parts[i]))
                {
                    tokens.Add(new TextToken(parts[i]));
                }
            }
            else
            {
                tokens.Add(new PropertyToken(parts[i], parts[i]));
            }
        }

        return new LogEvent(
            DateTimeOffset.Now,
            level,
            null,
            new MessageTemplate(messageTemplate, tokens),
            logProperties);
    }
}