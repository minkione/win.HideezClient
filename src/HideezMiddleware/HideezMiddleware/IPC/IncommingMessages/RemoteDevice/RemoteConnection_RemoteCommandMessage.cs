using Hideez.SDK.Communication;
using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.IncommingMessages.RemoteDevice
{
    public sealed class RemoteConnection_RemoteCommandMessage : PubSubMessageBase
    {
        public string ConnectionId { get; set; }

        public byte[] Data { get; set; }

        public RemoteConnection_RemoteCommandMessage(string connectionid, byte[] data)
        {
            ConnectionId = connectionid;
            Data = data;
            ResponseTimeout = SdkConfig.DefaultRemoteCommandTimeout;
        }
    }
}
