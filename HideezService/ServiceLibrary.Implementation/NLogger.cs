using Hideez.SDK.Communication.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLibrary.Implementation
{
    class NLogger : ILog
    {
        public void Shutdown()
        {
        }

        public void WriteDebugLine(string source, Exception ex, LogErrorSeverity severity = LogErrorSeverity.Debug)
        {
        }

        public void WriteDebugLine(string source, string message, LogErrorSeverity severity = LogErrorSeverity.Debug)
        {
        }

        public void WriteLine(string source, string message, LogErrorSeverity severity = LogErrorSeverity.Information, string stackTrace = null)
        {
        }

        public void WriteLine(string source, Exception ex, LogErrorSeverity severity = LogErrorSeverity.Error)
        {
        }
    }
}
