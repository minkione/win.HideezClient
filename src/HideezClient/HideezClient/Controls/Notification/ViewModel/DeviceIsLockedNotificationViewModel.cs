using HideezClient.Models;

namespace HideezClient.Controls
{
    class DeviceIsLockedNotificationViewModel : SimpleNotificationViewModel
    {
        Device _device;

        public Device Device
        {
            get { return _device; }
            set { Set(ref _device, value); }
        }
    }
}
