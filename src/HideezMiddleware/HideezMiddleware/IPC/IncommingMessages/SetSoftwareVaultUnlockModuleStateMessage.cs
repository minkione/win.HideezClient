using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.IncommingMessages
{
    public sealed class SetSoftwareVaultUnlockModuleStateMessage : PubSubMessageBase
    {
        public bool Enabled { get; set; }

        public SetSoftwareVaultUnlockModuleStateMessage(bool enabled)
        {
            Enabled = enabled;
        }
    }
}
