using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.IncommingMessages
{
    public sealed class ActivationCodeCancelledMessage : PubSubMessageBase
    {
        public string DeviceId { get; set; }

        public ActivationCodeCancelledMessage(string deviceId)
        {
            DeviceId = deviceId;
        }
    }
}
