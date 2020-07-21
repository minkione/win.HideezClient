using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.IncommingMessages
{
    public sealed class CancelPinMessage : PubSubMessageBase
    {
        public string DeviceId { get; set; }

        public CancelPinMessage(string deviceId)
        {
            DeviceId = deviceId;
        }
    }
}
