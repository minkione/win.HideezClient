using Hideez.SDK.Communication.Utils;
using HideezClient.Dialogs;
using HideezClient.Messages.Dialogs;
using HideezClient.Messages.Dialogs.MasterPassword;
using Meta.Lib.Modules.PubSub;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HideezClient.Tasks
{
    internal sealed class GetMasterPasswordProcResult
    {
        public string DeviceId { get; }

        public byte[] Password { get; }

        public byte[] OldPassword { get; }

        internal GetMasterPasswordProcResult(string deviceId, byte[] password, byte[] oldPassword)
        {
            DeviceId = deviceId;
            Password = password;
            OldPassword = oldPassword;
        }
    }
    
    internal sealed class GetMasterPasswordProc
    {
        readonly IMetaPubSub _messenger;
        readonly string _deviceId;
        readonly bool _withConfirm;
        readonly bool _askOldPassword;
        readonly TaskCompletionSource<GetMasterPasswordProcResult> _tcs = new TaskCompletionSource<GetMasterPasswordProcResult>();

        public GetMasterPasswordProc(IMetaPubSub messenger, string deviceId, bool withConfirm = false, bool askOldPassword = false)
        {
            _messenger = messenger;
            _deviceId = deviceId;
            _withConfirm = withConfirm;
            _askOldPassword = askOldPassword;
        }

        public async Task<GetMasterPasswordProcResult> Run(int timeout, CancellationToken ct)
        {
            try
            {
                _messenger.Subscribe<SendMasterPasswordMessage>(OnMasterPasswordReceived, msg => msg.DeviceId == _deviceId);
                _messenger.Subscribe<MasterPasswordCancelledMessage>(OnMasterPasswordCancelled, msg => msg.DeviceId == _deviceId);
                _messenger.Subscribe<HideDialogMessage>(OnHideMasterPasswordUi);

                await _messenger.Publish(new ShowMasterPasswordUiMessage(_deviceId, _withConfirm, _askOldPassword));

                return await _tcs.Task.TimeoutAfter(timeout, ct);
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                await _messenger.Unsubscribe<SendMasterPasswordMessage>(OnMasterPasswordReceived);
                await _messenger.Unsubscribe<MasterPasswordCancelledMessage>(OnMasterPasswordCancelled);
                await _messenger.Unsubscribe<HideDialogMessage>(OnHideMasterPasswordUi);
            }
        }

        private Task OnMasterPasswordReceived(SendMasterPasswordMessage msg)
        {
            var procResult = new GetMasterPasswordProcResult(msg.DeviceId, msg.Password, msg.OldPassword);
            _tcs.TrySetResult(procResult);

            return Task.CompletedTask;
        }

        private Task OnMasterPasswordCancelled(MasterPasswordCancelledMessage arg)
        {
            _tcs.TrySetCanceled();

            return Task.CompletedTask;
        }

        private Task OnHideMasterPasswordUi(HideDialogMessage msg)
        {
            if(msg.DialogType == typeof(MasterPasswordDialog))
                _tcs.TrySetCanceled();

            return Task.CompletedTask;
        }
    }
}
