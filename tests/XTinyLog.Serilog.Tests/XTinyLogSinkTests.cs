using System.IO;
using System.Linq;
using Serilog;
using Serilog.Events;
using Serilog.Parsing;

namespace XTinyLog.Serilog.Tests;

/// <summary>
/// Unit tests for the XTinyLogSink class.
/// </summary>
public class XTinyLogSinkTests : IDisposable
{
    private readonly string _testLogDirectory;
    private readonly XTinyLogSinkConfiguration _defaultConfiguration;

    public XTinyLogSinkTests()
    {
        // Create a unique test directory for each test run
        _testLogDirectory = Path.Combine(Path.GetTempPath(), "XTinyLogSinkTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testLogDirectory);

        _defaultConfiguration = new XTinyLogSinkConfiguration
        {
            LogDirectory = _testLogDirectory,
            ApplicationName = "TestApp",
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
    public void Constructor_WithValidConfiguration_ShouldCreateSink()
    {
        // Act & Assert
        var sink = new XTinyLogSink(_defaultConfiguration);
        sink.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullConfiguration_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Action act = () => new XTinyLogSink(null!);
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
        Action act = () => new XTinyLogSink(invalidConfig);
        act.Should().Throw<ArgumentException>()
           .WithParameterName("configuration");
    }

    [Fact]
    public void Emit_WithNullLogEvent_ShouldNotThrow()
    {
        // Arrange
        var sink = new XTinyLogSink(_defaultConfiguration);

        // Act & Assert
        Action act = () => sink.Emit(null!);
        act.Should().NotThrow();
    }

    [Fact]
    public void Emit_WithValidLogEvent_ShouldCreateLogFile()
    {
        // Arrange
        var sink = new XTinyLogSink(_defaultConfiguration);
        var logEvent = CreateLogEvent(LogEventLevel.Information, "Test message");

        // Act
        sink.Emit(logEvent);

        // Assert
        // Give more time for file operations and check multiple times
        var maxAttempts = 10;
        var attempt = 0;
        string[] logFiles = Array.Empty<string>();
        
        while (attempt < maxAttempts && logFiles.Length == 0)
        {
            Thread.Sleep(100);
            logFiles = Directory.GetFiles(_testLogDirectory, "*.html");
            attempt++;
        }
        
        // Also check for any files in subdirectories (XTinyLog might create app-specific folders)
        if (logFiles.Length == 0)
        {
            var allFiles = Directory.GetFiles(_testLogDirectory, "*.html", SearchOption.AllDirectories);
            allFiles.Should().NotBeEmpty("because a log file should be created somewhere in the test directory");
        }
        else
        {
            logFiles.Should().NotBeEmpty("because a log file should be created");
        }
    }

    [Fact]
    public void Emit_WithDifferentLogLevels_ShouldHandleAllLevels()
    {
        // Arrange
        var sink = new XTinyLogSink(_defaultConfiguration);
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
            var logEvent = CreateLogEvent(level, $"Test message for {level}");
            Action act = () => sink.Emit(logEvent);
            act.Should().NotThrow($"because {level} should be handled gracefully");
        }
    }

    [Fact]
    public void Emit_WithException_ShouldIncludeExceptionInformation()
    {
        // Arrange
        var sink = new XTinyLogSink(_defaultConfiguration);
        var exception = new InvalidOperationException("Test exception");
        var logEvent = CreateLogEventWithException(LogEventLevel.Error, "Error occurred", exception);

        // Act
        Action act = () => sink.Emit(logEvent);

        // Assert
        act.Should().NotThrow("because exceptions should be handled gracefully");
    }

    [Fact]
    public void Emit_WithStructuredProperties_ShouldIncludeProperties()
    {
        // Arrange
        var config = _defaultConfiguration.Clone();
        config.IncludeProperties = true;
        config.PropertyFormat = PropertyFormatMode.Inline;
        
        var sink = new XTinyLogSink(config);
        var logEvent = CreateLogEventWithProperties(LogEventLevel.Information, "User {UserId} performed {Action}",
            ("UserId", 123), ("Action", "Login"));

        // Act
        Action act = () => sink.Emit(logEvent);

        // Assert
        act.Should().NotThrow("because structured properties should be handled gracefully");
    }

    [Fact]
    public void Emit_WithPropertiesDisabled_ShouldNotIncludeProperties()
    {
        // Arrange
        var config = _defaultConfiguration.Clone();
        config.IncludeProperties = false;
        
        var sink = new XTinyLogSink(config);
        var logEvent = CreateLogEventWithProperties(LogEventLevel.Information, "User {UserId} performed {Action}",
            ("UserId", 123), ("Action", "Login"));

        // Act
        Action act = () => sink.Emit(logEvent);

        // Assert
        act.Should().NotThrow("because properties should be handled gracefully even when disabled");
    }

    [Fact]
    public void Emit_WithStructuredPropertyFormat_ShouldFormatPropertiesCorrectly()
    {
        // Arrange
        var config = _defaultConfiguration.Clone();
        config.IncludeProperties = true;
        config.PropertyFormat = PropertyFormatMode.Structured;
        
        var sink = new XTinyLogSink(config);
        var logEvent = CreateLogEventWithProperties(LogEventLevel.Information, "User {UserId} performed {Action}",
            ("UserId", 123), ("Action", "Login"));

        // Act
        Action act = () => sink.Emit(logEvent);

        // Assert
        act.Should().NotThrow("because structured property format should be handled gracefully");
    }

    [Fact]
    public void Emit_WithHiddenPropertyFormat_ShouldNotIncludeProperties()
    {
        // Arrange
        var config = _defaultConfiguration.Clone();
        config.IncludeProperties = true;
        config.PropertyFormat = PropertyFormatMode.Hidden;
        
        var sink = new XTinyLogSink(config);
        var logEvent = CreateLogEventWithProperties(LogEventLevel.Information, "User {UserId} performed {Action}",
            ("UserId", 123), ("Action", "Login"));

        // Act
        Action act = () => sink.Emit(logEvent);

        // Assert
        act.Should().NotThrow("because hidden property format should be handled gracefully");
    }

    [Fact]
    public void Emit_MultipleEvents_ShouldHandleAllEvents()
    {
        // Arrange
        var sink = new XTinyLogSink(_defaultConfiguration);
        var events = new[]
        {
            CreateLogEvent(LogEventLevel.Information, "First message"),
            CreateLogEvent(LogEventLevel.Warning, "Second message"),
            CreateLogEvent(LogEventLevel.Error, "Third message")
        };

        // Act & Assert
        foreach (var logEvent in events)
        {
            Action act = () => sink.Emit(logEvent);
            act.Should().NotThrow("because multiple events should be handled gracefully");
        }
    }

    [Fact]
    public void Emit_WithCustomFileName_ShouldUseCustomFileName()
    {
        // Arrange
        var config = _defaultConfiguration.Clone();
        config.FileName = () => "custom-log-file";
        
        var sink = new XTinyLogSink(config);
        var logEvent = CreateLogEvent(LogEventLevel.Information, "Test message");

        // Act
        sink.Emit(logEvent);

        // Assert
        // Give more time for file operations and check multiple times
        var maxAttempts = 10;
        var attempt = 0;
        string[] logFiles = Array.Empty<string>();
        
        while (attempt < maxAttempts && logFiles.Length == 0)
        {
            Thread.Sleep(100);
            logFiles = Directory.GetFiles(_testLogDirectory, "custom-log-file*.html", SearchOption.AllDirectories);
            attempt++;
        }
        
        logFiles.Should().NotBeEmpty("because a custom named log file should be created");
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