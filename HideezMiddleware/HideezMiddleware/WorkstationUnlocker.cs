using System;
using System.Threading.Tasks;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.Log;

namespace HideezMiddleware
{
    public class WorkstationUnlocker : Logger
    {
        readonly BleDeviceManager _deviceManager;
        readonly CredentialProviderConnection _credentialProviderConnection;
        readonly RfidServiceConnection _rfidService;
        readonly HesAppConnection _hesConnection;

        public WorkstationUnlocker(BleDeviceManager deviceManager,
            HesAppConnection hesConnection,
            CredentialProviderConnection credentialProviderConnection,
            RfidServiceConnection rfidService,
            ILog log)
            : base(nameof(WorkstationUnlocker), log)
        {
            _deviceManager = deviceManager;
            _hesConnection = hesConnection;
            _credentialProviderConnection = credentialProviderConnection;
            _rfidService = rfidService;

            _rfidService.RfidReceivedEvent += RfidService_RfidReceivedEvent;
        }

        async void RfidService_RfidReceivedEvent(object sender, RfidReceivedEventArgs e)
        {
            try
            {
                await UnlockByRfid(e.Rfid);
            }
            catch (Exception ex)
            {
                WriteLine(ex);
            }
        }

        public async Task UnlockByRfid(string rfid)
        {
            try
            {
                //await _credentialProviderConnection.SendNotification("Connecting device...");


                // get MAC address from the HES
                var info = await _hesConnection.GetInfoByRfid(rfid);

                if (info == null)
                    throw new Exception($"Device not found");

                if (info.IdFromDevice == null)
                    throw new Exception($"Device '{info.DeviceSerialNo}' has not a primary account stored");

                //info.DeviceMac = "D0:A8:9E:6B:CD:8D";
                string mac = info.DeviceMac.Replace(":", "");

                var device = _deviceManager.Find(mac);
                if (device == null)
                {
                    // connect Hideez Key with the MAC
                    device = await _deviceManager.ConnectByMac(mac);

                    if (device == null)
                        throw new Exception($"Cannot connect device '{info.DeviceSerialNo}', '{info.DeviceMac}'");
                }
                
                await device.WaitAuthentication(timeout: 10_000);
                await WaitForPrimaryAccountUpdate(rfid, info);
                
                // get the login and password from the Hideez Key
                string login = await device.ReadStorageAsString((byte)StorageTable.Logins, (ushort)info.IdFromDevice);
                string pass = await device.ReadStorageAsString((byte)StorageTable.Passwords, (ushort)info.IdFromDevice);
                string prevPass = ""; //todo

                if (login == null || pass == null)
                    throw new Exception($"Cannot read login and password from device '{info.DeviceSerialNo}'");

                // send credentials to the Credential Provider to unlock the PC
                await _credentialProviderConnection.SendLogonRequest(login, pass, prevPass);
            }
            catch (Exception ex)
            {
                WriteLine(ex);
                await _credentialProviderConnection.SendError(ex.Message);
                throw;
            }
        }

        async Task WaitForPrimaryAccountUpdate(string rfid, UserInfo info)
        {
            if (info.NeedUpdatePrimaryAccount == false)
                return;

            for (int i = 0; i < 10; i++)
            {
                info = await _hesConnection.GetInfoByRfid(rfid);
                if (info.NeedUpdatePrimaryAccount == false)
                    return;
                await Task.Delay(3000);
            }

            throw new Exception($"Update of the primary account is timed out");
        }
    }
}
