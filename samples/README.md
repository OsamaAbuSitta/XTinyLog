# XTinyLog Sample Projects

This directory contains comprehensive sample projects demonstrating various aspects of XTinyLog usage, from basic console applications to advanced web integrations and performance optimization techniques. It includes both direct XTinyLog usage samples and Serilog integration samples.

## Sample Projects Overview

### XTinyLog.Serilog Samples

#### 1. Serilog Console Application Sample (`XTinyLog.Serilog.Samples.Console`)
**Purpose**: Demonstrates XTinyLog.Serilog sink usage in console applications  
**Target Audience**: Developers integrating XTinyLog with Serilog  
**Key Features**:
- Basic sink configuration patterns (fluent, builder, direct parameters)
- Structured logging with different property formatting modes
- Asynchronous logging demonstrations
- Error handling and exception logging
- Dependency injection integration
- appsettings.json configuration

**Quick Start**:
```bash
cd XTinyLog.Serilog.Samples.Console
dotnet run
```

#### 2. Serilog Web Application Sample (`XTinyLog.Serilog.Samples.Web`)
**Purpose**: Shows XTinyLog.Serilog integration in ASP.NET Core applications  
**Target Audience**: Web developers building APIs with Serilog  
**Key Features**:
- ASP.NET Core integration with request logging
- Correlation ID tracking across requests
- Service-level structured logging
- Real-world API scenarios (Users, Orders)
- Performance metrics collection
- Environment-specific configuration
- Swagger API documentation

**Quick Start**:
```bash
cd XTinyLog.Serilog.Samples.Web
dotnet run
# Navigate to https://localhost:7xxx/swagger for API documentation
```

### Direct XTinyLog Samples

### 1. Console Application Sample (`XTinyLog.Samples.Console`)
**Purpose**: Demonstrates basic XTinyLog functionality and configuration patterns  
**Target Audience**: Developers new to XTinyLog  
**Key Features**:
- Default configuration usage
- Custom configuration with action delegates
- Fluent configuration builder patterns
- Async logging with cancellation tokens
- Multiple logger instances
- Error handling and exception logging

**Quick Start**:
```bash
cd XTinyLog.Samples.Console
dotnet run
```

### 2. Web Application Sample (`XTinyLog.Samples.Web`)
**Purpose**: Shows XTinyLog integration in ASP.NET Core applications  
**Target Audience**: Web developers building APIs and web services  
**Key Features**:
- Dependency injection integration
- Request logging middleware
- Performance monitoring
- Component-specific loggers
- Structured logging in controllers and services
- Real-world API scenarios

**Quick Start**:
```bash
cd XTinyLog.Samples.Web
dotnet run
# Navigate to https://localhost:7xxx/swagger for API documentation
```

### 3. Advanced Configuration Sample (`XTinyLog.Samples.Advanced`)
**Purpose**: Demonstrates advanced patterns and optimization techniques  
**Target Audience**: Experienced developers optimizing logging performance  
**Key Features**:
- Custom formatting and file naming strategies
- Multiple logger instances for different components
- Performance optimization and benchmarking
- Advanced file management and rotation
- Custom HTML styling and themes
- Structured logging patterns for analysis

**Quick Start**:
```bash
cd XTinyLog.Samples.Advanced
dotnet run
```

## Learning Path

### For Serilog Users

#### Beginner → Serilog Console Sample
Start here if you're using Serilog and want to add XTinyLog HTML output:
- Basic XTinyLog.Serilog sink configuration
- Different configuration patterns (fluent, builder, JSON)
- Structured logging with property formatting
- Async logging patterns

#### Intermediate → Serilog Web Sample
Move to this sample for web application integration:
- ASP.NET Core integration with dependency injection
- Request logging and correlation tracking
- Service-level logging patterns
- Real-world API scenarios with error handling

### For Direct XTinyLog Users

#### Beginner → Console Sample
Start here if you're new to XTinyLog. This sample covers:
- Basic setup and configuration
- Different ways to create logger instances
- Synchronous vs asynchronous logging
- Error handling patterns

### Intermediate → Web Sample
Move to this sample when building web applications:
- Integration with ASP.NET Core dependency injection
- Middleware for automatic request logging
- Performance monitoring patterns
- Component separation strategies

### Advanced → Advanced Sample
Use this sample for optimization and advanced scenarios:
- Performance benchmarking and optimization
- Custom file management strategies
- Advanced HTML styling and presentation
- Structured logging for log analysis tools

## Common Patterns Demonstrated

