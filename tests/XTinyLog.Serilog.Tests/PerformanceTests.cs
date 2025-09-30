using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Serilog;
using Serilog.Events;
using Serilog.Parsing;

namespace XTinyLog.Serilog.Tests;

/// <summary>
/// Performance tests for XTinyLog Serilog sink components.
/// Tests logging throughput, memory usage, and performance comparisons.
/// </summary>
public class PerformanceTests : IDisposable
{
    private readonly string _testLogDirectory;
    private readonly XTinyLogSinkConfiguration _defaultConfiguration;

    public PerformanceTests()
    {
        _testLogDirectory = Path.Combine(Path.GetTempPath(), "XTinyLogPerformanceTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testLogDirectory);

        _defaultConfiguration = new XTinyLogSinkConfiguration
        {
            LogDirectory = _testLogDirectory,
            ApplicationName = "PerformanceTestApp",
            MaxFileSize = 10240, // 10KB for faster file rotation in tests
            IncludeProperties = true,
            PropertyFormat = PropertyFormatMode.Inline
        };
    }

    public void Dispose()
    {
        if (Directory.Exists(_testLogDirectory))
        {
            try
            {
                Directory.Delete(_testLogDirectory, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    [Fact]
    public void SyncSink_LoggingThroughput_ShouldHandleHighVolumeLogging()
    {
        // Arrange
        var sink = new XTinyLogSink(_defaultConfiguration);
        var messageCount = 1000;
        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < messageCount; i++)
        {
            var logEvent = CreateLogEvent(LogEventLevel.Information, $"Performance test message {i}");
            sink.Emit(logEvent);
        }

        stopwatch.Stop();

        // Assert
        var messagesPerSecond = messageCount / stopwatch.Elapsed.TotalSeconds;
        messagesPerSecond.Should().BeGreaterThan(100, "because sync sink should handle at least 100 messages per second");
        
        // Verify that messages were actually logged
        Thread.Sleep(1000); // Give time for file operations
        var logFiles = Directory.GetFiles(_testLogDirectory, "*.html", SearchOption.AllDirectories);
        logFiles.Should().NotBeEmpty("because log files should be created during performance test");
    }

    [Fact]
    public async Task AsyncSink_LoggingThroughput_ShouldHandleHighVolumeLogging()
    {
        // Arrange
        var sink = new XTinyLogAsyncSink(_defaultConfiguration);
        var messageCount = 1000;
        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < messageCount; i++)
        {
            var logEvent = CreateLogEvent(LogEventLevel.Information, $"Async performance test message {i}");
            sink.Emit(logEvent);
        }

        stopwatch.Stop();

        // Assert
        var messagesPerSecond = messageCount / stopwatch.Elapsed.TotalSeconds;
        messagesPerSecond.Should().BeGreaterThan(500, "because async sink should handle at least 500 messages per second");
        
        // Wait for async operations to complete
        await Task.Delay(2000);
        
        // Verify that messages were actually logged
        var logFiles = Directory.GetFiles(_testLogDirectory, "*.html", SearchOption.AllDirectories);
        logFiles.Should().NotBeEmpty("because log files should be created during async performance test");
    }

    [Fact]
    public async Task SyncVsAsync_PerformanceComparison_AsyncShouldBeFaster()
    {
        // Arrange
        var syncConfig = _defaultConfiguration.Clone();
        syncConfig.ApplicationName = "SyncPerfTest";
        var asyncConfig = _defaultConfiguration.Clone();
        asyncConfig.ApplicationName = "AsyncPerfTest";

        var syncSink = new XTinyLogSink(syncConfig);
        var asyncSink = new XTinyLogAsyncSink(asyncConfig);
        var messageCount = 500;

        // Act - Test sync performance
        var syncStopwatch = Stopwatch.StartNew();
        for (int i = 0; i < messageCount; i++)
        {
            var logEvent = CreateLogEvent(LogEventLevel.Information, $"Sync perf message {i}");
            syncSink.Emit(logEvent);
        }
        syncStopwatch.Stop();

        // Act - Test async performance
        var asyncStopwatch = Stopwatch.StartNew();
        for (int i = 0; i < messageCount; i++)
        {
            var logEvent = CreateLogEvent(LogEventLevel.Information, $"Async perf message {i}");
            asyncSink.Emit(logEvent);
        }
        asyncStopwatch.Stop();

        // Wait for async operations to complete
        await Task.Delay(1000);

        // Assert
        var syncMessagesPerSecond = messageCount / syncStopwatch.Elapsed.TotalSeconds;
        var asyncMessagesPerSecond = messageCount / asyncStopwatch.Elapsed.TotalSeconds;

        // Async should be significantly faster for the emit operation (not including actual I/O completion)
        asyncMessagesPerSecond.Should().BeGreaterThan(syncMessagesPerSecond * 2, 
            "because async sink should be at least 2x faster for emit operations");
    }

    [Fact]
    public void SyncSink_MemoryUsage_ShouldNotLeakMemory()
    {
        // Arrange
        var sink = new XTinyLogSink(_defaultConfiguration);
        var initialMemory = GC.GetTotalMemory(true);

        // Act - Log many messages
        for (int i = 0; i < 1000; i++)
        {
            var logEvent = CreateLogEvent(LogEventLevel.Information, $"Memory test message {i}");
            sink.Emit(logEvent);
        }

        // Force garbage collection
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var finalMemory = GC.GetTotalMemory(false);

        // Assert
        var memoryIncrease = finalMemory - initialMemory;
        memoryIncrease.Should().BeLessThan(1024 * 1024, // 1MB
            "because sync sink should not consume excessive memory");
    }

    [Fact]
    public async Task AsyncSink_MemoryUsage_ShouldNotLeakMemory()
    {
        // Arrange
        var sink = new XTinyLogAsyncSink(_defaultConfiguration);
        var initialMemory = GC.GetTotalMemory(true);

        // Act - Log many messages
        for (int i = 0; i < 1000; i++)
        {
            var logEvent = CreateLogEvent(LogEventLevel.Information, $"Async memory test message {i}");
            sink.Emit(logEvent);
        }

        // Wait for async operations to complete
        await Task.Delay(2000);

        // Force garbage collection
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var finalMemory = GC.GetTotalMemory(false);

        // Assert
        var memoryIncrease = finalMemory - initialMemory;
        memoryIncrease.Should().BeLessThan(2 * 1024 * 1024, // 2MB (async may use more due to task overhead)
            "because async sink should not consume excessive memory");
    }

    [Fact]
    public void LogEventFormatter_FormattingPerformance_ShouldBeEfficient()
    {
        // Arrange
        var logEvent = CreateLogEventWithProperties(LogEventLevel.Information, 
            "User {UserId} performed {Action} with {Data}",
            ("UserId", 12345),
            ("Action", "ComplexOperation"),
            ("Data", new { Id = 1, Name = "TestData", Timestamp = DateTimeOffset.Now }));

        var iterations = 10000;
        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < iterations; i++)
        {
            var result = LogEventFormatter.FormatLogEvent(logEvent, true, PropertyFormatMode.Inline);
        }

        stopwatch.Stop();

        // Assert
        var formatsPerSecond = iterations / stopwatch.Elapsed.TotalSeconds;
        formatsPerSecond.Should().BeGreaterThan(1000, 
            "because log event formatting should handle at least 1000 formats per second");
    }

    [Fact]
    public void LogLevelMapper_MappingPerformance_ShouldBeEfficient()
    {
        // Arrange
        var logLevels = new[] 
        {
            LogEventLevel.Verbose,
            LogEventLevel.Debug,
            LogEventLevel.Information,
            LogEventLevel.Warning,
            LogEventLevel.Error,
            LogEventLevel.Fatal
        };

        var iterations = 100000;
        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < iterations; i++)
        {
            var level = logLevels[i % logLevels.Length];
            var result = LogLevelMapper.MapLogLevel(level);
        }

        stopwatch.Stop();

        // Assert
        var mappingsPerSecond = iterations / stopwatch.Elapsed.TotalSeconds;
        mappingsPerSecond.Should().BeGreaterThan(100000, 
            "because log level mapping should handle at least 100k mappings per second");
    }

    [Fact]
    public async Task ConcurrentLogging_ShouldHandleMultipleThreads()
    {
        // Arrange
        var sink = new XTinyLogAsyncSink(_defaultConfiguration);
        var threadCount = 10;
        var messagesPerThread = 100;
        var tasks = new List<Task>();

        // Act
        for (int t = 0; t < threadCount; t++)
        {
            var threadId = t;
            tasks.Add(Task.Run(() =>
            {
                for (int i = 0; i < messagesPerThread; i++)
                {
                    var logEvent = CreateLogEvent(LogEventLevel.Information, 
                        $"Concurrent message from thread {threadId}, message {i}");
                    sink.Emit(logEvent);
                }
            }));
        }

        var stopwatch = Stopwatch.StartNew();
        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Wait for async operations to complete
        await Task.Delay(3000);

        // Assert
        var totalMessages = threadCount * messagesPerThread;
        var messagesPerSecond = totalMessages / stopwatch.Elapsed.TotalSeconds;
        messagesPerSecond.Should().BeGreaterThan(100, 
            "because concurrent logging should maintain reasonable throughput");

        // Verify no exceptions occurred
        tasks.Should().AllSatisfy(t => t.IsCompletedSuccessfully.Should().BeTrue(
            "because all concurrent logging tasks should complete successfully"));
    }

    [Fact]
    public void LargeMessage_LoggingPerformance_ShouldHandleEfficientlyWithLargeMessages()
    {
        // Arrange
        var sink = new XTinyLogSink(_defaultConfiguration);
        var largeMessage = new string('A', 10000); // 10KB message
        var messageCount = 100;
        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < messageCount; i++)
        {
            var logEvent = CreateLogEvent(LogEventLevel.Information, $"{largeMessage} - Message {i}");
            sink.Emit(logEvent);
        }

        stopwatch.Stop();

        // Assert
        var messagesPerSecond = messageCount / stopwatch.Elapsed.TotalSeconds;
        messagesPerSecond.Should().BeGreaterThan(10, 
            "because sink should handle at least 10 large messages per second");
    }

