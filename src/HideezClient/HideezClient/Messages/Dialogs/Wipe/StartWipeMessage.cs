using Meta.Lib.Modules.PubSub;

namespace HideezClient.Messages.Dialogs.Wipe
{
    internal sealed class StartWipeMessage : PubSubMessageBase
    {
        public string DeviceId { get; }

        public StartWipeMessage(string deviceId)
        {
            DeviceId = deviceId;
        }

    }
}
