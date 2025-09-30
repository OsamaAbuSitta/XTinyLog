using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog.Events;
using Serilog.Parsing;

namespace XTinyLog.Serilog.Tests;

/// <summary>
/// Tests for error handling scenarios in XTinyLog Serilog sink components.
/// </summary>
public class ErrorHandlingTests
{
    [Fact]
    public void LogEventFormatter_FormatLogEvent_HandlesNullLogEvent()
    {
        // Act
        var result = LogEventFormatter.FormatLogEvent(null);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void LogEventFormatter_FormatLogEvent_HandlesExceptionWithNullMessage()
    {
        // Arrange
        var exception = new InvalidOperationException(null); // Exception with null message
        var logEvent = CreateLogEvent("Test message", LogEventLevel.Error, exception);

        // Act
        var result = LogEventFormatter.FormatLogEvent(logEvent);

        // Assert
        result.Should().Contain("InvalidOperationException: Exception of type &#39;System.InvalidOperationException&#39; was thrown.");
        result.Should().Contain("Test message");
    }

    [Fact]
    public void LogEventFormatter_FormatLogEvent_HandlesExceptionWithNullStackTrace()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception");
        var logEvent = CreateLogEvent("Test message", LogEventLevel.Error, exception);

        // Act
        var result = LogEventFormatter.FormatLogEvent(logEvent);

        // Assert
        result.Should().Contain("InvalidOperationException: Test exception");
        result.Should().Contain("Test message");
    }

    [Fact]
    public void LogEventFormatter_FormatLogEvent_HandlesNestedExceptions()
    {
        // Arrange
        var innerException = new ArgumentException("Inner exception");
        var outerException = new InvalidOperationException("Outer exception", innerException);
        var logEvent = CreateLogEvent("Test message", LogEventLevel.Error, outerException);

        // Act
        var result = LogEventFormatter.FormatLogEvent(logEvent);

        // Assert
        result.Should().Contain("InvalidOperationException: Outer exception");
        result.Should().Contain("Inner Exception:");
        result.Should().Contain("ArgumentException: Inner exception");
    }

    [Fact]
    public void LogEventFormatter_FormatLogEvent_HandlesPropertiesWithNullValues()
    {
        // Arrange
        var properties = new Dictionary<string, LogEventPropertyValue>
        {
            ["TestProperty"] = new ScalarValue("test value"),
            ["NullProperty"] = new ScalarValue(null),
        };

        var logEvent = CreateLogEventWithProperties("Test message", properties);

        // Act
        var result = LogEventFormatter.FormatLogEvent(logEvent, includeProperties: true);

        // Assert
        result.Should().Contain("TestProperty=&quot;test value&quot;");
        result.Should().Contain("NullProperty=null");
    }

    [Fact]
    public void LogLevelMapper_LogMessage_HandlesNullXLog()
    {
        // Act & Assert - Should not throw
        LogLevelMapper.LogMessage(null, LogEventLevel.Information, "test message");
    }

    [Fact]
    public void LogLevelMapper_LogMessage_HandlesEmptyMessage()
    {
        // Arrange
        using var tempDir = new TempDirectory();
        var config = CreateValidConfiguration(tempDir.Path);
        var xLog = XLog.Create(builder => builder
            .WithLogDirectory(config.LogDirectory)
            .WithApplicationName(config.ApplicationName));

        // Act & Assert - Should not throw
        LogLevelMapper.LogMessage(xLog, LogEventLevel.Information, null);
        LogLevelMapper.LogMessage(xLog, LogEventLevel.Information, string.Empty);
        LogLevelMapper.LogMessage(xLog, LogEventLevel.Information, "   ");
    }

    [Fact]
    public async Task LogLevelMapper_LogMessageAsync_HandlesNullXLog()
    {
        // Act & Assert - Should not throw
        await LogLevelMapper.LogMessageAsync(null, LogEventLevel.Information, "test message");
    }

