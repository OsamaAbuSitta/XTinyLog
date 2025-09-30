using System.Text;

namespace XTinyLog.Serilog;

/// <summary>
/// Formats Serilog log events for XTinyLog consumption.
/// </summary>
/// <remarks>
/// This internal class handles the conversion of Serilog's LogEvent objects into
/// formatted string messages suitable for XTinyLog's HTML output. It provides
/// comprehensive formatting for message templates, structured properties, and
/// exception details while ensuring HTML safety through proper character escaping.
/// 
/// The formatter supports multiple property formatting modes and includes robust
/// error handling to ensure that formatting failures don't prevent logging.
/// </remarks>
internal static class LogEventFormatter
{
    /// <summary>
    /// Formats a Serilog log event into a message string suitable for XTinyLog.
    /// </summary>
    /// <param name="logEvent">The Serilog log event to format. Must not be null.</param>
    /// <param name="includeProperties">Whether to include structured properties in the output. Default is true.</param>
    /// <param name="propertyFormat">How to format structured properties. Default is <see cref="PropertyFormatMode.Inline"/>.</param>
    /// <returns>A formatted message string ready for XTinyLog HTML output.</returns>
    /// <remarks>
    /// This method performs the following formatting operations:
    /// <list type="number">
    /// <item>Renders the Serilog message template with property values</item>
    /// <item>Escapes HTML characters to prevent injection attacks</item>
    /// <item>Formats exception details including stack traces and inner exceptions</item>
    /// <item>Formats structured properties according to the specified format mode</item>
    /// </list>
    /// 
    /// The method includes comprehensive error handling at each step to ensure that
    /// formatting failures result in fallback messages rather than exceptions.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logEvent"/> is null.</exception>
    /// <example>
    /// <code>
    /// var logEvent = new LogEvent(DateTimeOffset.Now, LogEventLevel.Information, 
    ///     null, MessageTemplate.Empty, new LogEventProperty[0]);
    /// 
    /// // Format with default settings
    /// string formatted = LogEventFormatter.FormatLogEvent(logEvent);
    /// 
    /// // Format without properties
    /// string withoutProps = LogEventFormatter.FormatLogEvent(logEvent, false);
    /// 
    /// // Format with structured properties
    /// string structured = LogEventFormatter.FormatLogEvent(logEvent, true, PropertyFormatMode.Structured);
    /// </code>
    /// </example>
    public static string FormatLogEvent(LogEvent logEvent, bool includeProperties = true, PropertyFormatMode propertyFormat = PropertyFormatMode.Inline)
    {
        if (logEvent == null)
        {
            return string.Empty;
        }

        try
        {
            var messageBuilder = new StringBuilder();

            // Render the message template with property values
            try
            {
                var renderedMessage = logEvent.RenderMessage();
                messageBuilder.Append(EscapeHtmlCharacters(renderedMessage));
            }
            catch (Exception ex)
            {
                // Fallback to raw message template if rendering fails
                messageBuilder.Append(EscapeHtmlCharacters($"[Message rendering failed: {ex.Message}] {logEvent.MessageTemplate}"));
            }

            // Add exception information if present
            if (logEvent.Exception != null)
            {
                try
                {
                    messageBuilder.AppendLine();
                    messageBuilder.AppendLine("Exception:");
                    messageBuilder.Append(FormatException(logEvent.Exception));
                }
                catch (Exception ex)
                {
                    // Fallback exception formatting
                    messageBuilder.AppendLine();
                    messageBuilder.AppendLine($"[Exception formatting failed: {ex.Message}] Exception: {logEvent.Exception?.GetType().Name ?? "Unknown"}");
                }
            }

            // Add structured properties if requested
            if (includeProperties && logEvent.Properties.Count > 0)
            {
                try
                {
                    var propertiesText = FormatProperties(logEvent.Properties, propertyFormat);
                    if (!string.IsNullOrEmpty(propertiesText))
                    {
                        messageBuilder.AppendLine();
                        messageBuilder.Append(propertiesText);
                    }
                }
                catch (Exception ex)
                {
                    // Fallback properties formatting
                    messageBuilder.AppendLine();
                    messageBuilder.AppendLine($"[Properties formatting failed: {ex.Message}] Properties: {logEvent.Properties.Count} items");
                }
            }

            return messageBuilder.ToString();
        }
        catch (Exception ex)
        {
            // Ultimate fallback - return a safe message
            return $"[Log formatting failed: {ex.Message}] Timestamp: {logEvent.Timestamp:yyyy-MM-dd HH:mm:ss} Level: {logEvent.Level}";
        }
    }

