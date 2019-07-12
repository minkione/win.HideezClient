using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.PasswordManager;
using Hideez.SDK.Communication.Utils;
using HideezMiddleware.Settings;
using NLog;

namespace HideezMiddleware
{
    public class AccessDeniedAuthException : Exception
    {
        public override string Message => "Authorization cancelled: Access denied";
    }

    // Todo: Code cleanup in WorkstationUnlocker after rapid proximity unlock development
    public class WorkstationUnlocker
    {
        readonly ILogger _log = LogManager.GetCurrentClassLogger();
        readonly BleDeviceManager _deviceManager;
        readonly CredentialProviderConnection _credentialProviderConnection;
        readonly RfidServiceConnection _rfidService;
        readonly IBleConnectionManager _connectionManager;
        readonly IScreenActivator _screenActivator;
        readonly ISettingsManager<UnlockerSettings> _unlockerSettingsManager;
        readonly bool _bypassWorkstationOwnershipSecurity;

        HesAppConnection _hesConnection;

        readonly ConcurrentDictionary<string, Guid> _pendingUnlocks =
            new ConcurrentDictionary<string, Guid>();

        // Ignore connect by proximity until settings are loaded. Max possible proximity value is 100
        int _connectProximity = 101;

        List<DeviceUnlockerSettingsInfo> _deviceConnectionFilters = 
            new List<DeviceUnlockerSettingsInfo>();

        // If proximity connection failed due to error, we ignore further attempts 
        // until a manual input from user occurs or credential provider reconnects to service
        // ConcurrentDictionary is used instead of other collections because we need atomic Clear() and efficient Contains()
        readonly ConcurrentDictionary<string, string> _failedProximityConnections = new ConcurrentDictionary<string, string>();

        public WorkstationUnlocker(BleDeviceManager deviceManager,
            HesAppConnection hesConnection,
            CredentialProviderConnection credentialProviderConnection,
            RfidServiceConnection rfidService,
            IBleConnectionManager connectionManager,
            IScreenActivator screenActivator,
            ISettingsManager<UnlockerSettings> unlockerSettingsManager,
            bool bypassWorkstationOwnershipSecurity = false)
        {
            _deviceManager = deviceManager;
            _credentialProviderConnection = credentialProviderConnection;
            _rfidService = rfidService;
            _connectionManager = connectionManager;
            _screenActivator = screenActivator;
            _unlockerSettingsManager = unlockerSettingsManager;
            _bypassWorkstationOwnershipSecurity = bypassWorkstationOwnershipSecurity;

            _rfidService.RfidReceivedEvent += RfidService_RfidReceivedEvent;
            _connectionManager.AdvertismentReceived += ConnectionManager_AdvertismentReceived;

            _credentialProviderConnection.OnProviderConnected += CredentialProviderConnection_OnProviderConnected;
            _rfidService.RfidServiceStateChanged += RfidService_RfidServiceStateChanged;
            _rfidService.RfidReaderStateChanged += RfidService_RfidReaderStateChanged;
            _connectionManager.AdapterStateChanged += ConnectionManager_AdapterStateChanged;
            _unlockerSettingsManager.SettingsChanged += UnlockerSettingsManager_SettingsChanged;

            Task.Run(async () =>
            {
                try
                {
                    var settings = await unlockerSettingsManager.GetSettingsAsync();
                    _connectProximity = settings.UnlockProximity;
                    _deviceConnectionFilters = new List<DeviceUnlockerSettingsInfo>(settings.DeviceUnlockerSettings);
                }
                catch (Exception ex)
                {
                    _log.Error(ex);
                }
            });

            SetHes(hesConnection);
        }

        void SetHes(HesAppConnection hesConnection)
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

