using HideezClient.HideezServiceReference;

namespace HideezClient.Messages
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
