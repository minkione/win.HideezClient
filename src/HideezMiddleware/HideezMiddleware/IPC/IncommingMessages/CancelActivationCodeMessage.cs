using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.IncommingMessages
{
    public sealed class CancelActivationCodeMessage : PubSubMessageBase
    {
        public string DeviceId { get; set; }

        public CancelActivationCodeMessage(string deviceId)
        {
            DeviceId = deviceId;
        }
    }
}
