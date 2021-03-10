using Meta.Lib.Modules.PubSub;

namespace HideezClient.Messages.Dialogs.Wipe
{
    internal sealed class CancelWipeMessage : PubSubMessageBase
    {
        public string DeviceId { get; }

        public CancelWipeMessage(string deviceId)
        {
            DeviceId = deviceId;
        }
    }
}
