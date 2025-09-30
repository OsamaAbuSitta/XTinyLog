# XTinyLog.Serilog Usage Examples

This document provides comprehensive examples of how to use the XTinyLog Serilog sink in various scenarios.

## Table of Contents

1. [Basic Setup](#basic-setup)
2. [Configuration Examples](#configuration-examples)
3. [Structured Logging](#structured-logging)
4. [Error Handling](#error-handling)
5. [Performance Optimization](#performance-optimization)
6. [Integration Examples](#integration-examples)
7. [Advanced Scenarios](#advanced-scenarios)

## Basic Setup

### Minimal Configuration

```csharp
using Serilog;
using XTinyLog.Serilog;

// Simplest possible setup with defaults
Log.Logger = new LoggerConfiguration()
    .WriteTo.XTinyLog()
    .CreateLogger();

Log.Information("Hello, XTinyLog!");
Log.CloseAndFlush();
```

### With Console Output

```csharp
using Serilog;
using XTinyLog.Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.XTinyLog()
    .CreateLogger();

Log.Information("This message goes to both console and HTML file");
Log.CloseAndFlush();
```

## Configuration Examples

### Fluent Configuration

```csharp
using Serilog;
using XTinyLog.Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.XTinyLog(config =>
    {
        config.LogDirectory = @"C:\MyApplication\Logs";
        config.ApplicationName = "MyApp";
        config.MaxFileSize = 2048; // 2MB
        config.IncludeProperties = true;
        config.PropertyFormat = PropertyFormatMode.Structured;
    })
    .CreateLogger();
```

### Builder Pattern Configuration

```csharp
using Serilog;
using XTinyLog.Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.XTinyLogWithBuilder(builder =>
        builder.WithLogDirectory(@"C:\MyApplication\Logs")
               .WithApplicationName("MyApp")
               .WithMaxFileSize(2048)
               .WithPropertyFormat(PropertyFormatMode.Structured)
               .WithIncludeProperties(true))
    .CreateLogger();
```

### Custom File Naming

```csharp
using Serilog;
using XTinyLog.Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.XTinyLog(config =>
    {
        config.LogDirectory = @"C:\Logs";
        config.ApplicationName = "MyApp";
        config.FileName = () => $"app-{DateTime.Now:yyyyMMdd-HHmm}.html";
    })
    .CreateLogger();
```

### appsettings.json Configuration

**appsettings.json:**
```json
{
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      {
        "Name": "Console"
      },
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

**Program.cs:**
```csharp
using Serilog;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();
```

## Structured Logging

### Basic Structured Logging

```csharp
using Serilog;
using XTinyLog.Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.XTinyLog(config =>
    {
        config.PropertyFormat = PropertyFormatMode.Structured;
    })
    .CreateLogger();

// Simple properties
Log.Information("User {UserId} logged in from {IPAddress}", 123, "192.168.1.1");

// Complex objects
var user = new { Id = 123, Name = "John Doe", Email = "john@example.com" };
Log.Information("User created: {@User}", user);

// Multiple properties
Log.Warning("Database query took {Duration}ms for {Query} with {RowCount} rows",
    1250, "SELECT * FROM Users", 42);
```

### Property Format Comparison

```csharp
using Serilog;
using XTinyLog.Serilog;

// Inline format (default)
var inlineLogger = new LoggerConfiguration()
    .WriteTo.XTinyLog(config => config.PropertyFormat = PropertyFormatMode.Inline)
    .CreateLogger();

inlineLogger.Information("User {UserId} action {Action}", 123, "LOGIN");
// Output: "User 123 action LOGIN Properties: UserId=123, Action=LOGIN"

// Structured format
var structuredLogger = new LoggerConfiguration()
    .WriteTo.XTinyLog(config => config.PropertyFormat = PropertyFormatMode.Structured)
    .CreateLogger();

structuredLogger.Information("User {UserId} action {Action}", 123, "LOGIN");
// Output: 
// "User 123 action LOGIN
// Properties:
//   UserId: 123
//   Action: LOGIN"

// Hidden format
var hiddenLogger = new LoggerConfiguration()
    .WriteTo.XTinyLog(config => config.PropertyFormat = PropertyFormatMode.Hidden)
    .CreateLogger();

hiddenLogger.Information("User {UserId} action {Action}", 123, "LOGIN");
// Output: "User 123 action LOGIN"
```

### Complex Object Logging

```csharp
using Serilog;
using XTinyLog.Serilog;

public class OrderInfo
{
    public int OrderId { get; set; }
    public string CustomerName { get; set; }
    public decimal Total { get; set; }
    public List<string> Items { get; set; }
    public DateTime OrderDate { get; set; }
}

Log.Logger = new LoggerConfiguration()
    .WriteTo.XTinyLog(config => config.PropertyFormat = PropertyFormatMode.Structured)
    .CreateLogger();

var order = new OrderInfo
{
    OrderId = 12345,
    CustomerName = "Jane Smith",
    Total = 99.99m,
    Items = new List<string> { "Widget A", "Widget B", "Widget C" },
    OrderDate = DateTime.Now
};

// Destructure the object with @
Log.Information("Order processed: {@Order}", order);

// Or log individual properties
Log.Information("Order {OrderId} for {CustomerName} totaling {Total:C} completed",
    order.OrderId, order.CustomerName, order.Total);
```

## Error Handling

### Exception Logging

```csharp
using Serilog;
using XTinyLog.Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.XTinyLog()
    .CreateLogger();

try
{
    // Some operation that might fail
    var result = 10 / 0;
}
catch (DivideByZeroException ex)
{
    Log.Error(ex, "Division by zero occurred in calculation");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unexpected error occurred");
}
```

### Nested Exception Logging

```csharp
using Serilog;
using XTinyLog.Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.XTinyLog()
    .CreateLogger();

try
{
    try
    {
        throw new InvalidOperationException("Inner exception");
    }
    catch (Exception inner)
    {
        throw new ApplicationException("Outer exception", inner);
    }
}
catch (Exception ex)
{
    Log.Error(ex, "Nested exception occurred while processing {Operation}", "UserRegistration");
    // XTinyLog will format both the outer and inner exceptions with full stack traces
}
```

### Graceful Error Handling

```csharp
using Serilog;
using XTinyLog.Serilog;

public class DatabaseService
{
    private readonly ILogger _logger = Log.ForContext<DatabaseService>();

    public async Task<User> GetUserAsync(int userId)
    {
        try
        {
            _logger.Information("Retrieving user {UserId}", userId);
            
            // Database operation
            var user = await GetUserFromDatabase(userId);
            
            _logger.Information("Successfully retrieved user {UserId}", userId);
            return user;
        }
        catch (SqlException ex)
        {
            _logger.Error(ex, "Database error while retrieving user {UserId}", userId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.Fatal(ex, "Unexpected error while retrieving user {UserId}", userId);
            throw;
        }
    }
}
```

## Performance Optimization

### Asynchronous Logging

```csharp
using Serilog;
using XTinyLog.Serilog;

// For high-throughput applications
Log.Logger = new LoggerConfiguration()
    .WriteTo.XTinyLogAsync(config =>
    {
        config.LogDirectory = @"C:\HighPerformanceApp\Logs";
        config.ApplicationName = "HighThroughputService";
        config.MaxFileSize = 5120; // 5MB for fewer file rotations
        config.PropertyFormat = PropertyFormatMode.Hidden; // Faster processing
    })
    .CreateLogger();

// Log intensive operations
for (int i = 0; i < 10000; i++)
{
    Log.Information("Processing item {ItemId} in batch {BatchId}", i, 42);
}
```

### Conditional Logging

```csharp
using Serilog;
using XTinyLog.Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information() // Skip debug messages in production
    .WriteTo.XTinyLog(config =>
    {
        config.PropertyFormat = PropertyFormatMode.Hidden; // Skip properties for performance
    })
    .CreateLogger();

// Use IsEnabled to avoid expensive operations
if (Log.IsEnabled(LogEventLevel.Debug))
{
    var expensiveData = CalculateExpensiveDebugInfo();
    Log.Debug("Debug info: {@Data}", expensiveData);
}
```

### Batched Logging

```csharp
using Serilog;
using XTinyLog.Serilog;

// Configure with batching for better performance
Log.Logger = new LoggerConfiguration()
    .WriteTo.Async(a => a.XTinyLog(config =>
    {
        config.LogDirectory = @"C:\BatchedLogs";
        config.ApplicationName = "BatchedApp";
    }))
    .CreateLogger();
```

## Integration Examples

### ASP.NET Core Web Application

**Program.cs:**
```csharp
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
            config.LogDirectory = Path.Combine(builder.Environment.ContentRootPath, "Logs");
            config.ApplicationName = builder.Environment.ApplicationName;
            config.PropertyFormat = PropertyFormatMode.Structured;
        });
});

var app = builder.Build();

// Add Serilog request logging
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.GetLevel = (httpContext, elapsed, ex) => ex != null 
        ? LogEventLevel.Error 
        : httpContext.Response.StatusCode > 499 
            ? LogEventLevel.Error 
            : LogEventLevel.Information;
});

app.MapGet("/", () => "Hello World!");

app.MapGet("/error", () =>
{
    Log.Error("Test error endpoint called");
    throw new InvalidOperationException("Test exception");
});

app.Run();
```

### Windows Service

```csharp
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using XTinyLog.Serilog;

public class WorkerService : BackgroundService
{
    private readonly ILogger<WorkerService> _logger;

    public WorkerService(ILogger<WorkerService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {Time}", DateTimeOffset.Now);
            
            try
            {
                await DoWork();
                _logger.LogInformation("Work completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during work execution");
            }

            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task DoWork()
    {
        // Simulate work
        await Task.Delay(100);
    }
}

// Program.cs
var builder = Host.CreateApplicationBuilder(args);

// Configure Serilog
builder.Services.AddSerilog((services, configuration) =>
{
    configuration
        .WriteTo.Console()
        .WriteTo.XTinyLogAsync(config =>
        {
            config.LogDirectory = @"C:\Services\MyService\Logs";
            config.ApplicationName = "MyWindowsService";
            config.MaxFileSize = 10240; // 10MB
        });
});

builder.Services.AddHostedService<WorkerService>();

var host = builder.Build();
await host.RunAsync();
```

### Console Application with Dependency Injection

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using XTinyLog.Serilog;

public class Application
{
    private readonly ILogger<Application> _logger;

    public Application(ILogger<Application> logger)
    {
        _logger = logger;
    }

    public async Task RunAsync()
    {
        _logger.LogInformation("Application starting");

        try
        {
            await ProcessData();
            _logger.LogInformation("Application completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Application failed");
            throw;
        }
    }

    private async Task ProcessData()
    {
        _logger.LogInformation("Processing data...");
        await Task.Delay(1000);
        _logger.LogInformation("Data processing completed");
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .UseSerilog((context, configuration) =>
            {
                configuration
                    .WriteTo.Console()
                    .WriteTo.XTinyLog(config =>
                    {
                        config.LogDirectory = @"C:\ConsoleApp\Logs";
                        config.ApplicationName = "ConsoleApplication";
                    });
            })
            .ConfigureServices(services =>
            {
                services.AddTransient<Application>();
            })
            .Build();

        try
        {
            var app = host.Services.GetRequiredService<Application>();
            await app.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            await host.StopAsync();
            Log.CloseAndFlush();
        }
    }
}
```

## Advanced Scenarios

### Multiple Sinks with Different Configurations

```csharp
using Serilog;
using XTinyLog.Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    // Detailed logs for debugging
    .WriteTo.XTinyLog(config =>
    {
        config.LogDirectory = @"C:\Logs\Debug";
        config.ApplicationName = "MyApp-Debug";
        config.PropertyFormat = PropertyFormatMode.Structured;
    })
    // Simplified logs for production monitoring
    .WriteTo.XTinyLog(config =>
    {
        config.LogDirectory = @"C:\Logs\Production";
        config.ApplicationName = "MyApp-Prod";
        config.PropertyFormat = PropertyFormatMode.Hidden;
        config.MaxFileSize = 10240; // Larger files for production
    })
    .CreateLogger();
```

### Conditional Sink Configuration

```csharp
using Serilog;
using XTinyLog.Serilog;

var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

var loggerConfig = new LoggerConfiguration()
    .WriteTo.Console();

if (isDevelopment)
{
    loggerConfig.WriteTo.XTinyLog(config =>
    {
        config.LogDirectory = @"C:\Dev\Logs";
        config.ApplicationName = "MyApp-Dev";
        config.PropertyFormat = PropertyFormatMode.Structured;
    });
}
else
{
    loggerConfig.WriteTo.XTinyLogAsync(config =>
    {
        config.LogDirectory = @"C:\Prod\Logs";
        config.ApplicationName = "MyApp-Prod";
        config.PropertyFormat = PropertyFormatMode.Inline;
        config.MaxFileSize = 5120;
    });
}

Log.Logger = loggerConfig.CreateLogger();
```

### Custom Context and Enrichment

```csharp
using Serilog;
using Serilog.Context;
using XTinyLog.Serilog;

Log.Logger = new LoggerConfiguration()
    .Enrich.WithProperty("Application", "MyApplication")
    .Enrich.WithProperty("Version", "1.0.0")
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentUserName()
    .WriteTo.XTinyLog(config =>
    {
        config.LogDirectory = @"C:\Logs";
        config.ApplicationName = "EnrichedApp";
        config.PropertyFormat = PropertyFormatMode.Structured;
    })
    .CreateLogger();

// Use context to add request-specific information
using (LogContext.PushProperty("RequestId", Guid.NewGuid()))
using (LogContext.PushProperty("UserId", 123))
{
    Log.Information("Processing user request");
    
    // All logs within this scope will include RequestId and UserId
    Log.Warning("Validation warning occurred");
    Log.Information("Request processing completed");
}
```

### Integration with Health Checks

```csharp
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using XTinyLog.Serilog;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(ILogger<DatabaseHealthCheck> logger)
    {
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Performing database health check");
            
            // Perform health check logic
            await CheckDatabaseConnection();
            
            _logger.LogInformation("Database health check passed");
            return HealthCheckResult.Healthy("Database is healthy");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return HealthCheckResult.Unhealthy("Database is unhealthy", ex);
        }
    }

    private async Task CheckDatabaseConnection()
    {
        // Database connection check logic
        await Task.Delay(100);
    }
}

// Configure in Program.cs
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database");

builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .WriteTo.XTinyLog(config =>
        {
            config.LogDirectory = @"C:\HealthChecks\Logs";
            config.ApplicationName = "HealthCheckService";
        });
});
```

### Testing with XTinyLog Sink

```csharp
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using XTinyLog.Serilog;
using Xunit;

public class ServiceTests
{
    [Fact]
    public void Service_Should_Log_Operations()
    {
        // Arrange
        var tempLogDir = Path.Combine(Path.GetTempPath(), "TestLogs", Guid.NewGuid().ToString());
        
        var serilogLogger = new LoggerConfiguration()
            .WriteTo.XTinyLog(config =>
            {
                config.LogDirectory = tempLogDir;
                config.ApplicationName = "TestApp";
            })
            .CreateLogger();

        var loggerFactory = new SerilogLoggerFactory(serilogLogger);
        var logger = loggerFactory.CreateLogger<MyService>();
        
        var service = new MyService(logger);

        // Act
        service.DoSomething();

        // Assert
        serilogLogger.Dispose();
        
        // Verify log files were created
        Assert.True(Directory.Exists(tempLogDir));
        var logFiles = Directory.GetFiles(tempLogDir, "*.html");
        Assert.NotEmpty(logFiles);

        // Cleanup
        Directory.Delete(tempLogDir, true);
    }
}

public class MyService
{
    private readonly ILogger<MyService> _logger;

    public MyService(ILogger<MyService> logger)
    {
        _logger = logger;
    }

    public void DoSomething()
    {
        _logger.LogInformation("Service operation started");
        _logger.LogInformation("Service operation completed");
    }
}
```

## Best Practices

1. **Always dispose the logger**: Use `Log.CloseAndFlush()` or dispose the logger properly
2. **Use structured logging**: Take advantage of Serilog's structured logging capabilities
3. **Choose appropriate property format**: Use `Structured` for debugging, `Inline` for production, `Hidden` for performance
4. **Handle exceptions properly**: Always log exceptions with context information
5. **Use async sinks for high throughput**: Consider `XTinyLogAsync` for performance-critical applications
6. **Configure appropriate log levels**: Use minimum log levels to control verbosity
7. **Test your logging configuration**: Verify that logs are being written correctly in your test environment

These examples should cover most common scenarios for using XTinyLog.Serilog in your applications.