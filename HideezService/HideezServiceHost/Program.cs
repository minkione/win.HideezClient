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
                Environment.FailFast(ex.Message);
            }
        }
    }
}
