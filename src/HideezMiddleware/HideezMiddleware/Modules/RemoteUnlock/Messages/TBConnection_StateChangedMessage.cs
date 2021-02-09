using Meta.Lib.Modules.PubSub;
using System;

namespace HideezMiddleware.Modules.RemoteUnlock.Messages
{
    internal sealed class TBConnection_StateChangedMessage : PubSubMessageBase
    {
        public object Sender { get; }
        public EventArgs Args { get; }

        public TBConnection_StateChangedMessage(object sender, EventArgs args)
        {
            Sender = sender;
            Args = args;
        }
    }
}
