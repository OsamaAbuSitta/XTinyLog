using System;

namespace XTinyLog
{
    /// <summary>
    /// Wrapper class to mange instance creation
    /// </summary>
    public class XLog
    {
        private static Lazy<XLog> _xTinyLogLazy = new Lazy<XLog>(() => new XLog());
        private readonly HtmlLog _logger;
        public readonly ILogConfiguration Config;

        /// <summary>
        ///Single log instance
        /// </summary>
        public static XLog Log => _xTinyLogLazy.Value;

        /// <summary>
        /// Return new log instance if multiple instances is needed
        /// </summary>
        /// <returns> new XLog() </returns>
        public static XLog Create()
        {
            return new XLog();
        }

        /// <summary>
        /// Return new log instance if multiple instances and different configuration is needed
        /// </summary>
        /// <returns> new XLog(configuration) </returns>
        public static XLog Create(Action<ILogConfiguration> configuration)
        {
            var config = new DefaultLogConfiguration();
            configuration.Invoke(config);
            
            //ToDo :: config validation 

            return new XLog(config);
        }

        protected XLog(ILogConfiguration configuration)
        {
            Config = configuration;
            _logger = new HtmlLog(configuration);
        }

        protected XLog()
        {
            _logger = new HtmlLog();
            Config = _logger.Config;
        }

        public virtual void Info(string message)
        {
            _logger.SafetyLog(message, LogTypeEnum.Info);
        }

        public virtual void Debug(string message)
        {
            _logger.SafetyLog(message, LogTypeEnum.Debug);
        }

        public virtual void Error(string message)
        {
            _logger.SafetyLog(message, LogTypeEnum.Error);
        }

        public virtual void Error(Exception exception)
        {
            _logger.SafetyLog(exception.Message, LogTypeEnum.Error);
        }

        public virtual void Warn(string message)
        {
            _logger.SafetyLog(message, LogTypeEnum.Warning);
        }
    }
}