using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hideez.SDK.Communication.Log;
using System.Collections.Concurrent;
using Hideez.SDK.Communication.Utils;
using Hideez.SDK.Communication;

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

    public class UiProxyManager : Logger, IClientUiProxy, IDisposable
    {
        readonly IClientUi _credentialProviderUi;
        readonly IClientUi _clientUi;
        readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _pendingGetPinRequests
            = new ConcurrentDictionary<string, TaskCompletionSource<string>>();

        public event EventHandler<EventArgs> ClientConnected;

        public bool IsConnected => _credentialProviderUi.IsConnected || _clientUi.IsConnected;



        public UiProxyManager(ILog log, IClientUi credentialProviderUi, IClientUi clientUi)
            :base(nameof(UiProxyManager), log)
        {
            _credentialProviderUi = credentialProviderUi;
            _clientUi = clientUi;

            if (_credentialProviderUi != null)
            {
                _credentialProviderUi.ClientConnected += CredentialProviderUi_ClientUiConnected;
                _credentialProviderUi.PinReceived += ClientUi_PinReceived;
            }

            if (_clientUi != null)
            {
                _clientUi.ClientConnected += ClientUi_ClientUiConnected;
                _credentialProviderUi.PinReceived += ClientUi_PinReceived;
            }
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

        void ClientUi_PinReceived(object sender, PinReceivedEventArgs e)
        {
            if (_pendingGetPinRequests.TryGetValue(e.DeviceId, out TaskCompletionSource<string> tcs))
                tcs.TrySetResult(e.Pin);
        }

        IClientUi GetCurrentClientUi()
        {
            if (_credentialProviderUi?.IsConnected ?? false)
                return _credentialProviderUi;
            else if (_clientUi?.IsConnected ?? false)
                return _clientUi;
            return null;
        }

        public Task ShowPinUi(string deviceId, bool withConfirm = false, bool askOldPin = false)
        {
            if (askOldPin)
            {

            }

            var ui = GetCurrentClientUi() ?? throw new HideezException(HideezErrorCode.NoConnectedUI);
            return ui.ShowPinUi(deviceId, withConfirm, askOldPin);
        }

        public async Task<string> GetPin(string deviceId, int timeout, bool withConfirm = false, bool askOldPin = false)
        {
            WriteLine($"SendGetPin: {deviceId}");

            if (askOldPin)
            {

            }

            var ui = GetCurrentClientUi() ?? throw new HideezException(HideezErrorCode.NoConnectedUI);

            await ui.ShowPinUi(deviceId, withConfirm, askOldPin);

            var tcs = _pendingGetPinRequests.GetOrAdd(deviceId, (x) =>
            {
                return new TaskCompletionSource<string>();
            });

            try
            {
                return await tcs.Task.TimeoutAfter(timeout);
            }
            catch (TimeoutException)
            {
                return null;
            }
            finally
            {
                _pendingGetPinRequests.TryRemove(deviceId, out TaskCompletionSource<string> removed);
            }
        }

        //// Todo:
        //internal async Task<string> GetPin(string deviceId, int timeout)
        //{
        //    //if (_credentialProviderUi.IsConnected)
        //    //    return await _credentialProviderUi.GetPin(deviceId, timeout);
        //    return null;

        //    ////todo
        //    //await Task.Delay(2000);
        //    //return "1111";
        //}

        //// Todo:
        //internal async Task<string> GetConfirmedPin(string deviceId, int timeout)
        //{
        //    //if (_credentialProviderUi.IsConnected)
        //    //    return await _credentialProviderUi.GetConfirmedPin(deviceId, timeout);
        //    return null;
        //}

        public Task HidePinUi()
        {
            return GetCurrentClientUi()?.HidePinUi();
            //Debug.WriteLine(">>>>>>>>>>>>>>> HidePinUi");
            //var ui = GetCurrentClientUi();

            //await ui.HidePinUi();
            //await ui.SendNotification("");
        }

        public async Task SendStatus(BluetoothStatus bluetoothStatus, RfidStatus rfidStatus, HesStatus hesStatus)
        {
            var ui = GetCurrentClientUi();

            if (ui != null)
                await ui.SendStatus(bluetoothStatus, rfidStatus, hesStatus);
        }

        public async Task SendNotification(string notification)
        {
            var ui = GetCurrentClientUi();

            if (ui != null)
                await ui.SendNotification(notification);
        }

        public async Task SendError(string error)
        {
            var ui = GetCurrentClientUi();

            if (ui != null)
                await ui.SendError(error);
        }

    }
}
