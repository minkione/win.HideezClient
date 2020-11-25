using Hideez.SDK.Communication.BLE;
using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.IncommingMessages.RemoteDevice
{
    public sealed class RemoteConnection_RemoteCommandMessage : PubSubMessageBase
    {
        public string ConnectionId { get; set; }

        public string Data { get; set; }

        public RemoteConnection_RemoteCommandMessage(string connectionid, string data)
        {
            ConnectionId = connectionid;
            Data = data;
        }
    }
}
