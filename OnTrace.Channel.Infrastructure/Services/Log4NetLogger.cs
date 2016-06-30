using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnTrace.Channel.Core.Interfaces;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace OnTrace.Channel.Infrastructure.Services
{
    public class Log4NetLogger : ILogger
    {
        private readonly log4net.ILog _logger;

        public Log4NetLogger(Type type)
        {
            _logger = log4net.LogManager.GetLogger(type);
        }


        public bool IsDebugEnabled
        {
            get { return _logger.IsDebugEnabled; }
        }
        public bool IsInfoEnabled
        {
            get { return _logger.IsInfoEnabled; }
        }

        public bool IsWarnEnabled
        {
            get { return _logger.IsWarnEnabled; }
        }

        public bool IsErrorEnabled
        {
            get { return _logger.IsErrorEnabled; }
        }

        public bool IsFatalEnabled
        {
            get { return _logger.IsFatalEnabled; }
        }
        
        public void Write(object message, EventSeverity severity)
        {
            Write(message, null, severity);
        }

        public void Write(object message, Exception exception, EventSeverity severity)
        {
            if (severity == EventSeverity.Debug && IsDebugEnabled )
            {
                _logger.Debug(message, exception);
            }
            else if (severity == EventSeverity.Information && IsInfoEnabled)
            {
                _logger.Info(message, exception);
            }
            else if (severity == EventSeverity.Warning && IsWarnEnabled)
            {
                _logger.Warn(message, exception);
            }
            else if (severity == EventSeverity.Error && IsErrorEnabled)
            {
                _logger.Error(message, exception);
            }
            else if(severity == EventSeverity.Fatal && IsFatalEnabled)
            {
                _logger.Fatal(message, exception);
            }

            /*
            ValidateSeverityEnabled(severity);

            switch (severity)
            {
                case EventSeverity.Debug: 
                    break;
                case EventSeverity.Information: _logger.Info(message, exception);
                    break;
                case EventSeverity.Warning: _logger.Warn(message, exception);
                    break;
                case EventSeverity.Error: _logger.Error(message, exception);
                    break;
                case EventSeverity.Fatal: _logger.Fatal(message, exception);
                    break;
            }*/
        }

        private void ValidateSeverityEnabled(EventSeverity severity)
        {
            switch (severity)
            {
                case EventSeverity.Information:
                    if (!IsInfoEnabled)
                        throw new InvalidOperationException("Informational log disabled");
                    break;
                case EventSeverity.Warning:
                    if (!IsWarnEnabled)
                        throw new InvalidOperationException("Warning log disabled");
                    break;
                case EventSeverity.Error:
                    if (!IsErrorEnabled)
                        throw new InvalidOperationException("Error log disabled");
                    break;
                case EventSeverity.Fatal:
                    if (!IsFatalEnabled)
                        throw new InvalidOperationException("Fatal log disabled");
                    break;
                case EventSeverity.Debug:
                    if (!IsDebugEnabled)
                        throw new InvalidOperationException("Debug log disabled");
                    break;
            }
        }
    }
}
