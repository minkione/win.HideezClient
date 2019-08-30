using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.Utils;

namespace HideezMiddleware
{
    public class UiProxyManager : Logger, IClientUiManager, IDisposable
    {
        readonly IClientUiProxy _credentialProviderUi;
        readonly IClientUiProxy _clientUi;
        readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _pendingGetPinRequests
            = new ConcurrentDictionary<string, TaskCompletionSource<string>>();

        public event EventHandler<EventArgs> ClientConnected;

        public bool IsConnected => _credentialProviderUi.IsConnected || _clientUi.IsConnected;


        public UiProxyManager(IClientUiProxy credentialProviderUi, IClientUiProxy clientUi, ILog log)
            :base(nameof(UiProxyManager), log)
        {
            _credentialProviderUi = credentialProviderUi;
            _clientUi = clientUi;

            if (_credentialProviderUi != null)
            {
                _credentialProviderUi.ClientConnected += ClientUi_ClientUiConnected;
                _credentialProviderUi.PinReceived += ClientUi_PinReceived;
            }

            if (_clientUi != null)
            {
                _clientUi.ClientConnected += ClientUi_ClientUiConnected;
                _clientUi.PinReceived += ClientUi_PinReceived;
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
                _credentialProviderUi.ClientConnected -= ClientUi_ClientUiConnected;
                _clientUi.ClientConnected -= ClientUi_ClientUiConnected;

                _credentialProviderUi.PinReceived -= ClientUi_PinReceived;
                _clientUi.PinReceived -= ClientUi_PinReceived;
            }

            disposed = false;
        }

        ~UiProxyManager()
        {
            Dispose(false);
        }
        #endregion

        void ClientUi_ClientUiConnected(object sender, EventArgs e)
        {
            ClientConnected?.Invoke(this, EventArgs.Empty);
        }

        void ClientUi_PinReceived(object sender, PinReceivedEventArgs e)
        {
            if (_pendingGetPinRequests.TryGetValue(e.DeviceId, out TaskCompletionSource<string> tcs))
                tcs.TrySetResult(e.Pin);
        }

        IClientUiProxy GetCurrentClientUi()
        {
            if (_credentialProviderUi?.IsConnected ?? false)
                return _credentialProviderUi;
            else if (_clientUi?.IsConnected ?? false)
                return _clientUi;
            return null;
        }

        public Task ShowPinUi(string deviceId, bool withConfirm = false, bool askOldPin = false)
        {
            var ui = GetCurrentClientUi() ?? throw new HideezException(HideezErrorCode.NoConnectedUI);
            return ui.ShowPinUi(deviceId, withConfirm, askOldPin);
        }

        public async Task<string> GetPin(string deviceId, int timeout, bool withConfirm = false, bool askOldPin = false)
        {
            WriteLine($"SendGetPin: {deviceId}");

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

        public async Task HidePinUi()
        {
            var ui = GetCurrentClientUi();

            if (ui != null)
                await ui.HidePinUi();
        }

        public async Task SendStatus(HesStatus hesStatus, RfidStatus rfidStatus, BluetoothStatus bluetoothStatus)
        {
            var ui = GetCurrentClientUi();

            if (ui != null)
                await ui.SendStatus(hesStatus, rfidStatus, bluetoothStatus);
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
