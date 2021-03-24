using Hideez.SDK.Communication.Utils;
using HideezClient.Dialogs;
using HideezClient.Messages.Dialogs;
using HideezClient.Messages.Dialogs.Wipe;
using Meta.Lib.Modules.PubSub;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HideezClient.Tasks
{
    internal sealed class GetUserWipeConfirmationProcResult
    {
        public string DeviceId { get; }

        public bool Confirmed { get; }

        internal GetUserWipeConfirmationProcResult(string deviceId, bool confirmed)
        {
            DeviceId = deviceId;
            Confirmed = confirmed;
        }
    }

    internal sealed class GetUserWipeConfirmationProc
    {
        readonly IMetaPubSub _messenger;
        readonly string _deviceId;
        readonly TaskCompletionSource<GetUserWipeConfirmationProcResult> _tcs = new TaskCompletionSource<GetUserWipeConfirmationProcResult>();

        public GetUserWipeConfirmationProc(IMetaPubSub messenger, string deviceId)
        {
            _messenger = messenger;
            _deviceId = deviceId;
        }

        public async Task<GetUserWipeConfirmationProcResult> Run(int timeout, CancellationToken ct)
        {
            try
            {
                _messenger.Subscribe<StartWipeMessage>(OnStartWipe, msg => msg.DeviceId == _deviceId);
                _messenger.Subscribe<CancelWipeMessage>(OnCancelWipe, msg => msg.DeviceId == _deviceId);
                _messenger.Subscribe<HideDialogMessage>(OnHideWipeDialog);

                await _messenger.Publish(new ShowWipeDialogMessage(_deviceId));

                return await _tcs.Task.TimeoutAfter(timeout, ct);
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                await _messenger.Unsubscribe<StartWipeMessage>(OnStartWipe);
                await _messenger.Unsubscribe<CancelWipeMessage>(OnCancelWipe);
                await _messenger.Unsubscribe<HideDialogMessage>(OnHideWipeDialog);
            }
        }

        private Task OnStartWipe(StartWipeMessage msg)
        {
            var procResult = new GetUserWipeConfirmationProcResult(msg.DeviceId, true);
            _tcs.TrySetResult(procResult);

            return Task.CompletedTask;
        }

        private Task OnCancelWipe(CancelWipeMessage msg)
        {
            var procResult = new GetUserWipeConfirmationProcResult(msg.DeviceId, false);
            _tcs.TrySetResult(procResult);

            return Task.CompletedTask;
        }

        private Task OnHideWipeDialog(HideDialogMessage msg)
        {
            if(msg.DialogType == typeof(WipeDialog))
                _tcs.TrySetCanceled();

            return Task.CompletedTask;
        }
    }
}
