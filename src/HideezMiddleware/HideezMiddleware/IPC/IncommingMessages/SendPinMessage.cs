using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.IncommingMessages
{
    public sealed class SendPinMessage : PubSubMessageBase
    {
        public string DeviceId { get; set; }

        public byte[] Pin { get; set; }

        public byte[] OldPin { get; set; }

        public SendPinMessage(string deviceId, byte[] pin, byte[] oldPin)
        {
            DeviceId = deviceId;
            Pin = pin;
            OldPin = oldPin;
        }
    }
}
