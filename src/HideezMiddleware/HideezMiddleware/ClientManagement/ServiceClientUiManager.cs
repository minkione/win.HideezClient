using HideezMiddleware.IPC.IncommingMessages;
using HideezMiddleware.IPC.Messages;
using Meta.Lib.Modules.PubSub;
using System;
using System.Threading.Tasks;

namespace HideezMiddleware.ClientManagement
{
    public class ServiceClientUiManager : IClientUiProxy
    {
        readonly IMetaPubSub _messenger;

        public event EventHandler<PinReceivedEventArgs> PinReceived;
        public event EventHandler<EventArgs> PinCancelled;
        public event EventHandler<ActivationCodeEventArgs> ActivationCodeReceived;
        public event EventHandler<ActivationCodeEventArgs> ActivationCodeCancelled;
        public event EventHandler<PasswordEventArgs> PasswordReceived;

        public ServiceClientUiManager(IMetaPubSub messenger)
        {
            _messenger = messenger;

            _messenger.Subscribe<CancelActivationCodeMessage>(CancelActivationCode);
            _messenger.Subscribe<SendActivationCodeMessage>(SendActivationCode);
        }

        private Task CancelActivationCode(CancelActivationCodeMessage msg)
        {
            try
            {
                var args = new ActivationCodeEventArgs() { DeviceId = msg.DeviceId };
                ActivationCodeCancelled?.Invoke(this, args);
            }
            catch (Exception) { }

            return Task.CompletedTask;
        }

        private Task SendActivationCode(SendActivationCodeMessage msg)
        {
            try
            {
                var args = new ActivationCodeEventArgs()
                {
                    DeviceId = msg.DeviceId,
                    Code = msg.ActivationCode,
                };

                ActivationCodeReceived?.Invoke(this, args);
            }
            catch (Exception) { }

            return Task.CompletedTask;
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
            await _messenger.Publish(new ShowActivationCodeUiMessage(deviceId));
        }

        public async Task HideActivationCodeUi()
        {
            await _messenger.Publish(new HideActivationCodeUi());
        }

        public async Task SendError(string message, string notificationId)
        {
            await _messenger.Publish(new UserErrorMessage(notificationId, message));
        }

        public async Task SendNotification(string message, string notificationId)
        {
            await _messenger.Publish(new UserNotificationMessage(notificationId, message));
        }

        public async Task SendStatus(HesStatus hesStatus, RfidStatus rfidStatus, BluetoothStatus dongleStatus, BluetoothStatus bluetoothStatus, HesStatus tbHesStatus)
        {
            await _messenger.Publish(new ServiceComponentsStateChangedMessage(hesStatus, rfidStatus, dongleStatus, tbHesStatus, bluetoothStatus));
        }

        public Task ShowPasswordUi(string deviceId)
        {
            return Task.CompletedTask; // Not implemented for client UI
        }

        public Task HidePasswordUi()
        {
            return Task.CompletedTask; // Not implemented for client UI
        }
    }
}
