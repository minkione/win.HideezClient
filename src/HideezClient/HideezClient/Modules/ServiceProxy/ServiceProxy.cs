﻿using System;
using System.ServiceModel;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;
using Hideez.SDK.Communication.Log;
using HideezClient.HideezServiceReference;
using HideezClient.Messages;
using HideezClient.Modules.Log;
using HideezClient.Modules.ServiceCallbackMessanger;

namespace HideezClient.Modules.ServiceProxy
{
    // Todo: Add lock for "service" to improve thread safety
    class ServiceProxy : IServiceProxy, IDisposable
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger(nameof(ServiceProxy));
        private readonly IHideezServiceCallback callback;
        private readonly IMessenger messenger;

        private HideezServiceClient service;

        public event EventHandler Connected;
        public event EventHandler Disconnected;

        public ServiceProxy(IHideezServiceCallback callback, IMessenger messenger)
        {
            this.callback = callback;
            this.messenger = messenger;

            messenger.Register<SendPinMessage>(this, OnSendPinMessage);
            messenger.Register<SendActivationCodeMessage>(this, OnSendActivationCodeMessage);

            this.Connected += ServiceProxy_ConnectionChanged;
            this.Disconnected += ServiceProxy_ConnectionChanged;
        }

        private void ServiceProxy_ConnectionChanged(object sender, EventArgs e)
        {
            messenger.Send(new ConnectionServiceChangedMessage(IsConnected));
        }

        private async void OnSendPinMessage(SendPinMessage obj)
        {
            try
            {
                if (IsConnected)
                {
                    await service.SendPinAsync(obj.DeviceId, obj.Pin ?? new byte[0], obj.OldPin ?? new byte[0]);
                }
            }
            catch (Exception ex)
            {
                log.WriteLine(ex);
            }
        }

        private async void OnSendActivationCodeMessage(SendActivationCodeMessage obj)
        {
            try
            {
                if (IsConnected)
                {
                    await service.SendActivationCodeAsync(obj.DeviceId, obj.Code ?? new byte[0]);
                }
            }
            catch (Exception ex)
            {
                log.WriteLine(ex);
            }
        }

        public bool IsConnected
        {
            get
            {
                if (service == null)
                    return false;
                else // TODO: sometimes service.State causes a NullReferenceException
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
                    await DisconnectAsync();

                var instanceContext = new InstanceContext(callback);
                service = new HideezServiceClient(instanceContext);

                SubscriveToServiceEvents(service);

                try
                {
                    var attached = await service.AttachClientAsync(new ServiceClientParameters()
                    {
                        ClientType = ClientType.DesktopClient
                    });

                    if (!attached)
                    {
                        await DisconnectAsync();
                    }

                    return attached;
                }
                catch (EndpointNotFoundException)
                {
                    await DisconnectAsync();

                    return false;
                }
                catch (Exception ex)
                {
                    log.WriteLine(ex.Message);

                    await DisconnectAsync();

                    return false;
                }
            });
        }

        public async Task DisconnectAsync()
        {
            try
            {
                if (service != null)
                {
                    if (service.State == CommunicationState.Opened)
                        await service.DetachClientAsync();

                    CloseServiceConnection(service);
                    UnsubscriveFromServiceEvents(service);
                    service = null;
                }
            }
            catch (System.Exception ex)
            {
                log.WriteLine(ex);
            }
        }

        private void CloseServiceConnection(HideezServiceClient service)
        {
            if (service == null)
                return;

            Task.Run(() =>
            {
                if (service.State != CommunicationState.Faulted)
                    service.Close();
                else
                    service.Abort();
            });
        }

        private void SubscriveToServiceEvents(HideezServiceClient service)
        {
            if (service == null)
                return;

            var clientChannel = service.InnerDuplexChannel;
            clientChannel.Opened += Connected;
            clientChannel.Closed += Disconnected;
            clientChannel.Faulted += Disconnected;
        }

        private void UnsubscriveFromServiceEvents(HideezServiceClient service)
        {
            if (service == null)
                return;

            var clientChannel = service.InnerDuplexChannel;
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

        ~ServiceProxy()
        {
            Dispose(false);
        }
        #endregion
    }
}
