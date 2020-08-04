using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.IncommingMessages.RemoteDevice
{
    public sealed class RemoteConnection_RemoteCommandMessageReply : PubSubMessageBase
    {
        public byte[] Data { get; set; }

        public RemoteConnection_RemoteCommandMessageReply(byte[] data)
        {
            Data = data;
        }
    }
}
