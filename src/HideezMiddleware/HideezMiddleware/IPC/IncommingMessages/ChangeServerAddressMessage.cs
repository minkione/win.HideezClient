using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.IncommingMessages
{
    public sealed class ChangeServerAddressMessage : PubSubMessageBase
    {
        public string ServerAddress { get; set; }

        public ChangeServerAddressMessage(string serverAddress)
        {
            ServerAddress = serverAddress;
            ResponseTimeout = 60_000;
        }
    }
}
