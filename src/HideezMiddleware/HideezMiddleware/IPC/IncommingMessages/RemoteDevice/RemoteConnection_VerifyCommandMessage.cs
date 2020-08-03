using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.IncommingMessages.RemoteDevice
{
    public sealed class RemoteConnection_VerifyCommandMessage : PubSubMessageBase
    {
        public string ConnectionId { get; set; }

        public byte[] Data { get; set; }

        public RemoteConnection_VerifyCommandMessage(string connectionId, byte[] data)
        {
            ConnectionId = connectionId;
            Data = data;
        }
    }
}
