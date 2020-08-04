using HideezMiddleware.IPC.DTO;
using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.Messages
{
    public sealed class RemoteConnection_DeviceStateChangedMessage : PubSubMessageBase
    {
        public string DeviceId { get; }

        public DeviceStateDTO State { get; }

        public RemoteConnection_DeviceStateChangedMessage(string deviceId, DeviceStateDTO state)
        {
            DeviceId = deviceId;
            State = state;
        }
    }
}
