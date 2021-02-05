using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.Utils;
using HideezMiddleware.CredentialProvider;

namespace HideezMiddleware
{
    public class UiProxyManager : Logger, IClientUiManager, IDisposable
    {
        IClientUiProxy _credentialProviderUi;
        IClientUiProxy _clientUi;
        readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _pendingGetPinRequests
            = new ConcurrentDictionary<string, TaskCompletionSource<string>>();
        readonly ConcurrentDictionary<string, TaskCompletionSource<byte[]>> _pendingGetActivationCodeRequests
            = new ConcurrentDictionary<string, TaskCompletionSource<byte[]>>();

        public UiProxyManager(IClientUiProxy credentialProviderUi, IClientUiProxy clientUi, ILog log)
            : base(nameof(UiProxyManager), log)
        {
            _credentialProviderUi = credentialProviderUi ?? throw new ArgumentNullException(nameof(credentialProviderUi));
            _credentialProviderUi.PinReceived += ClientUi_PinReceived;
            _credentialProviderUi.ActivationCodeReceived += ClientUi_ActivationCodeReceived;
            _credentialProviderUi.ActivationCodeCancelled += ClientUi_ActivationCodeCancelled;

            _clientUi = clientUi ?? throw new ArgumentNullException(nameof(clientUi));
            _clientUi.PinReceived += ClientUi_PinReceived;
            _clientUi.ActivationCodeReceived += ClientUi_ActivationCodeReceived;
            _clientUi.ActivationCodeCancelled += ClientUi_ActivationCodeCancelled;
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

            var uiList = GetClientUiList();
            if (uiList.Count == 0)
                throw new HideezException(HideezErrorCode.NoConnectedUI);

            foreach (var ui in uiList)
            {
                if (ui != null)
                    await ui.ShowPinUi(deviceId, withConfirm, askOldPin);
            }

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
            return _clientUi.ShowButtonConfirmUi(deviceId);
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
            var uiList = GetClientUiList();
            if (uiList.Count == 0)
                throw new HideezException(HideezErrorCode.NoConnectedUI);

            foreach (var ui in uiList)
            {
                if (ui != null)
                    await ui.ShowActivationCodeUi(deviceId);
            }

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

        public async Task SendStatus(HesStatus hesStatus, RfidStatus rfidStatus, BluetoothStatus dongleStatus, BluetoothStatus bluetoothStatus, HesStatus tbHesStatus)
        {
            WriteLine($"SendStatus: hes:{hesStatus}; rfid:{rfidStatus}; tb_hes: {tbHesStatus}; dongle:{dongleStatus}; ble:{bluetoothStatus}");

            var uiList = GetClientUiList();

            foreach (var ui in uiList)
            {
                if (ui != null)
                    await ui.SendStatus(hesStatus, rfidStatus, dongleStatus, bluetoothStatus, tbHesStatus);
            }
        }

        public async Task SendNotification(string notification, string notificationId = null)
        {
            WriteLine($"SendNotification: {notification}");

            var uiList = GetClientUiList();

            foreach (var ui in uiList)
            {
                if (ui != null)
                    await ui.SendNotification(notification, notificationId);
            }
        }

        public async Task SendError(string error, string notificationId = null)
        {
            WriteLine($"SendError: {error}");

            var uiList = GetClientUiList();

            foreach (var ui in uiList)
            {
                if (ui != null)
                    await ui.SendError(error, notificationId);
            }
        }
    }
}
