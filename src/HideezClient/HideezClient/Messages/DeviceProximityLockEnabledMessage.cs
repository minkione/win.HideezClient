using HideezMiddleware.IPC.DTO;

namespace HideezClient.Messages
{
    class DeviceProximityLockEnabledMessage
    {
        public DeviceDTO Device { get; }

        public DeviceProximityLockEnabledMessage(DeviceDTO device)
        {
            Device = device;
        }
    }
}
