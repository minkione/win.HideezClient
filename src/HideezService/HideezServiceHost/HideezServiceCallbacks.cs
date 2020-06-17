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

        public void ServiceComponentsStateChanged(HesStatus hesStatus, RfidStatus rfidStatus, BluetoothStatus bluetoothStatus, HesStatus tbHesStatus)
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

        public void ProximitySettingsChanged()
        {
        }

        public void DeviceProximityLockEnabled(DeviceDTO device)
        {
        }

        public void ShowActivationCodeUi(string deviceId)
        {
        }

        public void HideActivationCodeUi()
        {
        }
    }
}
