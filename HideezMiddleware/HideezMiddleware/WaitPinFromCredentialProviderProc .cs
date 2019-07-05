using Hideez.SDK.Communication.Utils;
using System;
using System.Threading.Tasks;

namespace HideezMiddleware
{
    internal class WaitPinFromCredentialProviderProc
    {
        readonly CredentialProviderConnection _credentialProvider;
        readonly TaskCompletionSource<string> _tcs = new TaskCompletionSource<string>();

        public WaitPinFromCredentialProviderProc(CredentialProviderConnection credentialProvider)
        {
            _credentialProvider = credentialProvider;
        }

        public async Task<string> Run(int timeout)
        {
            try
            {
                _credentialProvider.OnPinEntered += CredentialProvider_OnPinEntered;

                return await _tcs.Task.TimeoutAfter(timeout);
            }
            catch (TimeoutException)
            {
            }
            finally
            {
                _credentialProvider.OnPinEntered -= CredentialProvider_OnPinEntered;
            }

            return null;
        }

        private void CredentialProvider_OnPinEntered(object sender, string e)
        {
            _tcs.TrySetResult(e);
        }
    }
}
