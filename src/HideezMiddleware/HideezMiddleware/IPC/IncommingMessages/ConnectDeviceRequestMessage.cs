using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.IncommingMessages
{
    public class ConnectDeviceRequestMessage : PubSubMessageBase
    {
        public string Id { get; set; }

        public ConnectDeviceRequestMessage(string id)
        {
            Id = id;
        }
    }
}
