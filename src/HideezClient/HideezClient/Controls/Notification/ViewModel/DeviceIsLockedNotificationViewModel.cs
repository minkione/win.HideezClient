using HideezClient.Models;

namespace HideezClient.Controls
{
    class DeviceIsLockedNotificationViewModel : SimpleNotificationViewModel
    {
        DeviceModel _device;

        public DeviceModel Device
        {
            get { return _device; }
            set { Set(ref _device, value); }
        }
    }
}
