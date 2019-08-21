using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HideezMiddleware
{
    public enum BluetoothStatus
    {
        Ok,
        Unknown,
        Resetting,
        Unsupported,
        Unauthorized,
        PoweredOff,
    }

    public enum RfidStatus
    {
        Ok,
        RfidServiceNotConnected,
        RfidReaderNotConnected,
    }

    public enum HesStatus
    {
        Ok,
        HesNotConnected,
    }

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

        internal async Task SendStatus(BluetoothStatus bluetoothStatus, RfidStatus rfidStatus, HesStatus hesStatus)
        {
            if (_credentialProviderConnection.IsConnected)
            {
                var statuses = new List<string>();

                if (bluetoothStatus != BluetoothStatus.Ok)
                    statuses.Add($"Bluetooth not available (state: {bluetoothStatus})");

                // Todo: Check if user selected RFID for usage on this computer
                if (rfidStatus != RfidStatus.Ok)
                {
                    if (rfidStatus == RfidStatus.RfidServiceNotConnected)
                        statuses.Add("RFID service not connected");
                    else if (rfidStatus == RfidStatus.RfidReaderNotConnected)
                        statuses.Add("RFID reader not connected");
                }

                if (hesStatus != HesStatus.Ok)
                    statuses.Add("HES not connected");


                if (statuses.Count > 0)
                    await _credentialProviderConnection.SendStatus($"ERROR: {string.Join("; ", statuses)}");
                else
                    await _credentialProviderConnection.SendStatus(string.Empty);
            }
            else
            {

            }
        }

        internal async Task SendNotification(string notification)
        {
            if (_credentialProviderConnection.IsConnected)
            {
                await _credentialProviderConnection.SendNotification(notification);
            }
            else
            {

            }
        }

        internal async Task SendError(string error)
        {
            if (_credentialProviderConnection.IsConnected)
            {
                await _credentialProviderConnection.SendError(error);
            }
            else
            {

            }
        }
    }
}
