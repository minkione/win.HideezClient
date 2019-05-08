using HideezSafe.HideezServiceReference;

namespace HideezSafe.Messages
{
    class PairedDevicesCollectionChangedMessage
    {
        public PairedDevicesCollectionChangedMessage(BleDeviceDTO[] devices)
        {
            Devices = devices;
        }

        public BleDeviceDTO[] Devices { get; }
    }
}
