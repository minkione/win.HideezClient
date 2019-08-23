using NLog;
using System;
using System.ServiceProcess;

namespace HideezServiceHost
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            try
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    new HideezService()
                };
                ServiceBase.Run(ServicesToRun);
            }
            catch (Exception ex)
            {
                var log = LogManager.GetCurrentClassLogger();
                log.Fatal(ex, "An unhandled exception occured in service");
                LogManager.Flush();
                Environment.FailFast(ex.Message);
            }
        }
    }
}
