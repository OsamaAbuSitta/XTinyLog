namespace XTinyLog.Serilog;

/// <summary>
/// Defines how structured properties should be formatted in the log output.
/// </summary>
/// <remarks>
/// This enumeration controls how Serilog's structured logging properties are rendered
/// in the XTinyLog HTML output. Different modes provide different levels of detail
/// and formatting styles to suit various logging scenarios.
/// </remarks>
/// <example>
/// <code>
/// // Configure inline property formatting
/// Log.Logger = new LoggerConfiguration()
///     .WriteTo.XTinyLog(config => config.PropertyFormat = PropertyFormatMode.Inline)
///     .CreateLogger();
/// 
/// // Log with structured properties
/// Log.Information("User {UserId} logged in from {IPAddress}", 123, "192.168.1.1");
/// // Output: "User 123 logged in from 192.168.1.1 Properties: UserId=123, IPAddress=192.168.1.1"
/// </code>
/// </example>
public enum PropertyFormatMode
{
    /// <summary>
    /// Properties are rendered inline with the message as a comma-separated list.
    /// </summary>
    /// <remarks>
    /// This is the default format mode. Properties are appended to the log message
    /// in the format "Properties: Key1=Value1, Key2=Value2, ...". This provides
    /// a compact representation that's easy to read in a single line.
    /// </remarks>
    /// <example>
    /// Output: "User logged in Properties: UserId=123, IPAddress=192.168.1.1"
    /// </example>
    Inline,

    /// <summary>
    /// Properties are rendered as structured data with each property on a separate line.
    /// </summary>
    /// <remarks>
    /// This format mode renders properties in a multi-line format with proper indentation,
    /// making it easier to read complex property structures. Each property is displayed
    /// on its own line with consistent formatting.
    /// </remarks>
    /// <example>
    /// <code>
    /// Output:
    /// "User logged in
    /// Properties:
    ///   UserId: 123
    ///   IPAddress: 192.168.1.1"
    /// </code>
    /// </example>
    Structured,

    /// <summary>
    /// Properties are not included in the output, only the rendered message template.
    /// </summary>
    /// <remarks>
    /// This format mode excludes structured properties from the log output entirely.
    /// Only the message template rendered with property values is included. This is
    /// useful when you want cleaner log output without the additional property details.
    /// </remarks>
    /// <example>
    /// Output: "User 123 logged in from 192.168.1.1" (no properties section)
    /// </example>
    Hidden,
}