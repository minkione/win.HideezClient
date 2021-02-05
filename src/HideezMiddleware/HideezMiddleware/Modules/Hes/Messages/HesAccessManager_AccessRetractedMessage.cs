using Meta.Lib.Modules.PubSub;
using System;

namespace HideezMiddleware.Modules.Hes.Messages
{
    public sealed class HesAccessManager_AccessRetractedMessage : PubSubMessageBase
    {
        public object Sender { get; }
        public EventArgs EventArgs { get; }

        public HesAccessManager_AccessRetractedMessage(object sender, EventArgs eventArgs)
        {
            Sender = sender;
            EventArgs = eventArgs;
        }
    }
}