### Configuration Patterns
```csharp
// Basic configuration
XLog.Log.Info("Simple message");

// Custom configuration
var logger = XLog.Create(config =>
{
    config.LogDirectory = @"C:\MyApp\Logs";
    config.ApplicationName = "MyApp";
    config.MaxFileSize = 5000;
});

// Fluent configuration
var logger = XLog.Create(builder => builder
    .WithLogDirectory(@"C:\MyApp\Logs")
    .WithApplicationName("MyApp")
    .WithMaxFileSize(5000));
```

### Async Logging Patterns
```csharp
// Basic async logging
await logger.InfoAsync("Async message");

// With cancellation token
await logger.InfoAsync("Cancellable message", cancellationToken);

// Batch async operations
var tasks = messages.Select(msg => logger.InfoAsync(msg));
await Task.WhenAll(tasks);
```

### Dependency Injection (Web Applications)
```csharp
// Service registration
services.AddSingleton<ApplicationLogger>(provider =>
{
    var logger = XLog.Create(config => { /* configuration */ });
    return new ApplicationLogger(logger);
});

// Controller usage
public class ApiController : ControllerBase
{
    private readonly ApplicationLogger _logger;
    
    public ApiController(ApplicationLogger logger)
    {
        _logger = logger;
    }
}
```

### Structured Logging
```csharp
// Session tracking
logger.Info($"[SESSION:{sessionId}] [USER:{userId}] Operation completed");

// Request correlation
logger.Info($"[REQUEST:{requestId}] [METHOD:{method}] [ENDPOINT:{endpoint}] Processing started");

// Error correlation
logger.Error($"[ERROR:{errorId}] [CONTEXT:{context}] Operation failed: {exception.Message}");
```

## Output Examples

All samples generate rich HTML log files with:
- **Responsive Design**: Works on desktop, tablet, and mobile
- **Dark/Light Themes**: Toggle between themes for comfortable viewing
- **Interactive Features**: Real-time search, filtering, and navigation
- **Export Capabilities**: JSON and CSV export for analysis
- **Visual Enhancements**: Icons, colors, and structured formatting

## Performance Considerations

### Async vs Sync Logging
The samples demonstrate that async logging typically provides:
- 2-3x better throughput for high-volume scenarios
- Reduced blocking of application threads
- Better resource utilization under load

### File Management
- Automatic file rotation based on size limits
- Organized directory structures for easy navigation
- Timestamp-based file naming for chronological ordering

### Memory Usage
- Efficient HTML generation with minimal memory overhead
- Proper disposal patterns for resource management
- Optimized string handling for large log volumes

## Integration Examples

### Console Applications
```csharp
// Simple console app integration
class Program
{
    static async Task Main(string[] args)
    {
        XLog.Log.Info("Application started");
        
        try
        {
            await DoWork();
            XLog.Log.Info("Work completed successfully");
        }
        catch (Exception ex)
        {
            XLog.Log.Error(ex);
        }
    }
}
```

### Web Applications
```csharp
// Startup configuration
public void ConfigureServices(IServiceCollection services)
{
    // Register XTinyLog instances
    services.AddSingleton<ApplicationLogger>(/* configuration */);
    
    // Add middleware
    services.AddScoped<RequestLoggingMiddleware>();
}

public void Configure(IApplicationBuilder app)
{
    app.UseMiddleware<RequestLoggingMiddleware>();
}
```

### Background Services
```csharp
public class BackgroundTaskService : BackgroundService
{
    private readonly XLog _logger;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _logger.InfoAsync("Background service started");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessTasks();
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
```

## Best Practices Demonstrated

1. **Separation of Concerns**: Different loggers for different purposes
2. **Async First**: Use async methods for better performance
3. **Structured Messages**: Include context and correlation IDs
4. **Error Handling**: Proper exception logging with context
5. **Configuration Management**: Environment-specific settings
6. **Performance Monitoring**: Track and optimize logging overhead
7. **File Organization**: Logical directory and naming structures
8. **Resource Management**: Proper disposal and cleanup patterns

## Next Steps

After exploring these samples:
1. Choose the pattern that best fits your application type
2. Adapt the configuration to your specific requirements
3. Implement structured logging patterns for better analysis
4. Set up monitoring and alerting based on log content
5. Consider integration with log analysis tools and dashboards

## Support and Documentation

- **Main Documentation**: See the main project README.md
- **API Reference**: Check XML documentation in the source code
- **Issues and Questions**: Use the project's GitHub issues
- **Contributing**: See CONTRIBUTING.md for development guidelines