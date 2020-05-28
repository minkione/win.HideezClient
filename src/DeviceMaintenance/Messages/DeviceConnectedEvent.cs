using DeviceMaintenance.ViewModel;

namespace DeviceMaintenance.Messages
{
    public class DeviceConnectedEvent : MessageBase
    {
        public DeviceViewModel DeviceViewModel { get; }

        public DeviceConnectedEvent(DeviceViewModel deviceViewModel)
        {
            DeviceViewModel = deviceViewModel;
        }
    }
}
