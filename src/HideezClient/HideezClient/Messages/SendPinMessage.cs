using Meta.Lib.Modules.PubSub;

namespace HideezClient.Messages
{
    public class SendPinMessage: PubSubMessageBase
    {
        public string DeviceId { get; }

        public byte[] Pin { get; }

        public byte[] OldPin { get; }

        public SendPinMessage(string deviceId, byte[] pin, byte[] oldPin = null)
        {
            DeviceId = deviceId;
            Pin = pin;
            OldPin = oldPin;
        }
    }
}
