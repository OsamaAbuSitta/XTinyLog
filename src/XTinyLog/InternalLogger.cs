using System;
using System.Diagnostics;
using System.IO;

namespace XTinyLog
{
    /// <summary>
    /// Provides internal error logging for XTinyLog without causing recursion.
    /// This logger writes to the Windows Event Log or console in debug mode.
    /// </summary>
    internal static class InternalLogger
    {
        private const string EventSourceName = "XTinyLog";
        private const string LogName = "Application";
        private static readonly object LockObject = new object();
        private static bool _eventSourceChecked = false;
        private static bool _eventSourceAvailable = false;

        /// <summary>
        /// Logs an internal error without causing recursion in the main logging system.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="context">Additional context information about where the error occurred.</param>
        public static void LogInternalError(Exception exception, string context)
        {
            if (exception == null) return;

            var message = $"XTinyLog Internal Error in {context}: {exception.Message}";
            var detailedMessage = $"{message}{Environment.NewLine}Stack Trace: {exception.StackTrace}";

            try
            {
                // Try to write to Event Log first (Windows only)
                if (TryWriteToEventLog(message, detailedMessage))
                {
                    return;
                }

                // Fallback to console in debug mode
                WriteToConsole(detailedMessage);

                // Last resort: try to write to a simple text file
                TryWriteToFallbackFile(detailedMessage);
            }
            catch
            {
                // If all logging methods fail, we can't do much more without causing infinite recursion
                // This is a last resort to prevent the application from crashing
            }
        }

        /// <summary>
        /// Logs an internal warning message.
        /// </summary>
        /// <param name="message">The warning message to log.</param>
        /// <param name="context">Additional context information.</param>
        public static void LogInternalWarning(string message, string context)
        {
            var fullMessage = $"XTinyLog Warning in {context}: {message}";

            try
            {
                if (TryWriteToEventLog(fullMessage, fullMessage, EventLogEntryType.Warning))
                {
                    return;
                }

                WriteToConsole($"WARNING: {fullMessage}");
                TryWriteToFallbackFile($"WARNING: {fullMessage}");
            }
            catch
            {
                // Suppress any errors in internal logging
            }
        }

        private static bool TryWriteToEventLog(string message, string detailedMessage, EventLogEntryType entryType = EventLogEntryType.Error)
        {
            try
            {
                if (!IsEventSourceAvailable())
                {
                    return false;
                }

                using (var eventLog = new EventLog(LogName))
                {
                    eventLog.Source = EventSourceName;
                    eventLog.WriteEntry(detailedMessage, entryType);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private static bool IsEventSourceAvailable()
        {
            if (_eventSourceChecked)
            {
                return _eventSourceAvailable;
            }

            lock (LockObject)
            {
                if (_eventSourceChecked)
                {
                    return _eventSourceAvailable;
                }

                try
                {
                    // Check if we're on Windows and have permission to access Event Log
                    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    {
                        if (!EventLog.SourceExists(EventSourceName))
                        {
                            // Try to create the event source (requires admin privileges)
                            EventLog.CreateEventSource(EventSourceName, LogName);
                        }
                        _eventSourceAvailable = true;
                    }
                    else
                    {
                        _eventSourceAvailable = false;
                    }
                }
                catch
                {
                    _eventSourceAvailable = false;
                }

                _eventSourceChecked = true;
                return _eventSourceAvailable;
            }
        }

        private static void WriteToConsole(string message)
        {
#if DEBUG
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
            Debug.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
#endif
        }

        private static void TryWriteToFallbackFile(string message)
        {
            try
            {
                var tempPath = Path.GetTempPath();
                var logFile = Path.Combine(tempPath, "XTinyLog_Internal_Errors.log");
                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";

                // Use a simple file append operation
                File.AppendAllText(logFile, logEntry);
            }
            catch
            {
                // If we can't even write to temp, there's nothing more we can do
            }
        }
    }
}