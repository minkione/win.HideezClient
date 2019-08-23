using HideezSafe.HideezServiceReference;

namespace HideezSafe.Messages
{
    class DeviceInitializedMessage
    {
        public DeviceInitializedMessage(DeviceDTO device)
        {
            Device = device;
        }

        public DeviceDTO Device { get; }
    }
}
