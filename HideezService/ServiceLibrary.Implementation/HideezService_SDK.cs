using Hideez.CsrBLE;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.PasswordManager;
using Hideez.SDK.Communication.Proximity;
using Hideez.SDK.Communication.Utils;
using Hideez.SDK.Communication.WCF;
using Hideez.SDK.Communication.Workstation;
using HideezMiddleware;
using HideezMiddleware.Modules;
using HideezMiddleware.Settings;
using HideezMiddleware.Utils;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceLibrary.Implementation
{
    public partial class HideezService : IHideezService
    {
        static BleConnectionManager _connectionManager;
        static BleDeviceManager _deviceManager;
        static CredentialProviderConnection _credentialProviderConnection;
        static WorkstationUnlocker _workstationUnlocker;
        static HesAppConnection _hesConnection;
        static RfidServiceConnection _rfidService;
        static ProximityMonitorManager _proximityMonitorManager;
        static WorkstationLocker _workstationLocker;
        static IScreenActivator _screenActivator;
        static WcfDeviceFactory _wcfDeviceManager;
        static DeviceAccessController _deviceAccessController;
        static EventAggregator _eventAggregator;
        static IWorkstationEventFactory _workstationEventFactory = new WorkstationEventFactory();

        static ISettingsManager<UnlockerSettings> _unlockerSettingsManager;

        void InitializeSDK()
        {
            var sdkLogger = new NLogWrapper();

#if DEBUG
            _log.Info($">>>>>> Verifying error codes.");
            var _hideezExceptionLocalization = new HideezExceptionLocalization(sdkLogger);
            bool isVerified = _hideezExceptionLocalization.VerifyResourcesForErrorCode(new CultureInfo("en"));
            // Debug.Assert(isVerified, $">>>>>> Verifying error codes resalt: {isVerified}");
#endif

            // Combined path evaluates to '%ProgramData%\\Hideez\\Bonds'
            var commonAppData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            var bondsFilePath = $"{commonAppData}\\Hideez\\bonds";

            _connectionManager = new BleConnectionManager(sdkLogger, bondsFilePath);
            _connectionManager.AdapterStateChanged += ConnectionManager_AdapterStateChanged;
            _connectionManager.DiscoveryStopped += ConnectionManager_DiscoveryStopped;
            _connectionManager.DiscoveredDeviceAdded += ConnectionManager_DiscoveredDeviceAdded;
            _connectionManager.DiscoveredDeviceRemoved += ConnectionManager_DiscoveredDeviceRemoved;

            // BLE ============================
            _deviceManager = new BleDeviceManager(sdkLogger, _connectionManager);
            _deviceManager.DeviceAdded += DevicesManager_DeviceCollectionChanged;
            _deviceManager.DeviceRemoved += DevicesManager_DeviceCollectionChanged;
            _deviceManager.DeviceRemoved += DeviceManager_DeviceRemoved;
            _deviceManager.DeviceAdded += DeviceManager_DeviceAdded;

            // WCF ============================
            _wcfDeviceManager = new WcfDeviceFactory(_deviceManager, sdkLogger);

            // Named Pipes Server ==============================
            _credentialProviderConnection = new CredentialProviderConnection(sdkLogger);


            // RFID Service Connection ============================
            _rfidService = new RfidServiceConnection(sdkLogger);
            _rfidService.RfidReaderStateChanged += RFIDService_ReaderStateChanged;
            _rfidService.Start();

            // Settings
            string settingsDirectory = $@"{commonAppData}\Hideez\Service\Settings\";
            if (!Directory.Exists(settingsDirectory))
            {
                Directory.CreateDirectory(settingsDirectory);
            }
            string ulockerSettingsPath = Path.Combine(settingsDirectory, "Unlocker.xml");
            IFileSerializer fileSerializer = new XmlFileSerializer(sdkLogger);
            _unlockerSettingsManager = new SettingsManager<UnlockerSettings>(ulockerSettingsPath, fileSerializer);
            _unlockerSettingsManager.SettingsChanged += UnlockerSettingsManager_SettingsChanged;

            try
            {
                // HES ==================================
                // HKLM\SOFTWARE\Hideez\Safe, hs3_hes_address REG_SZ
                string hesAddres = GetHesAddress();
                UrlUtils.TryGetDomain(hesAddres, out string hesDomain);
                WorkstationHelper.Log = sdkLogger;
                var workstationInfoProvider = new WorkstationInfoProvider(hesDomain, sdkLogger);
                _hesConnection = new HesAppConnection(_deviceManager, hesAddres, workstationInfoProvider, sdkLogger);
                _hesConnection.HubSettingsArrived += (sender, settings) => _unlockerSettingsManager.SaveSettings(new UnlockerSettings(settings));
                _hesConnection.HubConnectionStateChanged += HES_ConnectionStateChanged;
                _hesConnection.Start();
            }
            catch (Exception ex)
            {
                _log.Error("Hideez Service has encountered an error during HES connection init." +
                    Environment.NewLine +
                    "New connection establishment will be attempted after service restart");
                _log.Error(ex);
            }

            // ScreenActivator ==================================
            _screenActivator = new UiScreenActivator(SessionManager);

            // WorkstationUnlocker 
            bool ignoreWorkstationOwnershipSecurity = false;
            try
            {
                ignoreWorkstationOwnershipSecurity = GetIgnoreWorkstationOwnershipSecurity();
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
            _workstationUnlocker = new WorkstationUnlocker(_deviceManager, _hesConnection,
                _credentialProviderConnection, _rfidService, _connectionManager, _screenActivator, _unlockerSettingsManager, ignoreWorkstationOwnershipSecurity);

            _credentialProviderConnection.Start();

            // Proximity Monitor 
            UnlockerSettings unlockerSettings = _unlockerSettingsManager.GetSettingsAsync().Result;
            _proximityMonitorManager = new ProximityMonitorManager(_deviceManager, sdkLogger, unlockerSettings.LockProximity, unlockerSettings.UnlockProximity, unlockerSettings.LockTimeoutSeconds);
            _proximityMonitorManager.Start();

            // WorkstationLocker ==================================
            _workstationLocker = new WorkstationLocker(SessionManager, _proximityMonitorManager);
            _workstationLocker.Start();

            // Device Access Controller ==================================
            if (ignoreWorkstationOwnershipSecurity)
            {
                _log.Warn("Device Access Controller is disabled due to workstation ownership options");
            }
            else
            {
                _deviceAccessController = new DeviceAccessController(_unlockerSettingsManager, _deviceManager, _workstationLocker);
                _deviceAccessController.Start();
            }

            _eventAggregator = new EventAggregator(_hesConnection);

            _connectionManager.StartDiscovery();

            SessionSwitchManager.SessionSwitch += we => _eventAggregator?.AddNewAsync(we);
        }

        #region Event Handlers

        private void UnlockerSettingsManager_SettingsChanged(object sender, SettingsChangedEventArgs<UnlockerSettings> e)
        {
            try
            {
                UnlockerSettings settings = e.NewSettings;
                if (_proximityMonitorManager != null)
                {
                    _proximityMonitorManager.LockProximity = settings.LockProximity;
                    _proximityMonitorManager.UnlockProximity = settings.UnlockProximity;
                    _proximityMonitorManager.LockTimeoutSeconds = settings.LockTimeoutSeconds;
                    _log.Info("Updated unlocker settings in proximity monitor.");
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
        }

        void DeviceManager_DeviceAdded(object sender, DeviceCollectionChangedEventArgs e)
        {
            var device = e.AddedDevice;

            if (device != null)
            {
                device.ConnectionStateChanged += Device_ConnectionStateChanged;
                device.Initialized += Device_Initialized;
                device.StorageModified += RemoteConnection_StorageModified;
                device.Disconnected += Device_Disconnected;
            }
        }

        void DeviceManager_DeviceRemoved(object sender, DeviceCollectionChangedEventArgs e)
        {
            var device = e.RemovedDevice;

            if (device != null)
            {
                device.ConnectionStateChanged -= Device_ConnectionStateChanged;
                device.Initialized -= Device_Initialized;
                device.StorageModified -= RemoteConnection_StorageModified;
                device.Disconnected -= Device_Disconnected;

                if (device is IWcfDevice wcfDevice)
                    UnsubscribeFromWcfDeviceEvents(wcfDevice);

                if (!device.IsRemote && device.IsInitialized)
                {
                    WorkstationEvent workstationEvent = _workstationEventFactory.GetBaseInitializedInstance();
                    workstationEvent.UserSession = SessionSwitchManager.UserSessionName;
                    workstationEvent.EventId = WorkstationEventId.DeviceDeleted;
                    workstationEvent.Severity = WorkstationEventSeverity.Warning;
                    workstationEvent.DeviceId = device.SerialNo;
                    _eventAggregator?.AddNewAsync(workstationEvent);
                }
            }
        }

        private void Device_Disconnected(object sender, EventArgs e)
        {
            if (sender is IDevice device && device.IsInitialized && (!device.IsRemote || device.ChannelNo > 2))
            {
                WorkstationEvent workstationEvent = _workstationEventFactory.GetBaseInitializedInstance();
                workstationEvent.UserSession = SessionSwitchManager.UserSessionName;
                workstationEvent.Severity = WorkstationEventSeverity.Info;
                workstationEvent.DeviceId = device.SerialNo;
                if (device.IsRemote)
                {
                    workstationEvent.EventId = WorkstationEventId.RemoteDisconnect;
                }
                else
                {
                    workstationEvent.EventId = WorkstationEventId.DeviceDisconnect;
                }
                _eventAggregator?.AddNewAsync(workstationEvent);
            }
        }

        void ConnectionManager_AdapterStateChanged(object sender, EventArgs e)
        {
            foreach (var client in SessionManager.Sessions)
                client.Callbacks.DongleConnectionStateChanged(_connectionManager?.State == BluetoothAdapterState.PoweredOn);


            if (_connectionManager != null && (_connectionManager.State == BluetoothAdapterState.Unknown || _connectionManager.State == BluetoothAdapterState.PoweredOn))
            {
                var we = _workstationEventFactory.GetBaseInitializedInstance();
                if (_connectionManager.State == BluetoothAdapterState.PoweredOn)
                {
                    we.EventId = WorkstationEventId.DonglePlugged;
                    we.Severity = WorkstationEventSeverity.Ok;
                }
                else
                {
                    we.EventId = WorkstationEventId.DongleUnplugged;
                    we.Severity = WorkstationEventSeverity.Warning;
                }
                Task.Run(() => _eventAggregator?.AddNewAsync(we));
            }
        }

        void RFIDService_ReaderStateChanged(object sender, EventArgs e)
        {
            bool isConnected = _rfidService != null ? _rfidService.ServiceConnected && _rfidService.ReaderConnected : false;

            foreach (var client in SessionManager.Sessions)
                client.Callbacks.RFIDConnectionStateChanged(isConnected);

            var we = _workstationEventFactory.GetBaseInitializedInstance();
            we.EventId = isConnected ? WorkstationEventId.RFIDAdapterPlugged : WorkstationEventId.RFIDAdapterUnplugged;
            we.Severity = isConnected ? WorkstationEventSeverity.Ok : WorkstationEventSeverity.Warning;
            Task.Run(() => _eventAggregator?.AddNewAsync(we));
        }

        void HES_ConnectionStateChanged(object sender, EventArgs e)
        {
            foreach (var client in SessionManager.Sessions)
                client.Callbacks.HESConnectionStateChanged(_hesConnection?.State == HesConnectionState.Connected);

            if (_hesConnection != null)
            {
                var we = _workstationEventFactory.GetBaseInitializedInstance();
                we.UserSession = SessionSwitchManager.UserSessionName;
                if (_hesConnection.State == HesConnectionState.Connected)
                {
                    we.EventId = WorkstationEventId.HESConnected;
                    we.Severity = WorkstationEventSeverity.Ok;
                }
                else
                {
                    we.EventId = WorkstationEventId.HESDisconnected;
                    we.Severity = WorkstationEventSeverity.Warning;
                }
                Task.Run(() => _eventAggregator?.AddNewAsync(we));
            }
        }

        void DevicesManager_DeviceCollectionChanged(object sender, DeviceCollectionChangedEventArgs e)
        {
            foreach (var client in SessionManager.Sessions)
                client.Callbacks.DevicesCollectionChanged(GetDevices());
        }

        void ConnectionManager_DiscoveredDeviceAdded(object sender, DiscoveredDeviceAddedEventArgs e)
        {
        }

        void ConnectionManager_DiscoveredDeviceRemoved(object sender, DiscoveredDeviceRemovedEventArgs e)
        {
        }

        void ConnectionManager_DiscoveryStopped(object sender, EventArgs e)
        {
        }

        void Device_ConnectionStateChanged(object sender, EventArgs e)
        {
            try
            {
                if (sender is IDevice device)
                {
                    foreach (var client in SessionManager.Sessions)
                    {
                        client.Callbacks.DeviceConnectionStateChanged(new DeviceDTO(device));
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        void Device_Initialized(object sender, EventArgs e)
        {
            try
            {
                if (sender is IDevice device)
                {
                    foreach (var client in SessionManager.Sessions)
                    {
                        client.Callbacks.DeviceInitialized(new DeviceDTO(device));
                    }

                    if (!device.IsRemote || device.ChannelNo > 2)
                    {
                        WorkstationEvent workstationEvent = _workstationEventFactory.GetBaseInitializedInstance();
                        workstationEvent.Severity = WorkstationEventSeverity.Info;
                        workstationEvent.DeviceId = device.SerialNo;
                        if (device.IsRemote)
                        {
                            workstationEvent.UserSession = SessionSwitchManager.UserSessionName;
                            workstationEvent.EventId = WorkstationEventId.RemoteConnect;
                        }
                        else
                        {
                            workstationEvent.EventId = WorkstationEventId.DeviceConnect;
                        }
                        _eventAggregator?.AddNewAsync(workstationEvent);
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        async void SessionManager_SessionClosed(object sender, ServiceClientSession e)
        {
            if (_client.Id == e.Id)
            {
                foreach (var wcfDevice in RemoteWcfDevices.ToArray())
                {
                    await _deviceManager.Remove(wcfDevice);
                    UnsubscribeFromWcfDeviceEvents(wcfDevice);
                }
            }

        }
        #endregion

        public bool GetAdapterState(Adapter adapter)
        {
            try
            {
                switch (adapter)
                {
                    case Adapter.Dongle:
                        return _connectionManager?.State == BluetoothAdapterState.PoweredOn;
                    case Adapter.HES:
                        return _hesConnection?.State == HesConnectionState.Connected;
                    case Adapter.RFID:
                        return _rfidService != null ? _rfidService.ServiceConnected && _rfidService.ReaderConnected : false;
                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
                ThrowException(ex);

                return false; // We will never reach this line
            }
        }

        public DeviceDTO[] GetDevices()
        {
            try
            {
                return _deviceManager.Devices.Where(d => !d.IsRemote).Select(d => new DeviceDTO(d)).ToArray();
            }
            catch (Exception ex)
            {
                LogException(ex);
                ThrowException(ex);

                return new DeviceDTO[0]; // We will never reach this line
            }
        }

        readonly string _hesAddressRegistryValueName = "hs3_hes_address";
        RegistryKey GetAppRegistryRootKey()
        {
            return RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default)?
                .OpenSubKey("SOFTWARE")?
                .OpenSubKey("Hideez")?
                .OpenSubKey("Safe");
        }

        string GetHesAddress()
        {
            var registryKey = GetAppRegistryRootKey();
            if (registryKey == null)
                throw new Exception("Couldn't find Hideez Safe registry key. (HKLM\\SOFTWARE\\Hideez\\Safe)");

            var value = registryKey.GetValue(_hesAddressRegistryValueName);
            if (value == null)
                throw new ArgumentNullException($"{_hesAddressRegistryValueName} value is null or empty. Please specify HES address in registry under value {_hesAddressRegistryValueName}. Key: HKLM\\SOFTWARE\\Hideez\\Safe");

            if (value is string == false)
                throw new FormatException($"{_hesAddressRegistryValueName} could not be cast to string. Check that its value has REG_SZ type");

            var address = value as string;

            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentNullException($"{_hesAddressRegistryValueName} value is null or empty. Please specify HES address in registry under value {_hesAddressRegistryValueName}. Key: HKLM\\SOFTWARE\\Hideez\\Safe");

            if (Uri.TryCreate(address, UriKind.Absolute, out Uri outUri)
                && (outUri.Scheme == Uri.UriSchemeHttp || outUri.Scheme == Uri.UriSchemeHttps))
            {
                return address;
            }
            else
            {
                throw new ArgumentException($"Specified HES address: ('{address}'), " +
                    $"is not a correct absolute uri");
            }
        }

        readonly string _ignoreWorkstationOwnershipSecurityValueName = "ignore_workstation_ownership_security";
        bool GetIgnoreWorkstationOwnershipSecurity()
        {
            var registryKey = GetAppRegistryRootKey();
            if (registryKey == null)
                throw new Exception("Couldn't find Hideez Safe registry key. (HKLM\\SOFTWARE\\Hideez\\Safe)");

            var value = registryKey.GetValue(_ignoreWorkstationOwnershipSecurityValueName);
            if (value == null)
            {
                _log.Warn($"{_ignoreWorkstationOwnershipSecurityValueName} value is null or empty.");
                return false;
            }

            if (!(int.TryParse(value.ToString(), out int result)))
                throw new FormatException($"Specified {_ignoreWorkstationOwnershipSecurityValueName} is not a correct.");

            return (result != 0);
        }

        public void DisconnectDevice(string id)
        {
            try
            {
                _deviceManager.Find(id)?.Disconnect();
            }
            catch (Exception ex)
            {
                LogException(ex);
                ThrowException(ex);
            }
        }

        public async Task RemoveDeviceAsync(string id)
        {
            try
            {
                var device = _deviceManager.Find(id);
                if (device != null)
                    await _deviceManager.RemoveAll(device.Connection);
            }
            catch (Exception ex)
            {
                LogException(ex);
                ThrowException(ex);
            }
        }

        public void PublishEvent(WorkstationEventDTO workstationEvent)
        {
            WorkstationEvent we = new WorkstationEvent
            {
                Id = workstationEvent.Id,
                Date = workstationEvent.Date,
                WorkstationId = workstationEvent.WorkstationId,
                EventId = (WorkstationEventId)workstationEvent.EventId,
                Severity = (WorkstationEventSeverity)workstationEvent.Severity,
                Note = workstationEvent.Note,
                DeviceId = workstationEvent.DeviceId,
                UserSession = workstationEvent.UserSession,
                AccountName = workstationEvent.AccountName,
                AccountLogin = workstationEvent.AccountLogin,
            };

            Task.Run(() => _eventAggregator.AddNewAsync(we));
        }

        #region Remote device management
        // This collection is unique for each client
        List<IWcfDevice> RemoteWcfDevices = new List<IWcfDevice>();

        public async Task<string> EstablishRemoteDeviceConnection(string serialNo, byte channelNo)
        {
            try
            {
                var wcfDevice = (IWcfDevice)_deviceManager.FindBySerialNo(serialNo, 2);
                if (wcfDevice == null)
                {
                    var device = _deviceManager.FindBySerialNo(serialNo, 1);
                    wcfDevice = await _wcfDeviceManager.EstablishRemoteDeviceConnection(device.Mac, channelNo);

                    SubscribeToWcfDeviceEvents(wcfDevice);
                }

                return wcfDevice.Id;
            }
            catch (Exception ex)
            {
                LogException(ex);
                ThrowException(ex);
                return null; // this line is unreachable
            }
        }

        void SubscribeToWcfDeviceEvents(IWcfDevice wcfDevice)
        {
            RemoteWcfDevices.Add(wcfDevice);
            wcfDevice.RssiReceived += RemoteConnection_RssiReceived;
            wcfDevice.BatteryChanged += RemoteConnection_BatteryChanged;
        }

        void UnsubscribeFromWcfDeviceEvents(IWcfDevice wcfDevice)
        {
            wcfDevice.RssiReceived -= RemoteConnection_RssiReceived;
            wcfDevice.BatteryChanged -= RemoteConnection_BatteryChanged;
            RemoteWcfDevices.Remove(wcfDevice);
        }

        void RemoteConnection_RssiReceived(object sender, double rssi)
        {
            try
            {
                if (RemoteWcfDevices.Count > 0)
                {
                    if (sender is IWcfDevice wcfDevice)
                    {
                        _client.Callbacks.RemoteConnection_RssiReceived(wcfDevice.SerialNo, rssi);
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        void RemoteConnection_BatteryChanged(object sender, int battery)
        {
            try
            {
                if (RemoteWcfDevices.Count > 0)
                {
                    if (sender is IWcfDevice wcfDevice)
                    {
                        _client.Callbacks.RemoteConnection_BatteryChanged(wcfDevice.SerialNo, battery);
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        void RemoteConnection_StorageModified(object sender, EventArgs e)
        {
            try
            {
                if (sender is IDevice device)
                {
                    foreach (var client in SessionManager.Sessions)
                        client.Callbacks.RemoteConnection_StorageModified(device.SerialNo);
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        public async Task<byte[]> RemoteConnection_AuthCommandAsync(string connectionId, byte[] data)
        {
            try
            {
                var wcfDevice = (IWcfDevice)_deviceManager.Find(connectionId);

                var response = await wcfDevice.OnAuthCommandAsync(data);

                return response;
            }
            catch (Exception ex)
            {
                LogException(ex);
                ThrowException(ex);
                return null; // this line is unreachable
            }
        }

        public async Task<byte[]> RemoteConnection_RemoteCommandAsync(string connectionId, byte[] data)
        {
            try
            {
                var wcfDevice = (IWcfDevice)_deviceManager.Find(connectionId);

                var response = await wcfDevice.OnRemoteCommandAsync(data);

                return response;
            }
            catch (Exception ex)
            {
                LogException(ex);
                ThrowException(ex);
                return null; // this line is unreachable
            }
        }

        public async Task RemoteConnection_ResetChannelAsync(string connectionId)
        {
            try
            {
                var wcfDevice = (IWcfDevice)_deviceManager.Find(connectionId);

                await wcfDevice.OnResetChannelAsync();
            }
            catch (Exception ex)
            {
                LogException(ex);
                ThrowException(ex);
            }
        }
        #endregion

        #region Host only
        public static void OnSessionChange(bool sessionLocked)
        {
            try
            {
                var newState = sessionLocked ? "locked" : "unlocked";
                _log.Info($"Session state changed to: {newState} (sessionLocked: {sessionLocked});");
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        public static void OnLaunchFromSleep()
        {
            try
            {
                _log.Info("System left suspended mode");
                _log.Info("Restarting connection manager");
                _connectionManager.Restart();
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        public static async Task OnServiceStartedAsync()
        {
            WorkstationEvent workstationEvent = _workstationEventFactory.GetBaseInitializedInstance();
            workstationEvent.UserSession = SessionSwitchManager.UserSessionName;
            workstationEvent.Severity = WorkstationEventSeverity.Info;
            workstationEvent.EventId = WorkstationEventId.ServiceStarted;
            await _eventAggregator?.AddNewAsync(workstationEvent);
        }

        public static async Task OnServiceStoppedAsync()
        {
            WorkstationEvent workstationEvent = _workstationEventFactory.GetBaseInitializedInstance();
            workstationEvent.UserSession = SessionSwitchManager.UserSessionName;
            workstationEvent.Severity = WorkstationEventSeverity.Info;
            workstationEvent.EventId = WorkstationEventId.ServiceStopped;
            await _eventAggregator?.AddNewAsync(workstationEvent, true);
        }
        #endregion
    }
}
