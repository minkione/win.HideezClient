using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.Modules.Csr.Messages
{
    internal sealed class CsrStatusChangedMessage : PubSubMessageBase
    {
        public object Sender { get; }
        public BluetoothStatus Status { get; }

        public CsrStatusChangedMessage(object sender, BluetoothStatus status)
        {
            Sender = sender;
            Status = status;
        }
    }
}
