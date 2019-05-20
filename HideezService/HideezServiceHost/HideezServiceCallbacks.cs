using HideezServiceHost.HideezServiceReference;

namespace HideezServiceHost
{
    class HideezServiceCallbacks : IHideezServiceCallback
    {
        // All callbacks in HideezServiceHost can be left empty / not implemented
        // The service host connects to the service briefly to initialize the primary library
        // After initialization the connection is closed

        // If new callback is added to interface, create empty implementation without any logic

        public void LockWorkstationRequest()
        {
        }

        public void DongleConnectionStateChanged(bool isConnected)
        {
        }

        public void HESConnectionStateChanged(bool isConnected)
        {
        }

        public void RFIDConnectionStateChanged(bool isConnected)
        {
        }

        public void PairedDevicePropertyChanged(DeviceDTO device)
        {
        }

        public void PairedDevicesCollectionChanged(DeviceDTO[] devices)
        {
        }

        public void ProximityChanged(string deviceId, double proximity)
        {
        }
    }
}
