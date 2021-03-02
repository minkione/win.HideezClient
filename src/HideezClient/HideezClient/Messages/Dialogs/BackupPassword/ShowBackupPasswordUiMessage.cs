using Meta.Lib.Modules.PubSub;

namespace HideezClient.Messages.Dialogs.BackupPassword
{
    internal class ShowBackupPasswordUiMessage: PubSubMessageBase
    {
        public string DeviceId { get; }
        public string BackupFileName { get; }
        public bool IsNewPassword { get; }

        public ShowBackupPasswordUiMessage(string deviceId, string backupFileName, bool isNewPassword)
        {
            DeviceId = deviceId;
            BackupFileName = backupFileName;
            IsNewPassword = isNewPassword;
        }
    }
}
