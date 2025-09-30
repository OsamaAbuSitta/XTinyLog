using System.Collections.Generic;
using System.Linq;
using Serilog.Events;
using Serilog.Parsing;

namespace XTinyLog.Serilog.Tests;

/// <summary>
/// Unit tests for the LogEventFormatter class.
/// </summary>
public class LogEventFormatterTests
{
    /// <summary>
    /// Tests basic message formatting without properties or exceptions.
    /// </summary>
    [Fact]
    public void FormatLogEvent_WithSimpleMessage_ShouldReturnFormattedMessage()
    {
        // Arrange
        var logEvent = CreateLogEvent("Hello World", LogEventLevel.Information);

        // Act
        var result = LogEventFormatter.FormatLogEvent(logEvent);

        // Assert
        result.Should().Be("Hello World");
    }

    /// <summary>
    /// Tests message formatting with HTML characters that need escaping.
    /// </summary>
    [Fact]
    public void FormatLogEvent_WithHtmlCharacters_ShouldEscapeHtml()
    {
        // Arrange
        var logEvent = CreateLogEvent("<script>alert('test')</script>", LogEventLevel.Information);

        // Act
        var result = LogEventFormatter.FormatLogEvent(logEvent);

        // Assert
        result.Should().Be("&lt;script&gt;alert(&#39;test&#39;)&lt;/script&gt;");
    }

    /// <summary>
    /// Tests message formatting with an exception.
    /// </summary>
    [Fact]
    public void FormatLogEvent_WithException_ShouldIncludeExceptionDetails()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception");
        var logEvent = CreateLogEvent("Error occurred", LogEventLevel.Error, exception);

        // Act
        var result = LogEventFormatter.FormatLogEvent(logEvent);

        // Assert
        result.Should().Contain("Error occurred");
        result.Should().Contain("Exception:");
        result.Should().Contain("InvalidOperationException: Test exception");
    }

    /// <summary>
    /// Tests message formatting with nested exceptions.
    /// </summary>
    [Fact]
    public void FormatLogEvent_WithNestedExceptions_ShouldIncludeAllExceptions()
    {
        // Arrange
        var innerException = new ArgumentException("Inner exception");
        var outerException = new InvalidOperationException("Outer exception", innerException);
        var logEvent = CreateLogEvent("Error occurred", LogEventLevel.Error, outerException);

        // Act
        var result = LogEventFormatter.FormatLogEvent(logEvent);

        // Assert
        result.Should().Contain("InvalidOperationException: Outer exception");
        result.Should().Contain("Inner Exception:");
        result.Should().Contain("ArgumentException: Inner exception");
    }

    /// <summary>
    /// Tests message formatting with properties in inline mode.
    /// </summary>
    [Fact]
    public void FormatLogEvent_WithPropertiesInline_ShouldIncludePropertiesInline()
    {
        // Arrange
        var properties = new Dictionary<string, LogEventPropertyValue>
        {
            ["UserId"] = new ScalarValue(123),
            ["UserName"] = new ScalarValue("TestUser"),
        };
        var logEvent = CreateLogEvent("User action", LogEventLevel.Information, properties: properties);

        // Act
        var result = LogEventFormatter.FormatLogEvent(logEvent, true, PropertyFormatMode.Inline);

        // Assert
        result.Should().Contain("User action");
        result.Should().Contain("Properties: ");
        result.Should().Contain("UserId=123");
        result.Should().Contain("UserName=&quot;TestUser&quot;");
    }

    /// <summary>
    /// Tests message formatting with properties in structured mode.
    /// </summary>
    [Fact]
    public void FormatLogEvent_WithPropertiesStructured_ShouldIncludePropertiesStructured()
    {
        // Arrange
        var properties = new Dictionary<string, LogEventPropertyValue>
        {
            ["UserId"] = new ScalarValue(123),
            ["UserName"] = new ScalarValue("TestUser"),
        };
        var logEvent = CreateLogEvent("User action", LogEventLevel.Information, properties: properties);

        // Act
        var result = LogEventFormatter.FormatLogEvent(logEvent, true, PropertyFormatMode.Structured);

        // Assert
        result.Should().Contain("User action");
        result.Should().Contain("Properties:");
        result.Should().Contain("  UserId: 123");
        result.Should().Contain("  UserName: &quot;TestUser&quot;");
    }

    /// <summary>
    /// Tests message formatting with properties hidden.
    /// </summary>
    [Fact]
    public void FormatLogEvent_WithPropertiesHidden_ShouldNotIncludeProperties()
    {
        // Arrange
        var properties = new Dictionary<string, LogEventPropertyValue>
        {
            ["UserId"] = new ScalarValue(123),
            ["UserName"] = new ScalarValue("TestUser"),
        };
        var logEvent = CreateLogEvent("User action", LogEventLevel.Information, properties: properties);

        // Act
        var result = LogEventFormatter.FormatLogEvent(logEvent, true, PropertyFormatMode.Hidden);

        // Assert
        result.Should().Be("User action");
        result.Should().NotContain("Properties");
        result.Should().NotContain("UserId");
        result.Should().NotContain("UserName");
    }

    /// <summary>
    /// Tests message formatting with properties disabled.
    /// </summary>
    [Fact]
    public void FormatLogEvent_WithPropertiesDisabled_ShouldNotIncludeProperties()
    {
        // Arrange
        var properties = new Dictionary<string, LogEventPropertyValue>
        {
            ["UserId"] = new ScalarValue(123),
        };
        var logEvent = CreateLogEvent("User action", LogEventLevel.Information, properties: properties);

        // Act
        var result = LogEventFormatter.FormatLogEvent(logEvent, false);

        // Assert
        result.Should().Be("User action");
        result.Should().NotContain("Properties");
        result.Should().NotContain("UserId");
    }

    /// <summary>
    /// Tests formatting with null log event.
    /// </summary>
    [Fact]
    public void FormatLogEvent_WithNullLogEvent_ShouldReturnEmptyString()
    {
        // Act
        var result = LogEventFormatter.FormatLogEvent(null);

        // Assert
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Tests formatting with message template and property values.
    /// </summary>
    [Fact]
    public void FormatLogEvent_WithMessageTemplate_ShouldRenderTemplate()
    {
        // Arrange
        var messageTemplate = new MessageTemplateParser().Parse("User {UserId} performed {Action}");
        var properties = new Dictionary<string, LogEventPropertyValue>
        {
            ["UserId"] = new ScalarValue(123),
            ["Action"] = new ScalarValue("Login"),
        };
        var logEvent = new LogEvent(
            DateTimeOffset.Now,
            LogEventLevel.Information,
            null,
            messageTemplate,
            properties.Select(kvp => new LogEventProperty(kvp.Key, kvp.Value)));

        // Act
        var result = LogEventFormatter.FormatLogEvent(logEvent, false); // Don't include properties separately

        // Assert
        result.Should().Be("User 123 performed &quot;Login&quot;");
    }

    /// <summary>
    /// Helper method to create a LogEvent for testing.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="level">The log level.</param>
    /// <param name="exception">Optional exception.</param>
    /// <param name="properties">Optional properties.</param>
    /// <returns>A LogEvent instance.</returns>
    private static LogEvent CreateLogEvent(
        string message, 
        LogEventLevel level, 
        Exception exception = null, 
        Dictionary<string, LogEventPropertyValue> properties = null)
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
}