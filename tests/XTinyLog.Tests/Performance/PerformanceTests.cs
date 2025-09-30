using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace XTinyLog.Tests.Performance
{
    /// <summary>
    /// Performance and load tests for XTinyLog
    /// </summary>
    public class PerformanceTests : IDisposable
    {
        private readonly string testLogDirectory;
        private readonly ITestOutputHelper output;

        public PerformanceTests(ITestOutputHelper output)
        {
            this.output = output;
            this.testLogDirectory = Path.Combine(Path.GetTempPath(), "XTinyLogPerformanceTests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(this.testLogDirectory);
        }

        public void Dispose()
        {
            if (Directory.Exists(this.testLogDirectory))
            {
                try
                {
                    Directory.Delete(this.testLogDirectory, true);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }

        [Fact]
        public void SynchronousLogging_PerformanceBenchmark()
        {
            // Arrange
            var log = XLog.Create(config =>
            {
                config.LogDirectory = this.testLogDirectory;
                config.ApplicationName = "SyncPerfTest";
                config.FileName = () => "sync_perf";
                config.MaxFileSize = 10000; // 10MB to avoid rotation during test
            });

            const int messageCount = 1000;
            var message = "Performance test message with moderate length to simulate real-world usage";

            // Measure memory before
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var memoryBefore = GC.GetTotalMemory(false);

            // Act
            var stopwatch = Stopwatch.StartNew();
            
            for (int i = 0; i < messageCount; i++)
            {
                log.Info($"{message} - {i}");
            }
            
            stopwatch.Stop();

            // Measure memory after
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var memoryAfter = GC.GetTotalMemory(false);

            // Assert
            var messagesPerSecond = messageCount / stopwatch.Elapsed.TotalSeconds;
            var memoryUsed = memoryAfter - memoryBefore;

            this.output.WriteLine($"Synchronous Logging Performance:");
            this.output.WriteLine($"  Messages: {messageCount}");
            this.output.WriteLine($"  Time: {stopwatch.ElapsedMilliseconds} ms");
            this.output.WriteLine($"  Messages/sec: {messagesPerSecond:F2}");
            this.output.WriteLine($"  Memory used: {memoryUsed / 1024.0:F2} KB");
            this.output.WriteLine($"  Memory per message: {memoryUsed / (double)messageCount:F2} bytes");

            // Performance assertions (these are reasonable expectations, not strict requirements)
            messagesPerSecond.Should().BeGreaterThan(100, "Should log at least 100 messages per second");
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(30000, "Should complete within 30 seconds");
            
            // Memory usage should be reasonable (less than 1MB for 1000 messages)
            memoryUsed.Should().BeLessThan(1024 * 1024, "Memory usage should be reasonable");
        }

        [Fact]
        public async Task AsynchronousLogging_PerformanceBenchmark()
        {
            // Arrange
            var log = XLog.Create(config =>
            {
                config.LogDirectory = this.testLogDirectory;
                config.ApplicationName = "AsyncPerfTest";
                config.FileName = () => "async_perf";
                config.MaxFileSize = 10000; // 10MB to avoid rotation during test
            });

            const int messageCount = 1000;
            var message = "Async performance test message with moderate length to simulate real-world usage";

            // Measure memory before
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var memoryBefore = GC.GetTotalMemory(false);

            // Act
            var stopwatch = Stopwatch.StartNew();
            
            var tasks = new List<Task>();
            for (int i = 0; i < messageCount; i++)
            {
                int messageId = i;
                tasks.Add(log.InfoAsync($"{message} - {messageId}"));
            }
            
            await Task.WhenAll(tasks);
            stopwatch.Stop();

            // Measure memory after
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var memoryAfter = GC.GetTotalMemory(false);

            // Assert
            var messagesPerSecond = messageCount / stopwatch.Elapsed.TotalSeconds;
            var memoryUsed = memoryAfter - memoryBefore;

            this.output.WriteLine($"Asynchronous Logging Performance:");
            this.output.WriteLine($"  Messages: {messageCount}");
            this.output.WriteLine($"  Time: {stopwatch.ElapsedMilliseconds} ms");
            this.output.WriteLine($"  Messages/sec: {messagesPerSecond:F2}");
            this.output.WriteLine($"  Memory used: {memoryUsed / 1024.0:F2} KB");
            this.output.WriteLine($"  Memory per message: {memoryUsed / (double)messageCount:F2} bytes");

            // Async should generally be faster or at least comparable
            messagesPerSecond.Should().BeGreaterThan(100, "Should log at least 100 messages per second");
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(30000, "Should complete within 30 seconds");
            
            // Memory usage should be reasonable
            memoryUsed.Should().BeLessThan(2 * 1024 * 1024, "Memory usage should be reasonable (allowing for async overhead)");
        }

        [Fact]
        public void HighVolumeLogging_LoadTest()
        {
            // Arrange
            var log = XLog.Create(config =>
            {
                config.LogDirectory = this.testLogDirectory;
                config.ApplicationName = "LoadTest";
                config.FileName = () => "load_test";
                config.MaxFileSize = 5000; // Smaller files to test rotation under load
            });

            const int messageCount = 5000;
            var message = "Load test message";

            // Act
            var stopwatch = Stopwatch.StartNew();
            
            Parallel.For(0, messageCount, i =>
            {
                log.Info($"{message} - {i}");
            });
            
            stopwatch.Stop();

            // Assert
            var messagesPerSecond = messageCount / stopwatch.Elapsed.TotalSeconds;

            this.output.WriteLine($"High Volume Load Test:");
            this.output.WriteLine($"  Messages: {messageCount}");
            this.output.WriteLine($"  Time: {stopwatch.ElapsedMilliseconds} ms");
            this.output.WriteLine($"  Messages/sec: {messagesPerSecond:F2}");

            // Verify all messages were logged
            var appDirectory = Path.Combine(this.testLogDirectory, "LoadTest");
            var allLogFiles = Directory.GetFiles(appDirectory, "*.html", SearchOption.AllDirectories);
            
            allLogFiles.Should().NotBeEmpty("Should create log files");

            // Count total logged messages
            var totalLoggedMessages = 0;
            foreach (var file in allLogFiles)
            {
                var content = File.ReadAllText(file);
                totalLoggedMessages += CountOccurrences(content, "'type': 'info'");
            }

            totalLoggedMessages.Should().Be(messageCount, "All messages should be logged");
            messagesPerSecond.Should().BeGreaterThan(50, "Should maintain reasonable throughput under load");
        }

        [Fact]
        public void ConcurrentMultipleInstances_PerformanceTest()
        {
            // Arrange
            const int instanceCount = 5;
            const int messagesPerInstance = 200;
            
            var logs = new List<XLog>();
            for (int i = 0; i < instanceCount; i++)
            {
                logs.Add(XLog.Create(config =>
                {
                    config.LogDirectory = this.testLogDirectory;
                    config.ApplicationName = $"ConcurrentInstance{i}";
                    config.FileName = () => $"concurrent_{i}";
                    config.MaxFileSize = 5000;
                }));
            }

            // Act
            var stopwatch = Stopwatch.StartNew();
            
            var tasks = new List<Task>();
            for (int i = 0; i < instanceCount; i++)
            {
                int instanceId = i;
                var log = logs[i];
                
                var task = Task.Run(() =>
                {
                    for (int j = 0; j < messagesPerInstance; j++)
                    {
                        log.Info($"Instance {instanceId} - Message {j}");
                    }
                });
                tasks.Add(task);
            }
            
            Task.WaitAll(tasks.ToArray());
            stopwatch.Stop();

            // Assert
            var totalMessages = instanceCount * messagesPerInstance;
            var messagesPerSecond = totalMessages / stopwatch.Elapsed.TotalSeconds;

            this.output.WriteLine($"Concurrent Multiple Instances Test:");
            this.output.WriteLine($"  Instances: {instanceCount}");
            this.output.WriteLine($"  Messages per instance: {messagesPerInstance}");
            this.output.WriteLine($"  Total messages: {totalMessages}");
            this.output.WriteLine($"  Time: {stopwatch.ElapsedMilliseconds} ms");
            this.output.WriteLine($"  Messages/sec: {messagesPerSecond:F2}");

            // Verify each instance created its own files
            for (int i = 0; i < instanceCount; i++)
            {
                var instanceDir = Path.Combine(this.testLogDirectory, $"ConcurrentInstance{i}");
                Directory.Exists(instanceDir).Should().BeTrue($"Instance {i} should have its own directory");
                
                var instanceFiles = Directory.GetFiles(instanceDir, "*.html", SearchOption.AllDirectories);
                instanceFiles.Should().NotBeEmpty($"Instance {i} should have log files");
            }

            messagesPerSecond.Should().BeGreaterThan(50, "Should maintain reasonable throughput with multiple instances");
        }

        [Fact]
        public void MemoryUsageUnderLoad_ShouldNotLeak()
        {
            // Arrange
            var log = XLog.Create(config =>
            {
                config.LogDirectory = this.testLogDirectory;
                config.ApplicationName = "MemoryTest";
                config.FileName = () => "memory_test";
                config.MaxFileSize = 1000; // Small files to force frequent rotation
            });

            const int iterations = 10;
            const int messagesPerIteration = 100;
            var memoryMeasurements = new List<long>();

            // Act & Assert
            for (int iteration = 0; iteration < iterations; iteration++)
            {
                // Log messages
                for (int i = 0; i < messagesPerIteration; i++)
                {
                    log.Info($"Memory test iteration {iteration} - message {i}");
                }

                // Force garbage collection and measure memory
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                
                var currentMemory = GC.GetTotalMemory(false);
                memoryMeasurements.Add(currentMemory);
                
                this.output.WriteLine($"Iteration {iteration}: Memory = {currentMemory / 1024.0:F2} KB");
            }

            // Assert - Memory should not grow indefinitely
            var firstMeasurement = memoryMeasurements[0];
            var lastMeasurement = memoryMeasurements[iterations - 1];
            var memoryGrowth = lastMeasurement - firstMeasurement;

            this.output.WriteLine($"Memory growth: {memoryGrowth / 1024.0:F2} KB");

            // Allow some memory growth but not excessive (less than 5MB)
            memoryGrowth.Should().BeLessThan(5 * 1024 * 1024, "Memory usage should not grow excessively");
        }

        [Fact]
        public async Task AsyncVsSyncPerformanceComparison()
        {
            // Arrange
            const int messageCount = 500;
            var message = "Performance comparison test message";

            var syncLog = XLog.Create(config =>
            {
                config.LogDirectory = this.testLogDirectory;
                config.ApplicationName = "SyncComparison";
                config.FileName = () => "sync_comparison";
                config.MaxFileSize = 10000;
            });

            var asyncLog = XLog.Create(config =>
            {
                config.LogDirectory = this.testLogDirectory;
                config.ApplicationName = "AsyncComparison";
                config.FileName = () => "async_comparison";
                config.MaxFileSize = 10000;
            });

            // Act - Synchronous test
            var syncStopwatch = Stopwatch.StartNew();
            for (int i = 0; i < messageCount; i++)
            {
                syncLog.Info($"{message} - sync {i}");
            }
            syncStopwatch.Stop();

            // Act - Asynchronous test
            var asyncStopwatch = Stopwatch.StartNew();
            var asyncTasks = new List<Task>();
            for (int i = 0; i < messageCount; i++)
            {
                int messageId = i;
                asyncTasks.Add(asyncLog.InfoAsync($"{message} - async {messageId}"));
            }
            await Task.WhenAll(asyncTasks);
            asyncStopwatch.Stop();

            // Assert
            var syncMessagesPerSecond = messageCount / syncStopwatch.Elapsed.TotalSeconds;
            var asyncMessagesPerSecond = messageCount / asyncStopwatch.Elapsed.TotalSeconds;

            this.output.WriteLine($"Performance Comparison:");
            this.output.WriteLine($"  Sync - Time: {syncStopwatch.ElapsedMilliseconds} ms, Rate: {syncMessagesPerSecond:F2} msg/sec");
            this.output.WriteLine($"  Async - Time: {asyncStopwatch.ElapsedMilliseconds} ms, Rate: {asyncMessagesPerSecond:F2} msg/sec");
            this.output.WriteLine($"  Async improvement: {(asyncMessagesPerSecond / syncMessagesPerSecond - 1) * 100:F1}%");

            // Both should perform reasonably well
            syncMessagesPerSecond.Should().BeGreaterThan(50, "Sync logging should be reasonably fast");
            asyncMessagesPerSecond.Should().BeGreaterThan(50, "Async logging should be reasonably fast");
        }

        [Fact]
        public void FileRotationPerformance_UnderLoad()
        {
            // Arrange
            var log = XLog.Create(config =>
            {
                config.LogDirectory = this.testLogDirectory;
                config.ApplicationName = "RotationPerfTest";
                config.FileName = () => "rotation_perf";
                config.MaxFileSize = 10; // Very small to force frequent rotation
            });

            const int messageCount = 200;
            var message = "File rotation performance test message with enough content to trigger rotations frequently";

            // Act
            var stopwatch = Stopwatch.StartNew();
            
            for (int i = 0; i < messageCount; i++)
            {
                log.Info($"{message} - {i}");
            }
            
            stopwatch.Stop();

            // Assert
            var messagesPerSecond = messageCount / stopwatch.Elapsed.TotalSeconds;

            this.output.WriteLine($"File Rotation Performance Test:");
            this.output.WriteLine($"  Messages: {messageCount}");
            this.output.WriteLine($"  Time: {stopwatch.ElapsedMilliseconds} ms");
            this.output.WriteLine($"  Messages/sec: {messagesPerSecond:F2}");

            // Verify multiple files were created due to rotation
            var appDirectory = Path.Combine(this.testLogDirectory, "RotationPerfTest");
            var allLogFiles = Directory.GetFiles(appDirectory, "*.html", SearchOption.AllDirectories);
            
            allLogFiles.Should().HaveCountGreaterThan(1, "Multiple files should be created due to frequent rotation");

            // Performance should still be reasonable even with frequent rotation
            messagesPerSecond.Should().BeGreaterThan(20, "Should maintain reasonable performance even with frequent file rotation");
            
            // Verify all messages are logged
            var totalContent = string.Empty;
            foreach (var file in allLogFiles)
            {
                totalContent += File.ReadAllText(file);
            }
            
            var loggedMessageCount = CountOccurrences(totalContent, "'type': 'info'");
            loggedMessageCount.Should().Be(messageCount, "All messages should be logged despite rotation");
        }

        private static int CountOccurrences(string text, string pattern)
        {
            int count = 0;
            int index = 0;
            while ((index = text.IndexOf(pattern, index, StringComparison.Ordinal)) != -1)
            {
                count++;
                index += pattern.Length;
            }
            return count;
        }
    }
}