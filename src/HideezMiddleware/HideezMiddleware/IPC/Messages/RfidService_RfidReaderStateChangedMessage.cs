using Meta.Lib.Modules.PubSub;
using System;

namespace HideezMiddleware.IPC.Messages
{
    public sealed class RfidService_RfidReaderStateChangedMessage : PubSubMessageBase
    {
        public object Sender { get; }

        public EventArgs EventArgs { get; }

        public RfidService_RfidReaderStateChangedMessage(object sender, EventArgs eventArgs)
        {
            Sender = sender;
            EventArgs = eventArgs;
        }
    }
}
