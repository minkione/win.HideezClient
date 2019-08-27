﻿using HideezMiddleware;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceLibrary.Implementation.SessionManagement
{
    class ServiceClientUiManager : IClientUi, IDisposable
    {
        readonly ServiceClientSessionManager _clientSessionManager;

        public event EventHandler<EventArgs> ClientConnected;
        public event EventHandler<PinReceivedEventArgs> PinReceived;

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


        // Todo:
        public Task<string> GetPin(string deviceId, int timeout, bool withConfirm = false)
        {
            throw new NotImplementedException();
        }

        // Todo:
        public Task HidePinUi()
        {
            throw new NotImplementedException();
        }

        public async Task SendError(string message)
        {
            await Task.Run(() =>
            {
                try
                {
                    foreach (var session in _clientSessionManager.Sessions)
                    {
                        session.Callbacks.ServiceErrorReceived(message);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            });
        }

        public async Task SendNotification(string message)
        {
            await Task.Run(() =>
            {
                
                foreach (var session in _clientSessionManager.Sessions)
                {
                    try
                    {
                        session.Callbacks.ServiceNotificationReceived(message);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }
            });
        }

        public async Task SendStatus(HesStatus hesStatus, RfidStatus rfidStatus, BluetoothStatus bluetoothStatus)
        {
            await Task.Run(() =>
            {
                var isHesOk = hesStatus == HesStatus.Ok;

                var showHesStatus = true; // Placeholder for the future. HES indicator will be hidden in a consumer version

                var isRfidOk = rfidStatus == RfidStatus.Ok;

                var showRfidStatus = rfidStatus != RfidStatus.Disabled;

                var isBleOk = bluetoothStatus == BluetoothStatus.Ok;

                foreach (var session in _clientSessionManager.Sessions)
                {
                    try
                    {
                        session.Callbacks.ServiceComponentsStateChanged(isHesOk, showHesStatus, isRfidOk, showRfidStatus, isBleOk);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }
            });
        }

        public Task ShowPinUi(string deviceId, bool withConfirm = false, bool askOldPin = false)
        {
            throw new NotImplementedException();
        }
    }
}
