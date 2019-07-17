using HideezServiceHost.HideezServiceReference;

namespace HideezServiceHost
{
    class HideezServiceCallbacks : IHideezServiceCallback
    {
        // All callbacks in HideezServiceHost can be left empty / not implemented
        // The service host connects to the service briefly to initialize the primary library
        // After initialization the connection is closed

        // If new callback is added to interface, create empty implementation without any logic

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

        public void DongleConnectionStateChanged(bool isConnected)
        {
        }

        public void HESConnectionStateChanged(bool isConnected)
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

        public void RFIDConnectionStateChanged(bool isConnected)
        {
        }
    }
}
