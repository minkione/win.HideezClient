using HideezMiddleware.IPC.DTO;

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
