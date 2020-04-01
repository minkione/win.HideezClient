using HideezClient.Models;

namespace HideezClient.Controls
{
    class DeviceIsLockedNotificationViewModel : SimpleNotificationViewModel
    {
        HardwareVaultModel _device;

        public HardwareVaultModel Device
        {
            get { return _device; }
            set { Set(ref _device, value); }
        }
    }
}
