using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.IncommingMessages.RemoteDevice
{
    public class RemoteConnection_GetRootKeyMessage : PubSubMessageBase
    {
        public string ConnectionId { get; set; }

        public RemoteConnection_GetRootKeyMessage(string connectionid)
        {
            ConnectionId = connectionid;
        }
    }
}
