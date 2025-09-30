using System.Diagnostics;
using XTinyLog;

namespace XTinyLog.Samples.Advanced;

/// <summary>
/// Advanced configuration examples demonstrating custom formatting, multiple instances, and performance optimization
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("XTinyLog Advanced Configuration Examples");
        Console.WriteLine("=======================================");
        
        // Example 1: Custom log formatting and styling
        await CustomFormattingExample();
        
        // Example 2: Multiple logger instances for different components
        await MultipleLoggerInstancesExample();
        
        // Example 3: Performance optimization techniques
        await PerformanceOptimizationExample();
        
        // Example 4: Advanced file management and rotation
        await AdvancedFileManagementExample();
        
        // Example 5: Custom HTML styling and themes
        await CustomHtmlStylingExample();
        
        // Example 6: Structured logging patterns
        await StructuredLoggingExample();
        
        Console.WriteLine("\nAll advanced examples completed!");
        Console.WriteLine("Check the various log directories for different HTML outputs with custom styling.");
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    /// <summary>
    /// Demonstrates custom log formatting and file naming patterns
    /// </summary>
    static async Task CustomFormattingExample()
    {
        Console.WriteLine("\n1. Custom Formatting Example");
        Console.WriteLine("----------------------------");
        
        // Create logger with custom file naming based on application context
        var customLogger = XLog.Create(config =>
        {
            config.LogDirectory = Path.Combine(Path.GetTempPath(), "XTinyLogAdvanced", "CustomFormatting");
            config.ApplicationName = "AdvancedApp";
            config.MaxFileSize = 1000; // Smaller files for demonstration
            
            // Custom file naming with timestamp and process ID
            config.FileName = () => $"app_{DateTime.Now:yyyyMMdd_HHmmss}_{Environment.ProcessId}";
        });

        // Log various message types with structured information
        customLogger.Info($"Application started - Version: 1.0.0, Environment: {Environment.MachineName}");
        customLogger.Debug($"Configuration loaded - Directory: {customLogger.Config.LogDirectory}");
        customLogger.Warn($"Performance warning - Memory usage: {GC.GetTotalMemory(false):N0} bytes");
        customLogger.Error($"Simulated error - Error code: E001, Timestamp: {DateTime.UtcNow:O}");
        
        // Async logging with context
        await customLogger.InfoAsync($"Async operation completed - Thread: {Thread.CurrentThread.ManagedThreadId}");
        
        Console.WriteLine("✓ Custom formatting completed");
        Console.WriteLine($"✓ Log files created with custom naming pattern");
    }

    /// <summary>
    /// Demonstrates multiple logger instances for different application components
    /// </summary>
    static async Task MultipleLoggerInstancesExample()
    {
        Console.WriteLine("\n2. Multiple Logger Instances Example");
        Console.WriteLine("-----------------------------------");
        
        var baseDirectory = Path.Combine(Path.GetTempPath(), "XTinyLogAdvanced", "MultipleInstances");
        
        // Database logger with specific configuration
        var dbLogger = XLog.Create(config =>
        {
            config.LogDirectory = Path.Combine(baseDirectory, "Database");
            config.ApplicationName = "DataAccess";
            config.MaxFileSize = 2000;
            config.FileName = () => $"db_operations_{DateTime.Now:yyyy-MM-dd}";
        });

        // API logger with different settings
        var apiLogger = XLog.Create(config =>
        {
            config.LogDirectory = Path.Combine(baseDirectory, "API");
            config.ApplicationName = "WebAPI";
            config.MaxFileSize = 3000;
            config.FileName = () => $"api_requests_{DateTime.Now:yyyy-MM-dd}";
        });

        // Security logger for audit trails
        var securityLogger = XLog.Create(config =>
        {
            config.LogDirectory = Path.Combine(baseDirectory, "Security");
            config.ApplicationName = "Security";
            config.MaxFileSize = 1500;
            config.FileName = () => $"security_audit_{DateTime.Now:yyyy-MM-dd}";
        });

        // Business logic logger
        var businessLogger = XLog.Create(config =>
        {
            config.LogDirectory = Path.Combine(baseDirectory, "Business");
            config.ApplicationName = "BusinessLogic";
            config.MaxFileSize = 2500;
            config.FileName = () => $"business_events_{DateTime.Now:yyyy-MM-dd}";
        });

        // Simulate different component activities
        await SimulateDatabaseOperations(dbLogger);
        await SimulateApiRequests(apiLogger);
        await SimulateSecurityEvents(securityLogger);
        await SimulateBusinessLogic(businessLogger);
        
        Console.WriteLine("✓ Multiple logger instances completed");
        Console.WriteLine("✓ Check separate directories for component-specific logs");
    }

    /// <summary>
    /// Demonstrates performance optimization techniques
    /// </summary>
    static async Task PerformanceOptimizationExample()
    {
        Console.WriteLine("\n3. Performance Optimization Example");
        Console.WriteLine("----------------------------------");
        
        var performanceLogger = XLog.Create(config =>
        {
            config.LogDirectory = Path.Combine(Path.GetTempPath(), "XTinyLogAdvanced", "Performance");
            config.ApplicationName = "PerformanceTest";
            config.MaxFileSize = 5000; // Larger files for performance testing
            config.FileName = () => $"perf_test_{DateTime.Now:yyyyMMdd_HHmm}";
        });

        // Measure logging performance
        var stopwatch = Stopwatch.StartNew();
        
        // Synchronous logging performance test
        Console.WriteLine("Testing synchronous logging performance...");
        var syncStart = stopwatch.ElapsedMilliseconds;
        
        for (int i = 0; i < 100; i++)
        {
            performanceLogger.Info($"Sync log message {i:D3} - Performance test data with timestamp {DateTime.Now:HH:mm:ss.fff}");
        }
        
        var syncEnd = stopwatch.ElapsedMilliseconds;
        var syncDuration = syncEnd - syncStart;
        
        // Asynchronous logging performance test
        Console.WriteLine("Testing asynchronous logging performance...");
        var asyncStart = stopwatch.ElapsedMilliseconds;
        
        var asyncTasks = new List<Task>();
        for (int i = 0; i < 100; i++)
        {
            int messageId = i;
            asyncTasks.Add(performanceLogger.InfoAsync($"Async log message {messageId:D3} - Performance test data with timestamp {DateTime.Now:HH:mm:ss.fff}"));
        }
        
        await Task.WhenAll(asyncTasks);
        
        var asyncEnd = stopwatch.ElapsedMilliseconds;
        var asyncDuration = asyncEnd - asyncStart;
        
        stopwatch.Stop();
        
        // Log performance results
        performanceLogger.Info($"Performance Test Results:");
        performanceLogger.Info($"Synchronous logging: 100 messages in {syncDuration}ms ({100.0 / syncDuration * 1000:F1} msg/sec)");
        performanceLogger.Info($"Asynchronous logging: 100 messages in {asyncDuration}ms ({100.0 / asyncDuration * 1000:F1} msg/sec)");
        performanceLogger.Info($"Performance improvement: {(double)syncDuration / asyncDuration:F2}x faster with async");
        
        Console.WriteLine($"✓ Performance test completed");
        Console.WriteLine($"✓ Sync: {syncDuration}ms, Async: {asyncDuration}ms");
        Console.WriteLine($"✓ Async is {(double)syncDuration / asyncDuration:F2}x faster");
    }

    /// <summary>
    /// Demonstrates advanced file management and rotation strategies
    /// </summary>
    static async Task AdvancedFileManagementExample()
    {
        Console.WriteLine("\n4. Advanced File Management Example");
        Console.WriteLine("----------------------------------");
        
        // Logger with small file size to demonstrate rotation
        var fileLogger = XLog.Create(config =>
        {
            config.LogDirectory = Path.Combine(Path.GetTempPath(), "XTinyLogAdvanced", "FileManagement");
            config.ApplicationName = "FileRotation";
            config.MaxFileSize = 50; // Very small for demonstration
            config.FileName = () => $"rotating_log_{DateTime.Now:HHmmss}";
        });

        // Generate enough content to trigger file rotation
        for (int i = 0; i < 20; i++)
        {
            var message = $"File rotation test message {i:D2} - This is a longer message to help reach the file size limit faster. " +
                         $"Current time: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}, Message length: {200 + i} characters.";
            
            fileLogger.Info(message);
            await fileLogger.DebugAsync($"Debug message {i:D2} - Additional content for file size testing");
            
            // Small delay to see file creation timing
            await Task.Delay(10);
        }

        Console.WriteLine("✓ File management example completed");
        Console.WriteLine("✓ Check for multiple log files due to size rotation");
    }

    /// <summary>
    /// Demonstrates custom HTML styling and theme customization
    /// </summary>
    static async Task CustomHtmlStylingExample()
    {
        Console.WriteLine("\n5. Custom HTML Styling Example");
        Console.WriteLine("------------------------------");
        
        var styledLogger = XLog.Create(config =>
        {
            config.LogDirectory = Path.Combine(Path.GetTempPath(), "XTinyLogAdvanced", "CustomStyling");
            config.ApplicationName = "StyledLogs";
            config.MaxFileSize = 3000;
            config.FileName = () => $"styled_log_{DateTime.Now:yyyy-MM-dd}";
        });

        // Log messages that will showcase different styling features
        styledLogger.Info("🚀 Application Startup - Welcome to the styled logging demo!");
        styledLogger.Debug("🔍 Debug Information - This message shows debug-level styling");
        styledLogger.Warn("⚠️  Warning Message - This demonstrates warning-level styling with icons");
        styledLogger.Error("❌ Error Occurred - This shows error-level styling with emphasis");
        
        await styledLogger.InfoAsync("📊 Data Processing - Async message with emoji for visual appeal");
        await styledLogger.DebugAsync("🛠️  Configuration - Debug message showing system configuration details");
        await styledLogger.WarnAsync("🔔 Notification - Warning about potential issues or important information");
        await styledLogger.ErrorAsync("💥 Critical Error - Serious error that requires immediate attention");

        // Log structured data that will be formatted nicely in HTML
        styledLogger.Info("📈 Performance Metrics - CPU: 45%, Memory: 2.1GB, Disk: 78%, Network: 125 Mbps");
        styledLogger.Info("🌐 Network Status - Connections: 42 active, Bandwidth: 95% utilized, Latency: 12ms");
        styledLogger.Info("💾 Database Stats - Queries/sec: 1,247, Cache hit ratio: 94.2%, Connection pool: 8/20");
        
        Console.WriteLine("✓ Custom HTML styling example completed");
        Console.WriteLine("✓ Open the HTML log file to see enhanced styling and emojis");
    }

    /// <summary>
    /// Demonstrates structured logging patterns for better log analysis
    /// </summary>
    static async Task StructuredLoggingExample()
    {
        Console.WriteLine("\n6. Structured Logging Example");
        Console.WriteLine("-----------------------------");
        
        var structuredLogger = XLog.Create(config =>
        {
            config.LogDirectory = Path.Combine(Path.GetTempPath(), "XTinyLogAdvanced", "StructuredLogging");
            config.ApplicationName = "StructuredLogs";
            config.MaxFileSize = 4000;
            config.FileName = () => $"structured_{DateTime.Now:yyyy-MM-dd_HH}";
        });

        // Simulate user session tracking
        var sessionId = Guid.NewGuid().ToString("N")[..8];
        var userId = "user_12345";
        
        structuredLogger.Info($"[SESSION:{sessionId}] [USER:{userId}] User login successful");
        await structuredLogger.DebugAsync($"[SESSION:{sessionId}] [USER:{userId}] Loading user preferences");
        
        // Simulate API request tracking
        var requestId = Guid.NewGuid().ToString("N")[..8];
        var endpoint = "/api/users/profile";
        var method = "GET";
        
        structuredLogger.Info($"[REQUEST:{requestId}] [METHOD:{method}] [ENDPOINT:{endpoint}] Request started");
        await Task.Delay(50); // Simulate processing time
        structuredLogger.Info($"[REQUEST:{requestId}] [METHOD:{method}] [ENDPOINT:{endpoint}] Request completed - 200 OK (50ms)");
        
        // Simulate error tracking with context
        var errorId = Guid.NewGuid().ToString("N")[..8];
        structuredLogger.Error($"[ERROR:{errorId}] [SESSION:{sessionId}] [USER:{userId}] Database connection failed - Retry attempt 1/3");
        await structuredLogger.WarnAsync($"[ERROR:{errorId}] [SESSION:{sessionId}] [USER:{userId}] Falling back to cached data");
        
        // Simulate business event tracking
        var orderId = "ORD-" + Random.Shared.Next(10000, 99999);
        var amount = Random.Shared.Next(10, 1000);
        
        structuredLogger.Info($"[ORDER:{orderId}] [USER:{userId}] Order created - Amount: ${amount}, Items: 3");
        await structuredLogger.InfoAsync($"[ORDER:{orderId}] [USER:{userId}] Payment processing initiated");
        await Task.Delay(100); // Simulate payment processing
        structuredLogger.Info($"[ORDER:{orderId}] [USER:{userId}] Payment successful - Transaction ID: TXN-{Random.Shared.Next(100000, 999999)}");
        
        Console.WriteLine("✓ Structured logging example completed");
        Console.WriteLine("✓ Logs include session, user, request, and business context for easy filtering");
    }

    // Helper methods for multiple logger instances example
    
    static async Task SimulateDatabaseOperations(XLog dbLogger)
    {
        dbLogger.Info("Database connection established - Connection string: Server=localhost;Database=AppDB");
        await dbLogger.DebugAsync("Executing query: SELECT * FROM Users WHERE Active = 1");
        await Task.Delay(25);
        dbLogger.Info("Query executed successfully - 1,247 rows returned in 25ms");
        await dbLogger.WarnAsync("Connection pool utilization high - 18/20 connections in use");
    }

    static async Task SimulateApiRequests(XLog apiLogger)
    {
        var endpoints = new[] { "/api/users", "/api/orders", "/api/products", "/api/reports" };
        var methods = new[] { "GET", "POST", "PUT", "DELETE" };
        
        foreach (var endpoint in endpoints)
        {
            var method = methods[Random.Shared.Next(methods.Length)];
            var statusCode = Random.Shared.Next(1, 10) > 8 ? 500 : 200; // 20% error rate
            var duration = Random.Shared.Next(10, 200);
            
            apiLogger.Info($"{method} {endpoint} - {statusCode} ({duration}ms)");
            await Task.Delay(10);
        }
    }

    static async Task SimulateSecurityEvents(XLog securityLogger)
    {
        var events = new[]
        {
            "User authentication successful - IP: 192.168.1.100",
            "Failed login attempt - IP: 10.0.0.50, User: admin",
            "Password changed - User: john.doe",
            "Suspicious activity detected - Multiple failed attempts from IP: 203.0.113.1"
        };

        foreach (var eventMsg in events)
        {
            if (eventMsg.Contains("Failed") || eventMsg.Contains("Suspicious"))
            {
                securityLogger.Warn($"SECURITY: {eventMsg}");
            }
            else
            {
                securityLogger.Info($"SECURITY: {eventMsg}");
            }
            await Task.Delay(15);
        }
    }

    static async Task SimulateBusinessLogic(XLog businessLogger)
    {
        businessLogger.Info("Order processing workflow started");
        await businessLogger.DebugAsync("Validating order items and inventory");
        await Task.Delay(30);
        businessLogger.Info("Inventory check completed - All items available");
        await businessLogger.DebugAsync("Calculating taxes and shipping costs");
        businessLogger.Info("Order total calculated - Subtotal: $89.99, Tax: $7.20, Shipping: $5.99, Total: $103.18");
        await businessLogger.InfoAsync("Order processing completed successfully");
    }
}