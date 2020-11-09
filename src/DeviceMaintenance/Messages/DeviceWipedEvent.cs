using DeviceMaintenance.ViewModel;
using Meta.Lib.Modules.PubSub;

namespace DeviceMaintenance.Messages
{
    public class DeviceWipedEvent : PubSubMessageBase
    {
        public DeviceViewModel DeviceViewModel { get; }

        public DeviceWipedEvent(DeviceViewModel deviceViewModel)
        {
            DeviceViewModel = deviceViewModel;
        }
    }
}
