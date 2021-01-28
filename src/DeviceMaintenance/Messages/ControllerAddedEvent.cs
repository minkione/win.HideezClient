using Meta.Lib.Modules.PubSub;

namespace DeviceMaintenance.Messages
{
    public class ControllerAddedEvent : PubSubMessageBase
    {
        public string DeviceId { get; }

        public ControllerAddedEvent(string deviceId)
        {
            DeviceId = deviceId;
        }
    }
}
