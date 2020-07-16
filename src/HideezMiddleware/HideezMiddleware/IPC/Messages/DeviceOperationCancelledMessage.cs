using HideezMiddleware.IPC.DTO;
using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.Messages
{
    public sealed class DeviceOperationCancelledMessage : PubSubMessageBase
    {
        public DeviceDTO Device { get; }

        public DeviceOperationCancelledMessage(DeviceDTO device)
        {
            Device = device;
        }
    }
}
