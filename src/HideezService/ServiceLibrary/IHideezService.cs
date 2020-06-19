using Hideez.SDK.Communication.Interfaces;
using HideezMiddleware;
using System.Diagnostics.Eventing.Reader;
using System.ServiceModel;
using System.Threading.Tasks;

namespace ServiceLibrary
{
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(ICallbacks))]
    public interface IHideezService
    {
        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        bool AttachClient(ServiceClientParameters parameters);

        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        void DetachClient();

        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        int Ping();

        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        void Shutdown();

        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        DeviceDTO[] GetDevices();

        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        byte[] GetAvailableChannels(string serialNo);

        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        Task DisconnectDevice(string id);

        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        Task RemoveDeviceAsync(string id);

        // Remote device connection
        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        Task<string> EstablishRemoteDeviceConnection(string serialNo, byte channelNo);

        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        Task<byte[]> RemoteConnection_VerifyCommandAsync(string connectionId, byte[] data);

        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        Task<byte[]> RemoteConnection_RemoteCommandAsync(string connectionId, byte[] data);

        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        Task RemoteConnection_ResetChannelAsync(string connectionId);

        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        void PublishEvent(WorkstationEventDTO workstationEvent);

        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        void SendPin(string deviceId, byte[] pin, byte[] oldPin);

        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        void CancelPin(string deviceId);

        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        void SendActivationCode(string deviceId, byte[] activationCode);

        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        void CancelActivationCode(string deviceId);

        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        void SetProximitySettings(string mac, int lockProximity, int unlockProximity);

        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        ProximitySettingsDTO GetCurrentProximitySettings(string mac);

        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        string GetServerAddress();

        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        Task<bool> ChangeServerAddress(string address);

        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        bool IsSoftwareVaultUnlockModuleEnabled();

        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        void SetSoftwareVaultUnlockModuleState(bool enabled);
    }

    public interface ICallbacks
    {
        [OperationContract(IsOneWay = true)]
        void ProximitySettingsChanged();

        [OperationContract(IsOneWay = true)]
        void LockWorkstationRequest();

        [OperationContract(IsOneWay = true)]
        void ActivateWorkstationScreenRequest();

        [OperationContract(IsOneWay = true)]
        void ServiceComponentsStateChanged(HesStatus hesStatus, RfidStatus rfidStatus, BluetoothStatus bluetoothStatus, HesStatus tbHesStatus);
        
        [OperationContract(IsOneWay = true)]
        void ServiceNotificationReceived(string message, string notificationId);

        [OperationContract(IsOneWay = true)]
        void ServiceErrorReceived(string error, string notificationId);



        [OperationContract(IsOneWay = true)]
        void DevicesCollectionChanged(DeviceDTO[] devices);

        [OperationContract(IsOneWay = true)]
        void DeviceConnectionStateChanged(DeviceDTO device);

        [OperationContract(IsOneWay = true)]
        void DeviceInitialized(DeviceDTO device);

        [OperationContract(IsOneWay = true)]
        void DeviceFinishedMainFlow(DeviceDTO device);

        [OperationContract(IsOneWay = true)]
        void DeviceOperationCancelled(DeviceDTO device);

        [OperationContract(IsOneWay = true)]
        void DeviceProximityChanged(string deviceId, double proximity);

        [OperationContract(IsOneWay = true)]
        void DeviceBatteryChanged(string deviceId, int battery);

        [OperationContract(IsOneWay = true)]
        void DeviceProximityLockEnabled(DeviceDTO device);

        [OperationContract(IsOneWay = true)]
        void RemoteConnection_DeviceStateChanged(string deviceId, DeviceStateDTO stateDto);

        // Todo: Simplify the whole back and forth messaging.
        // Maybe change the detailed interface to a more generalized one with just two methods, send receive
        [OperationContract(IsOneWay = true)]
        void ShowPinUi(string deviceId, bool withConfirm = false, bool askOldPin = false);

        [OperationContract(IsOneWay = true)]
        void ShowButtonConfirmUi(string deviceId);

        [OperationContract(IsOneWay = true)]
        void HidePinUi();

        [OperationContract(IsOneWay = true)]
        void ShowActivationCodeUi(string deviceId);

        [OperationContract(IsOneWay = true)]
        void HideActivationCodeUi();
    }

    public enum Adapter
    {
        HES,
        RFID,
        Dongle,
    }
}
