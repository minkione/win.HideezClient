using Meta.Lib.Modules.PubSub;

namespace HideezClient.Messages
{
    public class CancelActivationCodeEntryMessage:PubSubMessageBase
    {
        public string DeviceId { get; }

        public CancelActivationCodeEntryMessage(string deviceId)
        {
            DeviceId = deviceId;
        }
    }
}
