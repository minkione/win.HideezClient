using Hideez.SDK.Communication.Log;
using HideezMiddleware.CredentialProvider;
using HideezMiddleware.IPC.Messages;
using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.Modules.CredentialProvider
{
    public sealed class CredentialProviderModule : ModuleBase
    {
        readonly CredentialProviderProxy _credentialProviderProxy;

        public CredentialProviderModule(CredentialProviderProxy credentialProviderProxy, IMetaPubSub messenger, ILog log)
            : base(messenger, nameof(CredentialProviderModule), log)
        {
            _credentialProviderProxy = credentialProviderProxy;

            _credentialProviderProxy.CommandLinkPressed += CredentialProviderProxy_CommandLinkPressed;
            _credentialProviderProxy.Start();
        }

        private async void CredentialProviderProxy_CommandLinkPressed(object sender, System.EventArgs e)
        {
            await _messenger.Publish(new CredentialProvider_CommandLinkPressedMessage(sender, e));
        }
    }
}