        void UnlockerSettingsManager_SettingsChanged(object sender, SettingsChangedEventArgs<UnlockerSettings> e)
        {
            try
            {
                _deviceConnectionFilters = new List<DeviceUnlockerSettingsInfo>(e.NewSettings.DeviceUnlockerSettings);
                _connectProximity = e.NewSettings.UnlockProximity;
                _log.Info("Updated device connection filters received");
                _failedProximityConnections.Clear();
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
        }

        #region Status notification

        void CredentialProviderConnection_OnProviderConnected(object sender, EventArgs e)
        {
            _failedProximityConnections.Clear();
            SendStatusToCredentialProvider();
        }

        void HesConnection_HubConnectionStateChanged(object sender, EventArgs e)
        {
            SendStatusToCredentialProvider();
        }

        void ConnectionManager_AdapterStateChanged(object sender, EventArgs e)
        {
            _failedProximityConnections.Clear();
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
            catch (Exception ex)
            {
                _log.Debug(ex);
            }
        }

        #endregion

        #region Access checks
        bool IsDeviceAuthorized(string mac)
        {
            return _deviceConnectionFilters.Any(s => s.Mac == mac);
        }

        bool IsProximityAllowed(string mac)
        {
            return _deviceConnectionFilters.Any(s => s.Mac == mac && s.AllowProximity);
        }

        bool IsRfidAllowed(string mac)
        {
            return _bypassWorkstationOwnershipSecurity || _deviceConnectionFilters.Any(s => s.Mac == mac && s.AllowRfid);
        }

        bool IsBleTapAllowed(string mac)
        {
            return _bypassWorkstationOwnershipSecurity || _deviceConnectionFilters.Any(s => s.Mac == mac && s.AllowBleTap);
        }
        #endregion

        async void ConnectionManager_AdvertismentReceived(object sender, AdvertismentReceivedEventArgs e)
        {
            try
            {
                // Ble tap. Rssi of -27 was calculated empirically
                if (e.Rssi > -27)
                {
                    var newGuid = Guid.NewGuid();
                    var guid = _pendingUnlocks.GetOrAdd(e.Id, newGuid);

                    if (guid == newGuid)
                    {
                        await UnlockByMac(e.Id);
                        _pendingUnlocks.TryRemove(e.Id, out Guid removed);
                    }
                }
                else
                {
                    // Proximity
                    if (_credentialProviderConnection.IsConnected &&
                        BleUtils.RssiToProximity(e.Rssi) > _connectProximity &&
                        IsProximityAllowed(e.Id) &&
                        !_failedProximityConnections.ContainsKey(e.Id))
                    {
                        var newGuid = Guid.NewGuid();
                        var guid = _pendingUnlocks.GetOrAdd(e.Id, newGuid);

                        if (guid == newGuid)
                        {
                            try
                            {
                                await UnlockByProximity(e.Id);
                            }
                            catch (Exception ex)
                            {
                                _log.Error(ex);
                                _failedProximityConnections.GetOrAdd(e.Id, e.Id);
                            }
                            _pendingUnlocks.TryRemove(e.Id, out Guid removed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                _pendingUnlocks.TryRemove(e.Id, out Guid removed);
            }
        }

        async void RfidService_RfidReceivedEvent(object sender, RfidReceivedEventArgs e)
        {
            await UnlockByRfid(e.Rfid);
        }

        public async Task UnlockByMac(string mac)
        {
            try
            {
                ActivateWorkstationScreen();

                string deviceId = mac.Replace(":", "");

                // Prevent access for unauthorized devices
                if (!IsBleTapAllowed(deviceId))
                    throw new AccessDeniedAuthException();

                await _credentialProviderConnection.SendNotification("Connecting to the device...");
                var device = await _deviceManager.ConnectByMac(mac, timeout: 20_000);
                if (device == null)
                    throw new Exception($"Cannot connect device '{mac}'");

                //await _credentialProviderConnection.SendNotification("Please enter the PIN...");
                //string pin = await new WaitPinFromCredentialProviderProc(_credentialProviderConnection).Run(20_000);
                //if (pin == null)
                //    throw new Exception($"PIN timeout");

                //todo - verify PIN code

                await _credentialProviderConnection.SendNotification("Waiting for the device initialization...");
                await device.WaitInitialization(timeout: 10_000);

                // No point in reading credentials if CredentialProvider is not connected
                if (!_credentialProviderConnection.IsConnected)
                    return;

                // get MAC address from the HES
                var info = await _hesConnection.GetInfoByMac(mac);

                if (info == null)
                    throw new Exception($"Device not found");

                await _credentialProviderConnection.SendNotification("Waiting for the primary account update...");
                await WaitForPrimaryAccountUpdate(info);

                await _credentialProviderConnection.SendNotification("Reading credentials from the device...");
                ushort primaryAccountKey = await DevicePasswordManager.GetPrimaryAccountKey(device);
                var credentials = await GetCredentials(device, primaryAccountKey);

                // send credentials to the Credential Provider to unlock the PC
                await _credentialProviderConnection.SendNotification("Unlocking the PC...");
                await _credentialProviderConnection.SendLogonRequest(credentials.Login, credentials.Password, credentials.PreviousPassword);
            }
            catch (HideezException ex)
            {
                var message = HideezExceptionLocalization.GetErrorAsString(ex);
                _log.Error(message);
                await _credentialProviderConnection.SendNotification("");
                await _credentialProviderConnection.SendError(message);
                throw;
            }
            catch (Exception ex)
            {
                _log.Error(ex);
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

                // Prevent access for unauthorized devices
                if (!IsRfidAllowed(info.DeviceMac.Replace(":", "")))
                    throw new AccessDeniedAuthException();

                await _credentialProviderConnection.SendNotification("Connecting to the device...");
                var device = await _deviceManager.ConnectByMac(info.DeviceMac, timeout: 20_000);
                if (device == null)
                    throw new Exception($"Cannot connect device '{info.DeviceMac}'");

                //await _credentialProviderConnection.SendNotification("Please enter the PIN...");
                //string pin = await new WaitPinFromCredentialProviderProc(_credentialProviderConnection).Run(20_000);
                //if (pin == null)
                //    throw new Exception($"PIN timeout");

                //todo - verify PIN code

                await _credentialProviderConnection.SendNotification("Waiting for the device initialization...");
                await device.WaitInitialization(timeout: 10_000);

                // No point in reading credentials if CredentialProvider is not connected
                if (!_credentialProviderConnection.IsConnected)
                    return;

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
                _log.Error(message);
                await _credentialProviderConnection.SendNotification("");
                await _credentialProviderConnection.SendError(message);
                throw;
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                await _credentialProviderConnection.SendNotification("");
                await _credentialProviderConnection.SendError(ex.Message);
                throw;
            }
        }

        public async Task UnlockByProximity(string mac)
        {
            try
            {
                ActivateWorkstationScreen();

                string deviceId = mac.Replace(":", "");

                // Prevent access for unauthorized devices
                if (!IsProximityAllowed(deviceId))
                    throw new AccessDeniedAuthException();

                await _credentialProviderConnection.SendNotification("Connecting to the device...");
                var device = await _deviceManager.ConnectByMac(mac, timeout: 20_000);
                if (device == null)
                    throw new Exception($"Cannot connect device '{mac}'");

                //await _credentialProviderConnection.SendNotification("Please enter the PIN...");
                //string pin = await new WaitPinFromCredentialProviderProc(_credentialProviderConnection).Run(20_000);
                //if (pin == null)
                //    throw new Exception($"PIN timeout");

                //todo - verify PIN code

                await _credentialProviderConnection.SendNotification("Waiting for the device initialization...");
                await device.WaitInitialization(timeout: 10_000);

                // No point in reading credentials if CredentialProvider is not connected
                if (!_credentialProviderConnection.IsConnected)
                    return;

                // get MAC address from the HES
                var info = await _hesConnection.GetInfoByMac(mac);

                if (info == null)
                    throw new Exception($"Device not found");

                await _credentialProviderConnection.SendNotification("Waiting for the primary account update...");
                await WaitForPrimaryAccountUpdate(info);

                await _credentialProviderConnection.SendNotification("Reading credentials from the device...");
                ushort primaryAccountKey = await DevicePasswordManager.GetPrimaryAccountKey(device);
                var credentials = await GetCredentials(device, primaryAccountKey);

                // send credentials to the Credential Provider to unlock the PC
                await _credentialProviderConnection.SendNotification("Unlocking the PC...");
                await _credentialProviderConnection.SendLogonRequest(credentials.Login, credentials.Password, credentials.PreviousPassword);
            }
            catch (HideezException ex)
            {
                var message = HideezExceptionLocalization.GetErrorAsString(ex);
                _log.Error(message);
                await _credentialProviderConnection.SendNotification("");
                await _credentialProviderConnection.SendError(message);
                throw;
            }
            catch (Exception ex)
            {
                _log.Error(ex);
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

        async Task WaitForPrimaryAccountUpdate(UserInfo info)
        {
            if (_hesConnection == null)
                throw new Exception("Cannot update primary account. Not connected to the HES.");

            if (info.NeedUpdatePrimaryAccount == false)
                return;

            var mac = info.DeviceMac;
            for (int i = 0; i < 10; i++)
            {
                info = await _hesConnection.GetInfoByMac(mac);
                if (info.NeedUpdatePrimaryAccount == false)
                    return;
                await Task.Delay(3000);
            }

            throw new Exception($"Update of the primary account has been timed out");
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
