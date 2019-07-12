using Hideez.SDK.Communication.Log;
using NLog;
using System;

namespace HideezMiddleware
{
    public class NLogWrapper : ILog
    {
        public void Shutdown()
        {
        }

        public void WriteDebugLine(string source, Exception ex, LogErrorSeverity severity = LogErrorSeverity.Debug)
        {
            LogManager.GetLogger(source).Error(ex);
        }

        public void WriteDebugLine(string source, string message, LogErrorSeverity severity = LogErrorSeverity.Debug)
        {
            LogManager.GetLogger(source).Debug(message);
        }

        public void WriteLine(string source, string message, LogErrorSeverity severity = LogErrorSeverity.Information, string stackTrace = null)
        {
            switch (severity)
            {
                case LogErrorSeverity.Debug:
                    LogManager.GetLogger(source).Debug(message);
                    break;
                case LogErrorSeverity.Error:
                    LogManager.GetLogger(source).Error(message);
                    break;
                case LogErrorSeverity.Information:
                    LogManager.GetLogger(source).Info(message);
                    break;
                case LogErrorSeverity.Warning:
                    LogManager.GetLogger(source).Warn(message);
                    break;
            }
        }

        public void WriteLine(string source, Exception ex, LogErrorSeverity severity = LogErrorSeverity.Error)
        {
            LogManager.GetLogger(source).Error(ex);
        }
    }
}
