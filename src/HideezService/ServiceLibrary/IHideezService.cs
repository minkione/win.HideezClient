using HideezMiddleware;
using HideezMiddleware.IPC.DTO;
using System.ServiceModel;
using System.Threading.Tasks;

namespace ServiceLibrary
{
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(ICallbacks))]
    public interface IHideezService
    {
        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        bool AttachClient(ServiceClientParameters parameters); // +

        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        void DetachClient(); // --

        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        int Ping(); // --

        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        void Shutdown(); // --

        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        DeviceDTO[] GetDevices(); // +

        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        byte[] GetAvailableChannels(string serialNo); // +

        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        Task DisconnectDevice(string id); // +

        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        Task RemoveDeviceAsync(string id); // +

        // Remote device connection
        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        Task<string> EstablishRemoteDeviceConnection(string serialNo, byte channelNo); // +

        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        Task<byte[]> RemoteConnection_VerifyCommandAsync(string connectionId, byte[] data); // +

        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        Task<byte[]> RemoteConnection_RemoteCommandAsync(string connectionId, byte[] data); // +

        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        Task RemoteConnection_ResetChannelAsync(string connectionId); // +

        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        void PublishEvent(WorkstationEventDTO workstationEvent); // +

        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        void SendPin(string deviceId, byte[] pin, byte[] oldPin); // +

        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        void CancelPin(string deviceId); // +

        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        void SendActivationCode(string deviceId, byte[] activationCode); // +

        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        void CancelActivationCode(string deviceId); // +

        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        void SetProximitySettings(string mac, int lockProximity, int unlockProximity); // --

        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        ProximitySettingsDTO GetCurrentProximitySettings(string mac); // --

        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        string GetServerAddress(); // +

        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        Task<bool> ChangeServerAddress(string address); // +

        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        bool IsSoftwareVaultUnlockModuleEnabled(); // +

        [OperationContract]
        [FaultContract(typeof(HideezServiceFault))]
        void SetSoftwareVaultUnlockModuleState(bool enabled); // +
    }

    public interface ICallbacks
    {
    }

    public enum Adapter
    {
        HES,
        RFID,
        Dongle,
    }
}
