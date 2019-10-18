using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
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

        protected virtual void Dispose(bool disposing)
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

        List<IClientUiProxy> GetClientUiList()
        {
            return new List<IClientUiProxy>()
            {
                _credentialProviderUi,
                _clientUi,
            };
        }

        public Task ShowPinUi(string deviceId, bool withConfirm = false, bool askOldPin = false)
        {
            return Task.CompletedTask;

            // Intentionally not impelented
            //var ui = GetCurrentClientUi() ?? throw new HideezException(HideezErrorCode.NoConnectedUI);
            //return ui.ShowPinUi(deviceId, withConfirm, askOldPin);
        }

        public async Task<string> GetPin(string deviceId, int timeout, CancellationToken ct, bool withConfirm = false, bool askOldPin = false)
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
                return await tcs.Task.TimeoutAfter(timeout, ct);
            }
            catch (TimeoutException)
            {
                throw new HideezException(HideezErrorCode.GetPinTimeout);
            }
            finally
            {
                _pendingGetPinRequests.TryRemove(deviceId, out TaskCompletionSource<string> _);
            }
        }

        public Task ShowButtonConfirmUi(string deviceId)
        {
            var ui = GetCurrentClientUi() ?? throw new HideezException(HideezErrorCode.NoConnectedUI);
            return ui.ShowButtonConfirmUi(deviceId);
        }

        public async Task HidePinUi()
        {
            var uiList = GetClientUiList();
            foreach (var ui in uiList)
            {
                if (ui != null)
                    await ui.HidePinUi();
            }
        }

        public async Task SendStatus(HesStatus hesStatus, RfidStatus rfidStatus, BluetoothStatus bluetoothStatus)
        {
            var uiList = GetClientUiList();
            foreach (var ui in uiList)
            {
                if (ui != null)
                    await ui.SendStatus(hesStatus, rfidStatus, bluetoothStatus);
            }
        }

        public async Task SendNotification(string notification, string notificationId = null)
        {
            var ui = GetCurrentClientUi();

            if (string.IsNullOrWhiteSpace(notificationId))
                notificationId = Guid.NewGuid().ToString();

            if (ui != null)
                await ui.SendNotification(notification, notificationId);
        }

        public async Task SendError(string error, string notificationId = null)
        {
            var ui = GetCurrentClientUi();

            if (string.IsNullOrWhiteSpace(notificationId))
                notificationId = Guid.NewGuid().ToString();

            if (ui != null)
                await ui.SendError(error, notificationId);
        }
    }
}
