using HideezClient.HideezServiceReference;

namespace HideezClient.Messages
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
