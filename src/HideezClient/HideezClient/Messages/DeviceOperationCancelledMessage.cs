using HideezClient.HideezServiceReference;

namespace HideezClient.Messages
{
    class DeviceOperationCancelledMessage
    {
        public DeviceDTO Device { get; }

        public DeviceOperationCancelledMessage(DeviceDTO device)
        {
            Device = device;
        }
    }
}
