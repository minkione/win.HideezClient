using System;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;
using Hideez.SDK.Communication.Log;
using HideezClient.Messages;
using HideezClient.Modules.Log;
using Meta.Lib.Modules.PubSub;
using Meta.Lib.Modules.PubSub.Messages;

namespace HideezClient.Modules.ServiceProxy
{
    // Todo: Add lock for "service" to improve thread safety
    class ServiceProxy : IServiceProxy
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger(nameof(ServiceProxy));
        private readonly IMessenger messenger;
        private readonly IMetaPubSub _metaMessenger;

        public bool IsConnected { get; private set; }

        public ServiceProxy(IMessenger messenger, IMetaPubSub metaMessenger)
        {
            this.messenger = messenger;
            _metaMessenger = metaMessenger;

            messenger.Register<SendPinMessage>(this, OnSendPinMessage);
            messenger.Register<SendActivationCodeMessage>(this, OnSendActivationCodeMessage);
            messenger.Register<CancelActivationCodeEntryMessage>(this, OnCancelActivationCodeMessage);

            _metaMessenger.Subscribe<ConnectedToServerEvent>(OnConnected, null);
            _metaMessenger.Subscribe<DisconnectedFromServerEvent>(OnDisconnected, null);

        }

        Task OnConnected(ConnectedToServerEvent arg)
        {
            IsConnected = true;
            ServiceProxy_ConnectionChanged();
            return Task.CompletedTask;
        }

        Task OnDisconnected(DisconnectedFromServerEvent arg)
        {
            IsConnected = false;
            ServiceProxy_ConnectionChanged();
            return Task.CompletedTask;
        }

        void ServiceProxy_ConnectionChanged()
        {
            messenger.Send(new ConnectionServiceChangedMessage(IsConnected));
        }

        async void OnSendPinMessage(SendPinMessage obj)
        {
            try
            {
                await _metaMessenger.PublishOnServer(new HideezMiddleware.IPC.IncommingMessages.SendPinMessage(obj.DeviceId, obj.Pin ?? new byte[0], obj.OldPin ?? new byte[0]));
            }
            catch (Exception ex)
            {
                log.WriteLine(ex);
            }
        }

        async void OnSendActivationCodeMessage(SendActivationCodeMessage obj)
        {
            try
            {
                await _metaMessenger.PublishOnServer(new HideezMiddleware.IPC.IncommingMessages.SendActivationCodeMessage(obj.DeviceId, obj.Code));
            }
            catch (Exception ex)
            {
                log.WriteLine(ex);
            }
        }

        async void OnCancelActivationCodeMessage(CancelActivationCodeEntryMessage obj)
        {
            {
                try
                {
                    await _metaMessenger.PublishOnServer(new HideezMiddleware.IPC.IncommingMessages.CancelActivationCodeMessage(obj.DeviceId));
                }
                catch (Exception ex)
                {
                    log.WriteLine(ex);
                }
            }
        }

    }
}
