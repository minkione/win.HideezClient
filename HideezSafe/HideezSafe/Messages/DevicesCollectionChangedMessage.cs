using HideezSafe.HideezServiceReference;

namespace HideezSafe.Messages
{
    class DevicesCollectionChangedMessage
    {
        public DevicesCollectionChangedMessage(DeviceDTO[] devices)
        {
            Devices = devices;
        }

        public DeviceDTO[] Devices { get; }
    }
}
