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
            log.Info($"Device ({device.Id}) connection state changed to: {device.IsConnected}");
            messenger.Send(new DeviceConnectionStateChangedMessage(device));
        }

        public void DeviceInitialized(DeviceDTO device)
        {
            log.Info($"Device ({device.Id}) is initialized");
            messenger.Send(new DeviceInitializedMessage(device));
        }

        public void RemoteConnection_RssiReceived(string serialNo, double rssi)
        {
            // to many messages are printed into log due to the line bellow
            //log.Info($"Remote ({connectionId}) rssi received ({rssi})");
            messenger.Send(new Remote_RssiReceivedMessage(serialNo, rssi));
        }

        public void RemoteConnection_BatteryChanged(string serialNo, int battery)
        {
            log.Info($"Remote ({serialNo}) battery changed to {battery}");
            messenger.Send(new Remote_BatteryChangedMessage(serialNo, battery));
        }

        public void RemoteConnection_StorageModified(string serialNo)
        {
            log.Info($"Remote ({serialNo}) storage modified");
            messenger.Send(new Remote_StorageModifiedMessage(serialNo));
        }
    }
}
