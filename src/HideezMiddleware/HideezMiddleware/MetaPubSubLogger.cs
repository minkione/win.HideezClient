using Hideez.SDK.Communication.Log;
using Meta.Lib.Modules.Logger;
using System;

namespace HideezMiddleware
{
    public class MetaPubSubLogger : Logger, IMetaLogger
    {
        public string Name { get; }

        public MetaPubSubLogger(ILog log)
            : base(nameof(MetaPubSubLogger), log)
        {
            Name = nameof(MetaPubSubLogger);
        }

        public void Critical(string message)
        {
            WriteLine(message, LogErrorSeverity.Fatal);
        }

        public void Critical(Exception ex)
        {
            WriteLine(ex, LogErrorSeverity.Fatal);
        }

        public void Critical(string message, Exception ex)
        {
            WriteLine(message, ex, LogErrorSeverity.Fatal);
        }

        public void Debug(string message)
        {
            WriteLine(message, LogErrorSeverity.Debug);
        }

        public void Debug(Exception ex)
        {
            WriteLine(ex, LogErrorSeverity.Debug);
        }

        public void Debug(string message, Exception ex)
        {
            WriteLine(message, ex, LogErrorSeverity.Debug);
        }

        public void Error(string message)
        {
            WriteLine(message, LogErrorSeverity.Error);
        }

        public void Error(Exception ex)
        {
            WriteLine(ex, LogErrorSeverity.Error);
        }

        public void Error(string message, Exception ex)
        {
            WriteLine(message, ex, LogErrorSeverity.Error);
        }

        public void Info(string message)
        {
            WriteLine(message, LogErrorSeverity.Information);
        }

        public void Info(Exception ex)
        {
            WriteLine(ex, LogErrorSeverity.Information);
        }

        public void Info(string message, Exception ex)
        {
            WriteLine(message, ex, LogErrorSeverity.Information);
        }

        public void Trace(string message)
        {
            WriteLine(message, LogErrorSeverity.Information);
        }

        public void Trace(Exception ex)
        {
            WriteLine(ex, LogErrorSeverity.Information);
        }

        public void Trace(string message, Exception ex)
        {
            WriteLine(message, ex, LogErrorSeverity.Information);
        }

        public void Warning(string message)
        {
            WriteLine(message, LogErrorSeverity.Warning);
        }

        public void Warning(Exception ex)
        {
            WriteLine(ex, LogErrorSeverity.Warning);
        }

        public void Warning(string message, Exception ex)
        {
            WriteLine(message, ex, LogErrorSeverity.Warning);
        }
    }
}
