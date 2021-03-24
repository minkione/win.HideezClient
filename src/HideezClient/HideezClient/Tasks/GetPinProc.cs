using Hideez.SDK.Communication.Utils;
using HideezClient.Dialogs;
using HideezClient.Messages;
using HideezClient.Messages.Dialogs;
using HideezClient.Messages.Dialogs.Pin;
using Meta.Lib.Modules.PubSub;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HideezClient.Tasks
{
    internal sealed class GetPinProcResult
    {
        public string DeviceId { get; }

        public byte[] Pin { get; }

        public byte[] OldPin { get; }

        internal GetPinProcResult(string deviceId, byte[] pin, byte[] oldPin)
        {
            DeviceId = deviceId;
            Pin = pin;
            OldPin = oldPin;
        }
    }

    internal sealed class GetPinProc
    {
        readonly IMetaPubSub _messenger;
        readonly string _deviceId;
        readonly bool _withConfirm;
        readonly bool _askOldPin;
        readonly TaskCompletionSource<GetPinProcResult> _tcs = new TaskCompletionSource<GetPinProcResult>();

        public GetPinProc(IMetaPubSub messenger, string deviceId, bool withConfirm = false, bool askOldPin = false)
        {
            _messenger = messenger;
            _deviceId = deviceId;
            _withConfirm = withConfirm;
            _askOldPin = askOldPin;
        }

        public async Task<GetPinProcResult> Run(int timeout, CancellationToken ct)
        {
            try
            {
                _messenger.Subscribe<SendPinMessage>(OnPinReceived, msg => msg.DeviceId == _deviceId);
                _messenger.Subscribe<PinCancelledMessage>(OnPinCancelled, msg => msg.DeviceId == _deviceId);
                _messenger.Subscribe<HideDialogMessage>(OnHidePinUi);

                await _messenger.Publish(new ShowPinUiMessage(_deviceId, _withConfirm, _askOldPin));

                return await _tcs.Task.TimeoutAfter(timeout, ct);
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                await _messenger.Unsubscribe<SendPinMessage>(OnPinReceived);
                await _messenger.Unsubscribe<PinCancelledMessage>(OnPinCancelled);
                await _messenger.Unsubscribe<HideDialogMessage>(OnHidePinUi);
            }
        }

        private Task OnPinReceived(SendPinMessage msg)
        {
            var procResult = new GetPinProcResult(msg.DeviceId, msg.Pin, msg.OldPin);
            _tcs.TrySetResult(procResult);

            return Task.CompletedTask;
        }

        private Task OnPinCancelled(PinCancelledMessage arg)
        {
            _tcs.TrySetCanceled();

            return Task.CompletedTask;
        }

        private Task OnHidePinUi(HideDialogMessage msg)
        {
            if (msg.DialogType == typeof(PinDialog))
                _tcs.TrySetCanceled();

            return Task.CompletedTask;
        }
    }
}
