using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.IncommingMessages
{
    public enum ClientType
    {
        ServiceHost,
        DesktopClient,
        TestConsole,
        RemoteDeviceConnection,
    }

    public sealed class AttachClientMessage : PubSubMessageBase
    {
        public ClientType ClientType { get; set; }

        public AttachClientMessage(ClientType clientType)
        {
            ClientType = clientType;
        }
    }
}
