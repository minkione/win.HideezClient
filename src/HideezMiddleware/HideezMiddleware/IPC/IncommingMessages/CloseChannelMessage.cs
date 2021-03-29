using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.IncommingMessages
{
    public sealed class CloseChannelMessage : PubSubMessageBase
    {
        public string DeviceId { get; set; }

        public CloseChannelMessage(string deviceId)
        {
            DeviceId = deviceId;
        }
    }
}
