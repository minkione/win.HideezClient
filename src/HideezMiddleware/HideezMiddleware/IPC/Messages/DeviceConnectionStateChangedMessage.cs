using HideezMiddleware.IPC.DTO;
using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.Messages
{
    public sealed class DeviceConnectionStateChangedMessage : PubSubMessageBase
    {
        public DeviceDTO Device { get; }

        public DeviceConnectionStateChangedMessage(DeviceDTO device)
        {
            Device = device;
        }
    }
}
