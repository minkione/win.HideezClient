using HideezSafe.HideezServiceReference;

namespace HideezSafe.Messages
{
    class DeviceConnectionStateChangedMessage
    {
        public DeviceConnectionStateChangedMessage(DeviceDTO device)
        {
            Device = device;
        }

        public DeviceDTO Device { get; }
    }
}
