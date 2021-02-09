using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.Modules.WinBle.Messages
{
    internal sealed class WinBleStatusChangedMessage : PubSubMessageBase
    {
        public object Sender { get; }

        public BluetoothStatus Status { get; }

        public WinBleStatusChangedMessage(object sender, BluetoothStatus status)
        {
            Sender = sender;
            Status = status;
        }
    }
}
