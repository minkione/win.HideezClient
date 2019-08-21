using System;
using System.Diagnostics;
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

        internal async Task ShowPinUi(string deviceId)
        {
            if (_credentialProviderConnection.IsConnected)
                await _credentialProviderConnection.ShowPinUi(deviceId);
        }

        internal async Task<string> GetPin(string deviceId, int timeout)
        {
            if (_credentialProviderConnection.IsConnected)
                return await _credentialProviderConnection.GetPin(deviceId, timeout);
            return null;

            ////todo
            //await Task.Delay(2000);
            //return "1111";
        }

        internal async Task<string> GetConfirmedPin(string deviceId, int timeout)
        {
            if (_credentialProviderConnection.IsConnected)
                return await _credentialProviderConnection.GetConfirmedPin(deviceId, timeout);
            return null;
        }

        internal async Task HidePinUi()
        {
            Debug.WriteLine(">>>>>>>>>>>>>>> HidePinUi");
            if (_credentialProviderConnection.IsConnected)
            {
                await _credentialProviderConnection.HidePinUi();
                await _credentialProviderConnection.SendNotification("");
            }
        }

        internal async Task SendStatus(string status)
        {
            if (_credentialProviderConnection.IsConnected)
                await _credentialProviderConnection.SendStatus(status);
        }

        internal async Task SendNotification(string notification)
        {
            if (_credentialProviderConnection.IsConnected)
                await _credentialProviderConnection.SendNotification(notification);
        }

        internal async Task SendError(string error)
        {
            if (_credentialProviderConnection.IsConnected)
                await _credentialProviderConnection.SendError(error);
        }


    }
}
