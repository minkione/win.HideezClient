using HideezClient.Models;

namespace HideezClient.Messages
{
    class ShowDeviceLockedByCodeNotificationMessage
    {
        public Device Device { get; set; }

        public ShowDeviceLockedByCodeNotificationMessage(Device device)
        {
            Device = device;
        }
    }
}
