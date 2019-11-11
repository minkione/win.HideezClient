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
using Microsoft.Win32;

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
                try
                {
                    await service.AttachClientAsync(new ServiceClientParameters() { ClientType = ClientType.ServiceHost });

                    // Disconnect from service
                    service.Close();
                }
                catch (System.ServiceProcess.TimeoutException)
                {
                    // Handle the timeout exception
                    service.Abort();
                }
                catch (CommunicationException)
                {
                    // Handle the communication exception
                    service.Abort();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
        }

        protected override void OnShutdown()
        {
            ServiceLibrary.Implementation.HideezService.OnServiceStopped();
            base.OnShutdown();
        }

        protected override async void OnStop()
        {
            try
            {
                ServiceLibrary.Implementation.HideezService.OnServiceStopped();

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
                SessionSwitchMonitor.SystemSessionSwitch(sessionChangeDescription.SessionId, (SessionSwitchReason)sessionChangeDescription.Reason);
            }
            catch (Exception ex)
            {
                ServiceLibrary.Implementation.HideezService.Error(ex);
            }
        }

        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            HandlePowerEvent(powerStatus);
            return base.OnPowerEvent(powerStatus);
        }


        /*
         * https://stackoverflow.com/questions/30433432/difference-between-resumeautomatic-resumesuspend-modes-of-windows-service
         * 
         * How it's supposed to work
         * (This is not how it all works in practice - see below.)
         * 
         * ResumeAutomatic
         * This message is always sent when the computer has resumed after sleep.
         * 
         * ResumeSuspend
         * The computer has resumed after sleep, and Windows believes a user is present - i.e. that there is a human sitting in front of the machine. This message is sent when either 
         * a) the wake was caused by human interaction (someone pressing the power button,pressingakey,movingthemouse,etc); or b)thefirsttime thereishuman interaction after the machine wakes automatically due to a wake timer.
         * 
         * To summarise:
         * ResumeAutomatic is always sent when the computer resumes from sleep.
         * ResumeSuspend is sent as well as ResumeAutomatic when the computer resumes from sleep and Windows believes a user is present.
         * 
         * How it actually works
         * ResumeAutomatic occasionally isn't sent at all. This is a long-standing bug, presumably in Windows itself. Fortunately I've never seen the computer wake with both ResumeAutomatic and ResumeSuspend unsent. If you need to know that the system has resumed, but don't care whether a user's there or not, you need to listen for both ResumeAutomatic and ResumeSuspend and treat them as the same thing. 
         * 
         * ResumeSuspend is extremely unreliable. I've never seen it not sent when it's supposed to be, but it's often sent when it isn't supposed to be - when actually there's no user there at all. Whether this is due to one or more bugs in Windows, third party drivers, firmware or hardware, I have no idea. 
         * 
         * When ResumeAutomatic is sent with no corresponding ResumeSuspend the system idle timeout is brief (2 minutes by default in Windows 10) and attached displays are kept in power saving mode. When a corresponding ResumeSuspend is sent the system idle timeout is normal (30 minutes by default in Windows 10) and attached displays are woken up. This is so that the computer goes back to sleep as soon as possible if it wakes automatically to perform maintenance, etc. It would be fantastic if Microsoft could make it work reliably.
         * 
         */
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
                    OnSystemSuspending();
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
                ServiceLibrary.Implementation.HideezService.Error(ex);
            }
        }

        void OnSystemSuspending()
        {
            try
            {
                ServiceLibrary.Implementation.HideezService.OnGoingToSleep();
            }
            catch (Exception ex)
            {
                ServiceLibrary.Implementation.HideezService.Error(ex);
            }
        }
    }
}
