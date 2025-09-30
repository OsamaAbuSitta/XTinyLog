using XTinyLog;

namespace XTinyLog.Samples.Console;

/// <summary>
/// Console application demonstrating basic XTinyLog functionality
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        System.Console.WriteLine("XTinyLog Console Sample Application");
        System.Console.WriteLine("===================================");
        
        // Example 1: Basic logging with default configuration
        await BasicLoggingExample();
        
        // Example 2: Custom configuration using action delegate
        await CustomConfigurationExample();
        
        // Example 3: Fluent configuration builder
        await FluentConfigurationExample();
        
        // Example 4: Async logging patterns
        await AsyncLoggingExample();
        
        // Example 5: Multiple logger instances
        await MultipleInstancesExample();
        
        // Example 6: Error handling and exception logging
        await ErrorHandlingExample();
        
        System.Console.WriteLine("\nAll examples completed! Check the log files in the specified directories.");
        System.Console.WriteLine("Press any key to exit...");
        System.Console.ReadKey();
    }

    /// <summary>
    /// Demonstrates basic logging functionality with default configuration
    /// </summary>
    static async Task BasicLoggingExample()
    {
        System.Console.WriteLine("\n1. Basic Logging Example");
        System.Console.WriteLine("------------------------");
        
        // Using the singleton instance with default configuration
        XLog.Log.Info("Application started - Basic example");
        XLog.Log.Debug("This is a debug message");
        XLog.Log.Warn("This is a warning message");
        XLog.Log.Error("This is an error message");
        
        // Async versions
        await XLog.Log.InfoAsync("Async info message");
        await XLog.Log.DebugAsync("Async debug message");
        
        System.Console.WriteLine("✓ Basic logging completed");
        System.Console.WriteLine($"✓ Log directory: {XLog.Log.Config.LogDirectory}");
        System.Console.WriteLine($"✓ Application name: {XLog.Log.Config.ApplicationName}");
    }

    /// <summary>
    /// Demonstrates custom configuration using action delegate
    /// </summary>
    static async Task CustomConfigurationExample()
    {
        System.Console.WriteLine("\n2. Custom Configuration Example");
        System.Console.WriteLine("-------------------------------");
        
        try
        {
            // Create logger with custom configuration
            var customLogger = XLog.Create(config =>
            {
                config.LogDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "XTinyLogSamples");
                config.ApplicationName = "ConsoleApp";
                config.MaxFileSize = 2000; // 2MB
                config.FileName = () => $"custom_log_{DateTime.Now:yyyyMMdd_HHmm}";
            });

            customLogger.Info("Custom configuration logger started");
            customLogger.Debug("Custom log directory and file naming");
            await customLogger.WarnAsync("Async warning with custom config");
            
            System.Console.WriteLine("✓ Custom configuration logging completed");
            System.Console.WriteLine($"✓ Custom log directory: {customLogger.Config.LogDirectory}");
            System.Console.WriteLine($"✓ Custom application name: {customLogger.Config.ApplicationName}");
            System.Console.WriteLine($"✓ Custom max file size: {customLogger.Config.MaxFileSize} KB");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"✗ Configuration error: {ex.Message}");
        }
    }

    /// <summary>
    /// Demonstrates fluent configuration builder pattern
    /// </summary>
    static async Task FluentConfigurationExample()
    {
        System.Console.WriteLine("\n3. Fluent Configuration Example");
        System.Console.WriteLine("-------------------------------");
        
        try
        {
            // Using fluent configuration builder
            var fluentLogger = XLog.Create(builder => builder
                .WithLogDirectory(Path.Combine(Path.GetTempPath(), "XTinyLogFluent"))
                .WithApplicationName("FluentSample")
                .WithMaxFileSize(1500)
                .WithDateBasedFileName("yyyy-MM-dd_HH-mm"));

            fluentLogger.Info("Fluent configuration logger initialized");
            fluentLogger.Debug("Using fluent builder pattern for configuration");
            await fluentLogger.InfoAsync("Async logging with fluent config");
            
            System.Console.WriteLine("✓ Fluent configuration logging completed");
            System.Console.WriteLine($"✓ Fluent log directory: {fluentLogger.Config.LogDirectory}");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"✗ Fluent configuration error: {ex.Message}");
        }
    }

    /// <summary>
    /// Demonstrates async logging patterns and cancellation
    /// </summary>
    static async Task AsyncLoggingExample()
    {
        System.Console.WriteLine("\n4. Async Logging Example");
        System.Console.WriteLine("------------------------");
        
        var asyncLogger = XLog.Create(config =>
        {
            config.LogDirectory = Path.Combine(Path.GetTempPath(), "XTinyLogAsync");
            config.ApplicationName = "AsyncSample";
        });

        // Demonstrate async logging with cancellation token
        using var cts = new CancellationTokenSource();
        
        try
        {
            await asyncLogger.InfoAsync("Starting async operations", cts.Token);
            
            // Simulate multiple async operations
            var tasks = new List<Task>();
            for (int i = 0; i < 5; i++)
            {
                int taskId = i;
                tasks.Add(Task.Run(async () =>
                {
                    await asyncLogger.DebugAsync($"Async task {taskId} executing", cts.Token);
                    await Task.Delay(100, cts.Token); // Simulate work
                    await asyncLogger.InfoAsync($"Async task {taskId} completed", cts.Token);
                }));
            }
            
            await Task.WhenAll(tasks);
            await asyncLogger.InfoAsync("All async operations completed", cts.Token);
            
            System.Console.WriteLine("✓ Async logging completed successfully");
        }
        catch (OperationCanceledException)
        {
            System.Console.WriteLine("✓ Async operations were cancelled");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"✗ Async logging error: {ex.Message}");
        }
    }

    /// <summary>
    /// Demonstrates multiple logger instances for different purposes
    /// </summary>
    static async Task MultipleInstancesExample()
    {
        System.Console.WriteLine("\n5. Multiple Instances Example");
        System.Console.WriteLine("-----------------------------");
        
        // Create separate loggers for different components
        var authLogger = XLog.Create(config =>
        {
            config.LogDirectory = Path.Combine(Path.GetTempPath(), "XTinyLogMultiple");
            config.ApplicationName = "Authentication";
            config.FileName = () => "auth_log";
        });

        var dataLogger = XLog.Create(config =>
        {
            config.LogDirectory = Path.Combine(Path.GetTempPath(), "XTinyLogMultiple");
            config.ApplicationName = "DataAccess";
            config.FileName = () => "data_log";
        });

        var apiLogger = XLog.Create(config =>
        {
            config.LogDirectory = Path.Combine(Path.GetTempPath(), "XTinyLogMultiple");
            config.ApplicationName = "API";
            config.FileName = () => "api_log";
        });

        // Log to different instances
        authLogger.Info("User authentication started");
        dataLogger.Debug("Database connection established");
        apiLogger.Info("API request received");

        await authLogger.InfoAsync("User authenticated successfully");
        await dataLogger.DebugAsync("Query executed: SELECT * FROM Users");
        await apiLogger.InfoAsync("API response sent");

        authLogger.Warn("Authentication token expires in 5 minutes");
        dataLogger.Error("Database connection timeout");
        apiLogger.Error("API rate limit exceeded");

        System.Console.WriteLine("✓ Multiple instances logging completed");
        System.Console.WriteLine("✓ Check separate log files for each component");
    }

    /// <summary>
    /// Demonstrates error handling and exception logging
    /// </summary>
    static async Task ErrorHandlingExample()
    {
        System.Console.WriteLine("\n6. Error Handling Example");
        System.Console.WriteLine("-------------------------");
        
        var errorLogger = XLog.Create(config =>
        {
            config.LogDirectory = Path.Combine(Path.GetTempPath(), "XTinyLogErrors");
            config.ApplicationName = "ErrorHandling";
        });

        try
        {
            errorLogger.Info("Starting error handling demonstration");
            
            // Simulate various error scenarios
            try
            {
                throw new ArgumentNullException("testParameter", "This is a test exception");
            }
            catch (Exception ex)
            {
                errorLogger.Error(ex);
                await errorLogger.ErrorAsync(ex);
            }

            try
            {
                throw new InvalidOperationException("Invalid operation for demonstration");
            }
            catch (Exception ex)
            {
                errorLogger.Error($"Operation failed: {ex.Message}");
                await errorLogger.ErrorAsync($"Async error logging: {ex.Message}");
            }

            // Test configuration validation errors
            try
            {
                var invalidLogger = XLog.Create(config =>
                {
                    config.LogDirectory = ""; // Invalid empty directory
                    config.ApplicationName = "";
                });
            }
            catch (Exception ex)
            {
                errorLogger.Error($"Configuration validation caught: {ex.Message}");
                System.Console.WriteLine($"✓ Configuration validation working: {ex.Message}");
            }

            errorLogger.Info("Error handling demonstration completed");
            System.Console.WriteLine("✓ Error handling and exception logging completed");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"✗ Unexpected error in error handling example: {ex.Message}");
        }
    }
}