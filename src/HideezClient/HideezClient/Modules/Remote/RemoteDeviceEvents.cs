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

            _messenger.Register<Remote_RssiReceivedMessage>(this, OnRssiReceivedMessage);
            _messenger.Register<Remote_BatteryChangedMessage>(this, OnBatteryChangedMessage);
            _messenger.Register<Remote_StorageModifiedMessage>(this, OnStorageModified);
        }

        public event EventHandler<double> RssiReceived;
        public event EventHandler<int> BatteryChanged;
        public event EventHandler StorageModified;

        // Temporary duct tape, until IRemoteDeviceConnection is refactored
        public RemoteDevice RemoteDevice { get; set; }

        void OnRssiReceivedMessage(Remote_RssiReceivedMessage msg)
        {
            if (IsMessageFromCurrentDevice(msg))
                RssiReceived?.Invoke(this, msg.Rssi);
        }

        void OnBatteryChangedMessage(Remote_BatteryChangedMessage msg)
        {
            if (IsMessageFromCurrentDevice(msg))
                BatteryChanged?.Invoke(this, msg.Battery);
        }

        void OnStorageModified(Remote_StorageModifiedMessage msg)
        {
            if (IsMessageFromCurrentDevice(msg))
                StorageModified?.Invoke(this, EventArgs.Empty);
        }

        bool IsMessageFromCurrentDevice(Remote_BaseMessage msg)
        {
            if (string.IsNullOrWhiteSpace(msg.SerialNo))
                return false;

            if (RemoteDevice == null)
                return false;

            return msg.SerialNo == RemoteDevice.SerialNo;
        }
    }
}
