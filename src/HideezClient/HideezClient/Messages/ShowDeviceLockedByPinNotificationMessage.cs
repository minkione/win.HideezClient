using HideezClient.Models;

namespace HideezClient.Messages
{
    class ShowDeviceLockedByPinNotificationMessage
    {
        public Device Device { get; set; }

        public ShowDeviceLockedByPinNotificationMessage(Device device)
        {
            Device = device;
        }
    }
}
