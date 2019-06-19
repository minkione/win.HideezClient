using GalaSoft.MvvmLight.Messaging;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Remote;
using HideezSafe.Messages.Remote;
using HideezSafe.Modules.ServiceProxy;
using System;
using System.Threading.Tasks;

namespace HideezSafe.Modules
{
    class RemoteDeviceConnection : IRemoteDeviceConnection
    {
        readonly IServiceProxy _serviceProxy;
        readonly IMessenger _messenger;

        public RemoteDeviceConnection(IServiceProxy serviceProxy, IMessenger messenger)
        {
            _serviceProxy = serviceProxy;
            _messenger = messenger;

            _messenger.Register<Remote_RssiReceivedMessage>(this, OnRssiReceivedMessage);
            _messenger.Register<Remote_BatteryChangedMessage>(this, OnBatteryChangedMessage);
        }

        // Temporary duct tape, until IRemoteDeviceConnection is refactored
        public RemoteDevice RemoteDevice { get; set; }

        public event EventHandler<double> RssiReceived;

        public event EventHandler<int> BatteryChanged;

        public async Task ResetChannel()
        {
            await _serviceProxy.GetService().RemoteConnection_ResetChannelAsync(RemoteDevice.DeviceId);
        }

        public async Task SendAuthCommand(byte[] data)
        {
            var response = await _serviceProxy.GetService().RemoteConnection_AuthCommandAsync(RemoteDevice.DeviceId, data);
            RemoteDevice.OnAuthResponse(response);
        }

        public async Task SendRemoteCommand(byte[] data)
        {
            var response = await _serviceProxy.GetService().RemoteConnection_RemoteCommandAsync(RemoteDevice.DeviceId, data);
            RemoteDevice.OnCommandResponse(response);
        }

        void OnRssiReceivedMessage(Remote_RssiReceivedMessage msg)
        {
            if (string.IsNullOrWhiteSpace(msg.SerialNo))
                return;

            if (msg.SerialNo == RemoteDevice.SerialNo)
                RssiReceived?.Invoke(this, msg.Rssi);
        }

        void OnBatteryChangedMessage(Remote_BatteryChangedMessage msg)
        {
            if (string.IsNullOrWhiteSpace(msg.SerialNo))
                return;

            if (msg.SerialNo == RemoteDevice.SerialNo)
                BatteryChanged?.Invoke(this, msg.Battery);
        }
    }
}
