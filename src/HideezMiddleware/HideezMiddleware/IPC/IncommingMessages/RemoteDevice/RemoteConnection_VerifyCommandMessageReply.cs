using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.IncommingMessages.RemoteDevice
{
    public sealed class RemoteConnection_VerifyCommandMessageReply : PubSubMessageBase
    {
        public byte[] Data { get; set; }

        public RemoteConnection_VerifyCommandMessageReply(byte[] data)
        {
            Data = data;
        }
    }
}
