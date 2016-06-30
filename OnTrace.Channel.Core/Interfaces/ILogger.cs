using System;
using System.Runtime.ExceptionServices;

namespace OnTrace.Channel.Core.Interfaces
{
    public enum EventSeverity
    {
        Debug,
        Information,
        Warning,
        Error,
        Fatal
    }

    public interface ILogger
    {
        bool IsDebugEnabled { get; }
        bool IsInfoEnabled { get; }
        bool IsWarnEnabled { get; }
        bool IsErrorEnabled { get; }
        bool IsFatalEnabled { get; }
        
        void Write(object message, EventSeverity severity);
        void Write(object message, Exception xception, EventSeverity severity);
    }
}