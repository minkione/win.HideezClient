using Meta.Lib.Modules.PubSub;
using System;

namespace HideezMiddleware.Modules.Hes.Messages
{
    public sealed class HesAppConnection_HubConnectionStateChangedMessage : PubSubMessageBase
    {
        public object Sender { get; }
        public EventArgs Args { get; }

        public HesAppConnection_HubConnectionStateChangedMessage(object sender, EventArgs args)
        {
            Sender = sender;
            Args = args;
        }
    }
}