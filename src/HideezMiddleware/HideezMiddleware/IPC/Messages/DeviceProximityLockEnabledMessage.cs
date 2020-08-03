using HideezMiddleware.IPC.DTO;
using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.Messages
{
    public sealed class DeviceProximityLockEnabledMessage : PubSubMessageBase
    {
        public DeviceDTO Device { get; }

        public DeviceProximityLockEnabledMessage(DeviceDTO device)
        {
            Device = device;
        }
    }
}
