using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.Utils;

namespace HideezMiddleware
{
    public class WorkstationUnlocker : Logger
    {
        readonly BleDeviceManager _deviceManager;
        readonly CredentialProviderConnection _credentialProviderConnection;
        readonly RfidServiceConnection _rfidService;
        readonly IBleConnectionManager _connectionManager;
        readonly IScreenActivator _screenActivator;

        HesAppConnection _hesConnection;

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
            _credentialProviderConnection = credentialProviderConnection;
            _rfidService = rfidService;
            _connectionManager = connectionManager;
            _screenActivator = screenActivator;

            _rfidService.RfidReceivedEvent += RfidService_RfidReceivedEvent;
            _connectionManager.AdvertismentReceived += ConnectionManager_AdvertismentReceived;

            _credentialProviderConnection.ConnectionStateChanged += _credentialProviderConnection_ConnectionStateChanged;
            _rfidService.RfidServiceStateChanged += RfidService_RfidServiceStateChanged;
            _rfidService.RfidReaderStateChanged += RfidService_RfidReaderStateChanged;
            _connectionManager.AdapterStateChanged += ConnectionManager_AdapterStateChanged;

            SetHes(hesConnection);
        }

        public void SetHes(HesAppConnection hesConnection)
        {
            if (_hesConnection != null)
            {
                _hesConnection.HubConnectionStateChanged -= HesConnection_HubConnectionStateChanged;
                _hesConnection = null;
            }

            if (hesConnection != null)
            {
                _hesConnection = hesConnection;
                _hesConnection.HubConnectionStateChanged += HesConnection_HubConnectionStateChanged;
            }
        }

        #region Status notification

        void _credentialProviderConnection_ConnectionStateChanged(object sender, EventArgs e)
        {
            SendStatusToCredentialProvider();
        }

        void HesConnection_HubConnectionStateChanged(object sender, EventArgs e)
        {
            SendStatusToCredentialProvider();
        }

        void ConnectionManager_AdapterStateChanged(object sender, EventArgs e)
        {
            SendStatusToCredentialProvider();
        }

        void RfidService_RfidReaderStateChanged(object sender, EventArgs e)
        {
            SendStatusToCredentialProvider();
        }

        void RfidService_RfidServiceStateChanged(object sender, EventArgs e)
        {
            SendStatusToCredentialProvider();
        }

        async void SendStatusToCredentialProvider()
        {
            try
            {
                if (_credentialProviderConnection.Connected)
                {
                    var statuses = new List<string>();

                    // Bluetooth
                    switch (_connectionManager.State)
                    {
                        case BluetoothAdapterState.PoweredOn:
                        case BluetoothAdapterState.LoadingKnownDevices:
                            break;
                        default:
                            statuses.Add($"Bluetooth not available (state: {_connectionManager.State})");
                            break;
                    }

                    // RFID
                    if (!_rfidService.ServiceConnected)
                        statuses.Add("RFID service not connected");
                    else if (!_rfidService.ReaderConnected)
                        statuses.Add("RFID reader not connected");

                    // Server
                    if (_hesConnection == null || _hesConnection.State == HesConnectionState.Disconnected)
                        statuses.Add("HES not connected");

                    if (statuses.Count > 0)
                        await _credentialProviderConnection.SendStatus($"ERROR: {string.Join("; ", statuses)}");
                    else
                        await _credentialProviderConnection.SendStatus(string.Empty);
                }
            }
            catch (Exception ex)
            {
                WriteDebugLine(ex);
            }
        }

        #endregion

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

                string deviceId = mac.Replace(":", "");

                //await _credentialProviderConnection.SendStatus("Status: OK");
                await _credentialProviderConnection.SendNotification("Connecting to the device...");
                await Task.Delay(3000);
                var device = await _deviceManager.ConnectByMac(mac, timeout: 20_000);
                if (device == null)
                    throw new Exception($"Cannot connect device '{mac}'");

