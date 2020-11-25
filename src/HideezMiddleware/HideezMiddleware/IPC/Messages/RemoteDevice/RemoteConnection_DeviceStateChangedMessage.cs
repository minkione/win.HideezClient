using HideezMiddleware.IPC.DTO;
using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.Messages
{
    public sealed class RemoteConnection_DeviceStateChangedMessage : PubSubMessageBase
    {
        public string DeviceId { get; }

        public byte[] State { get; }

        public RemoteConnection_DeviceStateChangedMessage(string deviceId, byte[] state)
        {
            DeviceId = deviceId;
            State = state;
        }
    }
}
