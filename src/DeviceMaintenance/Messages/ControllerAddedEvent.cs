using Hideez.SDK.Communication.Connection;
using Meta.Lib.Modules.PubSub;

namespace DeviceMaintenance.Messages
{
    public class ControllerAddedEvent : PubSubMessageBase
    {
        public ConnectionId ConnectionId { get; }

        public ControllerAddedEvent(ConnectionId connectionId)
        {
            ConnectionId = connectionId;
        }
    }
}
