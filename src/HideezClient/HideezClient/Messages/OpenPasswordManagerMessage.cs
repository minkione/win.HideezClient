using Meta.Lib.Modules.PubSub;

namespace HideezClient.Messages
{
    public class OpenPasswordManagerMessage: PubSubMessageBase
    {
        public string DeviceId { get; }

        public OpenPasswordManagerMessage(string deviceId)
        {
            DeviceId = deviceId;
        }
    }
}
