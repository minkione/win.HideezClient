using Hideez.SDK.Communication.Interfaces;
using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.IncommingMessages.RemoteDevice
{
    public sealed class RemoteConnection_DeviceCommandMessageReply : PubSubMessageBase
    {
        public DeviceCommandReplyResult Data { get; set; }

        public RemoteConnection_DeviceCommandMessageReply(DeviceCommandReplyResult data)
        {
            Data = data;
        }
    }
}
