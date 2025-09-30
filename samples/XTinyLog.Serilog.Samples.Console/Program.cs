using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using XTinyLog.Serilog;

namespace XTinyLog.Serilog.Samples.Console;

/// <summary>
/// Console application demonstrating XTinyLog.Serilog sink usage.
/// This sample shows various configuration patterns and logging scenarios.
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        System.Console.WriteLine("XTinyLog.Serilog Console Sample");
        System.Console.WriteLine("================================");
        System.Console.WriteLine();

        // Run different demonstration scenarios
        await RunBasicExample();
        await RunConfigurationExamples();
        await RunStructuredLoggingExample();
        await RunErrorHandlingExample();
        await RunAsyncLoggingExample();
        await RunDependencyInjectionExample();

        System.Console.WriteLine();
        System.Console.WriteLine("Sample completed! Check the 'Logs' directory for HTML output files.");
        System.Console.WriteLine("Press any key to exit...");
        System.Console.ReadKey();
    }

    /// <summary>
    /// Demonstrates basic XTinyLog sink usage with default configuration.
    /// </summary>
    static async Task RunBasicExample()
    {
        System.Console.WriteLine("1. Basic Example - Default Configuration");
        System.Console.WriteLine("----------------------------------------");

        // Configure Serilog with XTinyLog sink using defaults
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.XTinyLog()
            .CreateLogger();

        Log.Information("Basic example started");
        Log.Debug("This is a debug message");
        Log.Information("This is an information message");
        Log.Warning("This is a warning message");
        Log.Error("This is an error message");
        Log.Fatal("This is a fatal message");
        Log.Information("Basic example completed");

        Log.CloseAndFlush();
        await Task.Delay(100); // Allow time for async operations to complete

        System.Console.WriteLine("✓ Basic logging completed");
        System.Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates different configuration patterns for the XTinyLog sink.
    /// </summary>
    static async Task RunConfigurationExamples()
    {
        System.Console.WriteLine("2. Configuration Examples");
        System.Console.WriteLine("-------------------------");

        // Example 1: Fluent configuration
        System.Console.WriteLine("  a) Fluent Configuration");
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.XTinyLog(config =>
            {
                config.LogDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "Fluent");
                config.ApplicationName = "FluentConfigSample";
                config.MaxFileSize = 1024; // 1MB
                config.PropertyFormat = PropertyFormatMode.Structured;
            })
            .CreateLogger();

        Log.Information("Fluent configuration example");
        Log.Information("Log directory: {LogDirectory}", Path.Combine(Directory.GetCurrentDirectory(), "Logs", "Fluent"));
        Log.CloseAndFlush();

        // Example 2: Builder pattern configuration
        System.Console.WriteLine("  b) Builder Pattern Configuration");
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.XTinyLogWithBuilder(builder =>
                builder.WithLogDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Logs", "Builder"))
                       .WithApplicationName("BuilderConfigSample")
                       .WithMaxFileSize(2048)
                       .WithPropertyFormat(PropertyFormatMode.Inline)
                       .WithIncludeProperties(true))
            .CreateLogger();

        Log.Information("Builder pattern configuration example");
        Log.Information("Using builder pattern for configuration");
        Log.CloseAndFlush();

        // Example 3: Direct parameter configuration
        System.Console.WriteLine("  c) Direct Parameter Configuration");
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.XTinyLog(
                logDirectory: Path.Combine(Directory.GetCurrentDirectory(), "Logs", "Direct"),
                applicationName: "DirectConfigSample",
                maxFileSize: 512,
                includeProperties: true,
                propertyFormat: PropertyFormatMode.Hidden)
            .CreateLogger();

        Log.Information("Direct parameter configuration example");
        Log.Information("Properties are hidden in this configuration");
        Log.CloseAndFlush();

        await Task.Delay(100);
        System.Console.WriteLine("✓ Configuration examples completed");
        System.Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates structured logging with different property formatting modes.
    /// </summary>
    static async Task RunStructuredLoggingExample()
    {
        System.Console.WriteLine("3. Structured Logging Example");
        System.Console.WriteLine("------------------------------");

        // Example with inline properties
        System.Console.WriteLine("  a) Inline Property Format");
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.XTinyLog(config =>
            {
                config.LogDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "Structured", "Inline");
                config.ApplicationName = "StructuredInline";
                config.PropertyFormat = PropertyFormatMode.Inline;
            })
            .CreateLogger();

        Log.Information("User {UserId} logged in from {IPAddress} at {LoginTime}",
            12345, "192.168.1.100", DateTime.Now);

        var user = new { Id = 12345, Name = "John Doe", Email = "john.doe@example.com", Role = "Administrator" };
        Log.Information("User details: {@User}", user);

        Log.Warning("Failed login attempt for user {UserId} from {IPAddress} - {Reason}",
            67890, "10.0.0.50", "Invalid password");

        Log.CloseAndFlush();

        // Example with structured properties
        System.Console.WriteLine("  b) Structured Property Format");
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.XTinyLog(config =>
            {
                config.LogDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "Structured", "Formatted");
                config.ApplicationName = "StructuredFormatted";
                config.PropertyFormat = PropertyFormatMode.Structured;
            })
            .CreateLogger();

        var order = new
        {
            OrderId = "ORD-2024-001",
            CustomerId = 12345,
            CustomerName = "Jane Smith",
            Items = new[]
            {
                new { ProductId = "PROD-001", Name = "Widget A", Quantity = 2, Price = 29.99m },
                new { ProductId = "PROD-002", Name = "Widget B", Quantity = 1, Price = 49.99m }
            },
            Total = 109.97m,
            OrderDate = DateTime.Now,
            ShippingAddress = new
            {
                Street = "123 Main St",
                City = "Anytown",
                State = "ST",
                ZipCode = "12345"
            }
        };

        Log.Information("Order processed: {@Order}", order);
        Log.Information("Payment processed for order {OrderId} - Amount: {Amount:C} - Method: {PaymentMethod}",
            order.OrderId, order.Total, "Credit Card");

        Log.CloseAndFlush();

        // Example with hidden properties
        System.Console.WriteLine("  c) Hidden Property Format");
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.XTinyLog(config =>
            {
                config.LogDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "Structured", "Hidden");
                config.ApplicationName = "StructuredHidden";
                config.PropertyFormat = PropertyFormatMode.Hidden;
            })
            .CreateLogger();

        Log.Information("User {UserId} performed {Action} on {Resource}",
            12345, "UPDATE", "UserProfile");
        Log.Information("Clean message without property details: {@ComplexObject}", user);

        Log.CloseAndFlush();

        await Task.Delay(100);
        System.Console.WriteLine("✓ Structured logging examples completed");
        System.Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates error handling and exception logging.
    /// </summary>
    static async Task RunErrorHandlingExample()
    {
        System.Console.WriteLine("4. Error Handling Example");
        System.Console.WriteLine("--------------------------");

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.XTinyLog(config =>
            {
                config.LogDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "ErrorHandling");
                config.ApplicationName = "ErrorHandlingSample";
                config.PropertyFormat = PropertyFormatMode.Structured;
            })
            .CreateLogger();

        Log.Information("Starting error handling demonstration");

        // Simple exception logging
        try
        {
            throw new InvalidOperationException("This is a test exception");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Simple exception occurred during operation {Operation}", "TestOperation");
        }

        // Nested exception logging
        try
        {
            try
            {
                throw new ArgumentException("Inner exception message", "parameterName");
            }
            catch (Exception inner)
            {
                throw new InvalidOperationException("Outer exception message", inner);
            }
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Nested exception occurred in {Component} while processing {RequestId}",
                "DataProcessor", Guid.NewGuid());
        }

        // Exception with structured data
        try
        {
            var context = new
            {
                UserId = 12345,
                SessionId = Guid.NewGuid(),
                RequestPath = "/api/users/12345",
                Timestamp = DateTime.UtcNow,
                Headers = new Dictionary<string, string>
                {
                    ["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)",
                    ["Accept"] = "application/json",
                    ["Authorization"] = "Bearer [REDACTED]"
                }
            };

            throw new UnauthorizedAccessException("User does not have permission to access this resource");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Authorization failed for user {UserId} accessing {Resource} with context {@Context}",
                12345, "/api/sensitive-data", new { RequestId = Guid.NewGuid(), Timestamp = DateTime.UtcNow });
        }

        Log.Information("Error handling demonstration completed");
        Log.CloseAndFlush();

        await Task.Delay(100);
        System.Console.WriteLine("✓ Error handling examples completed");
        System.Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates asynchronous logging capabilities.
    /// </summary>
    static async Task RunAsyncLoggingExample()
    {
        System.Console.WriteLine("5. Asynchronous Logging Example");
        System.Console.WriteLine("--------------------------------");

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.XTinyLogAsync(config =>
            {
                config.LogDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "Async");
                config.ApplicationName = "AsyncLoggingSample";
                config.PropertyFormat = PropertyFormatMode.Inline;
            })
            .CreateLogger();

        Log.Information("Starting asynchronous logging demonstration");

        // Simulate high-volume logging
        var tasks = new List<Task>();
        for (int i = 0; i < 100; i++)
        {
            int iteration = i;
            tasks.Add(Task.Run(() =>
            {
                Log.Information("Async log message {MessageNumber} from task {TaskId}",
                    iteration, Task.CurrentId);

                if (iteration % 10 == 0)
                {
                    Log.Warning("Checkpoint reached at iteration {Iteration}", iteration);
                }

                if (iteration % 25 == 0)
                {
                    var data = new { Iteration = iteration, Timestamp = DateTime.Now, ThreadId = Thread.CurrentThread.ManagedThreadId };
                    Log.Information("Detailed checkpoint: {@Data}", data);
                }
            }));
        }

        await Task.WhenAll(tasks);

        Log.Information("Asynchronous logging demonstration completed - processed {MessageCount} messages", 100);
        Log.CloseAndFlush();

        await Task.Delay(200); // Allow extra time for async operations
        System.Console.WriteLine("✓ Asynchronous logging examples completed");
        System.Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates integration with dependency injection and hosting.
    /// </summary>
    static async Task RunDependencyInjectionExample()
    {
        System.Console.WriteLine("6. Dependency Injection Example");
        System.Console.WriteLine("--------------------------------");

        var host = Host.CreateDefaultBuilder()
            .UseSerilog((context, configuration) =>
            {
                configuration
                    .WriteTo.Console()
                    .WriteTo.XTinyLog(config =>
                    {
                        config.LogDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "DependencyInjection");
                        config.ApplicationName = "DISample";
                        config.PropertyFormat = PropertyFormatMode.Structured;
                    });
            })
            .ConfigureServices(services =>
            {
                services.AddTransient<SampleService>();
                services.AddTransient<DataProcessor>();
            })
            .Build();

        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Dependency injection example started");

        var sampleService = host.Services.GetRequiredService<SampleService>();
        await sampleService.ProcessDataAsync();

        var dataProcessor = host.Services.GetRequiredService<DataProcessor>();
        await dataProcessor.ProcessBatchAsync(new[] { "Item1", "Item2", "Item3", "Item4", "Item5" });

        logger.LogInformation("Dependency injection example completed");

        await host.StopAsync();

        System.Console.WriteLine("✓ Dependency injection examples completed");
        System.Console.WriteLine();
    }
}

