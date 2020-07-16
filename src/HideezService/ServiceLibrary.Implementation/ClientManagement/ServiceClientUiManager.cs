using Hideez.SDK.Communication.Log;
using HideezMiddleware;
using HideezMiddleware.IPC.Messages;
using Meta.Lib.Modules.PubSub;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceLibrary.Implementation.ClientManagement
{
    class ServiceClientUiManager : IClientUiProxy, IDisposable
    {
        readonly ServiceClientSessionManager _clientSessionManager;
        readonly IMetaPubSub _messenger;

        public event EventHandler<EventArgs> ClientConnected;
        public event EventHandler<PinReceivedEventArgs> PinReceived;
        public event EventHandler<EventArgs> PinCancelled;
        public event EventHandler<ActivationCodeEventArgs> ActivationCodeReceived;
        public event EventHandler<ActivationCodeEventArgs> ActivationCodeCancelled;
        public bool IsConnected
        {
            get
            {
                // Every other connection type does not have or utilize UI
                return _clientSessionManager.Sessions.Any(s => s.ClientType == ClientType.DesktopClient);
            }
        }

        public ServiceClientUiManager(ServiceClientSessionManager clientSessionManager, IMetaPubSub messenger)
        {
            _messenger = messenger;
            _clientSessionManager = clientSessionManager;

            _clientSessionManager.SessionAdded += ClientSessionManager_SessionAdded;
        }

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed = false;
        void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                _clientSessionManager.SessionAdded -= ClientSessionManager_SessionAdded;
            }

            disposed = true;
        }

        ~ServiceClientUiManager()
        {
            Dispose(false);
        }
        #endregion

        void ClientSessionManager_SessionAdded(object sender, ServiceClientSession e)
        {
            ClientConnected?.Invoke(this, EventArgs.Empty);
        }

        public async Task ShowPinUi(string deviceId, bool withConfirm = false, bool askOldPin = false)
        {
            try
            {
                await _messenger.Publish(new ShowPinUiMessage(deviceId, withConfirm, askOldPin));
            }
            catch (Exception) { }
        }

        public async Task ShowButtonConfirmUi(string deviceId)
        {
            await _messenger.Publish(new ShowButtonConfirmUiMessage(deviceId));
        }

        public async Task HidePinUi()
        {
            await _messenger.Publish(new HidePinUiMessage());
        }

        public async Task ShowActivationCodeUi(string deviceId)
        {
            await _messenger.Publish(new ShowActivationCodeUiMessage(deviceId));
        }

        public async Task HideActivationCodeUi()
        {
            await _messenger.Publish(new HideActivationCodeUi());
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
            await _messenger.Publish(new UserErrorMessage(notificationId, message));
        }

        public async Task SendNotification(string message, string notificationId)
        {
            await _messenger.Publish(new UserNotificationMessage(notificationId, message));
        }

        public async Task SendStatus(HesStatus hesStatus, HesStatus tbHesStatus, RfidStatus rfidStatus, BluetoothStatus bluetoothStatus)
        {
            await _messenger.Publish(new ServiceComponentsStateChangedMessage(hesStatus, rfidStatus, bluetoothStatus, tbHesStatus));
        }

    }
}
