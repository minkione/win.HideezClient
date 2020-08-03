using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.IncommingMessages.RemoteDevice
{
    public sealed class RemoteConnection_ResetChannelMessage : PubSubMessageBase
    {
        public string ConnectionId { get; set; }

        public RemoteConnection_ResetChannelMessage(string connectionId)
        {
            ConnectionId = connectionId;
        }
    }
}
