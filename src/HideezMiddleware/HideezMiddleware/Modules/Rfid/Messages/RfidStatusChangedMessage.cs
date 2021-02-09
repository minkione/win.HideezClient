using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.Modules.Rfid.Messages
{
    internal sealed class RfidStatusChangedMessage : PubSubMessageBase
    {
        public object Sender { get; }

        public RfidStatus Status { get; }

        public RfidStatusChangedMessage(object sender, RfidStatus status)
        {
            Sender = sender;
            Status = status;
        }
    }
}
