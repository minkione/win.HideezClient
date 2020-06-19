using Hideez.SDK.Communication.Log;
using HideezMiddleware;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceLibrary.Implementation.ClientManagement
{
    class ServiceClientUiManager : IClientUiProxy, IDisposable
    {
        readonly ServiceClientSessionManager _clientSessionManager;

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

        public ServiceClientUiManager(ServiceClientSessionManager clientSessionManager)
        {
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

        public Task ShowPinUi(string deviceId, bool withConfirm = false, bool askOldPin = false)
        {
            foreach(var session in _clientSessionManager.Sessions)
            {
                try
                {
                    session.Callbacks.ShowPinUi(deviceId, withConfirm, askOldPin);
                }
                catch (Exception) { }
            }

            return Task.CompletedTask;
        }

        public Task ShowButtonConfirmUi(string deviceId)
        {
            foreach (var session in _clientSessionManager.Sessions)
            {
                try
                {
                    session.Callbacks.ShowButtonConfirmUi(deviceId);
                }
                catch (Exception) { }
            }

            return Task.CompletedTask;
        }

        public Task HidePinUi()
        {
            foreach(var session in _clientSessionManager.Sessions)
            {
                try
                {
                    session.Callbacks.HidePinUi();
                }
                catch (Exception) { }
            }

            return Task.CompletedTask;
        }

        public Task ShowActivationCodeUi(string deviceId)
        {
            foreach (var session in _clientSessionManager.Sessions)
            {
                try
                {
                    session.Callbacks.ShowActivationCodeUi(deviceId);
                }
                catch (Exception) { }
            }

            return Task.CompletedTask;
        }

        public Task HideActivationCodeUi()
        {
            foreach (var session in _clientSessionManager.Sessions)
            {
                try
                {
                    session.Callbacks.HideActivationCodeUi();
                }
                catch (Exception) { }
            }

            return Task.CompletedTask;
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
            await Task.Run(() =>
            {
                try
                {
                    foreach (var session in _clientSessionManager.Sessions)
                    {
                        session.Callbacks.ServiceErrorReceived(message, notificationId);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            });
        }

        public async Task SendNotification(string message, string notificationId)
        {
            await Task.Run(() =>
            {

                foreach (var session in _clientSessionManager.Sessions)
                {
                    try
                    {
                        session.Callbacks.ServiceNotificationReceived(message, notificationId);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }
            });
        }

        public async Task SendStatus(HesStatus hesStatus, HesStatus tbHesStatus, RfidStatus rfidStatus, BluetoothStatus bluetoothStatus)
        {
            await Task.Run(() =>
            {
                foreach (var session in _clientSessionManager.Sessions)
                {
                    try
                    {
                        session.Callbacks.ServiceComponentsStateChanged(hesStatus, rfidStatus, bluetoothStatus, tbHesStatus);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }
            });
        }

    }
}