    [Fact]
    public async Task LogLevelMapper_LogMessageAsync_HandlesEmptyMessage()
    {
        // Arrange
        using var tempDir = new TempDirectory();
        var config = CreateValidConfiguration(tempDir.Path);
        var xLog = XLog.Create(builder => builder
            .WithLogDirectory(config.LogDirectory)
            .WithApplicationName(config.ApplicationName));

        // Act & Assert - Should not throw
        await LogLevelMapper.LogMessageAsync(xLog, LogEventLevel.Information, null);
        await LogLevelMapper.LogMessageAsync(xLog, LogEventLevel.Information, string.Empty);
        await LogLevelMapper.LogMessageAsync(xLog, LogEventLevel.Information, "   ");
    }

    [Fact]
    public async Task LogLevelMapper_LogMessageAsync_HandlesCancellation()
    {
        // Arrange
        using var tempDir = new TempDirectory();
        var config = CreateValidConfiguration(tempDir.Path);
        var xLog = XLog.Create(builder => builder
            .WithLogDirectory(config.LogDirectory)
            .WithApplicationName(config.ApplicationName));

        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        // Act & Assert - Should handle cancellation gracefully
        await LogLevelMapper.LogMessageAsync(xLog, LogEventLevel.Information, "test message", cts.Token);
    }

    [Fact]
    public void XTinyLogSinkConfiguration_Validate_HandlesInvalidLogDirectory()
    {
        // Arrange
        var config = new XTinyLogSinkConfiguration
        {
            LogDirectory = string.Empty,
            ApplicationName = "TestApp",
        };

        // Act
        var errors = config.Validate();

        // Assert
        errors.Should().Contain("LogDirectory cannot be null or empty.");
    }

    [Fact]
    public void XTinyLogSinkConfiguration_Validate_HandlesInvalidApplicationName()
    {
        // Arrange
        var config = new XTinyLogSinkConfiguration
        {
            LogDirectory = Path.GetTempPath(),
            ApplicationName = "App<>Name", // Invalid characters
        };

        // Act
        var errors = config.Validate();

        // Assert
        errors.Should().Contain("ApplicationName contains invalid filename characters.");
    }

    [Fact]
    public void XTinyLogSinkConfiguration_Validate_HandlesInvalidMaxFileSize()
    {
        // Arrange
        var config = new XTinyLogSinkConfiguration
        {
            LogDirectory = Path.GetTempPath(),
            ApplicationName = "TestApp",
            MaxFileSize = -1,
        };

        // Act
        var errors = config.Validate();

        // Assert
        errors.Should().Contain("MaxFileSize must be greater than 0.");
    }

    [Fact]
    public void XTinyLogSinkConfiguration_Validate_HandlesInvalidFileNameFunction()
    {
        // Arrange
        var config = new XTinyLogSinkConfiguration
        {
            LogDirectory = Path.GetTempPath(),
            ApplicationName = "TestApp",
            FileName = () => "invalid<>filename", // Invalid characters
        };

        // Act
        var errors = config.Validate();

        // Assert
        errors.Should().Contain("FileName function returns a string with invalid filename characters.");
    }

    [Fact]
    public void XTinyLogSinkConfiguration_Validate_HandlesFileNameFunctionException()
    {
        // Arrange
        var config = new XTinyLogSinkConfiguration
        {
            LogDirectory = Path.GetTempPath(),
            ApplicationName = "TestApp",
            FileName = () => throw new InvalidOperationException("FileName function failed"),
        };

        // Act
        var errors = config.Validate();

        // Assert
        errors.Should().Contain("FileName function validation failed: FileName function failed");
    }

    [Fact]
    public void XTinyLogSinkConfiguration_ToLogConfiguration_HandlesInvalidConfiguration()
    {
        // Arrange
        var config = new XTinyLogSinkConfiguration
        {
            LogDirectory = string.Empty, // Invalid
            ApplicationName = "TestApp",
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => config.ToLogConfiguration());
        exception.Message.Should().Contain("Cannot convert invalid configuration");
    }