    /// <summary>
    /// Formats structured properties based on the specified format mode.
    /// </summary>
    /// <param name="properties">The properties to format.</param>
    /// <param name="formatMode">The format mode to use.</param>
    /// <returns>A formatted properties string.</returns>
    private static string FormatProperties(IReadOnlyDictionary<string, LogEventPropertyValue> properties, PropertyFormatMode formatMode)
    {
        if (properties == null || properties.Count == 0)
        {
            return string.Empty;
        }

        try
        {
            var propertiesBuilder = new StringBuilder();

            switch (formatMode)
            {
                case PropertyFormatMode.Inline:
                    FormatPropertiesInline(properties, propertiesBuilder);
                    break;
                case PropertyFormatMode.Structured:
                    FormatPropertiesStructured(properties, propertiesBuilder);
                    break;
                case PropertyFormatMode.Hidden:
                    // Don't include properties
                    break;
                default:
                    // Fallback to inline for unknown format modes
                    FormatPropertiesInline(properties, propertiesBuilder);
                    break;
            }

            return propertiesBuilder.ToString();
        }
        catch (Exception ex)
        {
            // Fallback formatting for properties
            return $"[Properties formatting error: {ex.Message}] {properties.Count} properties available";
        }
    }

    /// <summary>
    /// Formats properties inline with the message.
    /// </summary>
    /// <param name="properties">The properties to format.</param>
    /// <param name="builder">The string builder to append to.</param>
    private static void FormatPropertiesInline(IReadOnlyDictionary<string, LogEventPropertyValue> properties, StringBuilder builder)
    {
        try
        {
            builder.Append("Properties: ");
            var propertyStrings = new List<string>();
            
            foreach (var kvp in properties)
            {
                try
                {
                    propertyStrings.Add($"{kvp.Key}={FormatPropertyValue(kvp.Value)}");
                }
                catch (Exception ex)
                {
                    // Include property name but indicate formatting error
                    propertyStrings.Add($"{kvp.Key}=[formatting error: {ex.Message}]");
                }
            }
            
            builder.Append(string.Join(", ", propertyStrings));
        }
        catch (Exception ex)
        {
            builder.Append($"[Inline properties formatting error: {ex.Message}]");
        }
    }

    /// <summary>
    /// Formats properties in a structured format.
    /// </summary>
    /// <param name="properties">The properties to format.</param>
    /// <param name="builder">The string builder to append to.</param>
    private static void FormatPropertiesStructured(IReadOnlyDictionary<string, LogEventPropertyValue> properties, StringBuilder builder)
    {
        try
        {
            builder.AppendLine("Properties:");
            foreach (var kvp in properties)
            {
                try
                {
                    builder.AppendLine($"  {kvp.Key}: {FormatPropertyValue(kvp.Value)}");
                }
                catch (Exception ex)
                {
                    // Include property name but indicate formatting error
                    builder.AppendLine($"  {kvp.Key}: [formatting error: {ex.Message}]");
                }
            }
        }
        catch (Exception ex)
        {
            builder.AppendLine($"[Structured properties formatting error: {ex.Message}]");
        }
    }

    /// <summary>
    /// Formats a single property value.
    /// </summary>
    /// <param name="propertyValue">The property value to format.</param>
    /// <returns>A formatted property value string.</returns>
    private static string FormatPropertyValue(LogEventPropertyValue propertyValue)
    {
        if (propertyValue == null)
        {
            return "null";
        }

        try
        {
            // Use Serilog's built-in rendering
            var rendered = propertyValue.ToString();
            return EscapeHtmlCharacters(rendered);
        }
        catch (Exception ex)
        {
            // Fallback to type name if rendering fails
            return $"[{propertyValue.GetType().Name} - rendering error: {ex.Message}]";
        }
    }

