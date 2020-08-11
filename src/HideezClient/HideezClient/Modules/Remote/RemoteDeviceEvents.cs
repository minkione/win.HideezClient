using GalaSoft.MvvmLight.Messaging;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Device;
using Hideez.SDK.Communication.Remote;
using HideezClient.Extension;
using HideezClient.Messages.Remote;
using HideezMiddleware.IPC.Messages;
using Meta.Lib.Modules.PubSub;
using System;
using System.Threading.Tasks;

namespace HideezClient.Modules.Remote
{
    class RemoteDeviceEvents : IRemoteEvents
    {
        readonly IMetaPubSub _messenger;

        public RemoteDeviceEvents(IMetaPubSub messenger)
        {
            _messenger = messenger;

            _messenger.TrySubscribeOnServer<RemoteConnection_DeviceStateChangedMessage>(OnDeviceStateChanged);
        }

        public event EventHandler<DeviceState> DeviceStateChanged;

        // Todo: fix cyclic dependency between RemoteDevice and RemoteCommands/RemoteEvents
        public RemoteDevice RemoteDevice { get; set; }

        Task OnDeviceStateChanged(RemoteConnection_DeviceStateChangedMessage msg)
        {
            if (IsMessageFromCurrentDevice(msg))
                DeviceStateChanged?.Invoke(this, msg.State.ToDeviceState());

            return Task.CompletedTask;
        }

        bool IsMessageFromCurrentDevice(RemoteConnection_DeviceStateChangedMessage msg)
        {
            if (string.IsNullOrWhiteSpace(msg.DeviceId))
                return false;

            if (RemoteDevice == null)
                return false;

            return msg.DeviceId== RemoteDevice.Id;
        }
    }
}
