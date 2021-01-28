using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.Messages
{
    public sealed class UnpairDeviceMessage : PubSubMessageBase
    {
        public string Id { get; set; }

        public UnpairDeviceMessage(string id)
        {
            Id = id;
        }
    }
}
