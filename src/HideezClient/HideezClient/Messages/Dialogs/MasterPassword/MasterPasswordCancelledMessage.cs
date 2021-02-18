using Meta.Lib.Modules.PubSub;

namespace HideezClient.Messages.Dialogs.MasterPassword
{
    internal sealed class MasterPasswordCancelledMessage : PubSubMessageBase
    {
        public string DeviceId { get; }

        public MasterPasswordCancelledMessage(string deviceId)
        {
            DeviceId = deviceId;
        }
    }
}
