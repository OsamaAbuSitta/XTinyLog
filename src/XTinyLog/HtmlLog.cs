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
                Log(message, logTypeEnum);
            }
            catch (Exception ex)
            {
#if DEBUG_LOG
                Console.WriteLine(ex);
                throw;
#endif
            }
        }

        internal async Task SafetyLogAsync(string message, LogTypeEnum logTypeEnum, CancellationToken cancellationToken = default)
        {
            try
            {
                await LogAsync(message, logTypeEnum, cancellationToken);
            }
            catch (Exception ex)
            {
#if DEBUG_LOG
                Console.WriteLine(ex);
                throw;
#endif
            }
        }

        private void Log(string message, LogTypeEnum logTypeEnum)
        {
            Directory.CreateDirectory(Config.LogDirectory + $@"\{Config.ApplicationName}");

            var fileName = Config.FileName.Invoke();
            var filePath = $@"{Config.LogDirectory}\{Config.ApplicationName}\{fileName}.html";

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
                            var archiveDirectory =
                                Directory.CreateDirectory($@"{Config.LogDirectory}\{Config.ApplicationName}\Archive");
                            var newName =
                                $@"{archiveDirectory.FullName}\{fileName}_{DateTime.Now:HH mm ss}.html";
                            fileInfo.MoveTo(newName);
                            isLogFileExists = false;
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
                catch (Exception ex)
                {
#if DEBUG_LOG
                                      throw;
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
            Directory.CreateDirectory(Config.LogDirectory + $@"\{Config.ApplicationName}");

            var fileName = Config.FileName.Invoke();
            var filePath = $@"{Config.LogDirectory}\{Config.ApplicationName}\{fileName}.html";

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
                            var archiveDirectory =
                                Directory.CreateDirectory($@"{Config.LogDirectory}\{Config.ApplicationName}\Archive");
                            var newName =
                                $@"{archiveDirectory.FullName}\{fileName}_{DateTime.Now:HH mm ss}.html";
                            fileInfo.MoveTo(newName);
                            isLogFileExists = false;
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
                catch (Exception ex)
                {
#if DEBUG_LOG
                    throw;
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
            fileStream.WriteLine(Constants.SearchDivTag);
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
            await fileStream.WriteLineAsync(Constants.SearchDivTag);
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
    }
}