using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.IncommingMessages
{
    public sealed class EstablishRemoteDeviceConnectionMessageReply : PubSubMessageBase
    {
        public string RemoveDeviceId { get; set; }

        public EstablishRemoteDeviceConnectionMessageReply(string removeDeviceId)
        {
            RemoveDeviceId = removeDeviceId;
        }
    }
}
