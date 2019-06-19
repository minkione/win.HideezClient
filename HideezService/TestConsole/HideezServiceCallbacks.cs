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

        public void DongleConnectionStateChanged(bool isConnected)
        {
        }

        public void HESConnectionStateChanged(bool isConnected)
        {
        }

        public void LockWorkstationRequest()
        {
        }

        public void RemoteConnection_BatteryChanged(string connectioNid, int battery)
        {
        }

        public void RemoteConnection_RssiReceived(string connectionId, double rssi)
        {
        }

        public void RFIDConnectionStateChanged(bool isConnected)
        {
        }
    }
}
