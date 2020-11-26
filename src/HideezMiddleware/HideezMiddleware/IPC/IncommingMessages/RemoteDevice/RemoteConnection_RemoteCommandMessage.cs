using Hideez.SDK.Communication.BLE;
using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.IncommingMessages.RemoteDevice
{
    public sealed class RemoteConnection_RemoteCommandMessage : PubSubMessageBase
    {
        public string ConnectionId { get; set; }

        public EncryptedRequest EncryptedRequest { get; set; }

        public RemoteConnection_RemoteCommandMessage(string connectionid, EncryptedRequest data)
        {
            ConnectionId = connectionid;
            EncryptedRequest = data;
        }
    }
}
