using DeviceMaintenance.ViewModel;

namespace DeviceMaintenance.Messages
{
    public class DeviceWipedEvent : MessageBase
    {
        public DeviceViewModel DeviceViewModel { get; }

        public DeviceWipedEvent(DeviceViewModel deviceViewModel)
        {
            DeviceViewModel = deviceViewModel;
        }
    }
}
