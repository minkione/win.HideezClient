using System;
using System.Diagnostics;
using System.Collections.Generic;
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
    }

    public enum HesStatus
    {
        Ok,
        HesNotConnected,
    }

    public class UiProxyManager : IClientUi, IDisposable
    {
        readonly IClientUi _credentialProviderUi;
        readonly IClientUi _clientUi;

        public event EventHandler<EventArgs> ClientConnected;

        public bool IsConnected
        {
            get
            {
                return _credentialProviderUi.IsConnected || _clientUi.IsConnected;
            }
        }

        public UiProxyManager(IClientUi credentialProviderUi, IClientUi clientUi)
        {
            _credentialProviderUi = credentialProviderUi;
            _clientUi = clientUi;

            _credentialProviderUi.ClientConnected += CredentialProviderUi_ClientUiConnected;
            _clientUi.ClientConnected += ClientUi_ClientUiConnected;
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
                _credentialProviderUi.ClientConnected -= CredentialProviderUi_ClientUiConnected;
                _clientUi.ClientConnected -= ClientUi_ClientUiConnected;
            }

            disposed = false;
        }

        ~UiProxyManager()
        {
            Dispose(false);
        }
        #endregion

        void CredentialProviderUi_ClientUiConnected(object sender, EventArgs e)
        {
            ClientConnected?.Invoke(this, EventArgs.Empty);
        }

        void ClientUi_ClientUiConnected(object sender, EventArgs e)
        {
            ClientConnected?.Invoke(this, EventArgs.Empty);
        }

        IClientUi GetCurrentClientUi()
        {
            return _credentialProviderUi.IsConnected ? _credentialProviderUi : _clientUi;
        }

        // Todo:
        internal async Task ShowPinUi(string deviceId)
        {
            //if (_credentialProviderUi.IsConnected)
            //    await _credentialProviderUi.ShowPinUi(deviceId);
        }

        // Todo:
        internal async Task<string> GetPin(string deviceId, int timeout)
        {
            //if (_credentialProviderUi.IsConnected)
            //    return await _credentialProviderUi.GetPin(deviceId, timeout);
            return null;

            ////todo
            //await Task.Delay(2000);
            //return "1111";
        }

        // Todo:
        internal async Task<string> GetConfirmedPin(string deviceId, int timeout)
        {
            //if (_credentialProviderUi.IsConnected)
            //    return await _credentialProviderUi.GetConfirmedPin(deviceId, timeout);
            return null;
        }

        public async Task HidePinUi()
        {
            Debug.WriteLine(">>>>>>>>>>>>>>> HidePinUi");
            var ui = GetCurrentClientUi();

            await ui.HidePinUi();
            await ui.SendNotification("");
        }

        public async Task SendStatus(BluetoothStatus bluetoothStatus, RfidStatus rfidStatus, HesStatus hesStatus)
        {
            var ui = GetCurrentClientUi();

            await ui.SendStatus(bluetoothStatus, rfidStatus, hesStatus);
        }

        public async Task SendNotification(string notification)
        {
            var ui = GetCurrentClientUi();

            await ui.SendNotification(notification);
        }

        public async Task SendError(string error)
        {
            var ui = GetCurrentClientUi();

            await ui.SendError(error);
        }

        public async Task<string> GetPin(string deviceId, int timeout, bool withConfirm = false)
        {
            var ui = GetCurrentClientUi();

            return await ui.GetPin(deviceId, timeout, withConfirm);
        }
    }
}
