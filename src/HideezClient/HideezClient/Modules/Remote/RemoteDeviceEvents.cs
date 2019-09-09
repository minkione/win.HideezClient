using GalaSoft.MvvmLight.Messaging;
using Hideez.SDK.Communication;
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

            _messenger.Register<Remote_StorageModifiedMessage>(this, OnStorageModified);
            _messenger.Register<Remote_SystemStateReceivedMessage>(this, OnSystemStateReceived);
        }

        public event EventHandler<EventArgs> StorageModified;
        public event EventHandler<byte[]> SystemStateReceived;

        // Todo: fix cyclic dependency between RemoteDevice and RemoteCommands/RemoteEvents
        public RemoteDevice RemoteDevice { get; set; }

        void OnStorageModified(Remote_StorageModifiedMessage msg)
        {
            if (IsMessageFromCurrentDevice(msg))
                StorageModified?.Invoke(this, EventArgs.Empty);
        }

        void OnSystemStateReceived(Remote_SystemStateReceivedMessage msg)
        {
            if (IsMessageFromCurrentDevice(msg))
                SystemStateReceived?.Invoke(this, msg.SystemStateData);
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