                await _credentialProviderConnection.SendNotification("Waiting for the device initialization...");
                await device.WaitInitialization(timeout: 10_000);

                //todo - wait for primary account update?

                await _credentialProviderConnection.SendNotification("Reading credentials from the device...");
                ushort primaryAccountKey = await GetPrimaryAccountKey(device);
                var credentials = await GetCredentials(device, primaryAccountKey);

                // send credentials to the Credential Provider to unlock the PC
                await _credentialProviderConnection.SendNotification("Unlocking the PC...");
                await _credentialProviderConnection.SendLogonRequest(credentials.Login, credentials.Password, credentials.PreviousPassword);
            }
            catch (HideezException ex)
            {
                var message = HideezExceptionLocalization.GetErrorAsString(ex);
                WriteLine(message);
                await _credentialProviderConnection.SendNotification("");
                await _credentialProviderConnection.SendError(message);
                throw;
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
                await _credentialProviderConnection.SendNotification("Connecting to the HES server...");

                if (_hesConnection == null)
                    throw new Exception("Cannot connect device. Not connected to the HES.");

                // get MAC address from the HES
                var info = await _hesConnection.GetInfoByRfid(rfid);

                if (info == null)
                    throw new Exception($"Device not found");

                await _credentialProviderConnection.SendNotification("Connecting to the device...");
                var device = await _deviceManager.ConnectByMac(info.DeviceMac, timeout: 20_000);
                if (device == null)
                    throw new Exception($"Cannot connect device '{info.DeviceMac}'");

                await _credentialProviderConnection.SendNotification("Waiting for the device initialization...");
                await device.WaitInitialization(timeout: 10_000);

                await _credentialProviderConnection.SendNotification("Waiting for the primary account update...");
                await WaitForPrimaryAccountUpdate(rfid, info);

                await _credentialProviderConnection.SendNotification("Reading credentials from the device...");
                var credentials = await GetCredentials(device, info.IdFromDevice);


                // send credentials to the Credential Provider to unlock the PC
                await _credentialProviderConnection.SendNotification("Unlocking the PC...");
                await _credentialProviderConnection.SendLogonRequest(credentials.Login, credentials.Password, credentials.PreviousPassword);
            }
            catch (HideezException ex)
            {
                var message = HideezExceptionLocalization.GetErrorAsString(ex);
                WriteLine(message);
                await _credentialProviderConnection.SendNotification("");
                await _credentialProviderConnection.SendError(message);
                throw;
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
            if (_hesConnection == null)
                throw new Exception("Cannot update primary account. Not connected to the HES.");

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

        async Task<Credentials> GetCredentials(IDevice device, ushort key)
        {
            Credentials credentials;

            if (key == 0)
            {
                var str = await device.ReadStorageAsString(
                    (byte)StorageTable.BondVirtualTable1,
                    (ushort)BondVirtualTable1Item.PcUnlockCredentials);

                if (str != null)
                {
                    var parts = str.Split('\n');
                    if (parts.Length >= 2)
                    {
                        credentials.Login = parts[0];
                        credentials.Password = parts[1];
                    }
                    if (parts.Length >= 3)
                    {
                        credentials.PreviousPassword = parts[2];
                    }
                }

                if (credentials.IsEmpty)
                    throw new Exception($"Device '{device.SerialNo}' has not a primary account stored");
            }
            else
            {
                // get the login and password from the Hideez Key
                credentials.Login = await device.ReadStorageAsString((byte)StorageTable.Logins, key);
                credentials.Password = await device.ReadStorageAsString((byte)StorageTable.Passwords, key);
                credentials.PreviousPassword = ""; //todo

                if (credentials.IsEmpty)
                    throw new Exception($"Cannot read login or password from the device '{device.SerialNo}'");
            }

            return credentials;
        }

        async void ActivateWorkstationScreen()
        {
            await Task.Run(() => { _screenActivator?.ActivateScreen(); });
        }


    }
}
