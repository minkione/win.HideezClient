using HideezClient.HideezServiceReference;

namespace HideezClient.Messages
{
    class DeviceAuthorizedMessage
    {
        public DeviceAuthorizedMessage(DeviceDTO device)
        {
            Device = device;
        }

        public DeviceDTO Device { get; }
    }
}
