using Meta.Lib.Modules.PubSub;

namespace HideezClient.Messages.Dialogs.BackupPassword
{
    internal class SendBackupPasswordMessage: PubSubMessageBase
    {
        public string DeviceId { get; }
        public byte[] Password { get; }

        public SendBackupPasswordMessage(string deviceId, byte[] password)
        {
            DeviceId = deviceId;
            Password = password;
        }
    }
}