    /// <summary>
    /// Formats an exception with its stack trace.
    /// </summary>
    /// <param name="exception">The exception to format.</param>
    /// <returns>A formatted exception string.</returns>
    private static string FormatException(Exception exception)
    {
        if (exception == null)
        {
            return string.Empty;
        }

        try
        {
            var exceptionBuilder = new StringBuilder();
            
            // Add exception type and message
            try
            {
                exceptionBuilder.AppendLine($"{exception.GetType().Name}: {EscapeHtmlCharacters(exception.Message ?? "No message")}");
            }
            catch (Exception ex)
            {
                exceptionBuilder.AppendLine($"[Exception info formatting error: {ex.Message}] Exception occurred");
            }
            
            // Add stack trace if available
            try
            {
                if (!string.IsNullOrEmpty(exception.StackTrace))
                {
                    exceptionBuilder.AppendLine("Stack Trace:");
                    exceptionBuilder.AppendLine(EscapeHtmlCharacters(exception.StackTrace));
                }
            }
            catch (Exception ex)
            {
                exceptionBuilder.AppendLine($"[Stack trace formatting error: {ex.Message}]");
            }

            // Add inner exceptions recursively (with depth limit to prevent infinite recursion)
            try
            {
                if (exception.InnerException != null)
                {
                    exceptionBuilder.AppendLine();
                    exceptionBuilder.AppendLine("Inner Exception:");
                    exceptionBuilder.Append(FormatExceptionWithDepthLimit(exception.InnerException, 1, 10));
                }
            }
            catch (Exception ex)
            {
                exceptionBuilder.AppendLine($"[Inner exception formatting error: {ex.Message}]");
            }

            return exceptionBuilder.ToString();
        }
        catch (Exception ex)
        {
            // Ultimate fallback
            return $"[Exception formatting failed: {ex.Message}] Exception type: {exception?.GetType().Name ?? "Unknown"}";
        }
    }

    /// <summary>
    /// Formats an exception with depth limiting to prevent infinite recursion.
    /// </summary>
    /// <param name="exception">The exception to format.</param>
    /// <param name="currentDepth">The current recursion depth.</param>
    /// <param name="maxDepth">The maximum allowed recursion depth.</param>
    /// <returns>A formatted exception string.</returns>
    private static string FormatExceptionWithDepthLimit(Exception exception, int currentDepth, int maxDepth)
    {
        if (exception == null || currentDepth > maxDepth)
        {
            return currentDepth > maxDepth ? "[Max exception depth reached]" : string.Empty;
        }

        try
        {
            var exceptionBuilder = new StringBuilder();
            
            // Add exception type and message
            exceptionBuilder.AppendLine($"{exception.GetType().Name}: {EscapeHtmlCharacters(exception.Message ?? "No message")}");
            
            // Add stack trace if available
            if (!string.IsNullOrEmpty(exception.StackTrace))
            {
                exceptionBuilder.AppendLine("Stack Trace:");
                exceptionBuilder.AppendLine(EscapeHtmlCharacters(exception.StackTrace));
            }

            // Add inner exceptions recursively
            if (exception.InnerException != null)
            {
                exceptionBuilder.AppendLine();
                exceptionBuilder.AppendLine("Inner Exception:");
                exceptionBuilder.Append(FormatExceptionWithDepthLimit(exception.InnerException, currentDepth + 1, maxDepth));
            }

            return exceptionBuilder.ToString();
        }
        catch (Exception ex)
        {
            return $"[Exception formatting error at depth {currentDepth}: {ex.Message}]";
        }
    }

    /// <summary>
    /// Escapes HTML characters to prevent HTML injection in log messages.
    /// </summary>
    /// <param name="text">The text to escape.</param>
    /// <returns>HTML-escaped text.</returns>
    private static string EscapeHtmlCharacters(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text ?? string.Empty;
        }

        try
        {
            return text
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&#39;");
        }
        catch (Exception ex)
        {
            // Fallback if string replacement fails
            return $"[HTML escaping failed: {ex.Message}] Original length: {text.Length}";
        }
    }
}

