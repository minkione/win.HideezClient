using Hideez.SDK.Communication.Log;
using NLog;
using System;

namespace ServiceLibrary.Implementation
{
    class NLogger : ILog
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
            LogManager.GetLogger(source).Info(message);
        }

        public void WriteLine(string source, string message, LogErrorSeverity severity = LogErrorSeverity.Information, string stackTrace = null)
        {
            LogManager.GetLogger(source).Info(message);
        }

        public void WriteLine(string source, Exception ex, LogErrorSeverity severity = LogErrorSeverity.Error)
        {
            LogManager.GetLogger(source).Error(ex);
        }
    }
}