    [Fact]
    public void XTinyLogSink_Constructor_HandlesNullConfiguration()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new XTinyLogSink(null));
    }

    [Fact]
    public void XTinyLogSink_Constructor_HandlesInvalidConfiguration()
    {
        // Arrange
        var config = new XTinyLogSinkConfiguration
        {
            LogDirectory = string.Empty, // Invalid
            ApplicationName = "TestApp",
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new XTinyLogSink(config));
        exception.Message.Should().Contain("Invalid XTinyLogSink configuration");
    }

    [Fact]
    public void XTinyLogSink_Emit_HandlesNullLogEvent()
    {
        // Arrange
        using var tempDir = new TempDirectory();
        var config = CreateValidConfiguration(tempDir.Path);
        var sink = new XTinyLogSink(config);

        // Act & Assert - Should not throw
        sink.Emit(null);
    }

    [Fact]
    public void XTinyLogAsyncSink_Constructor_HandlesNullConfiguration()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new XTinyLogAsyncSink(null));
    }

    [Fact]
    public void XTinyLogAsyncSink_Constructor_HandlesInvalidConfiguration()
    {
        // Arrange
        var config = new XTinyLogSinkConfiguration
        {
            LogDirectory = string.Empty, // Invalid
            ApplicationName = "TestApp",
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new XTinyLogAsyncSink(config));
        exception.Message.Should().Contain("Invalid XTinyLogAsyncSink configuration");
    }

    [Fact]
    public void XTinyLogAsyncSink_Emit_HandlesNullLogEvent()
    {
        // Arrange
        using var tempDir = new TempDirectory();
        var config = CreateValidConfiguration(tempDir.Path);
        var sink = new XTinyLogAsyncSink(config);

        // Act & Assert - Should not throw
        sink.Emit(null);
    }

    // Helper methods
    private static XTinyLogSinkConfiguration CreateValidConfiguration(string? logDirectory = null)
    {
        return new XTinyLogSinkConfiguration
        {
            LogDirectory = logDirectory ?? Path.GetTempPath(),
            ApplicationName = "TestApp",
            MaxFileSize = 1024,
        };
    }

    private static LogEvent CreateLogEvent(
        string message,
        LogEventLevel level,
        Exception? exception = null,
        Dictionary<string, LogEventPropertyValue>? properties = null)
    {
        var messageTemplate = new MessageTemplateParser().Parse(message);
        var logEventProperties = properties?.Select(kvp => new LogEventProperty(kvp.Key, kvp.Value)) ?? Enumerable.Empty<LogEventProperty>();

        return new LogEvent(
            DateTimeOffset.Now,
            level,
            exception,
            messageTemplate,
            logEventProperties);
    }

    private static LogEvent CreateLogEventWithProperties(string message, Dictionary<string, LogEventPropertyValue> properties)
    {
        return CreateLogEvent(message, LogEventLevel.Information, null, properties);
    }

    /// <summary>
    /// Helper class for managing temporary directories in tests.
    /// </summary>
    private sealed class TempDirectory : IDisposable
    {
        public string Path { get; }

        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());
            Directory.CreateDirectory(Path);
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(Path))
                {
                    Directory.Delete(Path, true);
                }
            }
            catch
            {
                // Ignore cleanup errors in tests
            }
        }
    }

    [Fact]
    public void XTinyLogSinkConfiguration_Validate_HandlesEmptyLogDirectory()
    {
        // Arrange
        var config = new XTinyLogSinkConfiguration
        {
            LogDirectory = string.Empty, // Empty directory
            ApplicationName = "TestApp",
        };

        // Act
        var errors = config.Validate();

        // Assert
        errors.Should().Contain("LogDirectory cannot be null or empty.");
    }

    [Fact]
    public void XTinyLogSinkConfiguration_Validate_HandlesVeryLongApplicationName()
    {
        // Arrange
        var config = new XTinyLogSinkConfiguration
        {
            LogDirectory = Path.GetTempPath(),
            ApplicationName = new string('A', 300), // Too long
        };

        // Act
        var errors = config.Validate();

        // Assert
        errors.Should().Contain("ApplicationName is too long (maximum 255 characters).");
    }

    [Fact]
    public void XTinyLogSinkConfiguration_Validate_HandlesInvalidPropertyFormat()
    {
        // Arrange
        var config = new XTinyLogSinkConfiguration
        {
            LogDirectory = Path.GetTempPath(),
            ApplicationName = "TestApp",
            PropertyFormat = (PropertyFormatMode)999, // Invalid enum value
        };

        // Act
        var errors = config.Validate();

        // Assert
        errors.Should().Contain("PropertyFormat value '999' is not valid.");
    }

    [Fact]
    public void XTinyLogSinkConfiguration_Validate_HandlesFileNameFunctionReturningNull()
    {
        // Arrange
        var config = new XTinyLogSinkConfiguration
        {
            LogDirectory = Path.GetTempPath(),
            ApplicationName = "TestApp",
            FileName = () => null, // Returns null
        };

        // Act
        var errors = config.Validate();

        // Assert
        errors.Should().Contain("FileName function returns null or empty string.");
    }

    [Fact]
    public void XTinyLogSinkConfiguration_Validate_HandlesFileNameFunctionReturningEmpty()
    {
        // Arrange
        var config = new XTinyLogSinkConfiguration
        {
            LogDirectory = Path.GetTempPath(),
            ApplicationName = "TestApp",
            FileName = () => string.Empty, // Returns empty
        };

        // Act
        var errors = config.Validate();

        // Assert
        errors.Should().Contain("FileName function returns null or empty string.");
    }

    [Fact]
    public void XTinyLogSinkConfiguration_Validate_HandlesExtremelyLargeMaxFileSize()
    {
        // Arrange
        var config = new XTinyLogSinkConfiguration
        {
            LogDirectory = Path.GetTempPath(),
            ApplicationName = "TestApp",
            MaxFileSize = long.MaxValue, // Extremely large
        };

        // Act
        var errors = config.Validate();

        // Assert
        errors.Should().Contain("MaxFileSize is too large.");
    }

    [Fact]
    public void XTinyLogSinkConfiguration_Validate_HandlesValidConfiguration()
    {
        // Arrange
        var config = new XTinyLogSinkConfiguration
        {
            LogDirectory = Path.GetTempPath(),
            ApplicationName = "TestApp",
            MaxFileSize = 1024,
            FileName = () => "test.log",
            PropertyFormat = PropertyFormatMode.Inline,
        };

        // Act
        var errors = config.Validate();

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public void XTinyLogSinkConfiguration_ToLogConfiguration_HandlesValidationFailure()
    {
        // Arrange
        var config = new XTinyLogSinkConfiguration
        {
            LogDirectory = string.Empty, // Invalid
            ApplicationName = "TestApp",
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => config.ToLogConfiguration());
        exception.Message.Should().Contain("Cannot convert invalid configuration to ILogConfiguration");
        exception.Message.Should().Contain("LogDirectory cannot be null or empty");
    }

    [Fact]
    public void XTinyLogSinkConfiguration_Validate_HandlesMultipleErrors()
    {
        // Arrange
        var config = new XTinyLogSinkConfiguration
        {
            LogDirectory = string.Empty, // Invalid
            ApplicationName = string.Empty, // Invalid
            MaxFileSize = -1, // Invalid
        };

        // Act
        var errors = config.Validate();

        // Assert
        errors.Should().HaveCount(3);
        errors.Should().Contain("LogDirectory cannot be null or empty.");
        errors.Should().Contain("ApplicationName cannot be null or empty.");
        errors.Should().Contain("MaxFileSize must be greater than 0.");
    }}