/// <summary>
/// Sample service demonstrating logging in a dependency injection context.
/// </summary>
public class SampleService
{
    private readonly ILogger<SampleService> _logger;

    public SampleService(ILogger<SampleService> logger)
    {
        _logger = logger;
    }

    public async Task ProcessDataAsync()
    {
        var correlationId = Guid.NewGuid();
        _logger.LogInformation("Starting data processing with correlation ID {CorrelationId}", correlationId);

        try
        {
            await SimulateWork("Initialize", 100);
            await SimulateWork("Process", 200);
            await SimulateWork("Validate", 150);
            await SimulateWork("Finalize", 100);

            _logger.LogInformation("Data processing completed successfully for correlation ID {CorrelationId}", correlationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Data processing failed for correlation ID {CorrelationId}", correlationId);
            throw;
        }
    }

    private async Task SimulateWork(string operation, int delayMs)
    {
        _logger.LogDebug("Starting operation {Operation}", operation);
        await Task.Delay(delayMs);
        _logger.LogDebug("Completed operation {Operation} in {Duration}ms", operation, delayMs);
    }
}

/// <summary>
/// Sample data processor demonstrating batch processing with logging.
/// </summary>
public class DataProcessor
{
    private readonly ILogger<DataProcessor> _logger;

    public DataProcessor(ILogger<DataProcessor> logger)
    {
        _logger = logger;
    }

