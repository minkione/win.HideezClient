using NLog;
using HideezSafe.Messages;
using GalaSoft.MvvmLight.Messaging;
using HideezSafe.HideezServiceReference;
using HideezSafe.Messages.Remote;

namespace HideezSafe.Modules.ServiceCallbackMessanger
{
    class ServiceCallbackMessanger : IHideezServiceCallback
    {
        private readonly IMessenger messenger;
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        
        public ServiceCallbackMessanger(IMessenger messenger)
        {
            this.messenger = messenger;
        }

        public void ActivateWorkstationScreenRequest()
        {
            log.Info($"Activate screen request");
            messenger.Send(new ActivateScreenMessage());
        }

        public void LockWorkstationRequest()
        {
            log.Info($"Lock workstation request");
            messenger.Send(new LockWorkstationMessage());
        }

        public void DongleConnectionStateChanged(bool isConnected)
        {
            log.Info($"Dongle connection state changed: {isConnected}");
            messenger.Send(new ConnectionDongleChangedMessage(isConnected));
        }

        public void HESConnectionStateChanged(bool isConnected)
        {
            log.Info($"HES connection state changed: {isConnected}");
            messenger.Send(new ConnectionHESChangedMessage(isConnected));
        }

        public void RFIDConnectionStateChanged(bool isConnected)
        {
            log.Info($"RFID connection state changed: {isConnected}");
            messenger.Send(new ConnectionRFIDChangedMessage(isConnected));
        }

        public void DevicesCollectionChanged(DeviceDTO[] devices)
        {
            log.Info($"Paired devices collection changed. Count: {devices.Length}");
            messenger.Send(new DevicesCollectionChangedMessage(devices));
        }

        public void DeviceConnectionStateChanged(DeviceDTO device)
        {
            log.Info($"Device ({device.SerialNo}) connection state changed to: {device.IsConnected}");
            messenger.Send(new DeviceConnectionStateChangedMessage(device));
        }

        public void DeviceInitialized(DeviceDTO device)
        {
            log.Info($"Device ({device.SerialNo}) is initialized");
            messenger.Send(new DeviceInitializedMessage(device));
        }

        public void RemoteConnection_RssiReceived(string connectionId, double rssi)
        {
            // to many messages are printed into log due to the line bellow
            //log.Info($"Remote ({connectionId}) rssi received ({rssi})");
            messenger.Send(new Remote_RssiReceivedMessage(connectionId, rssi));
        }

        public void RemoteConnection_BatteryChanged(string connectionId, int battery)
        {
            log.Info($"Remote ({connectionId}) battery changed to {battery}");
            messenger.Send(new Remote_BatteryChangedMessage(connectionId, battery));
        }
    }
}
