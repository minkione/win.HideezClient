using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.IncommingMessages.RemoteDevice
{
    public sealed class RemoteConnection_ResetChannelMessage : PubSubMessageBase
    {
        public string ConnectionId { get; set; }
        public byte ChannelNo{ get; set; }

        public RemoteConnection_ResetChannelMessage(string connectionId, byte channelNo)
        {
            ConnectionId = connectionId;
            ChannelNo = channelNo;
        }
    }
}
