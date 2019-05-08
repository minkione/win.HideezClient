using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using System;
using System.Threading.Tasks;

namespace HideezMiddleware
{
    public class WorkstationUnlocker : Logger
    {
        readonly BleDeviceManager _deviceManager;
        readonly ICredentialProviderConnection _credentialProviderConnection;
        readonly RfidServiceConnection _rfidService;
        readonly HesAppConnection _hesConnection;

        public WorkstationUnlocker(BleDeviceManager deviceManager,
            HesAppConnection hesConnection,
            ICredentialProviderConnection credentialProviderConnection,
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
                // get MAC address from the HES
                var info = await _hesConnection.GetInfoByRfid(rfid);

                if (info.IdFromDevice == null)
                    throw new Exception($"Device '{info.DeviceSerialNo}' has not a primary account stored");

                //info.DeviceMac = "D0:A8:9E:6B:CD:8D";
                info.DeviceMac = info.DeviceMac.Replace(":", "");

                var device = _deviceManager.Find(info.DeviceMac);
                if (device == null)
                {
                    // connect Hideez Key with the MAC
                    bool connected = await _deviceManager.ConnectByMac(info.DeviceMac);
                    if (!connected)
                        throw new Exception($"Cannot connect device '{info.DeviceSerialNo}', '{info.DeviceMac}'");

                    device = _deviceManager.Find(info.DeviceMac);
                    if (device == null)
                        throw new Exception($"Cannot connect device '{info.DeviceSerialNo}', '{info.DeviceMac}'");
                }

                await device.WaitAuthentication();

                // get the login and password from the Hideez Key
                string login = await device.ReadStorageAsString((byte)StorageTable.Logins, (ushort)info.IdFromDevice);
                string pass = await device.ReadStorageAsString((byte)StorageTable.Passwords, (ushort)info.IdFromDevice);

                if (login == null || pass == null)
                    throw new Exception($"Cannot read login and password from device '{info.DeviceSerialNo}'");

                // send credentials to the Credential Provider to unlock the PC
                await _credentialProviderConnection.SendLogonRequest(login, pass);
            }
            catch (Exception ex)
            {
                WriteLine(ex);
                await _credentialProviderConnection.SendError(ex.Message);
                throw;
            }
        }
    }
}
