using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.Command;
using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.PasswordManager;
using Hideez.SDK.Communication.Utils;
using Hideez.SDK.Communication.WorkstationEvents;
using HideezMiddleware.Settings;
using HideezMiddleware.Utils;
using NLog;

namespace HideezMiddleware
{
    public class AccessDeniedAuthException : Exception
    {
        public override string Message => "Authorization cancelled: Access denied";
    }

    // Todo: Code cleanup in WorkstationUnlocker after rapid proximity unlock development
    public class ConnectionFlowProcessor
    {
        readonly ILogger _log = LogManager.GetCurrentClassLogger();
        readonly BleDeviceManager _deviceManager;
        readonly RfidServiceConnection _rfidService;
        readonly IBleConnectionManager _connectionManager;
        readonly IWorkstationUnlocker _workstationUnlocker;
        readonly IScreenActivator _screenActivator;
        readonly ISettingsManager<UnlockerSettings> _unlockerSettingsManager;
        readonly UiProxy _ui;
        readonly bool _ignoreWorkstationOwnershipSecurity;

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
        readonly ConcurrentDictionary<string, string> _proximityAccessBlacklist = new ConcurrentDictionary<string, string>();
        readonly ConcurrentDictionary<string, string> _rfidAccessBlacklist = new ConcurrentDictionary<string, string>();
        readonly ConcurrentDictionary<string, string> _bleAccessBlacklist = new ConcurrentDictionary<string, string>();

        public ConnectionFlowProcessor(BleDeviceManager deviceManager,
            HesAppConnection hesConnection,
            RfidServiceConnection rfidService,
            IBleConnectionManager connectionManager,
            IWorkstationUnlocker workstationUnlocker,
            IScreenActivator screenActivator,
            ISettingsManager<UnlockerSettings> unlockerSettingsManager,
            UiProxy ui,
            bool ignoreWorkstationOwnershipSecurity = false)
        {
#if DEBUG
            ignoreWorkstationOwnershipSecurity = true;
#endif

            _deviceManager = deviceManager;
            _rfidService = rfidService;
            _connectionManager = connectionManager;
            _workstationUnlocker = workstationUnlocker;
            _screenActivator = screenActivator;
            _unlockerSettingsManager = unlockerSettingsManager;
            _ui = ui;
            _ignoreWorkstationOwnershipSecurity = ignoreWorkstationOwnershipSecurity;

            _rfidService.RfidReceivedEvent += RfidService_RfidReceivedEvent;
            _connectionManager.AdvertismentReceived += ConnectionManager_AdvertismentReceived;

            //_credentialProviderConnection.OnProviderConnected += CredentialProviderConnection_OnProviderConnected;
            //_rfidService.RfidServiceStateChanged += RfidService_RfidServiceStateChanged;
            //_rfidService.RfidReaderStateChanged += RfidService_RfidReaderStateChanged;
            //_connectionManager.AdapterStateChanged += ConnectionManager_AdapterStateChanged;
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
                //_hesConnection.HubConnectionStateChanged -= HesConnection_HubConnectionStateChanged;
                _hesConnection = null;
            }

            if (hesConnection != null)
            {
                _hesConnection = hesConnection;
                //_hesConnection.HubConnectionStateChanged += HesConnection_HubConnectionStateChanged;
            }
        }

        void UnlockerSettingsManager_SettingsChanged(object sender, SettingsChangedEventArgs<UnlockerSettings> e)
        {
            try
            {
                _deviceConnectionFilters = new List<DeviceUnlockerSettingsInfo>(e.NewSettings.DeviceUnlockerSettings);
                _connectProximity = e.NewSettings.UnlockProximity;
                _log.Info("New device connection filters received");
                ClearAccessBlacklists();
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
        }

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
            return _ignoreWorkstationOwnershipSecurity || _deviceConnectionFilters.Any(s => s.Mac == mac && s.AllowRfid);
        }

        bool IsBleTapAllowed(string mac)
        {
            return _ignoreWorkstationOwnershipSecurity || _deviceConnectionFilters.Any(s => s.Mac == mac && s.AllowBleTap);
        }
        #endregion

