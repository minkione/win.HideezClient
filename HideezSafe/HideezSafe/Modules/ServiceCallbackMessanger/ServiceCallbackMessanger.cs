using NLog;
using System.Text;
using HideezSafe.Messages;
using GalaSoft.MvvmLight.Messaging;
using HideezSafe.HideezServiceReference;

namespace HideezSafe.Modules.ServiceCallbackMessanger
{
    class ServiceCallbackMessanger : IHideezServiceCallback
    {
        // Todo: Implement callback events through MvvmLight messanger 
        private readonly IMessenger messenger;
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        
        public ServiceCallbackMessanger(IMessenger messenger)
        {
            this.messenger = messenger;
        }

        public void DongleConnectionStateChanged(bool isConnected)
        {
            messenger.Send(new ConnectionDongleChangedMessage(isConnected));
            log.Info($"Dongle connection state changed: {isConnected}");
        }

        public void HESConnectionStateChanged(bool isConnected)
        {
            messenger.Send(new ConnectionHESChangedMessage(isConnected));
            log.Info($"HES connection state changed: {isConnected}");
        }

        public void RFIDConnectionStateChanged(bool isConnected)
        {
            messenger.Send(new ConnectionRFIDChangedMessage(isConnected));
            log.Info($"RFID connection state changed: {isConnected}");
        }

        public void PairedDevicesCollectionChanged(DeviceDTO[] devices)
        {
            messenger.Send(new PairedDevicesCollectionChangedMessage(devices));
            StringBuilder devicesInfo = new StringBuilder();
            foreach (var device in devices)
            {
                devicesInfo.Append(GetDeviceInfo(device)).Append(". ");
            }
            log.Info($"Paired devices collection changed. {devicesInfo}");
        }

        public void PairedDevicePropertyChanged(DeviceDTO device)
        {
            messenger.Send(new DevicePropertiesUpdatedMessage(device));
            log.Info($"Paired device property changed. {GetDeviceInfo(device)}");
        }

        private string GetDeviceInfo(DeviceDTO device)
        {
            return $"Id: {device.Id}, Name: {device.Name}, Owner: {device.Owner}, Proximity: {device.Proximity}, IsConnected: {device.IsConnected}";
        }

        public void ProximityChanged(string deviceId, double proximity)
        {
            messenger.Send(new DeviceProximityChangedMessage(deviceId, proximity));
            log.Info($"Device proximity changed. DeviceId: {deviceId}, Proximity: {proximity}");
        }

        public void LockWorkstationRequest()
        {
            messenger.Send(new LockWorkstationMessage());
            log.Info($"Lock workstation request");
        }

        public void ActivateWorkstationScreenRequest()
        {
            messenger.Send(new ActivateScreenMessage());
            log.Info($"Activate screen request");
        }
    }
}
