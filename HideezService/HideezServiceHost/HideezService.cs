using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.ServiceModel;
using ServiceLibrary.Implementation;
using HideezServiceHost.HideezServiceReference;

namespace HideezServiceHost
{
    public partial class HideezService : ServiceBase
    {
        ServiceHost serviceHost = null;
        HideezServiceClient service = null;

        public HideezService()
        {
            InitializeComponent();
        }

        protected override async void OnStart(string[] args)
        {
            try
            {
                serviceHost = new ServiceHost(typeof(ServiceLibrary.Implementation.HideezService), new Uri("net.pipe://localhost/HideezService/"))
                {
                    CloseTimeout = new TimeSpan(0, 0, 1),
                };

                serviceHost.Open();

                // Connect to service to initialize it
                var callback = new HideezServiceCallbacks();
                var instanceContext = new InstanceContext(callback);

                var service = new HideezServiceClient(instanceContext);
                await service.AttachClientAsync(new ServiceClientParameters() { ClientType = ClientType.ServiceHost });

                // Disconnect is no longer possible, we need to maintain connection with the service we 
                // are hosting to notify about session change
                //service.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
        }

        protected override void OnStop()
        {
            try
            {
                // connect and ask the service to finish all works and close all connections
                var callback = new HideezServiceCallbacks();
                var instanceContext = new InstanceContext(callback);

                service = new HideezServiceClient(instanceContext);
                service.ShutdownAsync().Wait();

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
            if (sessionChangeDescription.Reason == SessionChangeReason.SessionLock)
            {
                // Session locked
                service?.OnSessionChange(true);
            }
            else if (sessionChangeDescription.Reason == SessionChangeReason.SessionUnlock)
            {
                // Session unlocked
                service?.OnSessionChange(false);
            }
        }
    }
}
