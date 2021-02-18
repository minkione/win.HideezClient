using Meta.Lib.Modules.PubSub;

namespace HideezClient.Messages.Dialogs.MasterPassword
{
    internal sealed class MassterPasswordCancelledMessage : PubSubMessageBase
    {
        public string DeviceId { get; }

        public MassterPasswordCancelledMessage(string deviceId)
        {
            DeviceId = deviceId;
        }
    }
}
