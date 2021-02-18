using Meta.Lib.Modules.PubSub;

namespace HideezClient.Messages.Dialogs.Pin
{
    internal sealed class PinCancelledMessage : PubSubMessageBase
    {
        public string DeviceId { get; }

        public PinCancelledMessage(string deviceId)
        {
            DeviceId = deviceId;
        }
    }
}
