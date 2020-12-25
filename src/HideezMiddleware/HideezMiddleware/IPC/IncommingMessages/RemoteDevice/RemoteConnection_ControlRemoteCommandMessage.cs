using Hideez.SDK.Communication.Device.Exchange;
using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.IncommingMessages.RemoteDevice
{
    public class RemoteConnection_ControlRemoteCommandMessage : PubSubMessageBase
    {
        public string ConnectionId { get; set; }

        public ControlRequest ControlRequest { get; set; }

        public RemoteConnection_ControlRemoteCommandMessage(string connectionid, ControlRequest controlRequest)
        {
            ConnectionId = connectionid;
            ControlRequest = controlRequest;
        }
    }
}
