using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.Modules.Hes.Messages
{
    public sealed class HesAppConnection_LiftHwVaultStorageLockMessage : PubSubMessageBase
    {
        public object Sender { get; }
        public string SerialNo { get; }

        public HesAppConnection_LiftHwVaultStorageLockMessage(object sender, string serialNo)
        {
            Sender = sender;
            SerialNo = serialNo;
        }
    }
}