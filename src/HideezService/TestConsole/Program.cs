using HideezMiddleware;
using Microsoft.Win32;
using ServiceLibrary.Implementation;
using System;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
                var service = new HideezServiceFactory().GetHideezService();

                while (true)
                {
                    string line = Console.ReadLine();
                    if (line == "q" || line == "exit")
                    {
                        Console.WriteLine("exiting...");
                        service.Shutdown();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        protected static void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            // This is a direct copy of HideezServiceHost.HideezService.OnSessionChange
            SessionSwitchMonitor.SystemSessionSwitch(System.Diagnostics.Process.GetCurrentProcess().SessionId, e.Reason);
        }
    }
}
