using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dasync.Collections;
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
        readonly ConcurrentDictionary<string, TaskCompletionSource<byte[]>> _pendingGetActivationCodeRequests
            = new ConcurrentDictionary<string, TaskCompletionSource<byte[]>>();

        public event EventHandler<EventArgs> ClientConnected;

        public bool IsConnected => _credentialProviderUi.IsConnected || _clientUi.IsConnected;


        public UiProxyManager(IClientUiProxy credentialProviderUi, IClientUiProxy clientUi, ILog log)
            : base(nameof(UiProxyManager), log)
        {
            _credentialProviderUi = credentialProviderUi;
            _clientUi = clientUi;

            if (_credentialProviderUi != null)
            {
                _credentialProviderUi.ClientConnected += ClientUi_ClientUiConnected;
                _credentialProviderUi.PinReceived += ClientUi_PinReceived;
                _credentialProviderUi.ActivationCodeReceived += ClientUi_ActivationCodeReceived;
                _credentialProviderUi.ActivationCodeCancelled += ClientUi_ActivationCodeCancelled;
            }

            if (_clientUi != null)
            {
                _clientUi.ClientConnected += ClientUi_ClientUiConnected;
                _clientUi.PinReceived += ClientUi_PinReceived;
                _clientUi.ActivationCodeReceived += ClientUi_ActivationCodeReceived;
                _clientUi.ActivationCodeCancelled += ClientUi_ActivationCodeCancelled;
            }
        }

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _credentialProviderUi.ClientConnected -= ClientUi_ClientUiConnected;
                _clientUi.ClientConnected -= ClientUi_ClientUiConnected;

                _credentialProviderUi.PinReceived -= ClientUi_PinReceived;
                _clientUi.PinReceived -= ClientUi_PinReceived;
            }

            _disposed = true;
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
        void ClientUi_ActivationCodeReceived(object sender, ActivationCodeEventArgs e)
        {
            if (_pendingGetActivationCodeRequests.TryGetValue(e.DeviceId, out TaskCompletionSource<byte[]> tcs))
                tcs.TrySetResult(e.Code);
        }

        void ClientUi_ActivationCodeCancelled(object sender, ActivationCodeEventArgs e)
        {
            if (_pendingGetActivationCodeRequests.TryGetValue(e.DeviceId, out TaskCompletionSource<byte[]> tcs))
                tcs.TrySetCanceled();
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

        public async Task<string> GetPin(string deviceId, int timeout, CancellationToken ct, bool withConfirm = false, bool askOldPin = false)
        {
            WriteDebugLine($"SendGetPin: {deviceId}");

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

        public async Task<byte[]> GetActivationCode(string deviceId, int timeout, CancellationToken ct)
        {
            var ui = GetCurrentClientUi() ?? throw new HideezException(HideezErrorCode.NoConnectedUI);

            await ui.ShowActivationCodeUi(deviceId);

            var tcs = _pendingGetActivationCodeRequests.GetOrAdd(deviceId, (x) =>
            {
                return new TaskCompletionSource<byte[]>();
            });

            try
            {
                return await tcs.Task.TimeoutAfter(timeout, ct);
            }
            catch (TimeoutException)
            {
                throw new HideezException(HideezErrorCode.GetActivationCodeTimeout);
            }
            finally
            {
                _pendingGetActivationCodeRequests.TryRemove(deviceId, out TaskCompletionSource<byte[]> _);
            }
        }

        public async Task HideActivationCodeUi()
        {
            var uiList = GetClientUiList();
            foreach (var ui in uiList)
            {
                if (ui != null)
                    await ui.HideActivationCodeUi();
            }
        }

        public async Task SendStatus(HesStatus hesStatus, HesStatus tbHesStatus, RfidStatus rfidStatus, BluetoothStatus bluetoothStatus)
        {
            WriteLine($"SendStatus: hes:{hesStatus}; tb_hes: {tbHesStatus}; rfid:{rfidStatus}; ble:{bluetoothStatus};");

            var uiList = GetClientUiList();

            foreach (var ui in uiList)
            {
                if (ui != null)
                    await ui.SendStatus(hesStatus, tbHesStatus, rfidStatus, bluetoothStatus);
            }
        }

        public async Task SendNotification(string notification, string notificationId = null)
        {
            WriteLine($"SendNotification: {notification}");

            var ui = GetCurrentClientUi();

            //if (string.IsNullOrWhiteSpace(notificationId))
            //    notificationId = Guid.NewGuid().ToString();

            if (ui != null)
                await ui.SendNotification(notification, notificationId);
        }

        public async Task SendError(string error, string notificationId = null)
        {
            WriteLine($"SendError: {error}");

            var ui = GetCurrentClientUi();

            //if (string.IsNullOrWhiteSpace(notificationId))
            //    notificationId = Guid.NewGuid().ToString();

            if (ui != null)
                await ui.SendError(error, notificationId);
        }
    }
}
