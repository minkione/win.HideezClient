using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;

namespace HideezMiddleware
{
    public class WorkstationUnlocker : Logger
    {
        readonly BleDeviceManager _deviceManager;
        readonly CredentialProviderConnection _credentialProviderConnection;
        readonly RfidServiceConnection _rfidService;
        readonly HesAppConnection _hesConnection;
        readonly IBleConnectionManager _connectionManager;
        readonly IScreenActivator _screenActivator;

        readonly ConcurrentDictionary<string, Guid> _pendingUnlocks =
            new ConcurrentDictionary<string, Guid>();

        public WorkstationUnlocker(BleDeviceManager deviceManager,
            HesAppConnection hesConnection,
            CredentialProviderConnection credentialProviderConnection,
            RfidServiceConnection rfidService,
            IBleConnectionManager connectionManager,
            IScreenActivator screenActivator,
            ILog log)
            : base(nameof(WorkstationUnlocker), log)
        {
            _deviceManager = deviceManager;
            _hesConnection = hesConnection;
            _credentialProviderConnection = credentialProviderConnection;
            _rfidService = rfidService;
            _connectionManager = connectionManager;
            _screenActivator = screenActivator;

            _rfidService.RfidReceivedEvent += RfidService_RfidReceivedEvent;
            _connectionManager.AdvertismentReceived += ConnectionManager_AdvertismentReceived;
        }

        async void ConnectionManager_AdvertismentReceived(object sender, AdvertismentReceivedEventArgs e)
        {
            try
            {
                if (e.Rssi > -25)
                {
                    var newGuid = Guid.NewGuid();
                    var guid = _pendingUnlocks.GetOrAdd(e.Id, newGuid);

                    if (guid == newGuid)
                    {
                        await UnlockByMac(e.Id);
                        _pendingUnlocks.TryRemove(e.Id, out Guid removed);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLine(ex);
                _pendingUnlocks.TryRemove(e.Id, out Guid removed);
            }
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

        public async Task UnlockByMac(string mac)
        {
            try
            {
                ActivateWorkstationScreen();
                await _credentialProviderConnection.SendNotification("Connecting device...");

                string deviceId = mac.Replace(":", "");

                var device = await _deviceManager.ConnectByMac(mac, timeout: 20_000);
                if (device == null)
                    throw new Exception($"Cannot connect device '{mac}'");

                await device.WaitInitialization(timeout: 10_000);

                //todo - wait for primary account update?

                string login = null;
                string pass = null;
                string prevPass = "";

                ushort primaryAccountKey = await GetPrimaryAccountKey(device);
                if (primaryAccountKey == 0)
                {
                    var credentials = await device.ReadStorageAsString(
                        (byte)StorageTable.BondVirtualTable1, 
                        (ushort)BondVirtualTable1Item.PcUnlockCredentials);

                    var parts = credentials.Split('\n');
                    if (parts.Length >= 2)
                    {
                        login = parts[0];
                        pass = parts[1];
                    }
                    if (parts.Length >= 3)
                    {
                        prevPass = parts[2];
                    }

                    if (login == null || pass == null)
                        throw new Exception($"Device '{mac}' has not a primary account stored");
                }
                else
                {
                    // get the login and password from the Hideez Key
                    login = await device.ReadStorageAsString((byte)StorageTable.Logins, primaryAccountKey);
                    pass = await device.ReadStorageAsString((byte)StorageTable.Passwords, primaryAccountKey);
                    prevPass = ""; //todo

                    if (login == null || pass == null)
                        throw new Exception($"Cannot read login or password from device '{mac}'");
                }

                // send credentials to the Credential Provider to unlock the PC
                await _credentialProviderConnection.SendLogonRequest(login, pass, prevPass);
            }
            catch (Exception ex)
            {
                WriteLine(ex);
                await _credentialProviderConnection.SendNotification("");
                await _credentialProviderConnection.SendError(ex.Message);
                throw;
            }
        }

        public async Task UnlockByRfid(string rfid)
        {
            try
            {
                ActivateWorkstationScreen();
                await _credentialProviderConnection.SendNotification("Connecting device...");

                // get MAC address from the HES
                var info = await _hesConnection.GetInfoByRfid(rfid);

                if (info == null)
                    throw new Exception($"Device not found");

                if (info.IdFromDevice == 0 && !info.NeedUpdatePrimaryAccount)
                    throw new Exception($"Device '{info.DeviceSerialNo}' has not a primary account stored");

                var device = await _deviceManager.ConnectByMac(info.DeviceMac, timeout: 20_000);
                if (device == null)
                    throw new Exception($"Cannot connect device '{info.DeviceMac}'");

                await device.WaitInitialization(timeout: 10_000);
                await WaitForPrimaryAccountUpdate(rfid, info);
                
                // get the login and password from the Hideez Key
                string login = await device.ReadStorageAsString((byte)StorageTable.Logins, info.IdFromDevice);
                string pass = await device.ReadStorageAsString((byte)StorageTable.Passwords, info.IdFromDevice);
                string prevPass = ""; //todo

                if (login == null || pass == null)
                    throw new Exception($"Cannot read login or password from the device '{info.DeviceSerialNo}'");

                // send credentials to the Credential Provider to unlock the PC
                await _credentialProviderConnection.SendLogonRequest(login, pass, prevPass);
            }
            catch (Exception ex)
            {
                WriteLine(ex);
                await _credentialProviderConnection.SendNotification("");
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

            throw new Exception($"Update of the primary account has been timed out");
        }

        static async Task<ushort> GetPrimaryAccountKey(IDeviceStorage storage)
        {
            string primaryAccountKeyString = await storage.ReadStorageAsString((byte)StorageTable.Config, (ushort)StorageConfigItem.PrimaryAccountKey);
            if (string.IsNullOrEmpty(primaryAccountKeyString))
                return 0;

            try
            {
                ushort primaryAccountKey = Convert.ToUInt16(primaryAccountKeyString);
                return primaryAccountKey;
            }
            catch (FormatException)
            {
            }
            catch (OverflowException)
            {
            }
            return 0;
        }

        async void ActivateWorkstationScreen()
        {
            await Task.Run(() => { _screenActivator?.ActivateScreen(); });
        }
    }
}
