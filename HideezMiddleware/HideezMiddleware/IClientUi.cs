using System.Threading.Tasks;

namespace HideezMiddleware
{
    public interface IClientUi
    {
        bool IsConnected { get; }

        Task<string> GetPin(string deviceId, int timeout, bool withConfirm = false);
        Task HidePinUi();

        Task SendStatus(BluetoothStatus bluetoothStatus, RfidStatus rfidStatus, HesStatus hesStatus);
        //Task SendError(string message);
        //Task SendNotification(string message);
    }
}