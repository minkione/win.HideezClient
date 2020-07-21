using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.IncommingMessages
{
    public sealed class GetAvailableChannelsMessageReply : PubSubMessageBase
    {
        public byte[] FreeChannels { get; set; }

        public GetAvailableChannelsMessageReply(byte[] freeChannels)
        {
            FreeChannels = freeChannels;
        }
    }
}
