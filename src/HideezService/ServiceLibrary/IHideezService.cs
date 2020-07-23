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
        void SetProximitySettings(string mac, int lockProximity, int unlockProximity);
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
