# XTinyLog.Serilog Console Sample

This console application demonstrates comprehensive usage of the XTinyLog.Serilog sink in various scenarios.

## Features Demonstrated

### 1. Basic Configuration
- Default sink configuration
- Simple logging with different log levels
- Console and HTML output combination

### 2. Configuration Patterns
- **Fluent Configuration**: Using action delegates to configure the sink
- **Builder Pattern**: Using the configuration builder for fluent setup
- **Direct Parameters**: Passing parameters directly for appsettings.json compatibility
- **JSON Configuration**: Configuration through appsettings.json

### 3. Structured Logging
- **Inline Properties**: Properties displayed inline with the message
- **Structured Properties**: Properties displayed in a structured format
- **Hidden Properties**: Clean messages without property details
- **Complex Object Logging**: Logging complex objects with destructuring

### 4. Error Handling
- Simple exception logging
- Nested exception handling
- Exception logging with structured context
- Graceful error handling patterns

### 5. Asynchronous Logging
- High-volume async logging demonstration
- Task-based parallel logging
- Performance comparison scenarios

### 6. Dependency Injection
- Integration with Microsoft.Extensions.Hosting
- Service-based logging patterns
- Scoped logger usage
- Batch processing with correlation IDs

## Running the Sample

### Prerequisites
- .NET 8.0 or later
- Visual Studio 2022 or VS Code (optional)

### Command Line
```bash
cd samples/XTinyLog.Serilog.Samples.Console
dotnet run
```

### Visual Studio
1. Open the solution file in the root directory
2. Set `XTinyLog.Serilog.Samples.Console` as the startup project
3. Press F5 to run

## Output

The sample creates HTML log files in various subdirectories under `Logs/`:

```
Logs/
├── Fluent/                 # Fluent configuration example
├── Builder/                # Builder pattern example  
├── Direct/                 # Direct parameter example
├── Structured/
│   ├── Inline/            # Inline property format
│   ├── Formatted/         # Structured property format
│   └── Hidden/            # Hidden property format
├── ErrorHandling/         # Exception logging examples
├── Async/                 # Asynchronous logging examples
├── DependencyInjection/   # DI integration examples
└── Configuration/         # appsettings.json configuration
```

Each directory contains HTML files with rich, interactive log output that you can open in any web browser.

## Key Code Examples

### Basic Usage
```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.XTinyLog()
    .CreateLogger();

Log.Information("Hello, XTinyLog!");
```

### Custom Configuration
```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.XTinyLog(config =>
    {
        config.LogDirectory = @"C:\MyApp\Logs";
        config.ApplicationName = "MyApp";
        config.PropertyFormat = PropertyFormatMode.Structured;
    })
    .CreateLogger();
```

### Structured Logging
```csharp
var user = new { Id = 123, Name = "John Doe", Email = "john@example.com" };
Log.Information("User logged in: {@User}", user);
```

### Async Logging
```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.XTinyLogAsync(config => { /* configuration */ })
    .CreateLogger();
```

### Dependency Injection
```csharp
var host = Host.CreateDefaultBuilder()
    .UseSerilog((context, configuration) =>
    {
        configuration.WriteTo.XTinyLog(/* configuration */);
    })
    .Build();
```

## Configuration Options

### PropertyFormatMode Options

| Mode | Description | Example Output |
|------|-------------|----------------|
| `Inline` | Properties inline with message | `User logged in Properties: UserId=123, Name=John` |
| `Structured` | Properties on separate lines | `User logged in`<br>`Properties:`<br>`  UserId: 123`<br>`  Name: John` |
| `Hidden` | No properties shown | `User John logged in` |

### Configuration Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `LogDirectory` | string | `"c:\Log"` | Directory for log files |
| `ApplicationName` | string | `"Application"` | Application name for organization |
| `MaxFileSize` | long | `1024` | Max file size in KB before rotation |
| `IncludeProperties` | bool | `true` | Whether to include structured properties |
| `PropertyFormat` | PropertyFormatMode | `Inline` | How to format properties |

## Best Practices Demonstrated

1. **Always dispose loggers**: Use `Log.CloseAndFlush()` or proper disposal
2. **Use structured logging**: Include context with structured properties
3. **Handle exceptions properly**: Log exceptions with context information
4. **Choose appropriate formats**: Use structured for debugging, inline for production
5. **Use async for high throughput**: Consider async sinks for performance-critical scenarios
6. **Implement correlation IDs**: Track related operations across service boundaries
7. **Configure minimum levels**: Control log verbosity appropriately

## Performance Notes

- **Synchronous vs Asynchronous**: Async logging provides better throughput for high-volume scenarios
- **Property Formatting**: Hidden properties provide the best performance
- **File Size Management**: Larger max file sizes reduce I/O overhead but increase individual file sizes
- **Structured Objects**: Use `@` destructuring sparingly for complex objects

## Troubleshooting

### Common Issues

1. **Permission Errors**: Ensure the application has write access to the log directory
2. **File Locking**: Close log files before trying to delete or move them
3. **Configuration Errors**: Check the console output for configuration validation messages
4. **Missing Output**: Verify the log directory path and ensure `Log.CloseAndFlush()` is called

### Diagnostic Information

The XTinyLog sink includes internal diagnostics. Check the HTML log files for any sink-related error messages or warnings.

## Next Steps

After running this sample:

1. **Explore the HTML Output**: Open the generated HTML files in a web browser
2. **Try Different Configurations**: Modify the configuration parameters and observe the changes
3. **Integrate with Your Application**: Adapt the patterns shown here to your specific use case
4. **Performance Testing**: Use the async logging patterns for high-throughput scenarios
5. **Web Integration**: Check out the web application sample for ASP.NET Core integration

## Related Samples

- **Web Sample**: ASP.NET Core integration with request logging
- **Advanced Sample**: Performance optimization and custom formatting
- **Main XTinyLog Samples**: Direct XTinyLog usage without Serilog