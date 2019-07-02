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
        bool GetAdapterState(Adapter adapter);

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
        Task<byte[]> RemoteConnection_AuthCommandAsync(string connectionId, byte[] data);

        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        Task<byte[]> RemoteConnection_RemoteCommandAsync(string connectionId, byte[] data);

        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        Task RemoteConnection_ResetChannelAsync(string connectionId);
    }

    public interface ICallbacks
    {
        [OperationContract(IsOneWay = true)]
        void LockWorkstationRequest();

        [OperationContract(IsOneWay = true)]
        void ActivateWorkstationScreenRequest();


        [OperationContract(IsOneWay = true)]
        void HESConnectionStateChanged(bool isConnected);

        [OperationContract(IsOneWay = true)]
        void RFIDConnectionStateChanged(bool isConnected);

        [OperationContract(IsOneWay = true)]
        void DongleConnectionStateChanged(bool isConnected);


        [OperationContract(IsOneWay = true)]
        void DevicesCollectionChanged(DeviceDTO[] devices);

        [OperationContract(IsOneWay = true)]
        void DeviceConnectionStateChanged(DeviceDTO device);

        [OperationContract(IsOneWay = true)]
        void DeviceInitialized(DeviceDTO device);

        [OperationContract(IsOneWay = true)]
        void RemoteConnection_RssiReceived(string serialNo, double rssi);

        [OperationContract(IsOneWay = true)]
        void RemoteConnection_BatteryChanged(string serialNo, int battery);

        [OperationContract(IsOneWay = true)]
        void RemoteConnection_StorageModified(string serialNo);
    }

    public enum Adapter
    {
        HES,
        RFID,
        Dongle,
    }
}