    [Fact]
    public void ComplexStructuredLogging_PerformanceTest_ShouldHandleComplexObjects()
    {
        // Arrange
        var sink = new XTinyLogSink(_defaultConfiguration);
        var complexObject = new
        {
            Id = 12345,
            Name = "ComplexTestObject",
            Properties = new Dictionary<string, object>
            {
                ["Property1"] = "Value1",
                ["Property2"] = 42,
                ["Property3"] = DateTimeOffset.Now,
                ["Property4"] = new { NestedId = 1, NestedName = "Nested" }
            },
            Tags = new[] { "tag1", "tag2", "tag3" },
            Metadata = new { Version = "1.0", Environment = "Test" }
        };

        var messageCount = 500;
        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < messageCount; i++)
        {
            var logEvent = CreateLogEventWithProperties(LogEventLevel.Information,
                "Complex operation {OperationId} completed with {Result}",
                ("OperationId", i),
                ("Result", complexObject));
            sink.Emit(logEvent);
        }

        stopwatch.Stop();

        // Assert
        var messagesPerSecond = messageCount / stopwatch.Elapsed.TotalSeconds;
        messagesPerSecond.Should().BeGreaterThan(50, 
            "because sink should handle at least 50 complex structured messages per second");
    }

    // Helper methods
    private static LogEvent CreateLogEvent(LogEventLevel level, string message)
    {
        var tokens = new MessageTemplateToken[] { new TextToken(message) };
        return new LogEvent(
            DateTimeOffset.Now,
            level,
            null,
            new MessageTemplate(message, tokens),
            Array.Empty<LogEventProperty>());
    }

    private static LogEvent CreateLogEventWithProperties(LogEventLevel level, string messageTemplate, params (string key, object value)[] properties)
    {
        var logProperties = properties.Select(p => 
            new LogEventProperty(p.key, new ScalarValue(p.value))).ToArray();

        var tokens = new List<MessageTemplateToken>();
        var parts = messageTemplate.Split(new[] { '{', '}' }, StringSplitOptions.RemoveEmptyEntries);
        
        for (int i = 0; i < parts.Length; i++)
        {
            if (i % 2 == 0)
            {
                if (!string.IsNullOrEmpty(parts[i]))
                {
                    tokens.Add(new TextToken(parts[i]));
                }
            }
            else
            {
                tokens.Add(new PropertyToken(parts[i], parts[i]));
            }
        }

        return new LogEvent(
            DateTimeOffset.Now,
            level,
            null,
            new MessageTemplate(messageTemplate, tokens),
            logProperties);
    }
}