using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.IncommingMessages.RemoteDevice
{
    public class RemoteConnection_GetRootKeyMessage : PubSubMessageBase
    {
        public string ConnectionId { get; set; }

        public byte[] Data { get; set; }

        public RemoteConnection_GetRootKeyMessage(string connectionid, byte[] data)
        {
            ConnectionId = connectionid;
            Data = data;
        }
    }


}
