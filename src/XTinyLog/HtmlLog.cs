using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XTinyLog
{
    internal class HtmlLog
    {
        internal readonly ILogConfiguration Config;
        private static Dictionary<string, object> _fileLocksBag;
        private static Dictionary<string, SemaphoreSlim> _fileSemaphoresBag;
        private static object _lockFileLocksBag;
        private static readonly Encoding UTF8NoBOM = new UTF8Encoding(false, true);

        static HtmlLog()
        {
            _lockFileLocksBag = new object();
            _fileLocksBag = new Dictionary<string, object>();
            _fileSemaphoresBag = new Dictionary<string, SemaphoreSlim>();
        }

        public HtmlLog() : this(new DefaultLogConfiguration())
        {
        }

        internal HtmlLog(ILogConfiguration _config)
        {
            // Validate configuration before using it
            var validationResult = ConfigurationValidator.Validate(_config);
            if (!validationResult.IsValid)
            {
                throw new InvalidOperationException($"Configuration validation failed:{Environment.NewLine}{validationResult.GetErrorsAsString()}");
            }

            this.Config = _config;
        }

        internal void SafetyLog(string message, LogTypeEnum logTypeEnum)
        {
            // Synchronous version for backward compatibility
            try
            {
                // Validate input parameters
                if (string.IsNullOrEmpty(message))
                {
                    InternalLogger.LogInternalWarning("Attempted to log null or empty message", "SafetyLog");
                    return;
                }

                // Validate configuration before logging
                if (Config == null)
                {
                    throw new InvalidOperationException("Configuration is null. Cannot proceed with logging.");
                }

                Log(message, logTypeEnum);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Configuration"))
            {
                InternalLogger.LogInternalError(ex, "SafetyLog - Configuration error");

                // Don't try fallback logging for configuration errors as it would likely fail too
#if DEBUG_LOG
                Console.WriteLine($"Configuration error in SafetyLog: {ex}");
                throw;
#endif
            }
            catch (Exception ex)
            {
                InternalLogger.LogInternalError(ex, "SafetyLog");

                // Try fallback logging mechanism
                TryFallbackLogging(message, logTypeEnum, ex);

#if DEBUG_LOG
                Console.WriteLine($"Error in SafetyLog: {ex}");
                throw;
#endif
            }
        }

        internal async Task SafetyLogAsync(string message, LogTypeEnum logTypeEnum, CancellationToken cancellationToken = default)
        {
            try
            {
                // Validate input parameters
                if (string.IsNullOrEmpty(message))
                {
                    InternalLogger.LogInternalWarning("Attempted to log null or empty message", "SafetyLogAsync");
                    return;
                }

                // Validate configuration before logging
                if (Config == null)
                {
                    throw new InvalidOperationException("Configuration is null. Cannot proceed with logging.");
                }

                // Check for cancellation before starting
                cancellationToken.ThrowIfCancellationRequested();

                await LogAsync(message, logTypeEnum, cancellationToken);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Configuration"))
            {
                InternalLogger.LogInternalError(ex, "SafetyLogAsync - Configuration error");

                // Don't try fallback logging for configuration errors as it would likely fail too
#if DEBUG_LOG
                Console.WriteLine($"Configuration error in SafetyLogAsync: {ex}");
                throw;
#endif
            }
            catch (OperationCanceledException ex)
            {
                InternalLogger.LogInternalWarning($"Logging operation was cancelled for message: {message}", "SafetyLogAsync");
#if DEBUG_LOG
                Console.WriteLine($"Operation cancelled in SafetyLogAsync: {ex}");
                throw;
#endif
            }
            catch (Exception ex)
            {
                InternalLogger.LogInternalError(ex, "SafetyLogAsync");

                // Try fallback logging mechanism
                TryFallbackLogging(message, logTypeEnum, ex);

#if DEBUG_LOG
                Console.WriteLine($"Error in SafetyLogAsync: {ex}");
                throw;
#endif
            }
        }

        private void Log(string message, LogTypeEnum logTypeEnum)
        {
            var logDirectory = Path.Combine(Config.LogDirectory, Config.ApplicationName);

            if (!TryCreateLogDirectory(logDirectory))
            {
                throw new InvalidOperationException($"Failed to create or access log directory: {logDirectory}");
            }

            var fileName = Config.FileName.Invoke();
            var filePath = Path.Combine(logDirectory, $"{fileName}.html");

            var lockObj = GetLockObj(filePath);
            lock (lockObj)
            {
                StreamWriter fileStream = null;
                try
                {
                    var isLogFileExists = File.Exists(filePath);

                    if (isLogFileExists)
                    {
                        var fileInfo = new FileInfo(filePath);
                        if (fileInfo.Length > Config.MaxFileSize * 1024)
                        {
                            if (TryArchiveLogFile(fileInfo, fileName, logDirectory))
                            {
                                isLogFileExists = false;
                            }
                            else
                            {
                                InternalLogger.LogInternalWarning($"Failed to archive log file: {filePath}", "Log method");
                                // Continue with existing file if archiving fails
                            }
                        }
                    }

                    if (!isLogFileExists)
                    {
                        fileStream = File.CreateText(filePath);
                        InitiateHtmlFile(fileStream, fileName);
                    }
                    else
                    {
                        fileStream = new StreamWriter(
                            File.Open(filePath, FileMode.Open, FileAccess.Write, FileShare.Read | FileShare.Delete),
                            UTF8NoBOM);
                        fileStream.BaseStream.Seek(Constants.EngTags.Length * -1, SeekOrigin.End);
                        fileStream.Write(",{");
                    }

                    fileStream.Write(GetLogDiv(message, logTypeEnum));
                    fileStream.Write(Constants.EngTags);
                }
                catch (UnauthorizedAccessException ex)
                {
                    var context = $"Log method - unauthorized access to file: {filePath}";
                    InternalLogger.LogInternalError(ex, context);
#if DEBUG_LOG
                    throw new InvalidOperationException($"Access denied when writing to log file: {filePath}. Check file permissions.", ex);
#endif
                }
                catch (DirectoryNotFoundException ex)
                {
                    var context = $"Log method - directory not found for file: {filePath}";
                    InternalLogger.LogInternalError(ex, context);
#if DEBUG_LOG
                    throw new InvalidOperationException($"Log directory not found: {Path.GetDirectoryName(filePath)}. Ensure the directory exists.", ex);
#endif
                }
                catch (IOException ex)
                {
                    var context = $"Log method - IO error writing to file: {filePath}";
                    InternalLogger.LogInternalError(ex, context);
#if DEBUG_LOG
                    throw new InvalidOperationException($"IO error occurred while writing to log file: {filePath}. The file may be locked by another process.", ex);
#endif
                }
                catch (Exception ex)
                {
                    var context = $"Log method - unexpected error writing to file: {filePath}";
                    InternalLogger.LogInternalError(ex, context);
#if DEBUG_LOG
                    throw new InvalidOperationException($"Unexpected error occurred while logging to file: {filePath}. See inner exception for details.", ex);
#endif
                }
                finally
                {
                    fileStream?.Dispose();
                }
            }
        }

        private async Task LogAsync(string message, LogTypeEnum logTypeEnum, CancellationToken cancellationToken = default)
        {
            var logDirectory = Path.Combine(Config.LogDirectory, Config.ApplicationName);

            if (!TryCreateLogDirectory(logDirectory))
            {
                throw new InvalidOperationException($"Failed to create or access log directory: {logDirectory}");
            }

            var fileName = Config.FileName.Invoke();
            var filePath = Path.Combine(logDirectory, $"{fileName}.html");

            var lockObj = GetLockObj(filePath);

            // Use SemaphoreSlim for async-compatible locking
            var semaphore = GetSemaphore(filePath);
            await semaphore.WaitAsync(cancellationToken);

            try
            {
                StreamWriter fileStream = null;
                try
                {
                    var isLogFileExists = File.Exists(filePath);

                    if (isLogFileExists)
                    {
                        var fileInfo = new FileInfo(filePath);
                        if (fileInfo.Length > Config.MaxFileSize * 1024)
                        {
                            if (TryArchiveLogFile(fileInfo, fileName, logDirectory))
                            {
                                isLogFileExists = false;
                            }
                            else
                            {
                                InternalLogger.LogInternalWarning($"Failed to archive log file: {filePath}", "LogAsync method");
                                // Continue with existing file if archiving fails
                            }
                        }
                    }

                    if (!isLogFileExists)
                    {
                        fileStream = File.CreateText(filePath);
                        await InitiateHtmlFileAsync(fileStream, fileName);
                    }
                    else
                    {
                        fileStream = new StreamWriter(
                            File.Open(filePath, FileMode.Open, FileAccess.Write, FileShare.Read | FileShare.Delete),
                            UTF8NoBOM);
                        fileStream.BaseStream.Seek(Constants.EngTags.Length * -1, SeekOrigin.End);
                        await fileStream.WriteAsync(",{");
                    }

                    await fileStream.WriteAsync(GetLogDiv(message, logTypeEnum));
                    await fileStream.WriteAsync(Constants.EngTags);
                }
                catch (UnauthorizedAccessException ex)
                {
                    var context = $"LogAsync method - unauthorized access to file: {filePath}";
                    InternalLogger.LogInternalError(ex, context);
#if DEBUG_LOG
                    throw new InvalidOperationException($"Access denied when writing to log file: {filePath}. Check file permissions.", ex);
#endif
                }
                catch (DirectoryNotFoundException ex)
                {
                    var context = $"LogAsync method - directory not found for file: {filePath}";
                    InternalLogger.LogInternalError(ex, context);
#if DEBUG_LOG
                    throw new InvalidOperationException($"Log directory not found: {Path.GetDirectoryName(filePath)}. Ensure the directory exists.", ex);
#endif
                }
                catch (IOException ex)
                {
                    var context = $"LogAsync method - IO error writing to file: {filePath}";
                    InternalLogger.LogInternalError(ex, context);
#if DEBUG_LOG
                    throw new InvalidOperationException($"IO error occurred while writing to log file: {filePath}. The file may be locked by another process.", ex);
#endif
                }
                catch (OperationCanceledException ex)
                {
                    var context = $"LogAsync method - operation cancelled for file: {filePath}";
                    InternalLogger.LogInternalError(ex, context);
#if DEBUG_LOG
                    throw new InvalidOperationException($"Logging operation was cancelled for file: {filePath}.", ex);
#endif
                }
                catch (Exception ex)
                {
                    var context = $"LogAsync method - unexpected error writing to file: {filePath}";
                    InternalLogger.LogInternalError(ex, context);
#if DEBUG_LOG
                    throw new InvalidOperationException($"Unexpected error occurred while logging to file: {filePath}. See inner exception for details.", ex);
#endif
                }
                finally
                {
                    fileStream?.Dispose();
                }
            }
            finally
            {
                semaphore.Release();
            }
        }

        private object GetLockObj(string filePath)
        {
            lock (_lockFileLocksBag)
            {
                if (!_fileLocksBag.ContainsKey(filePath))
                    _fileLocksBag[filePath] = new object();

                return _fileLocksBag[filePath];
            }
        }

        private SemaphoreSlim GetSemaphore(string filePath)
        {
            lock (_lockFileLocksBag)
            {
                if (!_fileSemaphoresBag.ContainsKey(filePath))
                    _fileSemaphoresBag[filePath] = new SemaphoreSlim(1, 1);

                return _fileSemaphoresBag[filePath];
            }
        }

        private void InitiateHtmlFile(StreamWriter fileStream, string fileName)
        {
            fileStream.Write(Constants.BeginLogHtmlFile);
            fileStream.WriteLine(Constants.MetaTag);
            fileStream.WriteLine(Constants.TitleTag, fileName);
            fileStream.WriteLine(Constants.StyleTag);
            fileStream.WriteLine(Constants.BeginBodyTag);
            fileStream.WriteLine(Constants.SearchDivTag, fileName);
            fileStream.WriteLine(Constants.ContainerDivTag);
            fileStream.WriteLine(Constants.BeginScriptTag);
        }

        private async Task InitiateHtmlFileAsync(StreamWriter fileStream, string fileName)
        {
            await fileStream.WriteAsync(Constants.BeginLogHtmlFile);
            await fileStream.WriteLineAsync(Constants.MetaTag);
            await fileStream.WriteLineAsync(string.Format(Constants.TitleTag, fileName));
            await fileStream.WriteLineAsync(Constants.StyleTag);
            await fileStream.WriteLineAsync(Constants.BeginBodyTag);
            await fileStream.WriteLineAsync(string.Format(Constants.SearchDivTag, fileName));
            await fileStream.WriteLineAsync(Constants.ContainerDivTag);
            await fileStream.WriteLineAsync(Constants.BeginScriptTag);
        }

        private string GetLogDiv(string message, LogTypeEnum logType)
        {
            return
               $@"
                            'type': '{logType.ToString().ToLower()}',
                            'time': '{DateTime.Now.TimeOfDay}',
                            'message': '{message}'
                        }}

                   ";
        }

        /// <summary>/// Attempts to log using a simple fallback mechanism when the main logging fails.
        /// This method implements multiple fallback strategies to ensure logging never completely fails.
        /// .</summary>
        /// <param name="message">The message to log.</param>
        /// <param name="logType">The type of log entry.</param>
        /// <param name="originalException">The exception that caused the main logging to fail.</param>
        private void TryFallbackLogging(string message, LogTypeEnum logType, Exception originalException)
        {
            var fallbackStrategies = new List<Action>
            {
                () => TryFallbackToTempFile(message, logType, originalException),
                () => TryFallbackToUserProfile(message, logType, originalException),
                () => TryFallbackToCurrentDirectory(message, logType, originalException),
                () => TryFallbackToConsole(message, logType, originalException)
            };

            foreach (var strategy in fallbackStrategies)
            {
                try
                {
                    strategy();
                    InternalLogger.LogInternalInfo($"Successfully used fallback logging for message: {message}", "TryFallbackLogging");
                    return; // Success, no need to try other strategies
                }
                catch (Exception fallbackEx)
                {
                    InternalLogger.LogInternalError(fallbackEx, "TryFallbackLogging - fallback strategy failed");
                    // Continue to next strategy
                }
            }

            // If all fallback strategies fail, log to internal logger only
            InternalLogger.LogInternalError(originalException, "TryFallbackLogging - all fallback strategies failed");
        }

        /// <summary>/// Attempts to write to a fallback file in the system temp directory.
        /// .</summary>
        private void TryFallbackToTempFile(string message, LogTypeEnum logType, Exception originalException)
        {
            var fallbackDir = Path.Combine(Path.GetTempPath(), "XTinyLog_Fallback");

            if (!Directory.Exists(fallbackDir))
            {
                Directory.CreateDirectory(fallbackDir);
            }

            var fallbackFile = Path.Combine(fallbackDir, $"fallback_{DateTime.Now:yyyyMMdd}.log");
            WriteFallbackLogEntry(fallbackFile, message, logType, originalException);
        }

        /// <summary>/// Attempts to write to a fallback file in the user's profile directory.
        /// .</summary>
        private void TryFallbackToUserProfile(string message, LogTypeEnum logType, Exception originalException)
        {
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var fallbackDir = Path.Combine(userProfile, "XTinyLog_Fallback");

            if (!Directory.Exists(fallbackDir))
            {
                Directory.CreateDirectory(fallbackDir);
            }

            var fallbackFile = Path.Combine(fallbackDir, $"fallback_{DateTime.Now:yyyyMMdd}.log");
            WriteFallbackLogEntry(fallbackFile, message, logType, originalException);
        }

        /// <summary>/// Attempts to write to a fallback file in the current directory.
        /// .</summary>
        private void TryFallbackToCurrentDirectory(string message, LogTypeEnum logType, Exception originalException)
        {
            var currentDir = Directory.GetCurrentDirectory();
            var fallbackFile = Path.Combine(currentDir, $"XTinyLog_fallback_{DateTime.Now:yyyyMMdd}.log");
            WriteFallbackLogEntry(fallbackFile, message, logType, originalException);
        }

        /// <summary>/// Last resort: attempts to write to console/debug output.
        /// .</summary>
        private void TryFallbackToConsole(string message, LogTypeEnum logType, Exception originalException)
        {
            var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{logType}] {message}";
            var errorEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [ERROR] Original logging failed: {originalException.Message}";

#if DEBUG
            Console.WriteLine(logEntry);
            Console.WriteLine(errorEntry);
            System.Diagnostics.Debug.WriteLine(logEntry);
            System.Diagnostics.Debug.WriteLine(errorEntry);
#endif
        }

        /// <summary>/// Writes a fallback log entry to the specified file.
        /// .</summary>
        private void WriteFallbackLogEntry(string filePath, string message, LogTypeEnum logType, Exception originalException)
        {
            var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{logType}] {message}{Environment.NewLine}";
            logEntry += $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [ERROR] Original logging failed: {originalException.Message}{Environment.NewLine}";

            if (originalException.StackTrace != null)
            {
                logEntry += $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [ERROR] Stack Trace: {originalException.StackTrace}{Environment.NewLine}";
            }

            File.AppendAllText(filePath, logEntry);
        }

        /// <summary>/// Safely creates the log directory with enhanced error handling and retry logic.
        /// .</summary>
        /// <param name="directoryPath">The directory path to create.</param>
        /// <returns>True if the directory was created or already exists, false otherwise.</returns>
        private bool TryCreateLogDirectory(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                InternalLogger.LogInternalError(new ArgumentException("Directory path cannot be null or empty"), "TryCreateLogDirectory");
                return false;
            }

            const int maxRetries = 3;
            const int retryDelayMs = 100;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    if (Directory.Exists(directoryPath))
                    {
                        // Verify we can actually write to the directory
                        var testFile = Path.Combine(directoryPath, $"test_write_{Guid.NewGuid()}.tmp");
                        try
                        {
                            File.WriteAllText(testFile, "test");
                            File.Delete(testFile);
                            return true;
                        }
                        catch (Exception writeEx)
                        {
                            InternalLogger.LogInternalError(writeEx, $"TryCreateLogDirectory - Cannot write to existing directory: {directoryPath}");
                            return false;
                        }
                    }

                    var directoryInfo = Directory.CreateDirectory(directoryPath);
                    if (directoryInfo.Exists)
                    {
                        InternalLogger.LogInternalInfo($"Successfully created log directory: {directoryPath}", "TryCreateLogDirectory");
                        return true;
                    }
                    else
                    {
                        InternalLogger.LogInternalWarning($"Directory.CreateDirectory returned but directory does not exist: {directoryPath}", "TryCreateLogDirectory");
                        return false;
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    InternalLogger.LogInternalError(ex, $"TryCreateLogDirectory - Access denied to create directory: {directoryPath} (attempt {attempt}/{maxRetries})");
                    if (attempt == maxRetries) return false;
                }
                catch (DirectoryNotFoundException ex)
                {
                    InternalLogger.LogInternalError(ex, $"TryCreateLogDirectory - Parent directory not found for: {directoryPath} (attempt {attempt}/{maxRetries})");
                    if (attempt == maxRetries) return false;
                }
                catch (PathTooLongException ex)
                {
                    InternalLogger.LogInternalError(ex, $"TryCreateLogDirectory - Path too long: {directoryPath}");
                    return false; // No point in retrying for path length issues
                }
                catch (NotSupportedException ex)
                {
                    InternalLogger.LogInternalError(ex, $"TryCreateLogDirectory - Path format not supported: {directoryPath}");
                    return false; // No point in retrying for format issues
                }
                catch (IOException ex)
                {
                    InternalLogger.LogInternalError(ex, $"TryCreateLogDirectory - IO error creating directory: {directoryPath} (attempt {attempt}/{maxRetries})");
                    if (attempt == maxRetries) return false;
                }
                catch (Exception ex)
                {
                    InternalLogger.LogInternalError(ex, $"TryCreateLogDirectory - Unexpected error creating directory: {directoryPath} (attempt {attempt}/{maxRetries})");
                    if (attempt == maxRetries) return false;
                }

                // Wait before retrying (except on last attempt)
                if (attempt < maxRetries)
                {
                    System.Threading.Thread.Sleep(retryDelayMs * attempt);
                }
            }

            return false;
        }

        /// <summary>/// Safely archives a log file when it exceeds the maximum size with enhanced error handling and retry logic.
        /// .</summary>
        /// <param name="fileInfo">The file to archive.</param>
        /// <param name="fileName">The base file name.</param>
        /// <param name="logDirectory">The log directory path.</param>
        /// <returns>True if the file was successfully archived, false otherwise.</returns>
        private bool TryArchiveLogFile(FileInfo fileInfo, string fileName, string logDirectory)
        {
            if (fileInfo == null)
            {
                InternalLogger.LogInternalError(new ArgumentNullException(nameof(fileInfo)), "TryArchiveLogFile - FileInfo is null");
                return false;
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                InternalLogger.LogInternalError(new ArgumentException("FileName cannot be null or empty"), "TryArchiveLogFile");
                return false;
            }

            if (string.IsNullOrWhiteSpace(logDirectory))
            {
                InternalLogger.LogInternalError(new ArgumentException("LogDirectory cannot be null or empty"), "TryArchiveLogFile");
                return false;
            }

            const int maxRetries = 3;
            const int retryDelayMs = 200;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    var archiveDirectory = Path.Combine(logDirectory, "Archive");

                    if (!TryCreateLogDirectory(archiveDirectory))
                    {
                        InternalLogger.LogInternalWarning($"Failed to create archive directory: {archiveDirectory}", "TryArchiveLogFile");
                        return false;
                    }

                    // Create a unique timestamp to avoid conflicts
                    var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
                    var newName = Path.Combine(archiveDirectory, $"{fileName}_{timestamp}.html");

                    // Ensure the target file doesn't already exist
                    int counter = 1;
                    while (File.Exists(newName))
                    {
                        newName = Path.Combine(archiveDirectory, $"{fileName}_{timestamp}_{counter}.html");
                        counter++;
                    }

                    // Refresh file info to ensure it's current
                    fileInfo.Refresh();

                    if (!fileInfo.Exists)
                    {
                        InternalLogger.LogInternalWarning($"File no longer exists, cannot archive: {fileInfo.FullName}", "TryArchiveLogFile");
                        return true; // File is gone, consider this success
                    }

                    fileInfo.MoveTo(newName);

                    // Verify the move was successful
                    if (File.Exists(newName) && !File.Exists(fileInfo.FullName))
                    {
                        InternalLogger.LogInternalInfo($"Successfully archived log file to: {newName}", "TryArchiveLogFile");
                        return true;
                    }
                    else
                    {
                        InternalLogger.LogInternalWarning($"File move operation completed but verification failed. Target: {newName}, Source exists: {File.Exists(fileInfo.FullName)}", "TryArchiveLogFile");
                        return false;
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    InternalLogger.LogInternalError(ex, $"TryArchiveLogFile - Access denied archiving file: {fileInfo.FullName} (attempt {attempt}/{maxRetries})");
                    if (attempt == maxRetries) return false;
                }
                catch (DirectoryNotFoundException ex)
                {
                    InternalLogger.LogInternalError(ex, $"TryArchiveLogFile - Directory not found when archiving file: {fileInfo.FullName} (attempt {attempt}/{maxRetries})");
                    if (attempt == maxRetries) return false;
                }
                catch (FileNotFoundException ex)
                {
                    InternalLogger.LogInternalWarning($"File not found when trying to archive: {fileInfo.FullName}", "TryArchiveLogFile");
                    return true; // File is gone, consider this success
                }
                catch (IOException ex)
                {
                    InternalLogger.LogInternalError(ex, $"TryArchiveLogFile - IO error archiving file: {fileInfo.FullName} (attempt {attempt}/{maxRetries})");
                    if (attempt == maxRetries) return false;
                }
                catch (Exception ex)
                {
                    InternalLogger.LogInternalError(ex, $"TryArchiveLogFile - Unexpected error archiving file: {fileInfo.FullName} (attempt {attempt}/{maxRetries})");
                    if (attempt == maxRetries) return false;
                }

                // Wait before retrying (except on last attempt)
                if (attempt < maxRetries)
                {
                    System.Threading.Thread.Sleep(retryDelayMs * attempt);
                }
            }

            return false;
        }
    }
}
