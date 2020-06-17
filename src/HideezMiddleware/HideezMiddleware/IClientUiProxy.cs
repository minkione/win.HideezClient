using System;
using System.Threading;
using System.Threading.Tasks;

namespace HideezMiddleware
{
    public enum BluetoothStatus
    {
        Ok,
        Unknown,
        Resetting,
        Unsupported,
        Unauthorized,
        PoweredOff,
    }

    public enum RfidStatus
    {
        Ok,
        RfidServiceNotConnected,
        RfidReaderNotConnected,
        Disabled,
    }

    public enum HesStatus
    {
        Ok,
        HesNotConnected,
        Disabled,
        NotApproved,
    }

    public class PinReceivedEventArgs : EventArgs
    {
        public string DeviceId { get; set; }
        public string Pin { get; set; }
        public string OldPin { get; set; }
    }

    public class ActivationCodeReceivedEventArgs : EventArgs
    {
        public string DeviceId { get; set; }
        public byte[] Code { get; set; }
    }

    public interface IClientUiProxy
    {
        event EventHandler<EventArgs> ClientConnected;
        event EventHandler<PinReceivedEventArgs> PinReceived;
        event EventHandler<EventArgs> PinCancelled;
        event EventHandler<ActivationCodeReceivedEventArgs> ActivationCodeReceived;
        event EventHandler<EventArgs> ActivationCodeCancelled;

        bool IsConnected { get; }

        Task ShowPinUi(string deviceId, bool withConfirm = false, bool askOldPin = false);
        Task ShowButtonConfirmUi(string deviceId);
        Task HidePinUi();
        Task ShowActivationCodeUi(string deviceId);
        Task HideActivationCodeUi();


        Task SendStatus(HesStatus hesStatus, HesStatus tbHesStatus, RfidStatus rfidStatus, BluetoothStatus bluetoothStatus);
        Task SendError(string message, string notificationId);
        Task SendNotification(string message, string notificationId);
    }

    public interface IClientUiManager
    {
        event EventHandler<EventArgs> ClientConnected;

        bool IsConnected { get; }

        Task<string> GetPin(string deviceId, int timeout, CancellationToken ct, bool withConfirm = false, bool askOldPin = false);
        Task ShowButtonConfirmUi(string deviceId);
        Task HidePinUi();
        Task<byte[]> GetActivationCode(string deviceId, int timeout, CancellationToken ct);
        Task HideActivationCodeUi();

        Task SendStatus(HesStatus hesStatus, HesStatus tbHesStatus, RfidStatus rfidStatus, BluetoothStatus bluetoothStatus);
        Task SendError(string message, string notificationId = null);
        Task SendNotification(string message, string notificationId = null);
    }
}