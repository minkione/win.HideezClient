using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.ServiceModel;
using ServiceLibrary.Implementation;
using HideezServiceHost.HideezServiceReference;
using HideezMiddleware;
using System.Management;
using System.IO;
using System.Threading.Tasks;

namespace HideezServiceHost
{
    public partial class HideezService : ServiceBase
    {
        ServiceHost serviceHost = null;
        HideezServiceClient service = null;

        public HideezService()
        {
            CanHandlePowerEvent = true;
            CanHandleSessionChangeEvent = true;

            InitializeComponent();
        }

        protected override async void OnStart(string[] args)
        {
            try
            {
                serviceHost = new ServiceHost(typeof(ServiceLibrary.Implementation.HideezService), new Uri("net.pipe://localhost/HideezService/"))
                {
                    CloseTimeout = new TimeSpan(0, 0, 20),
                };

                serviceHost.Open();

                // Connect to service to initialize it
                var callback = new HideezServiceCallbacks();
                var instanceContext = new InstanceContext(callback);

                var service = new HideezServiceClient(instanceContext);
                await service.AttachClientAsync(new ServiceClientParameters() { ClientType = ClientType.ServiceHost });

                // Disconnect from service
                service.Close();

                await ServiceLibrary.Implementation.HideezService.OnServiceStartedAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
        }

        protected override void OnShutdown()
        {
            ServiceLibrary.Implementation.HideezService.OnServiceStoppedAsync().Wait();
            base.OnShutdown();
        }

        protected override async void OnStop()
        {
            try
            {
                ServiceLibrary.Implementation.HideezService.OnServiceStoppedAsync().Wait();

                // connect and ask the service to finish all works and close all connections
                var callback = new HideezServiceCallbacks();
                var instanceContext = new InstanceContext(callback);

                service = new HideezServiceClient(instanceContext);
                await service.ShutdownAsync();

                // close the host
                if (serviceHost.State == CommunicationState.Faulted)
                {
                    serviceHost.Abort();
                }
                else
                {
                    serviceHost.Close();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        // https://stackoverflow.com/questions/44980/programmatically-determine-a-duration-of-a-locked-workstation
        protected override void OnSessionChange(SessionChangeDescription sessionChangeDescription)
        {
            try
            {
                switch (sessionChangeDescription.Reason)
                {
                    case SessionChangeReason.SessionLock:
                    case SessionChangeReason.SessionLogoff:
                        // Session locked
                        ServiceLibrary.Implementation.HideezService.OnSessionChange(true);
                        break;
                    case SessionChangeReason.SessionUnlock:
                    case SessionChangeReason.SessionLogon:
                        // Session unlocked
                        ServiceLibrary.Implementation.HideezService.OnSessionChange(false);
                        break;
                    default:
                        return;
                }

                SessionSwitchManager.SystemSessionSwitch(sessionChangeDescription.SessionId, (Microsoft.Win32.SessionSwitchReason)sessionChangeDescription.Reason);
            }
            catch (Exception ex)
            {
                ServiceLibrary.Implementation.HideezService.LogException(ex);
            }
        }

        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            HandlePowerEvent(powerStatus);
            return base.OnPowerEvent(powerStatus);
        }

        void HandlePowerEvent(PowerBroadcastStatus powerStatus)
        {
            switch (powerStatus)
            {
                case PowerBroadcastStatus.BatteryLow:
                    break;
                case PowerBroadcastStatus.OemEvent:
                    break;
                case PowerBroadcastStatus.PowerStatusChange:
                    break;
                case PowerBroadcastStatus.QuerySuspend: // System is trying to schedule suspend
                    break;
                case PowerBroadcastStatus.QuerySuspendFailed: // Some application canceled suspend
                    break;
                case PowerBroadcastStatus.ResumeAutomatic: // Sleep or hibernation ended, brief system timeout (2m)
                case PowerBroadcastStatus.ResumeCritical: // Suspension because of low battery charge ended
                case PowerBroadcastStatus.ResumeSuspend: // Sleep or hibernation ended, normal system timeout (30m)
                    OnSystemLeftSuspendedMode();
                    break;
                case PowerBroadcastStatus.Suspend: // System is about to be suspended, approximately 2 seconds before it happens
                    break;
                default:
                    break;
            }
        }

        void OnSystemLeftSuspendedMode()
        {
            try
            {
                ServiceLibrary.Implementation.HideezService.OnLaunchFromSleep();
            }
            catch (Exception ex)
            {
                ServiceLibrary.Implementation.HideezService.LogException(ex);
            }
        }
    }
}
