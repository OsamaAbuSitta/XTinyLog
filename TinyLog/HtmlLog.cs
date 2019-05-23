using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace XTinyLog
{
    internal class HtmlLog
    {
        internal readonly ILogConfiguration Config;
        private static Dictionary<string, object> _fileLocksBag;
        private static object _lockFileLocksBag;
        private static readonly Encoding UTF8NoBOM = new UTF8Encoding(false, true);

        static HtmlLog()
        {
            _lockFileLocksBag = new object();
            _fileLocksBag = new Dictionary<string, object>();
        }

        public HtmlLog() : this(new DefaultLogConfiguration())
        {
        }

        internal HtmlLog(ILogConfiguration _config)
        {
            this.Config = _config;
        }

        internal void SafetyLog(string message, LogTypeEnum logTypeEnum)
        {
            Task.Run(() =>
            {
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
            }).Wait();
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

        private object GetLockObj(string filePath)
        {
            lock (_lockFileLocksBag)
            {
                if (!_fileLocksBag.ContainsKey(filePath))
                    _fileLocksBag[filePath] = new object();

                return _fileLocksBag[filePath];
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