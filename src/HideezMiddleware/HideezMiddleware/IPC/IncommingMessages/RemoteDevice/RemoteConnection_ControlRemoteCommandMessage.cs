using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.IncommingMessages.RemoteDevice
{
    public class RemoteConnection_ControlRemoteCommandMessage : PubSubMessageBase
    {
        public string ConnectionId { get; set; }

        public string Data { get; set; }

        public RemoteConnection_ControlRemoteCommandMessage(string connectionid, string data)
        {
            ConnectionId = connectionid;
            Data = data;
        }
    }
}
