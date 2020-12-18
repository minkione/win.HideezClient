using Hideez.SDK.Communication;
using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.IncommingMessages.RemoteDevice
{
    public class RemoteConnection_GetConnectionProviderMessageReply : PubSubMessageBase
    {
        public DefaultConnectionIdProvider ConnectionIdProvider { get; set; }

        public RemoteConnection_GetConnectionProviderMessageReply(byte connectionIdProvider)
        {
            ConnectionIdProvider = (DefaultConnectionIdProvider)connectionIdProvider;
        }
    }
}
