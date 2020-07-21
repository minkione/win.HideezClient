using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.IncommingMessages
{
    public sealed class IsSoftwareVaultUnlockModuleEnabledReply : PubSubMessageBase
    {
        public bool IsEnabled { get; set; }

        public IsSoftwareVaultUnlockModuleEnabledReply(bool isEnabled)
        {
            IsEnabled = isEnabled;
        }
    }
}
