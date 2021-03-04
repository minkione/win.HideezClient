using Hideez.SDK.Communication;
using Hideez.SDK.Communication.BackupManager;
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
using System.Reactive.Linq;
using System.Security.Cryptography;
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
        readonly CredentialsBackupManager _credentialsBackupManager;

        public BackupCredentialsControlViewModel(
            IApplicationModeProvider applicationModeProvider,
            IActiveDevice activeDevice,
            IMetaPubSub messenger,
            CredentialsBackupManager credentialsBackupManager)
        {
            var mode = applicationModeProvider.GetApplicationMode();
            if (mode != ApplicationMode.Standalone)
                return;
            _activeDevice = activeDevice;
            _messenger = messenger;
            _credentialsBackupManager = credentialsBackupManager;
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
                    var backup = await _credentialsBackupManager.GetBackup(_activeDevice.Device.Storage);

                    using (var fileStream = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        using (Aes encryptor = Aes.Create())
                        {
                            encryptor.Mode = CipherMode.CBC;
                            encryptor.Key = GetKey(passwordResult.Password);
                            encryptor.IV = AesCryptoHelper.CreateRandomBuf(16);

                            XmlSerializer xs = new XmlSerializer(typeof(List<TableRecord>), new Type[] { typeof(TableRecord) });
                            // write IV
                            fileStream.Write(encryptor.IV, 0, 16);

                            using (var cryptoStream = new CryptoStream(fileStream, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                            {
                                // write version and magic
                                var buf = new byte[encryptor.BlockSize];
                                buf[0] = 0x01;
                                buf[1] = 0x00;
                                buf[2] = 0x02;
                                buf[3] = 0x28;
                                cryptoStream.Write(buf, 0, buf.Length);

                                xs.Serialize(cryptoStream, backup);
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
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
                OpenFileDialog dlg = new OpenFileDialog
                {
                    DefaultExt = ".hvb",
                    Filter = "Hideez vault backups (.hvb)|*.hvb" // Filter files by extension
                };

                // Show open file dialog box
                var result = dlg.ShowDialog();

                if (result == true)
                {
                    string filename = dlg.FileName;
                    var passwordResult = await GetBackupPassword(false, filename);

                    List<TableRecord> tableRecords = new List<TableRecord>();
                    using (Aes encryptor = Aes.Create())
                    {
                        encryptor.Mode = CipherMode.CBC;
                        encryptor.Key = GetKey(passwordResult.Password);

                        var ser = new XmlSerializer(typeof(List<TableRecord>), new Type[] { typeof(TableRecord) });

                        using (var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
                        {
                            var iv = new byte[16];
                            fileStream.Read(iv, 0, 16);
                            encryptor.IV = iv;

                            try
                            {
                                using (var cyptorStream = new CryptoStream(fileStream, encryptor.CreateDecryptor(), CryptoStreamMode.Read))
                                {
                                    // read version and magic
                                    var buf = new byte[encryptor.BlockSize];
                                    cyptorStream.Read(buf, 0, buf.Length);

                                    if (buf[1] != 0x00 || buf[2] != 0x02 || buf[3] != 0x28)
                                        throw new Exception("Credentials backup wrong pass or file is incorrect");

                                    if (buf[0] != 0x01)
                                        throw new Exception("File is incorrect");
                                    var des = ser.Deserialize(cyptorStream);
                                    tableRecords = (List<TableRecord>)des;
                                }
                            }
                            catch (CryptographicException)
                            {
                                // MS bug - если установлен русский язык, возникает какое-то исключение
                                // при попытке выхода из блока using через throw (на англ. языке все нормально)
                            }
                        }
                    }

                    await _credentialsBackupManager.Restore(_activeDevice.Device.Storage, tableRecords);
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
    }
}
