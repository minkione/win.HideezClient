using GalaSoft.MvvmLight.Messaging;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Remote;
using HideezSafe.HideezServiceReference;
using HideezSafe.Messages.Remote;
using HideezSafe.Modules.ServiceProxy;
using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace HideezSafe.Modules
{
    class RemoteDeviceConnection : IRemoteCommands, IRemoteEvents
    {
        readonly IServiceProxy _serviceProxy;
        readonly IMessenger _messenger;

        public RemoteDeviceConnection(IServiceProxy serviceProxy, IMessenger messenger)
        {
            _serviceProxy = serviceProxy;
            _messenger = messenger;

            _messenger.Register<Remote_RssiReceivedMessage>(this, OnRssiReceivedMessage);
            _messenger.Register<Remote_BatteryChangedMessage>(this, OnBatteryChangedMessage);
            _messenger.Register<Remote_StorageModifiedMessage>(this, OnStorageModified);
        }

        // Temporary duct tape, until IRemoteDeviceConnection is refactored
        public RemoteDevice RemoteDevice { get; set; }

        public event EventHandler<double> RssiReceived;

        public event EventHandler<int> BatteryChanged;

        public event EventHandler StorageModified;

        public async Task ResetChannel()
        {
            try
            {
                await _serviceProxy.GetService().RemoteConnection_ResetChannelAsync(RemoteDevice.DeviceId);
            }
            catch (FaultException<HideezServiceFault> ex)
            {
                throw ex.InnerException;
            }
        }

        public async Task SendAuthCommand(byte[] data)
        {
            try
            {
                var response = await _serviceProxy.GetService().RemoteConnection_AuthCommandAsync(RemoteDevice.DeviceId, data);
                RemoteDevice.OnAuthResponse(response);
            }
            catch (FaultException<HideezServiceFault> ex)
            {
                throw ex.InnerException;
            }

        }

        public async Task SendRemoteCommand(byte[] data)
        {
            try
            {
                var response = await _serviceProxy.GetService().RemoteConnection_RemoteCommandAsync(RemoteDevice.DeviceId, data);
                RemoteDevice.OnCommandResponse(response);
            }
            catch (FaultException<HideezServiceFault> ex)
            {
                throw ex.InnerException;
            }
        }

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
