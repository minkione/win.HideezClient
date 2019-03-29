using System;
using System.ServiceModel;
using System.Threading.Tasks;
using HideezSafe.HideezServiceReference;
using NLog;
using Unity;

namespace HideezSafe.Modules.ServiceProxy
{
    class ServiceProxy : IServiceProxy, IDisposable
    {
        private Logger log = LogManager.GetCurrentClassLogger();
        private HideezServiceClient service;
        private IHideezServiceCallback callback;

        public event EventHandler Connected;
        public event EventHandler Disconnected;

        [Dependency]
        public IHideezServiceCallback Callback { get; private set; }

        public bool IsConnected
        {
            get
            {
                if (service == null)
                    return false;
                else
                    return service.State != CommunicationState.Faulted &&
                        service.State != CommunicationState.Closed;
            }
        }

        public IHideezService GetService()
        {
            if (!IsConnected)
                throw new ServiceNotConnectedException();

            return service;
        }

        public Task<bool> ConnectAsync()
        {
            return Task.Run(async () =>
            {
                if (service != null)
                    return true;

                var instanceContext = new InstanceContext(callback);
                service = new HideezServiceClient(instanceContext);

                SubscriveToServiceEvents(service);

                try
                {
                    return await service.AttachClientAsync(new ServiceClientParameters()
                    {
                        ClientType = ClientType.DesktopClient
                    });
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                    return false;
                }
            });
        }

        public Task DisconnectAsync()
        {
            return Task.Run(() =>
            {
                if (service != null)
                {
                    if (service.State == CommunicationState.Opened)
                        service.DetachClient();

                    CloseServiceConnection(service);
                    UnsubscriveFromServiceEvents(service);
                    service = null;
                }
            });
        }

        private void CloseServiceConnection(HideezServiceClient service)
        {
            if (service == null)
                return;

            if (service.State != CommunicationState.Faulted)
                service.Close();
            else
                service.Abort();
        }

        private void SubscriveToServiceEvents(HideezServiceClient service)
        {
            if (service == null)
                return;

            var clientChannel = (IClientChannel)service;
            clientChannel.Opened += Connected;
            clientChannel.Closed += Disconnected;
            clientChannel.Faulted += Disconnected;
        }

        private void UnsubscriveFromServiceEvents(HideezServiceClient service)
        {
            if (service == null)
                return;

            var clientChannel = (IClientChannel)service;
            clientChannel.Opened -= Connected;
            clientChannel.Closed -= Disconnected;
            clientChannel.Faulted -= Disconnected;
        }

        #region IDisposable
        private bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                CloseServiceConnection(service);
                UnsubscriveFromServiceEvents(service);
                service = null;
            }

            disposed = true;
        }
        #endregion
    }
}
