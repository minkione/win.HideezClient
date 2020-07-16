using Hideez.SDK.Communication.Log;
using Meta.Lib.Modules.Logger;
using NLog;
using System;

namespace HideezMiddleware
{
    public class NLogWrapper : ILog
    {
        readonly NLog.Logger _log = LogManager.GetCurrentClassLogger();

        public void Shutdown()
        {
        }

        public void WriteDebugLine(string source, Exception ex, LogErrorSeverity severity = LogErrorSeverity.Debug)
        {
            _log.Debug(ex, FormatMessage(source));
        }

        public void WriteDebugLine(string source, string message, LogErrorSeverity severity = LogErrorSeverity.Debug)
        {
            _log.Debug(FormatMessage(source, message));
        }

        public void WriteDebugLine(string source, string message, Exception ex, LogErrorSeverity severity = LogErrorSeverity.Error)
        {
            _log.Debug(ex, FormatMessage(source, message));
        }


        public void WriteLine(string source, string message, LogErrorSeverity severity = LogErrorSeverity.Information, string stackTrace = null)
        {
            _log.Log(GetLogLevel(severity), FormatMessage(source, message));

        }

        public void WriteLine(string source, Exception ex, LogErrorSeverity severity = LogErrorSeverity.Error)
        {
            _log.Log(GetLogLevel(severity), ex, FormatMessage(source));
        }

        public void WriteLine(string source, string message, Exception ex, LogErrorSeverity severity = LogErrorSeverity.Error)
        {
            _log.Log(GetLogLevel(severity), ex, FormatMessage(source, message));
        }


        LogLevel GetLogLevel(LogErrorSeverity severity)
        {
            switch (severity)
            {
                case LogErrorSeverity.Fatal:
                    return LogLevel.Fatal;
                case LogErrorSeverity.Error:
                    return LogLevel.Error;
                case LogErrorSeverity.Warning:
                    return LogLevel.Warn;
                case LogErrorSeverity.Information:
                    return LogLevel.Info;
                case LogErrorSeverity.Debug:
                    return LogLevel.Debug;
                default:
                    return LogLevel.Debug;
            }
        }

        string FormatMessage(string source, string message = "")
        {
            return $"{source} | {message}";
        }
    }
}
