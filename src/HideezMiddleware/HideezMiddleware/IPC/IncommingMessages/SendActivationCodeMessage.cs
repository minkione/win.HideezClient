using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.IncommingMessages
{
    public sealed class SendActivationCodeMessage : PubSubMessageBase
    {
        public string DeviceId { get; set; }

        public byte[] ActivationCode { get; set; }

        public SendActivationCodeMessage(string deviceId, byte[] activationCode)
        {
            DeviceId = deviceId;
            ActivationCode = activationCode;
        }
    }
}
