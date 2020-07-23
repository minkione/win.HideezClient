using Hideez.SDK.Communication.Log;
using HideezMiddleware;
using HideezMiddleware.IPC.Messages;
using Meta.Lib.Modules.PubSub;
using Meta.Lib.Modules.PubSub.Messages;
using System;
using System.Diagnostics;
using System.Linq;
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

        public async Task ShowPinUi(string deviceId, bool withConfirm = false, bool askOldPin = false)
        {
            try
            {
                await _metaMessenger.Publish(new ShowPinUiMessage(deviceId, withConfirm, askOldPin));
            }
            catch (Exception) { }
        }

        public async Task ShowButtonConfirmUi(string deviceId)
        {
            await _metaMessenger.Publish(new ShowButtonConfirmUiMessage(deviceId));
        }

        public async Task HidePinUi()
        {
            await _metaMessenger.Publish(new HidePinUiMessage());
        }

        public async Task ShowActivationCodeUi(string deviceId)
        {
            await _metaMessenger.Publish(new ShowActivationCodeUiMessage(deviceId));
        }

        public async Task HideActivationCodeUi()
        {
            await _metaMessenger.Publish(new HideActivationCodeUi());
        }

        public void EnterPin(string deviceId, string pin, string oldPin = "")
        {
            try
            {
                var args = new PinReceivedEventArgs()
                {
                    DeviceId = deviceId,
                    Pin = pin,
                    OldPin = oldPin,
                };

                PinReceived?.Invoke(this, args);
            }
            catch (Exception) { }
        }

        public void CancelPin()
        {
            try
            {
                PinCancelled?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception) { }
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

        public async Task SendStatus(HesStatus hesStatus, HesStatus tbHesStatus, RfidStatus rfidStatus, BluetoothStatus bluetoothStatus)
        {
            await _metaMessenger.Publish(new ServiceComponentsStateChangedMessage(hesStatus, rfidStatus, bluetoothStatus, tbHesStatus));
        }

    }
}
