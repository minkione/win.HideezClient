using HideezMiddleware;
using HideezMiddleware.IPC.Messages;
using Meta.Lib.Modules.PubSub;
using System;
using System.Threading.Tasks;

namespace ServiceLibrary.Implementation.ClientManagement
{
    class ServiceClientUiManager : IClientUiProxy
    {
        readonly IMetaPubSub _metaMessenger;

        public event EventHandler<PinReceivedEventArgs> PinReceived;
        public event EventHandler<EventArgs> PinCancelled;
        public event EventHandler<ActivationCodeEventArgs> ActivationCodeReceived;
        public event EventHandler<ActivationCodeEventArgs> ActivationCodeCancelled;

        public ServiceClientUiManager(IMetaPubSub metaMessenger)
        {
            _metaMessenger = metaMessenger;
        }

        public Task ShowPinUi(string deviceId, bool withConfirm = false, bool askOldPin = false)
        {
            return Task.CompletedTask; // Not implemented for client UI
        }

        public Task ShowButtonConfirmUi(string deviceId)
        {
            return Task.CompletedTask; // Not implemented for client UI
        }

        public Task HidePinUi()
        {
            return Task.CompletedTask; // Not implemented for client UI
        }

        public async Task ShowActivationCodeUi(string deviceId)
        {
            await _metaMessenger.Publish(new ShowActivationCodeUiMessage(deviceId));
        }

        public async Task HideActivationCodeUi()
        {
            await _metaMessenger.Publish(new HideActivationCodeUi());
        }

        public void EnterActivationCode(string deviceId, byte[] code) // Todo:
        {
            try
            {
                var args = new ActivationCodeEventArgs()
                {
                    DeviceId = deviceId,
                    Code = code,
                };

                ActivationCodeReceived?.Invoke(this, args);
            }
            catch (Exception) { }
        }

        public void CancelActivationCode(string deviceId) // Todo:
        {
            try
            {
                var args = new ActivationCodeEventArgs() { DeviceId = deviceId };
                ActivationCodeCancelled?.Invoke(this, args);
            }
            catch (Exception) { }
        }

        public async Task SendError(string message, string notificationId)
        {
            await _metaMessenger.Publish(new UserErrorMessage(notificationId, message));
        }

        public async Task SendNotification(string message, string notificationId)
        {
            await _metaMessenger.Publish(new UserNotificationMessage(notificationId, message));
        }

        public async Task SendStatus(HesStatus hesStatus, RfidStatus rfidStatus, BluetoothStatus dongleStatus, BluetoothStatus bluetoothStatus, HesStatus tbHesStatus)
        {
            await _metaMessenger.Publish(new ServiceComponentsStateChangedMessage(hesStatus, rfidStatus, dongleStatus, tbHesStatus, bluetoothStatus));
        }

    }
}