    public async Task ProcessBatchAsync(IEnumerable<string> items)
    {
        var batchId = Guid.NewGuid();
        var itemList = items.ToList();

        _logger.LogInformation("Starting batch processing for {ItemCount} items with batch ID {BatchId}",
            itemList.Count, batchId);

        var processed = 0;
        var failed = 0;

        foreach (var item in itemList)
        {
            try
            {
                await ProcessItemAsync(item, batchId);
                processed++;
                _logger.LogDebug("Successfully processed item {Item} in batch {BatchId}", item, batchId);
            }
            catch (Exception ex)
            {
                failed++;
                _logger.LogWarning(ex, "Failed to process item {Item} in batch {BatchId}", item, batchId);
            }
        }

        var result = new
        {
            BatchId = batchId,
            TotalItems = itemList.Count,
            ProcessedItems = processed,
            FailedItems = failed,
            SuccessRate = (double)processed / itemList.Count * 100
        };

        _logger.LogInformation("Batch processing completed: {@BatchResult}", result);
    }

    private async Task ProcessItemAsync(string item, Guid batchId)
    {
        // Simulate processing with occasional failures
        await Task.Delay(50);

        if (item.Contains("Item3"))
        {
            throw new InvalidOperationException($"Simulated processing failure for {item}");
        }

        _logger.LogTrace("Item {Item} processed successfully in batch {BatchId}", item, batchId);
    }
}