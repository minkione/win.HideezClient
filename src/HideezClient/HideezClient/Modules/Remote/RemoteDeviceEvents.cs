using GalaSoft.MvvmLight.Messaging;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Device;
using Hideez.SDK.Communication.Remote;
using HideezClient.Messages.Remote;
using System;

namespace HideezClient.Modules.Remote
{
    class RemoteDeviceEvents : IRemoteEvents
    {
        readonly IMessenger _messenger;

        public RemoteDeviceEvents(IMessenger messenger)
        {
            _messenger = messenger;

            _messenger.Register<Remote_DeviceStateChangedMessage>(this, OnDeviceStateChanged);
        }

        public event EventHandler<DeviceState> DeviceStateChanged;

        // Todo: fix cyclic dependency between RemoteDevice and RemoteCommands/RemoteEvents
        public RemoteDevice RemoteDevice { get; set; }

        void OnDeviceStateChanged(Remote_DeviceStateChangedMessage msg)
        {
            if (IsMessageFromCurrentDevice(msg))
                DeviceStateChanged?.Invoke(this, msg.State);
        }

        bool IsMessageFromCurrentDevice(Remote_BaseMessage msg)
        {
            if (string.IsNullOrWhiteSpace(msg.Id))
                return false;

            if (RemoteDevice == null)
                return false;

            return msg.Id == RemoteDevice.Id;
        }
    }
}
