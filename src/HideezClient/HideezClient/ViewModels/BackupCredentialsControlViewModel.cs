using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Log;
using HideezClient.Messages;
using HideezClient.Messages.Dialogs.BackupPassword;
using HideezClient.Modules;
using HideezClient.Modules.Localize;
using HideezClient.Modules.Log;
using HideezClient.Modules.ServiceProxy;
using HideezClient.Tasks;
using HideezMiddleware.ApplicationModeProvider;
using Meta.Lib.Modules.PubSub;
using Microsoft.Win32;
using MvvmExtensions.Commands;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HideezClient.ViewModels
{
    class BackupCredentialsControlViewModel : ReactiveObject
    {
        readonly IServiceProxy _serviceProxy;
        readonly Logger _log = LogManager.GetCurrentClassLogger(nameof(BackupCredentialsControlViewModel));
        readonly IMetaPubSub _messenger;

        public BackupCredentialsControlViewModel(
            IApplicationModeProvider applicationModeProvider,
            IActiveDevice activeDevice,
            IServiceProxy serviceProxy,
            IMetaPubSub messenger)
        {
            var mode = applicationModeProvider.GetApplicationMode();
            if (mode != ApplicationMode.Standalone)
                return;

            _serviceProxy = serviceProxy;
            _messenger = messenger;

            _messenger.Subscribe<ActiveDeviceChangedMessage>(OnActiveDeviceChanged);

            Device = activeDevice.Device != null ? new DeviceViewModel(activeDevice.Device) : null;
        }

        #region Properties
        [Reactive] public DeviceViewModel Device { get; set; }

        #endregion

        #region Commands
        public ICommand SelectFileCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = async x =>
                    {
                        await OnSelectFile();
                    }
                };
            }
        }

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
        #endregion

        private async Task OnRestoreCredentials()
        {
            try
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.DefaultExt = ".hvb";
                dlg.Filter = "Hideez vault backups (.hvb)|*.hvb"; // Filter files by extension
                dlg.FileName = "CredentialsBackup";

                // Show open file dialog box
                var result = dlg.ShowDialog();

                if (result == true)
                {
                    string filename = dlg.FileName;
                    var res = await GetBackupPassword(true, filename);
                }
            }
            catch (OperationCanceledException ex)
            {
                _log.WriteLine(ex);
            }
            finally
            {
                await _messenger.Publish(new HideBackupPasswordUiMessage());
            }
        }

        private async Task OnSelectFile()
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.DefaultExt = ".hvb";
                //dlg.Filter = "Hideez vault backups (.hvb)|*.hvb"; // Filter files by extension

                // Show open file dialog box
                var result = dlg.ShowDialog();

                if (result == true)
                {
                    string filename = dlg.FileName;
                    var res = await GetBackupPassword(false, filename);
                }
            }
            catch (OperationCanceledException ex)
            {
                _log.WriteLine(ex);
            }
            finally
            {
                await _messenger.Publish(new HideBackupPasswordUiMessage());
            }
        }

        Task OnActiveDeviceChanged(ActiveDeviceChangedMessage obj)
        {
            // Todo: ViewModel should be reused instead of being recreated each time active device is changed
            Device = obj.NewDevice != null ? new DeviceViewModel(obj.NewDevice) : null;

            return Task.CompletedTask;
        }

        async Task<GetBackupPasswordProcResult> GetBackupPassword(bool isNewPassword, string fileName)
        {
            Debug.WriteLine(">>>>>>>>>>>>>>> GetBackupPasswordWorkflow +++++++++++++++++++++++++++++++++++++++");
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
    }
}
