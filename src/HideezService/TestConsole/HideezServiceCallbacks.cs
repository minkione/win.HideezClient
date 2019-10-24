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

        public void DeviceFinishedMainFlow(DeviceDTO device)
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

        public void RemoteConnection_StorageModified(string serialNo)
        {
        }

        public void ServiceComponentsStateChanged(bool hesConnected, bool showHesStatus, bool rfidConnected, bool showRfidStatus, bool bleConnected)
        {
        }

        public void ServiceErrorReceived(string error, string notificationId)
        {
        }

        public void ServiceNotificationReceived(string message, string notificationId)
        {
        }

        public void ShowPinUi(string deviceId, bool withConfirm, bool askOldPin)
        {
        }

        public void RemoteConnection_DeviceStateChanged(string deviceId, DeviceStateDTO stateDto)
        {
        }

        public void ShowButtonConfirmUi(string deviceId)
        {
        }

        public void DeviceOperationCancelled(DeviceDTO device)
        {
        }

        public void DeviceProximityChanged(string deviceId, double proximity)
        {
        }

        public void DeviceBatteryChanged(string deviceId, int battery)
        {
        }
    }
}