        async void ConnectionManager_AdvertismentReceived(object sender, AdvertismentReceivedEventArgs e)
        {
            var mac = MacUtils.GetMacFromShortMac(e.Id);
            try
            {
                // Ble tap. Rssi of -27 was calculated empirically
                if (e.Rssi > -27 && !_bleAccessBlacklist.ContainsKey(mac))
                {
                    await MainWorkflow(mac);
                    //var newGuid = Guid.NewGuid();
                    //var guid = _pendingUnlocks.GetOrAdd(mac, newGuid);

                    //if (guid == newGuid)
                    //{
                    //    await UnlockByMac(mac);
                    //    _pendingUnlocks.TryRemove(mac, out Guid removed);
                    //}
                }
                else
                {
                    // Proximity
                    // Connection occurs only when workstation is locked
                    if (_workstationUnlocker.IsConnected
                        && BleUtils.RssiToProximity(e.Rssi) > _connectProximity
                        && IsProximityAllowed(mac)
                        && !_proximityAccessBlacklist.ContainsKey(mac))
                    {
                        var newGuid = Guid.NewGuid(); //todo - dublicated code
                        var guid = _pendingUnlocks.GetOrAdd(mac, newGuid);

                        if (guid == newGuid)
                        {
                            try
                            {
                                await UnlockByProximity(mac);
                                _pendingUnlocks.TryRemove(mac, out Guid removed);
                            }
                            finally
                            {
                                // Max of 1 attempt per device, either successfull or no
                                _proximityAccessBlacklist.GetOrAdd(mac, mac);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                _pendingUnlocks.TryRemove(mac, out Guid removed);
            }
        }

        //------------------------------
        async Task MainWorkflow(string mac)
        {
            //[1] ignore, if workflow for this device already initialized
            var newGuid = Guid.NewGuid();
            var guid = _pendingUnlocks.GetOrAdd(mac, newGuid); //todo - replace to TryAdd

            if (guid != newGuid)
                return;

            try
            {
                //[2] connect device
                IDevice device = await ConnectDevice(mac);

                //[3] wait auth and init
                await WaitDeviceInitialization(mac, device);


                if (device.AccessLevel.IsLocked)
                {
                    //if (!device.HasUpdatedRemotelly) //todo
                    {
                        // request hes to update this device
                        //_hesConnection?.UpdateDevice(device);//todo
                    }
                    //else
                    {
                        // disconnect device
                        await device.Disconnect();

                        // show error message
                        throw new HideezException(HideezErrorCode.DeviceIsLocked);
                    }
                }
                else
                {
                    if (!await MasterKeyWorkflow(device, 10))
                        throw new HideezException(HideezErrorCode.HesAuthenticationFailed);

                    var t1 = ButtonWorkflow(device, 10);
                    var t2 = PinWorkflow(device, 10);

                    await Task.WhenAll(t1, t2);

                    if (t1.Result && t2.Result)
                    {
                        await _ui.SendNotification("Reading credentials from the device...");
                        ushort primaryAccountKey = await DevicePasswordManager.GetPrimaryAccountKey(device);
                        var credentials = await GetCredentials(device, primaryAccountKey);

                        // send credentials to the Credential Provider to unlock the PC
                        await _ui.SendNotification("Unlocking the PC...");
                        var logonSuccessful = await _workstationUnlocker.SendLogonRequest(credentials.Login, credentials.Password, credentials.PreviousPassword);

                        if (!logonSuccessful)
                            await device.Disconnect();
                    }
                }

            }
            catch (HideezException ex)
            {
                var message = HideezExceptionLocalization.GetErrorAsString(ex);
                _log.Error(message);
                await _ui.SendNotification("");
                await _ui.SendError(message);
                throw;
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                await _ui.SendNotification("");
                await _ui.SendError(ex.Message);
                throw;
            }
            finally
            {
                //todo - when remove from _pendingUnlocks?
                _pendingUnlocks.TryRemove(mac, out Guid removed);
                await Task.Delay(3000);
            }
        }

        private async Task<bool> MasterKeyWorkflow(IDevice device, int timeout)
        {
            //todo - replase with hes.UpdateDevice()
            var accessParams = new AccessParams()
            {
                MasterKey_Bond = true,
                MasterKey_Connect = false,
                MasterKey_Link = false,
                MasterKey_Channel = false,

                Button_Bond = false,
                Button_Connect = false,
                Button_Link = true,
                Button_Channel = true,

                Pin_Bond = false,
                Pin_Connect = true,
                Pin_Link = false,
                Pin_Channel = false,

                PinMinLength = 4,
                PinMaxTries = 3,
                MasterKeyExpirationPeriod = 24 * 60 * 60,
                PinExpirationPeriod = 15 * 60,
                ButtonExpirationPeriod = 15,
            };

            await device.Access(
                DateTime.UtcNow,
                Encoding.UTF8.GetBytes("passphrase"),
                accessParams);

            await Task.Delay(1000);
            return true;
        }

        Task<bool> ButtonWorkflow(IDevice device, int timeout)
        {
            return device.WaitButtonConfirmation(timeout);
        }

        async Task<bool> PinWorkflow(IDevice device, int timeout)
        {
            if (device.AccessLevel.IsPinRequired)
            {
                string pin = await _ui.GetPin(timeout);
                return await device.EnterPin(pin);
            }
            return false;
        }

        //------------------------------------

        async void RfidService_RfidReceivedEvent(object sender, RfidReceivedEventArgs e)
        {
            try
            {
                if (!_rfidAccessBlacklist.ContainsKey(e.Rfid))
                    await UnlockByRfid(e.Rfid);
            }
            catch (Exception)
            {
                // silent handling...
            }
        }

        async Task<IDevice> ConnectDevice(string mac)
        {
            await _ui.SendNotification("Connecting to the device...");
            var device = await _deviceManager.ConnectByMac(mac, BleDefines.ConnectDeviceTimeout);
            if (device == null)
            {
                await _deviceManager.RemoveByMac(mac);
                throw new Exception($"Failed to connect device '{mac}'. Please try again.");
            }
            return device;
        }

        async Task WaitDeviceInitialization(string mac, IDevice device)
        {
            await _ui.SendNotification("Waiting for the device initialization...");
            await device.WaitInitialization(BleDefines.DeviceInitializationTimeout);
            if (device.IsErrorState)
            {
                await _deviceManager.Remove(device);
                throw new Exception($"Failed to initialize device connection '{mac}'. Please try again.");
            }
        }

        /// <summary>
        /// Unlock workstation using device with specified mac address
        /// </summary>
        /// <exception cref="HideezException" />
        /// <exception cref="Exception" />
        async Task UnlockWorkstation(string mac, SessionSwitchSubject unlockMethod, UserInfo info = null)
        {
            try
            {
                IDevice device = await ConnectDevice(mac);

                await WaitDeviceInitialization(mac, device);

                // No point in reading credentials if CredentialProvider is not connected
                if (!_workstationUnlocker.IsConnected)
                    return;

                // get info from the HES to check if primary account update is needed
                if (_hesConnection?.State == HesConnectionState.Connected)
                {
                    if (info == null)
                        info = await _hesConnection.GetInfoByMac(mac);

                    if (info == null)
                        throw new Exception($"Device not found");

                    await _ui.SendNotification("Waiting for the primary account update...");
                    await WaitForPrimaryAccountUpdate(info);
                }

                await _ui.SendNotification("Reading credentials from the device...");
                ushort primaryAccountKey = await DevicePasswordManager.GetPrimaryAccountKey(device);
                var credentials = await GetCredentials(device, primaryAccountKey);

                SessionSwitchManager.SetEventSubject(unlockMethod, device.SerialNo);

                // send credentials to the Credential Provider to unlock the PC
                await _ui.SendNotification("Unlocking the PC...");
                var logonSuccessful = await _workstationUnlocker.SendLogonRequest(credentials.Login, credentials.Password, credentials.PreviousPassword);

                if (!logonSuccessful)
                    await device.Disconnect();

                _log.Info($"UnlockWorkstation result: {logonSuccessful}");
            }
            catch (HideezException ex)
            {
                var message = HideezExceptionLocalization.GetErrorAsString(ex);
                _log.Error(message);
                await _ui.SendNotification("");
                await _ui.SendError(message);
                throw;
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                await _ui.SendNotification("");
                await _ui.SendError(ex.Message);
                throw;
            }
        }

        public async Task UnlockByMac(string mac)
        {
            try
            {
                ActivateWorkstationScreen();

                // Prevent access for unauthorized devices
                if (!IsBleTapAllowed(mac))
                {
                    _bleAccessBlacklist.GetOrAdd(mac, mac);
                    throw new AccessDeniedAuthException();
                }

                await UnlockWorkstation(mac, SessionSwitchSubject.Dongle);
            }
            catch (AccessDeniedAuthException ex)
            {
                _log.Error(ex);
                await _ui.SendNotification("");
                await _ui.SendError(ex.Message);
                throw;
            }
        }

        public async Task UnlockByRfid(string rfid)
        {
            try
            {
                ActivateWorkstationScreen();
                await _ui.SendNotification("Connecting to the HES server...");

                if (_hesConnection == null)
                    throw new Exception("Cannot connect device. Not connected to the HES.");

                // get MAC address from the HES
                var info = await _hesConnection.GetInfoByRfid(rfid);

                if (info == null)
                    throw new Exception($"Device not found");

                // Prevent access for unauthorized devices
                if (!IsRfidAllowed(info.DeviceMac))
                {
                    _rfidAccessBlacklist.GetOrAdd(rfid, rfid);
                    throw new AccessDeniedAuthException();
                }

                await UnlockWorkstation(info.DeviceMac, SessionSwitchSubject.RFID, info);
            }
            catch (AccessDeniedAuthException ex)
            {
                _log.Error(ex);
                await _ui.SendNotification("");
                await _ui.SendError(ex.Message);
                throw;
            }
        }

        public async Task UnlockByProximity(string mac)
        {
            try
            {
                ActivateWorkstationScreen();

                // Prevent access for unauthorized devices
                if (!IsProximityAllowed(mac))
                    throw new AccessDeniedAuthException();

                await UnlockWorkstation(mac, SessionSwitchSubject.Proximity);
            }
            catch (AccessDeniedAuthException ex)
            {
                _log.Error(ex);
                await _ui.SendNotification("");
                await _ui.SendError(ex.Message);
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
            await Task.Run(() =>
            {
                try
                {
                    _screenActivator?.ActivateScreen();
                }
                catch (Exception ex)
                {
                    _log.Error(ex);
                }
            });
        }

        void ClearAccessBlacklists()
        {
            _log.Info("Access blacklist cleared");
            _proximityAccessBlacklist.Clear();
            _rfidAccessBlacklist.Clear();
            _bleAccessBlacklist.Clear();
        }
    }
}
