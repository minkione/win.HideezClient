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
        private readonly IMetaPubSub _metaMessenger;

        public bool IsConnected { get; private set; }

        public ServiceProxy(IMetaPubSub metaMessenger)
        {
            _metaMessenger = metaMessenger;

            _metaMessenger.Subscribe<SendActivationCodeMessage>(OnSendActivationCodeMessage);
            _metaMessenger.Subscribe<CancelActivationCodeEntryMessage>(OnCancelActivationCodeMessage);

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
            _metaMessenger.Publish(new ConnectionServiceChangedMessage(IsConnected));
        }

        async Task OnSendActivationCodeMessage(SendActivationCodeMessage obj)
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

        async Task OnCancelActivationCodeMessage(CancelActivationCodeEntryMessage obj)
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
