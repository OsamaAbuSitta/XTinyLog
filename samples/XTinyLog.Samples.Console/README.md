# XTinyLog Console Sample

This console application demonstrates the basic functionality of XTinyLog, including configuration options, async logging patterns, and error handling.

## Features Demonstrated

### 1. Basic Logging
- Default configuration usage
- Synchronous logging methods (Info, Debug, Warn, Error)
- Asynchronous logging methods (InfoAsync, DebugAsync, etc.)

### 2. Custom Configuration
- Custom log directory and application name
- Custom file naming patterns
- Custom file size limits
- Configuration validation

### 3. Fluent Configuration Builder
- Method chaining for configuration
- Date-based file naming
- Validation during build process

### 4. Async Logging Patterns
- Proper async/await usage
- Cancellation token support
- Concurrent logging operations
- Task-based logging scenarios

### 5. Multiple Logger Instances
- Separate loggers for different components
- Component-specific log files
- Isolated configurations

### 6. Error Handling
- Exception logging
- Configuration validation errors
- Graceful error recovery

## Running the Sample

```bash
cd samples/XTinyLog.Samples.Console
dotnet run
```

## Expected Output

The application will create log files in various directories:
- Default logs: `C:\Log` (Windows) or system temp directory
- Custom logs: Desktop `XTinyLogSamples` folder
- Fluent logs: System temp `XTinyLogFluent` folder
- Async logs: System temp `XTinyLogAsync` folder
- Multiple instance logs: System temp `XTinyLogMultiple` folder
- Error logs: System temp `XTinyLogErrors` folder

Each log file will be in HTML format with interactive features like:
- Responsive design for different screen sizes
- Dark/light theme toggle
- Real-time filtering and search
- Collapsible log entries
- Export functionality (JSON/CSV)

## Configuration Examples

### Basic Configuration
```csharp
// Using default singleton
XLog.Log.Info("Simple logging");
```

### Custom Configuration
```csharp
var logger = XLog.Create(config =>
{
    config.LogDirectory = @"C:\MyApp\Logs";
    config.ApplicationName = "MyApp";
    config.MaxFileSize = 5000; // 5MB
    config.FileName = () => $"log_{DateTime.Now:yyyyMMdd}";
});
```

### Fluent Configuration
```csharp
var logger = XLog.Create(builder => builder
    .WithLogDirectory(@"C:\MyApp\Logs")
    .WithApplicationName("MyApp")
    .WithMaxFileSize(5000)
    .WithDateBasedFileName("yyyy-MM-dd"));
```

### Async Logging
```csharp
await logger.InfoAsync("Async message", cancellationToken);
```

## Key Learning Points

1. **Configuration Flexibility**: Multiple ways to configure the logger
2. **Async Support**: Proper async/await patterns with cancellation
3. **Multiple Instances**: Different loggers for different purposes
4. **Error Handling**: Robust error handling and validation
5. **HTML Output**: Rich, interactive log viewing experience