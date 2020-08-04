using HideezMiddleware.IPC.DTO;
using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.Messages
{
    public sealed class DeviceInitializedMessage : PubSubMessageBase
    {
        public DeviceDTO Device { get; }

        public DeviceInitializedMessage(DeviceDTO device)
        {
            Device = device;
        }
    }
}
