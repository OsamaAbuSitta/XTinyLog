# XTinyLog.Serilog API Documentation

## Overview

XTinyLog.Serilog is a Serilog sink that integrates XTinyLog's HTML logging capabilities into the Serilog ecosystem. This package allows developers using Serilog to output their logs in XTinyLog's interactive HTML format without changing their existing logging code.

## Key Features

- **Seamless Integration**: Works with existing Serilog configurations
- **HTML Output**: Generates interactive HTML log files using XTinyLog
- **Structured Logging**: Supports Serilog's structured logging properties
- **Async Support**: Provides both synchronous and asynchronous logging
- **Flexible Configuration**: Multiple configuration options including fluent API and JSON
- **Error Handling**: Robust error handling that never crashes your application

## Quick Start

### Basic Usage

```csharp
using Serilog;
using XTinyLog.Serilog;

// Configure Serilog with XTinyLog sink
Log.Logger = new LoggerConfiguration()
    .WriteTo.XTinyLog()
    .CreateLogger();

// Use Serilog as normal
Log.Information("Application started");
Log.Warning("This is a warning with {Property}", "value");
Log.Error(exception, "An error occurred");
```

### Custom Configuration

```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.XTinyLog(config =>
    {
        config.LogDirectory = @"C:\MyApp\Logs";
        config.ApplicationName = "MyApplication";
        config.MaxFileSize = 2048; // 2MB
        config.PropertyFormat = PropertyFormatMode.Structured;
    })
    .CreateLogger();
```

### Asynchronous Logging

```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.XTinyLogAsync(config =>
    {
        config.LogDirectory = @"C:\MyApp\Logs";
        config.ApplicationName = "MyApplication";
    })
    .CreateLogger();
```

## Core Classes

### XTinyLogSink

The main synchronous sink implementation.

**Constructor:**
```csharp
public XTinyLogSink(XTinyLogSinkConfiguration configuration)
```

**Key Methods:**
- `Emit(LogEvent logEvent)` - Processes and logs a Serilog log event

**Usage Example:**
```csharp
var config = new XTinyLogSinkConfiguration
{
    LogDirectory = @"C:\Logs",
    ApplicationName = "TestApp"
};
var sink = new XTinyLogSink(config);

// Use with Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Sink(sink)
    .CreateLogger();
```

### XTinyLogAsyncSink

The asynchronous sink implementation for high-performance scenarios.

**Constructor:**
```csharp
public XTinyLogAsyncSink(XTinyLogSinkConfiguration configuration)
```

**Key Methods:**
- `Emit(LogEvent logEvent)` - Asynchronously processes and logs a Serilog log event

**Usage Example:**
```csharp
var config = new XTinyLogSinkConfiguration
{
    LogDirectory = @"C:\Logs",
    ApplicationName = "TestApp"
};
var asyncSink = new XTinyLogAsyncSink(config);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Sink(asyncSink)
    .CreateLogger();
```

### XTinyLogSinkConfiguration

Configuration class for the XTinyLog sinks.

**Properties:**
- `LogDirectory` (string) - Directory where log files are stored (default: "c:\Log")
- `ApplicationName` (string) - Application name for log organization (default: "Application")
- `MaxFileSize` (long) - Maximum file size in KB before archiving (default: 1024)
- `FileName` (Func<string>) - Custom file name generator function (optional)
- `IncludeProperties` (bool) - Whether to include structured properties (default: true)
- `PropertyFormat` (PropertyFormatMode) - How to format properties (default: Inline)

**Methods:**
- `ToLogConfiguration()` - Converts to XTinyLog's ILogConfiguration
- `Clone()` - Creates a copy of the configuration
- `Validate()` - Validates the configuration and returns error messages

**Usage Example:**
```csharp
var config = new XTinyLogSinkConfiguration
{
    LogDirectory = @"C:\MyApp\Logs",
    ApplicationName = "MyApplication",
    MaxFileSize = 2048,
    IncludeProperties = true,
    PropertyFormat = PropertyFormatMode.Structured,
    FileName = () => $"log-{DateTime.Now:yyyyMMdd}.html"
};

// Validate configuration
var errors = config.Validate();
if (errors.Count > 0)
{
    throw new InvalidOperationException($"Invalid configuration: {string.Join(", ", errors)}");
}
```

### XTinyLogSinkConfigurationBuilder

Fluent builder for creating sink configurations.

**Methods:**
- `WithLogDirectory(string)` - Sets the log directory
- `WithApplicationName(string)` - Sets the application name
- `WithMaxFileSize(long)` - Sets the maximum file size in KB
- `WithFileName(Func<string>)` - Sets the file name generator
- `WithIncludeProperties(bool)` - Sets whether to include properties
- `WithPropertyFormat(PropertyFormatMode)` - Sets the property format mode
- `Build()` - Builds and validates the configuration
- `Reset()` - Resets to default values

**Usage Example:**
```csharp
var config = new XTinyLogSinkConfigurationBuilder()
    .WithLogDirectory(@"C:\MyApp\Logs")
    .WithApplicationName("MyApplication")
    .WithMaxFileSize(2048)
    .WithPropertyFormat(PropertyFormatMode.Structured)
    .Build();

Log.Logger = new LoggerConfiguration()
    .WriteTo.XTinyLog(c => c = config)
    .CreateLogger();
```

## Extension Methods

### XTinyLogSinkExtensions

Provides fluent configuration methods for Serilog.

#### Synchronous Sink Extensions

**XTinyLog()** - Basic configuration with defaults
```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.XTinyLog()
    .CreateLogger();
```

