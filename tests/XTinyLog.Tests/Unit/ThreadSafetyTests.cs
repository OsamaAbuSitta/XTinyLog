using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace XTinyLog.Tests.Unit
{
    /// <summary>
    /// Thread safety tests for concurrent logging operations
    /// </summary>
    public class ThreadSafetyTests : IDisposable
    {
        private readonly string _testLogDirectory;

        public ThreadSafetyTests()
        {
            _testLogDirectory = Path.Combine(Path.GetTempPath(), "XTinyLogTests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testLogDirectory);
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
        public void ConcurrentSynchronousLogging_ShouldNotCorruptLogFile()
        {
            // Arrange
            var log = XLog.Create(config =>
            {
                config.LogDirectory = _testLogDirectory;
                config.ApplicationName = "ConcurrentTest";
                config.MaxFileSize = 10000; // Large enough to avoid file rotation
            });

            const int threadCount = 10;
            const int messagesPerThread = 50;
            var tasks = new List<Task>();
            var exceptions = new List<Exception>();

            // Act
            for (int i = 0; i < threadCount; i++)
            {
                int threadId = i;
                var task = Task.Run(() =>
                {
                    try
                    {
                        for (int j = 0; j < messagesPerThread; j++)
                        {
                            log.Info($"Thread {threadId} - Message {j}");
                            log.Debug($"Thread {threadId} - Debug {j}");
                            log.Error($"Thread {threadId} - Error {j}");
                            log.Warn($"Thread {threadId} - Warning {j}");
                        }
                    }
                    catch (Exception ex)
                    {
                        lock (exceptions)
                        {
                            exceptions.Add(ex);
                        }
                    }
                });
                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray(), TimeSpan.FromSeconds(30));

            // Assert
            exceptions.Should().BeEmpty("No exceptions should occur during concurrent logging");

            var logFiles = Directory.GetFiles(Path.Combine(_testLogDirectory, "ConcurrentTest"), "*.html");
            logFiles.Should().HaveCount(1, "All messages should go to the same log file");

            var logContent = File.ReadAllText(logFiles[0]);
            
            // Count expected messages
            var expectedInfoCount = threadCount * messagesPerThread;
            var expectedDebugCount = threadCount * messagesPerThread;
            var expectedErrorCount = threadCount * messagesPerThread;
            var expectedWarnCount = threadCount * messagesPerThread;

            // Verify all messages are present
            CountOccurrences(logContent, "'type': 'info'").Should().Be(expectedInfoCount);
            CountOccurrences(logContent, "'type': 'debug'").Should().Be(expectedDebugCount);
            CountOccurrences(logContent, "'type': 'error'").Should().Be(expectedErrorCount);
            CountOccurrences(logContent, "'type': 'warning'").Should().Be(expectedWarnCount);
        }

        [Fact]
        public async Task ConcurrentAsynchronousLogging_ShouldNotCorruptLogFile()
        {
            // Arrange
            var log = XLog.Create(config =>
            {
                config.LogDirectory = _testLogDirectory;
                config.ApplicationName = "AsyncConcurrentTest";
                config.MaxFileSize = 10000; // Large enough to avoid file rotation
            });

            const int taskCount = 10;
            const int messagesPerTask = 50;
            var tasks = new List<Task>();

            // Act
            for (int i = 0; i < taskCount; i++)
            {
                int taskId = i;
                var task = Task.Run(async () =>
                {
                    for (int j = 0; j < messagesPerTask; j++)
                    {
                        await log.InfoAsync($"AsyncTask {taskId} - Message {j}");
                        await log.DebugAsync($"AsyncTask {taskId} - Debug {j}");
                        await log.ErrorAsync($"AsyncTask {taskId} - Error {j}");
                        await log.WarnAsync($"AsyncTask {taskId} - Warning {j}");
                    }
                });
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            // Assert
            var logFiles = Directory.GetFiles(Path.Combine(_testLogDirectory, "AsyncConcurrentTest"), "*.html");
            logFiles.Should().HaveCount(1, "All messages should go to the same log file");

            var logContent = File.ReadAllText(logFiles[0]);
            
            // Count expected messages
            var expectedInfoCount = taskCount * messagesPerTask;
            var expectedDebugCount = taskCount * messagesPerTask;
            var expectedErrorCount = taskCount * messagesPerTask;
            var expectedWarnCount = taskCount * messagesPerTask;

            // Verify all messages are present
            CountOccurrences(logContent, "'type': 'info'").Should().Be(expectedInfoCount);
            CountOccurrences(logContent, "'type': 'debug'").Should().Be(expectedDebugCount);
            CountOccurrences(logContent, "'type': 'error'").Should().Be(expectedErrorCount);
            CountOccurrences(logContent, "'type': 'warning'").Should().Be(expectedWarnCount);
        }

        [Fact]
        public void MixedSyncAndAsyncLogging_ShouldNotCorruptLogFile()
        {
            // Arrange
            var log = XLog.Create(config =>
            {
                config.LogDirectory = _testLogDirectory;
                config.ApplicationName = "MixedTest";
                config.MaxFileSize = 10000; // Large enough to avoid file rotation
            });

            const int syncThreadCount = 5;
            const int asyncTaskCount = 5;
            const int messagesPerWorker = 30;
            var allTasks = new List<Task>();

            // Act - Create synchronous logging tasks
            for (int i = 0; i < syncThreadCount; i++)
            {
                int threadId = i;
                var task = Task.Run(() =>
                {
                    for (int j = 0; j < messagesPerWorker; j++)
                    {
                        log.Info($"SyncThread {threadId} - Message {j}");
                        log.Error($"SyncThread {threadId} - Error {j}");
                    }
                });
                allTasks.Add(task);
            }

            // Create asynchronous logging tasks
            for (int i = 0; i < asyncTaskCount; i++)
            {
                int taskId = i;
                var task = Task.Run(async () =>
                {
                    for (int j = 0; j < messagesPerWorker; j++)
                    {
                        await log.InfoAsync($"AsyncTask {taskId} - Message {j}");
                        await log.ErrorAsync($"AsyncTask {taskId} - Error {j}");
                    }
                });
                allTasks.Add(task);
            }

            Task.WaitAll(allTasks.ToArray(), TimeSpan.FromSeconds(30));

            // Assert
            var logFiles = Directory.GetFiles(Path.Combine(_testLogDirectory, "MixedTest"), "*.html");
            logFiles.Should().HaveCount(1, "All messages should go to the same log file");

            var logContent = File.ReadAllText(logFiles[0]);
            
            // Count expected messages
            var expectedInfoCount = (syncThreadCount + asyncTaskCount) * messagesPerWorker;
            var expectedErrorCount = (syncThreadCount + asyncTaskCount) * messagesPerWorker;

            // Verify all messages are present
            CountOccurrences(logContent, "'type': 'info'").Should().Be(expectedInfoCount);
            CountOccurrences(logContent, "'type': 'error'").Should().Be(expectedErrorCount);
        }

        [Fact]
        public void MultipleLogInstances_ConcurrentLogging_ShouldNotInterfere()
        {
            // Arrange
            var log1 = XLog.Create(config =>
            {
                config.LogDirectory = _testLogDirectory;
                config.ApplicationName = "Instance1";
                config.MaxFileSize = 10000;
            });

            var log2 = XLog.Create(config =>
            {
                config.LogDirectory = _testLogDirectory;
                config.ApplicationName = "Instance2";
                config.MaxFileSize = 10000;
            });

            const int messagesPerInstance = 100;
            var tasks = new List<Task>();

            // Act
            var task1 = Task.Run(() =>
            {
                for (int i = 0; i < messagesPerInstance; i++)
                {
                    log1.Info($"Instance1 - Message {i}");
                }
            });

            var task2 = Task.Run(() =>
            {
                for (int i = 0; i < messagesPerInstance; i++)
                {
                    log2.Info($"Instance2 - Message {i}");
                }
            });

            tasks.Add(task1);
            tasks.Add(task2);

            Task.WaitAll(tasks.ToArray(), TimeSpan.FromSeconds(30));

            // Assert
            var instance1Files = Directory.GetFiles(Path.Combine(_testLogDirectory, "Instance1"), "*.html");
            var instance2Files = Directory.GetFiles(Path.Combine(_testLogDirectory, "Instance2"), "*.html");

            instance1Files.Should().HaveCount(1);
            instance2Files.Should().HaveCount(1);

            var log1Content = File.ReadAllText(instance1Files[0]);
            var log2Content = File.ReadAllText(instance2Files[0]);

            CountOccurrences(log1Content, "Instance1 - Message").Should().Be(messagesPerInstance);
            CountOccurrences(log2Content, "Instance2 - Message").Should().Be(messagesPerInstance);

            // Ensure no cross-contamination
            log1Content.Should().NotContain("Instance2");
            log2Content.Should().NotContain("Instance1");
        }

        [Fact]
        public async Task ConcurrentLoggingWithCancellation_ShouldHandleGracefully()
        {
            // Arrange
            var log = XLog.Create(config =>
            {
                config.LogDirectory = _testLogDirectory;
                config.ApplicationName = "CancellationTest";
                config.MaxFileSize = 10000;
            });

            var cts = new CancellationTokenSource();
            var tasks = new List<Task>();
            var completedTasks = 0;
            var cancelledTasks = 0;

            // Act
            for (int i = 0; i < 20; i++)
            {
                int taskId = i;
                var task = Task.Run(async () =>
                {
                    try
                    {
                        for (int j = 0; j < 50; j++)
                        {
                            await log.InfoAsync($"Task {taskId} - Message {j}", cts.Token);
                            
                            // Cancel after some tasks have started
                            if (taskId == 5 && j == 10)
                            {
                                cts.Cancel();
                            }
                        }
                        Interlocked.Increment(ref completedTasks);
                    }
                    catch (OperationCanceledException)
                    {
                        Interlocked.Increment(ref cancelledTasks);
                    }
                });
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            // Assert
            (completedTasks + cancelledTasks).Should().Be(20, "All tasks should either complete or be cancelled");
            cancelledTasks.Should().BeGreaterThan(0, "Some tasks should be cancelled");
            
            // Verify that the log file is still valid and contains some messages
            var logFiles = Directory.GetFiles(Path.Combine(_testLogDirectory, "CancellationTest"), "*.html");
            if (logFiles.Length > 0)
            {
                var logContent = File.ReadAllText(logFiles[0]);
                logContent.Should().Contain("'type': 'info'", "Some messages should have been logged before cancellation");
            }
        }

        [Fact]
        public void HighVolumeLogging_ShouldMaintainPerformance()
        {
            // Arrange
            var log = XLog.Create(config =>
            {
                config.LogDirectory = _testLogDirectory;
                config.ApplicationName = "PerformanceTest";
                config.MaxFileSize = 50000; // Larger file to avoid frequent rotation
            });

            const int messageCount = 1000;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            Parallel.For(0, messageCount, i =>
            {
                log.Info($"Performance test message {i} with some additional content to make it realistic");
            });

            stopwatch.Stop();

            // Assert
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(10000, "High volume logging should complete within reasonable time");

            var logFiles = Directory.GetFiles(Path.Combine(_testLogDirectory, "PerformanceTest"), "*.html");
            logFiles.Should().NotBeEmpty("Log files should be created");

            // Count total messages across all files (in case of file rotation)
            var totalMessages = 0;
            foreach (var file in logFiles)
            {
                var content = File.ReadAllText(file);
                totalMessages += CountOccurrences(content, "'type': 'info'");
            }

            totalMessages.Should().Be(messageCount, "All messages should be logged");
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