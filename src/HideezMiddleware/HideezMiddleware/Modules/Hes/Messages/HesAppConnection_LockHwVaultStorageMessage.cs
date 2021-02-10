using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.Modules.Hes.Messages
{
    public sealed class HesAppConnection_LockHwVaultStorageMessage : PubSubMessageBase
    {
        public object Sender { get; }
        public string SerialNo { get; }

        public HesAppConnection_LockHwVaultStorageMessage(object sender, string serialNo)
        {
            Sender = sender;
            SerialNo = serialNo;
        }
    }
}