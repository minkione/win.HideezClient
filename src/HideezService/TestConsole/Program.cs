using HideezMiddleware;
using Microsoft.Win32;
using ServiceLibrary.Implementation;
using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading;
using System.Threading.Tasks;
using TestConsole.HideezServiceReference;

namespace TestConsole
{
    class Program
    {
        static ServiceHost serviceHost;
        static HideezServiceClient service;

        static void Main(string[] args)
        {
            try
            {
                SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
                ThreadPool.QueueUserWorkItem(ConnectToHideezService);

                while (true)
                {
                    string line = Console.ReadLine();
                    if (line == "q" || line == "exit")
                    {
                        Console.WriteLine("exiting...");
                        HideezService.OnServiceStoppedAsync().Wait();
                        try
                        {
                            service.ShutdownAsync().Wait();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }

                        if (serviceHost.State == CommunicationState.Faulted)
                        {
                            serviceHost.Abort();
                        }
                        else
                        {
                            serviceHost.Close();
                        }
                        break;
                    }
                }

                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async static void ConnectToHideezService(Object param)
        {
            try
            {
                serviceHost = new ServiceHost(typeof(HideezService),
                                    new Uri("net.pipe://localhost/HideezService/"));

                // Enable debug information behavior
                ServiceDebugBehavior debug = serviceHost.Description.Behaviors.Find<ServiceDebugBehavior>();

                // if not found - add behavior with setting turned on 
                if (debug == null)
                {
                    serviceHost.Description.Behaviors.Add(new ServiceDebugBehavior() { IncludeExceptionDetailInFaults = true });
                }
                else
                {
                    // make sure setting is turned ON
                    if (!debug.IncludeExceptionDetailInFaults)
                    {
                        debug.IncludeExceptionDetailInFaults = true;
                    }
                }

                serviceHost.Open();

                Console.WriteLine("running...");

                foreach (Uri uri in serviceHost.BaseAddresses)
                {
                    Console.WriteLine("Uri: {0}", uri.AbsoluteUri);
                }

                foreach (ServiceEndpoint endpoint in serviceHost.Description.Endpoints)
                {
                    Console.WriteLine("Address - {0}, binding: {1}, contract: {2}",
                        endpoint.Address,
                        endpoint.Binding.Name,
                        endpoint.Contract.Name);
                }

                // подключаемся к серверу, чтобы он стартовал
                var callback = new HideezServiceCallbacks();
                var instanceContext = new InstanceContext(callback);

                // NOTE: If an ambiguous reference error occurs, check that TestConsole DOES NOT have 
                // a reference to 'ServiceLibrary'. There should be only 'ServiceLibrary.Implementation' ref
                service = new HideezServiceClient(instanceContext);
                await service.AttachClientAsync(new ServiceClientParameters() { ClientType = ClientType.TestConsole });

                // Disconnect from service
                service.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        protected static async void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            // This is a direct copy of HideezServiceHost.HideezService.OnSessionChange
            try
            {
                switch (e.Reason)
                {
                    case SessionSwitchReason.SessionLock:
                    case SessionSwitchReason.SessionLogoff:
                        // Session locked
                        HideezService.OnSessionChange(true);
                        await Task.Delay(250);
                        break;
                    case SessionSwitchReason.SessionUnlock:
                    case SessionSwitchReason.SessionLogon:
                        // Session unlocked
                        HideezService.OnSessionChange(false);
                        break;
                    default:
                        return;
                }
                SessionSwitchMonitor.SystemSessionSwitch(System.Diagnostics.Process.GetCurrentProcess().SessionId, e.Reason);
            }
            catch (Exception ex)
            {
                HideezService.Error(ex);
            }
        }
    }
}
