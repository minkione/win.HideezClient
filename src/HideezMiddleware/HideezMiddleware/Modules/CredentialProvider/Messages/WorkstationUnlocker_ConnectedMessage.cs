using Meta.Lib.Modules.PubSub;
using System;

namespace HideezMiddleware.Modules.CredentialProvider.Messages
{
    internal sealed class WorkstationUnlocker_ConnectedMessage : PubSubMessageBase
    {
        public object Sender { get; }
        public EventArgs Args { get; }

        public WorkstationUnlocker_ConnectedMessage(object sender, EventArgs args)
        {
            Sender = sender;
            Args = args;
        }
    }
}
