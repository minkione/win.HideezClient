using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.Modules.Hes.Messages
{
    public sealed class HesAppConnection_HUbRFIDIndicatorStateArrivedMessage : PubSubMessageBase
    {
        public object Sender { get; }
        public bool IsEnabled { get; }

        public HesAppConnection_HUbRFIDIndicatorStateArrivedMessage(object sender, bool isEnabled)
        {
            Sender = sender;
            IsEnabled = isEnabled;
        }
    }
}