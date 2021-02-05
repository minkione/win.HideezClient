using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.Modules.Hes.Messages
{
    public sealed class HesAppConnection_LiftHwVaultStorageLockMessage : PubSubMessageBase
    {
        public object Sender { get; }
        public string Args { get; }

        public HesAppConnection_LiftHwVaultStorageLockMessage(object sender, string args)
        {
            Sender = sender;
            Args = args;
        }
    }
}