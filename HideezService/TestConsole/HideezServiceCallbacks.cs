using TestConsole.HideezServiceReference;

namespace TestConsole
{
    class HideezServiceCallbacks : IHideezServiceCallback
    {
        public void LockWorkstationRequest()
        {
        }

        public void RFIDConnectionStateChanged(bool isConnected)
        {
        }

        public void DongleConnectionStateChanged(bool isConnected)
        {
        }

        public void HESConnectionStateChanged(bool isConnected)
        {
        }

        public void PairedDevicePropertyChanged(BleDeviceDTO device)
        {
        }

        public void PairedDevicesCollectionChanged(BleDeviceDTO[] devicesCollection)
        {
        }
    }
}
