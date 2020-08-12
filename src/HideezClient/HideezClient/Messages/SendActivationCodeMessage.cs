using Meta.Lib.Modules.PubSub;

namespace HideezClient.Messages
{
    public class SendActivationCodeMessage: PubSubMessageBase
    {
        public string DeviceId { get; }

        public byte[] Code { get; }

        public SendActivationCodeMessage(string deviceId, byte[] code)
        {
            DeviceId = deviceId;
            Code = code;
        }
    }
}
