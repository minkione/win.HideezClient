using Meta.Lib.Modules.PubSub;
using System.Threading;

namespace HideezMiddleware.IPC.IncommingMessages
{
    public sealed class ChangeServerAddressMessage : PubSubMessageBase
    {
        public string ServerAddress { get; set; }

        public ChangeServerAddressMessage(string serverAddress, int timeout)
        {
            ServerAddress = serverAddress;
            ResponseTimeout = timeout;
        }
    }
}
