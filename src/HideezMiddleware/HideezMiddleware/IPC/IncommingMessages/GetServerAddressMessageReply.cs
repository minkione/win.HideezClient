using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.IncommingMessages
{
    public sealed class GetServerAddressMessageReply : PubSubMessageBase
    {
        public string ServerAddress { get; set; }

        public GetServerAddressMessageReply(string serverAddress)
        {
            ServerAddress = serverAddress;
        }
    }
}
