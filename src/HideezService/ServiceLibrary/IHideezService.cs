using Hideez.SDK.Communication.Interfaces;
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
        void DisconnectDevice(string id);

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
        void CancelPin();
    }

    public interface ICallbacks
    {
        [OperationContract(IsOneWay = true)]
        void LockWorkstationRequest();

        [OperationContract(IsOneWay = true)]
        void ActivateWorkstationScreenRequest();


        [OperationContract(IsOneWay = true)]
        void ServiceComponentsStateChanged(bool hesConnected, bool showHesStatus, bool rfidConnected, bool showRfidStatus, bool bleConnected);

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
        void RemoteConnection_DeviceStateChanged(string deviceId, DeviceStateDTO stateDto);

        [OperationContract(IsOneWay = true)]
        void ShowPinUi(string deviceId, bool withConfirm = false, bool askOldPin = false);

        [OperationContract(IsOneWay = true)]
        void ShowButtonConfirmUi(string deviceId);

        [OperationContract(IsOneWay = true)]
        void HidePinUi();

    }

    public enum Adapter
    {
        HES,
        RFID,
        Dongle,
    }
}
