using DeviceMaintenance.ViewModel;
using Meta.Lib.Modules.PubSub;

namespace DeviceMaintenance.Messages
{
    public class DeviceConnectedEvent : PubSubMessageBase
    {
        public DeviceViewModel DeviceViewModel { get; }

        public DeviceConnectedEvent(DeviceViewModel deviceViewModel)
        {
            DeviceViewModel = deviceViewModel;
        }
    }
}
