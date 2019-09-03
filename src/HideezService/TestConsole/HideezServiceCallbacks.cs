using TestConsole.HideezServiceReference;

namespace TestConsole
{
    class HideezServiceCallbacks : IHideezServiceCallback
    {
        public void ActivateWorkstationScreenRequest()
        {
        }

        public void DeviceConnectionStateChanged(DeviceDTO device)
        {
        }

        public void DeviceInitialized(DeviceDTO device)
        {
        }

        public void DevicesCollectionChanged(DeviceDTO[] devices)
        {
        }

        public void HidePinUi()
        {
        }

        public void LockWorkstationRequest()
        {
        }

        public void RemoteConnection_BatteryChanged(string serialNo, int battery)
        {
        }

        public void RemoteConnection_RssiReceived(string serialNo, double rssi)
        {
        }

        public void RemoteConnection_StorageModified(string serialNo)
        {
        }

        public void ServiceComponentsStateChanged(bool hesConnected, bool showHesStatus, bool rfidConnected, bool showRfidStatus, bool bleConnected)
        {
        }

        public void ServiceErrorReceived(string error)
        {
        }

        public void ServiceNotificationReceived(string message)
        {
        }

        public void ShowPinUi(string deviceId, bool withConfirm, bool askOldPin)
        {
        }
    }
}
