using Serilog;
using XTinyLog.Serilog;

// Test 1: Basic configuration
Console.WriteLine("Testing XTinyLog Serilog Sink Integration...");

try
{
    // Test basic configuration
    Log.Logger = new LoggerConfiguration()
        .WriteTo.XTinyLog()
        .CreateLogger();

    Log.Information("Basic configuration test - this should appear in HTML logs");
    Log.Warning("Warning message with basic configuration");
    Log.Error("Error message with basic configuration");

    // Test custom configuration
    Log.Logger = new LoggerConfiguration()
        .WriteTo.XTinyLog(config =>
        {
            config.LogDirectory = Path.Combine(Directory.GetCurrentDirectory(), "TestLogs");
            config.ApplicationName = "PackageTest";
            config.MaxFileSize = 1024 * 1024; // 1MB
            config.IncludeProperties = true;
        })
        .CreateLogger();

    Log.Information("Custom configuration test - User {UserId} logged in from {IPAddress}", 
        "user123", "192.168.1.100");

    // Test structured logging
    var order = new { OrderId = 12345, CustomerId = "CUST001", Amount = 99.99m };
    Log.Information("Processing order {@Order} for customer {CustomerId}", order, order.CustomerId);

    // Test exception logging
    try
    {
        throw new InvalidOperationException("Test exception for logging");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Test exception occurred while processing order {OrderId}", order.OrderId);
    }

    // Test async configuration
    Log.Logger = new LoggerConfiguration()
        .WriteTo.XTinyLogAsync(config =>
        {
            config.LogDirectory = Path.Combine(Directory.GetCurrentDirectory(), "AsyncTestLogs");
            config.ApplicationName = "AsyncPackageTest";
        })
        .CreateLogger();

    Log.Information("Async configuration test - this should be logged asynchronously");

    // Test builder pattern
    Log.Logger = new LoggerConfiguration()
        .WriteTo.XTinyLogWithBuilder(builder => builder
            .WithLogDirectory(Path.Combine(Directory.GetCurrentDirectory(), "BuilderTestLogs"))
            .WithApplicationName("BuilderTest")
            .WithMaxFileSize(2 * 1024 * 1024) // 2MB
            .WithIncludeProperties(true))
        .CreateLogger();

    Log.Information("Builder pattern test - User {UserId} performed action {Action}", 
        "admin", "package_test");

    Console.WriteLine("✅ All tests completed successfully!");
    Console.WriteLine("Check the following directories for HTML log files:");
    Console.WriteLine($"- {Path.Combine(Directory.GetCurrentDirectory(), "TestLogs")}");
    Console.WriteLine($"- {Path.Combine(Directory.GetCurrentDirectory(), "AsyncTestLogs")}");
    Console.WriteLine($"- {Path.Combine(Directory.GetCurrentDirectory(), "BuilderTestLogs")}");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Test failed: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

return 0;
