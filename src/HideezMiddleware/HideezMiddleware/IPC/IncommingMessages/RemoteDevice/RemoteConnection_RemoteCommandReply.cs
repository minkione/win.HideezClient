using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.IncommingMessages.RemoteDevice
{
    public sealed class RemoteConnection_RemoteCommandReply : PubSubMessageBase
    {
        public byte[] Data { get; set; }

        public RemoteConnection_RemoteCommandReply(byte[] data)
        {
            Data = data;
        }
    }
}
