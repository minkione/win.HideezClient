using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.Messages
{
    public sealed class ServiceSettingsChangedMessage : PubSubMessageBase
    {
        public bool SoftwareVaultUnlockEnabled { get; set; }

        public string ServerAddress { get; set; }

        public ServiceSettingsChangedMessage(bool softwareVaultUnlockEnabled, string serverAddress)
        {
            SoftwareVaultUnlockEnabled = softwareVaultUnlockEnabled;
            ServerAddress = serverAddress;
        }
    }
}
