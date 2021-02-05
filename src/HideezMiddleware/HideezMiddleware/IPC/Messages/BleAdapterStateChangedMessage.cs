using Hideez.SDK.Communication.Interfaces;
using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.Messages
{
    public sealed class BleAdapterStateChangedMessage : PubSubMessageBase
    {
        public IBleConnectionManager Sender { get; }

        public BluetoothAdapterState NewState { get; }

        public BleAdapterStateChangedMessage(IBleConnectionManager sender, BluetoothAdapterState newState)
        {
            Sender = sender;
            NewState = newState;
        }
    }
}
