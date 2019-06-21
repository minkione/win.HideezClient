using HideezSafe.HideezServiceReference;

namespace HideezSafe.Messages
{
    class PairedDevicesCollectionChangedMessage
    {
        public PairedDevicesCollectionChangedMessage(DeviceDTO[] devices)
        {
            Devices = devices;
        }

        public DeviceDTO[] Devices { get; }
    }
}
