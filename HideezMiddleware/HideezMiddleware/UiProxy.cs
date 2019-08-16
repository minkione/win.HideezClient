using System;
using System.Threading.Tasks;

namespace HideezMiddleware
{
    public class UiProxy
    {
        readonly CredentialProviderConnection _credentialProviderConnection;

        public UiProxy(CredentialProviderConnection credentialProviderConnection)
        {
            _credentialProviderConnection = credentialProviderConnection;
        }

        internal async Task<string> GetPin(int timeout)
        {
            //todo
            await Task.Delay(2000);
            return "1111";
        }

        internal async Task SendStatus(string status)
        {
            await _credentialProviderConnection.SendStatus(status);
        }

        internal async Task SendNotification(string notification)
        {
            await _credentialProviderConnection.SendNotification(notification);
        }

        internal async Task SendError(string error)
        {
            await _credentialProviderConnection.SendError(error);
        }
    }
}
