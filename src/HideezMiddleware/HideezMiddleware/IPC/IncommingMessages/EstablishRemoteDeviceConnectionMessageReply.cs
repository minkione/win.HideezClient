using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.IncommingMessages
{
    public sealed class EstablishRemoteDeviceConnectionMessageReply : PubSubMessageBase
    {
        public string RemoteDeviceId { get; set; }
        public string ConnectionId { get; set; }

        public EstablishRemoteDeviceConnectionMessageReply(string removeDeviceId, string connectionId)
        {
            RemoteDeviceId = removeDeviceId;
            ConnectionId = connectionId;
        }
    }
}
