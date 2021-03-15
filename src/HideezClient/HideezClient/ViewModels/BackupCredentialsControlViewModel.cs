using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Backup;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.Security;
using HideezClient.Messages;
using HideezClient.Messages.Dialogs.BackupPassword;
using HideezClient.Modules;
using HideezClient.Modules.Localize;
using HideezClient.Modules.Log;
using HideezClient.Tasks;
using HideezMiddleware.ApplicationModeProvider;
using Meta.Lib.Modules.PubSub;
using Microsoft.Win32;
using MvvmExtensions.Commands;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Serialization;

namespace HideezClient.ViewModels
{
    class BackupCredentialsControlViewModel : ReactiveObject
    {
        readonly IActiveDevice _activeDevice;
        readonly Logger _log = LogManager.GetCurrentClassLogger(nameof(BackupCredentialsControlViewModel));
        readonly IMetaPubSub _messenger;

        CancellationTokenSource _cts;

        public BackupCredentialsControlViewModel(
            IApplicationModeProvider applicationModeProvider,
            IActiveDevice activeDevice,
            IMetaPubSub messenger)
        {
            var mode = applicationModeProvider.GetApplicationMode();
            if (mode != ApplicationMode.Standalone)
                return;
            _activeDevice = activeDevice;
            _messenger = messenger;
            _messenger.Subscribe<ActiveDeviceChangedMessage>(OnActiveDeviceChanged);

            Device = activeDevice.Device != null ? new DeviceViewModel(activeDevice.Device) : null;
        }

        #region Properties
        [Reactive] public DeviceViewModel Device { get; set; }

        #endregion

        #region Commands
        public ICommand RestoreCredentialsCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = async x =>
                    {
                        await OnRestoreCredentials();
                    }
                };
            }
        }

        public ICommand BackupCredentialsCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = async x =>
                    {
                        await OnBackupCredentials();
                    }
                };
            }
        }
        #endregion

        private async Task OnBackupCredentials()
        {
            try
            {
                _cts = new CancellationTokenSource();

                _messenger.Subscribe<BackupPasswordCancelledMessage>(OnBackupPasswordCancelled, msg => msg.DeviceId == Device.Id);

                SaveFileDialog dlg = new SaveFileDialog
                {
                    DefaultExt = ".hvb",
                    Filter = "Hideez vault backups (.hvb)|*.hvb", // Filter files by extension
                    FileName = "CredentialsBackup"
                };

                // Show open file dialog box
                var result = dlg.ShowDialog();

                if (result == true)
                {
                    string filename = dlg.FileName;
                    var passwordResult = await GetBackupPassword(true, filename);

                    if (passwordResult != null)
                    {
                        var backupProc = new CredentialsBackupProcedure();

                        await backupProc.Run(_activeDevice.Device.Storage, filename, passwordResult.Password, _cts.Token);

                        await _messenger.Publish(new SetResultUIBackupPasswordMessage(true));
                    }
                }
            }
            catch(Exception ex)
            {
                await _messenger.Publish(new SetResultUIBackupPasswordMessage(false));
                _log.WriteLine(ex);
            }
            finally
            {
                await _messenger.Unsubscribe<BackupPasswordCancelledMessage>(OnBackupPasswordCancelled);
                _cts.Dispose();
                _cts = null;
            }
        }

        private async Task OnRestoreCredentials()
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog
                {
                    DefaultExt = ".hvb",
                    Filter = "Hideez vault backup (.hvb)|*.hvb|Hideez key backup (*.hb)|*.hb|All files (*.*)|*.*" // Filter files by extension
                };

                // Show open file dialog box
                var result = dlg.ShowDialog();

                if (result == true)
                {
                    string filename = dlg.FileName;
                    var passwordResult = await GetBackupPassword(false, filename);

                    if (passwordResult != null)
                    {
                        var restoreProc = new CredentialsRestoreProcedure();

                        await restoreProc.Run(_activeDevice.Device.Storage, filename, passwordResult.Password);

                        await _messenger.Publish(new SetResultUIBackupPasswordMessage(true));
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                _log.WriteLine(ex);
                await _messenger.Publish(new SetResultUIBackupPasswordMessage(false));
            }
            catch(CryptographicException ex)
            {
                _log.WriteLine(ex);
                await _messenger.Publish(new SetResultUIBackupPasswordMessage(false, "Failed! Password is incorrect"));
            }
            catch(Exception ex)
            {
                _log.WriteLine(ex);
                await _messenger.Publish(new SetResultUIBackupPasswordMessage(false));
            }
        }

        Task OnActiveDeviceChanged(ActiveDeviceChangedMessage obj)
        {
            Device = obj.NewDevice != null ? new DeviceViewModel(obj.NewDevice) : null;

            return Task.CompletedTask;
        }

        async Task<GetBackupPasswordProcResult> GetBackupPassword(bool isNewPassword, string fileName)
        {
            if(isNewPassword)
                ShowInfo(TranslationSource.Instance["Vault.Notification.NewBackupPassword"]);
            else
                ShowInfo(TranslationSource.Instance["Vault.Notification.EnterCurrentBackupPassword"]);
            bool passwordOk = false;
            while (!passwordOk)
            {
                var bpProc = new GetBackupPasswordProc(_messenger, Device.Id, fileName, isNewPassword);
                var procResult = await bpProc.Run(SdkConfig.MainWorkflowTimeout);

                if (procResult == null)
                    return null;

                if (procResult.Password.Length == 0)
                {
                    // we received an empty PIN from the user. Trying again with the same timeout.
                    Debug.WriteLine($">>>>>>>>>>>>>>> EMPTY Backup Password");
                    _log.WriteLine("Received empty Backup Password");
                    continue;
                }
                return procResult;
            }
            return null;
        }

        void ShowInfo(string message)
        {
            _messenger?.Publish(new ShowInfoNotificationMessage(message,notificationId: Device.Id));
        }

        private byte[] GetKey(byte[] password)
        {
            var buf = new byte[32];
            int len = password.Length < buf.Length ? password.Length : buf.Length;
            for (int i = 0; i < len; i++)
            {
                buf[i] = (byte)(password[i] ^ buf[i]);
            }
            return buf;
        }

        private Task OnBackupPasswordCancelled(BackupPasswordCancelledMessage msg)
        {
            _cts?.Cancel();

            return Task.CompletedTask;
        }
    }
}