**XTinyLog(Action<XTinyLogSinkConfiguration>)** - Custom configuration
```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.XTinyLog(config =>
    {
        config.LogDirectory = @"C:\Logs";
        config.ApplicationName = "MyApp";
    })
    .CreateLogger();
```

**XTinyLog(parameters)** - Direct parameter configuration (suitable for appsettings.json)
```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.XTinyLog(
        logDirectory: @"C:\Logs",
        applicationName: "MyApp",
        maxFileSize: 2048,
        includeProperties: true,
        propertyFormat: PropertyFormatMode.Structured)
    .CreateLogger();
```

**XTinyLogWithBuilder(Action<XTinyLogSinkConfigurationBuilder>)** - Builder pattern
```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.XTinyLogWithBuilder(builder =>
        builder.WithLogDirectory(@"C:\Logs")
               .WithApplicationName("MyApp")
               .WithMaxFileSize(2048))
    .CreateLogger();
```

#### Asynchronous Sink Extensions

All synchronous extensions have async equivalents:
- `XTinyLogAsync()`
- `XTinyLogAsync(Action<XTinyLogSinkConfiguration>)`
- `XTinyLogAsync(parameters)`
- `XTinyLogAsyncWithBuilder(Action<XTinyLogSinkConfigurationBuilder>)`

## Configuration Options

### appsettings.json Configuration

```json
{
  "Serilog": {
    "WriteTo": [
      {
        "Name": "XTinyLog",
        "Args": {
          "logDirectory": "C:\\MyApp\\Logs",
          "applicationName": "MyApplication",
          "maxFileSize": 2048,
          "includeProperties": true,
          "propertyFormat": "Structured"
        }
      }
    ]
  }
}
```

### PropertyFormatMode Options

#### Inline (Default)
Properties are rendered inline with the message:
```
User logged in Properties: UserId=123, IPAddress=192.168.1.1
```

#### Structured
Properties are rendered on separate lines:
```
User logged in
Properties:
  UserId: 123
  IPAddress: 192.168.1.1
```

#### Hidden
Properties are not included in the output:
```
User 123 logged in from 192.168.1.1
```

## Log Level Mapping

| Serilog Level | XTinyLog Method |
|---------------|-----------------|
| Verbose       | Debug           |
| Debug         | Debug           |
| Information   | Info            |
| Warning       | Warn            |
| Error         | Error           |
| Fatal         | Error           |

## Error Handling

The XTinyLog sink follows Serilog best practices for error handling:

1. **Never throws exceptions** from the sink to prevent application crashes
2. **Graceful degradation** when formatting or logging fails
3. **Fallback mechanisms** for configuration and runtime errors
4. **Internal diagnostics** using XTinyLog's internal logging

## Performance Considerations

### Synchronous vs Asynchronous

- **Synchronous sink** (`XTinyLogSink`): Simpler, lower overhead, blocks calling thread
- **Asynchronous sink** (`XTinyLogAsyncSink`): Higher throughput, non-blocking, slightly more overhead

### Recommendations

- Use **synchronous sink** for most applications
- Use **asynchronous sink** for high-throughput scenarios or when logging performance is critical
- Consider file I/O performance when setting `MaxFileSize`
- Use `PropertyFormatMode.Hidden` if you don't need structured properties for better performance

## Common Usage Patterns

### Console Application

```csharp
using Serilog;
using XTinyLog.Serilog;

class Program
{
    static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.XTinyLog(config =>
            {
                config.LogDirectory = @"C:\MyApp\Logs";
                config.ApplicationName = "ConsoleApp";
            })
            .CreateLogger();

        try
        {
            Log.Information("Application starting");
            // Your application code here
            Log.Information("Application completed successfully");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
```

### Web Application (ASP.NET Core)

```csharp
// Program.cs
using Serilog;
using XTinyLog.Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console()
        .WriteTo.XTinyLogAsync(config =>
        {
            config.LogDirectory = @"C:\WebApp\Logs";
            config.ApplicationName = "WebApplication";
            config.PropertyFormat = PropertyFormatMode.Structured;
        });
});

var app = builder.Build();

// Use Serilog for request logging
app.UseSerilogRequestLogging();

app.MapGet("/", () => "Hello World!");

app.Run();
```

### Structured Logging Example

```csharp
// Log with structured properties
Log.Information("User {UserId} performed {Action} on {Resource} at {Timestamp}",
    userId, "UPDATE", "UserProfile", DateTime.UtcNow);

// Log with complex objects
var user = new { Id = 123, Name = "John Doe", Email = "john@example.com" };
Log.Information("User details: {@User}", user);

// Log with exception
try
{
    // Some operation that might fail
}
catch (Exception ex)
{
    Log.Error(ex, "Failed to process user {UserId}", userId);
}
```

## Troubleshooting

### Common Issues

1. **Configuration Validation Errors**
   - Check that `LogDirectory` is a valid path
   - Ensure `ApplicationName` doesn't contain invalid filename characters
   - Verify `MaxFileSize` is greater than 0

2. **File Permission Issues**
   - Ensure the application has write permissions to the log directory
   - Check that the log directory exists or can be created

3. **Performance Issues**
   - Consider using the async sink for high-throughput scenarios
   - Adjust `MaxFileSize` based on your needs
   - Use `PropertyFormatMode.Hidden` if properties aren't needed

### Diagnostic Information

The sink logs diagnostic information using XTinyLog's internal logging system. Check your XTinyLog configuration for internal logging settings to troubleshoot issues.

## Version Compatibility

- **Serilog**: 2.0+ (tested with latest versions)
- **XTinyLog**: Compatible with the version included in this package
- **.NET**: .NET 6.0+ (check project file for specific target frameworks)

## License

This package follows the same license as the main XTinyLog project. See the LICENSE file for details.