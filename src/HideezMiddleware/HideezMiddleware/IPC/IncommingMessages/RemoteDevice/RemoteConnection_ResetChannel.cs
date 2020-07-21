using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.IncommingMessages.RemoteDevice
{
    public sealed class RemoteConnection_ResetChannel : PubSubMessageBase
    {
        public string ConnectionId { get; set; }

        public RemoteConnection_ResetChannel(string connectionId)
        {
            ConnectionId = connectionId;
        }
    }
}
