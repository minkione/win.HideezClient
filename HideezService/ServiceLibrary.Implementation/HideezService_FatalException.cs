using NLog;
using System;
using System.Threading.Tasks;

namespace ServiceLibrary.Implementation
{
    public partial class HideezService : IHideezService
    {
        void SetupExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            LogUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

            TaskScheduler.UnobservedTaskException += (s, e) =>
                LogUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException");
        }

        void LogUnhandledException(Exception e, string source)
        {
            try
            {
                LogManager.EnableLogging();

                var fatalLogger = LogManager.GetCurrentClassLogger();
                var assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName();

                fatalLogger.Fatal($"Unhandled exception in {assemblyName.Name} v{assemblyName.Version}");
                fatalLogger.Fatal(e);
                LogManager.Flush();
                LogManager.Shutdown();
            }
            catch (Exception)
            {
                try
                {
                    Environment.FailFast("An error occured while handling fatal error", e as Exception);
                }
                catch (Exception exc)
                {
                    Environment.FailFast("An error occured while handling an error during fatal error handling", exc);
                }
            }
        }
    }
}
