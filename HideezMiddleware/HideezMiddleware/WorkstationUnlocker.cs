using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.Log;
using System;

namespace HideezMiddleware
{
    public class WorkstationUnlocker : Logger
    {
        readonly BleDeviceManager _deviceManager;
        readonly CredentialProviderConnection _credentialProviderConnection;
        readonly RfidServiceConnection _rfidService;

        public WorkstationUnlocker(BleDeviceManager deviceManager,
            CredentialProviderConnection credentialProviderConnection,
            RfidServiceConnection rfidService,
            ILog log)
            : base(nameof(WorkstationUnlocker), log)
        {
            _deviceManager = deviceManager;
            _credentialProviderConnection = credentialProviderConnection;
            _rfidService = rfidService;

            _rfidService.RfidReceivedEvent += RfidService_RfidReceivedEvent;
        }

        async void RfidService_RfidReceivedEvent(object sender, RfidReceivedEventArgs e)
        {
            try
            {
                // get MAC address from the HES
                string mac = "D0A89E6BCD8D";

                var device = _deviceManager.Find(mac);
                if (device == null)
                {
                    // connect Hideez Key with the MAC
                    bool connected = await _deviceManager.ConnectByMac(mac);
                    if (!connected)
                        throw new Exception($"Cannot connect device '{mac}'");

                    device = _deviceManager.Find(mac);
                    if (device == null)
                        throw new Exception($"Cannot connect device '{mac}'");
                }


                await device.WaitAuthentication();

                // get the password from the Hideez Key

                // send credentials to the Credential Provider to unlock the PC
                await _credentialProviderConnection.SendLogonRequest(@"", "");
            }
            catch (Exception ex)
            {
                await _credentialProviderConnection.SendError(ex.Message);
            }
        }
    }
}
