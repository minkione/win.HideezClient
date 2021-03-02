using Meta.Lib.Modules.PubSub;

namespace HideezClient.Messages.Dialogs.BackupPassword
{
    internal class BackupPasswordCancelledMessage: PubSubMessageBase
    {
        public string DeviceId { get; }

        public BackupPasswordCancelledMessage(string deviceId)
        {
            DeviceId = deviceId;
        }
    }
}
