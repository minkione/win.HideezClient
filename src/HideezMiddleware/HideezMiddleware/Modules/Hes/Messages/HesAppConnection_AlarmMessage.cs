using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.Modules.Hes.Messages
{
    public sealed class HesAppConnection_AlarmMessage : PubSubMessageBase
    {
        public object Sender { get; }
        public bool Args { get; }

        public HesAppConnection_AlarmMessage(object sender, bool args)
        {
            Sender = sender;
            Args = args;
        }
    }
}