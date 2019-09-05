using Hideez.CsrBLE;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Proximity;
using Hideez.SDK.Communication.WCF;
using Hideez.SDK.Communication.WorkstationEvents;
using Hideez.SDK.Communication.Log;
using HideezMiddleware;
using HideezMiddleware.Settings;
using Microsoft.Win32;
using ServiceLibrary.Implementation.ScreenActivation;
using ServiceLibrary.Implementation.SessionManagement;
using ServiceLibrary.Implementation.WorkstationLock;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HideezMiddleware.DeviceConnection;
using Hideez.SDK.Communication;
using System.Text;

namespace ServiceLibrary.Implementation
{
    public partial class HideezService : IHideezService
    {
        static BleConnectionManager _connectionManager;
        static BleDeviceManager _deviceManager;
        static CredentialProviderProxy _credentialProviderProxy;
        static HesAppConnection _hesConnection;
        static RfidServiceConnection _rfidService;
        static ProximityMonitorManager _proximityMonitorManager;
        static IScreenActivator _screenActivator;
        static WcfDeviceFactory _wcfDeviceManager;
        static EventAggregator _eventAggregator;
        static ServiceClientUiManager _clientProxy;
        static UiProxyManager _uiProxy;
        static StatusManager _statusManager;
        static WcfWorkstationLocker _workstationLocker;
        static WorkstationLockProcessor _workstationLockProcessor;

        static ISettingsManager<ProximitySettings> _proximitySettingsManager;

        static ConnectionFlowProcessor _connectionFlowProcessor;
        static AdvertisementIgnoreList _advIgnoreList;
        static RfidConnectionProcessor _rfidProcessor;
        static TapConnectionProcessor _tapProcessor;
        static ProximityConnectionProcessor _proximityProcessor;

        void InitializeSDK()
        {
            var sdkLogger = new NLogWrapper();

#if DEBUG
            _log.WriteLine($">>>>>> Verifying error codes.");
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
            _credentialProviderProxy = new CredentialProviderProxy(sdkLogger);

            // RFID Service Connection ============================
            _rfidService = new RfidServiceConnection(sdkLogger);
            _rfidService.RfidReaderStateChanged += RFIDService_ReaderStateChanged;

            // Settings
            string settingsDirectory = $@"{commonAppData}\Hideez\Service\Settings\";
            if (!Directory.Exists(settingsDirectory))
            {
                Directory.CreateDirectory(settingsDirectory);
            }
            string proximitySettingsPath = Path.Combine(settingsDirectory, "Proximity.xml");
            IFileSerializer fileSerializer = new XmlFileSerializer(sdkLogger);
            _proximitySettingsManager = new SettingsManager<ProximitySettings>(proximitySettingsPath, fileSerializer);
            _proximitySettingsManager.SettingsChanged += ProximitySettingsManager_SettingsChanged;

            // Get HES address from registry ==================================
            // HKLM\SOFTWARE\Hideez\Client, client_hes_address REG_SZ
            string hesAddress = string.Empty;
            try
            {
                hesAddress = GetHesAddress();
            }
            catch (Exception ex)
            {
                Error(ex);
            }

            // WorkstationInfoProvider ==================================
            WorkstationHelper.Log = sdkLogger;
            var workstationInfoProvider = new WorkstationInfoProvider(hesAddress, sdkLogger);

            // HES Connection ==================================
            _hesConnection = new HesAppConnection(_deviceManager, workstationInfoProvider, sdkLogger);
            _hesConnection.HubProximitySettingsArrived += async (sender, receivedSettings) =>
            {
                ProximitySettings settings = await _proximitySettingsManager.GetSettingsAsync();
                settings.DevicesProximity = receivedSettings.ToArray();
                _proximitySettingsManager.SaveSettings(settings);
            };
            _hesConnection.HubRFIDIndicatorStateArrived += async (sender, isEnabled) =>
            {
                ProximitySettings settings = await _proximitySettingsManager.GetSettingsAsync();
                settings.IsRFIDIndicatorEnabled = isEnabled;
                _proximitySettingsManager.SaveSettings(settings);
            };
            _hesConnection.HubConnectionStateChanged += HES_ConnectionStateChanged;

            // ScreenActivator ==================================
            _screenActivator = new WcfScreenActivator(SessionManager);

            // Client Proxy =============================
            _clientProxy = new ServiceClientUiManager(SessionManager);

            // UI Proxy =============================
            _uiProxy = new UiProxyManager(_credentialProviderProxy, _clientProxy, sdkLogger);

            // StatusManager =============================
            _statusManager = new StatusManager(_hesConnection, _rfidService, _connectionManager, _uiProxy, _proximitySettingsManager, sdkLogger);

            // ConnectionFlowProcessor
            _connectionFlowProcessor = new ConnectionFlowProcessor(
                _deviceManager,
                _hesConnection,
                _credentialProviderProxy,
                _screenActivator,
                _uiProxy,
                sdkLogger);
            _advIgnoreList = new AdvertisementIgnoreList(
                _connectionManager,
                _deviceManager,
                _proximitySettingsManager,
                sdkLogger);
            _rfidProcessor = new RfidConnectionProcessor(
                _connectionFlowProcessor,
                _hesConnection,
                _rfidService,
                _screenActivator,
                _uiProxy,
                sdkLogger);
            _tapProcessor = new TapConnectionProcessor(
                _connectionFlowProcessor,
                _connectionManager,
                _screenActivator,
                _uiProxy,
                sdkLogger);
            _proximityProcessor = new ProximityConnectionProcessor(
                _connectionFlowProcessor,
                _connectionManager,
                _screenActivator,
                _uiProxy,
                _proximitySettingsManager,
                _advIgnoreList,
                _deviceManager,
                _credentialProviderProxy,
                sdkLogger);

            // Proximity Monitor ==================================
            ProximitySettings proximitySettings = _proximitySettingsManager.GetSettingsAsync().Result;
            _proximityMonitorManager = new ProximityMonitorManager(_deviceManager, sdkLogger, proximitySettings.DevicesProximity);

            // WorkstationLocker ==================================
            _workstationLocker = new WcfWorkstationLocker(SessionManager, sdkLogger);

            // WorkstationLockProcessor ==================================
            _workstationLockProcessor = new WorkstationLockProcessor(_proximityMonitorManager, _workstationLocker, sdkLogger);


            // Audit Log / Event Aggregator =============================
            _eventAggregator = new EventAggregator(_hesConnection, sdkLogger);
            SessionSwitchManager.SessionSwitch += we => _eventAggregator?.AddNewAsync(we);

            // SDK initialization finished, start essential components
            _credentialProviderProxy.Start();
            _rfidService.Start();
            _hesConnection.Start(hesAddress);

            _workstationLockProcessor.Start();
            _proximityMonitorManager.Start();

            _connectionManager.StartDiscovery();

            if (_hesConnection.State == HesConnectionState.Error)
            {
                Task.Run(async () => { await _hesConnection.Stop(); });

                Error("Hideez Service has encountered an error during HES connection initialization"
                    + Environment.NewLine
                    + "New connection establishment will be attempted after service restart"
                    + Environment.NewLine
                    + _hesConnection.ErrorMessage);
            }
        }

        #region Event Handlers

        private void ProximitySettingsManager_SettingsChanged(object sender, SettingsChangedEventArgs<ProximitySettings> e)
        {
            try
            {
                if (_proximityMonitorManager != null)
                {
                    _proximityMonitorManager.ProximitySettings = e.NewSettings.DevicesProximity;
                    _log.WriteLine("Updated proximity settings in proximity monitor.");
                }
            }
            catch (Exception ex)
            {
                _log.WriteLine(ex);
            }
        }

        void DeviceManager_DeviceAdded(object sender, DeviceCollectionChangedEventArgs e)
        {
            var device = e.AddedDevice;

            if (device != null)
            {
                device.ConnectionStateChanged += Device_ConnectionStateChanged;
                device.Initialized += Device_Initialized;
                device.Authorized += Device_Authorized;
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
                device.Authorized -= Device_Authorized;
                device.StorageModified -= RemoteConnection_StorageModified;
                device.Disconnected -= Device_Disconnected;

                if (device is IWcfDevice wcfDevice)
                    UnsubscribeFromWcfDeviceEvents(wcfDevice);

                if (!device.IsRemote && device.IsInitialized)
                {
                    WorkstationEvent workstationEvent = WorkstationEvent.GetBaseInitializedInstance();
                    workstationEvent.UserSession = SessionSwitchManager.UserSessionName;
                    workstationEvent.EventId = WorkstationEventType.DeviceDeleted;
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
                WorkstationEvent workstationEvent = WorkstationEvent.GetBaseInitializedInstance();
                workstationEvent.UserSession = SessionSwitchManager.UserSessionName;
                workstationEvent.Severity = WorkstationEventSeverity.Info;
                workstationEvent.DeviceId = device.SerialNo;
                if (device.IsRemote)
                {
                    workstationEvent.EventId = WorkstationEventType.RemoteDisconnect;
                }
                else
                {
                    workstationEvent.EventId = WorkstationEventType.DeviceDisconnect;
                }
                _eventAggregator?.AddNewAsync(workstationEvent);
            }
        }

        void ConnectionManager_AdapterStateChanged(object sender, EventArgs e)
        {
            if (_connectionManager != null && (_connectionManager.State == BluetoothAdapterState.Unknown || _connectionManager.State == BluetoothAdapterState.PoweredOn))
            {
                var we = WorkstationEvent.GetBaseInitializedInstance();
                if (_connectionManager.State == BluetoothAdapterState.PoweredOn)
                {
                    we.EventId = WorkstationEventType.DonglePlugged;
                    we.Severity = WorkstationEventSeverity.Ok;
                }
                else
                {
                    we.EventId = WorkstationEventType.DongleUnplugged;
                    we.Severity = WorkstationEventSeverity.Warning;
                }
                Task.Run(() => _eventAggregator?.AddNewAsync(we));
            }
        }

        //todo - if RFID is not present, do not monitor this event
        void RFIDService_ReaderStateChanged(object sender, EventArgs e)
        {
            bool isConnected = _rfidService != null ? _rfidService.ServiceConnected && _rfidService.ReaderConnected : false;

            var we = WorkstationEvent.GetBaseInitializedInstance();
            we.EventId = isConnected ? WorkstationEventType.RFIDAdapterPlugged : WorkstationEventType.RFIDAdapterUnplugged;
            we.Severity = isConnected ? WorkstationEventSeverity.Ok : WorkstationEventSeverity.Warning;
            Task.Run(() => _eventAggregator?.AddNewAsync(we));
        }

        void HES_ConnectionStateChanged(object sender, EventArgs e)
        {
            if (_hesConnection != null)
            {
                var we = WorkstationEvent.GetBaseInitializedInstance();
                we.UserSession = SessionSwitchManager.UserSessionName;
                if (_hesConnection.State == HesConnectionState.Connected)
                {
                    we.EventId = WorkstationEventType.HESConnected;
                    we.Severity = WorkstationEventSeverity.Ok;
                }
                else
                {
                    we.EventId = WorkstationEventType.HESDisconnected;
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
                Error(ex);
            }
        }

        void Device_Initialized(object sender, EventArgs e)
        {
            try
            {
                if (sender is IDevice device)
                {
                    foreach (var session in SessionManager.Sessions)
                    {
                        // Separate error handling block for each callback ensures we try to notify 
                        // every session, even if an error occurs
                        try
                        {
                            session.Callbacks.DeviceInitialized(new DeviceDTO(device));
                        }
                        catch (Exception ex)
                        {
                            Error(ex);
                        }
                    }

                    if (!device.IsRemote || device.ChannelNo > 2)
                    {
                        WorkstationEvent workstationEvent = WorkstationEvent.GetBaseInitializedInstance();
                        workstationEvent.Severity = WorkstationEventSeverity.Info;
                        workstationEvent.UserSession = SessionSwitchManager.UserSessionName;
                        workstationEvent.DeviceId = device.SerialNo;
                        if (device.IsRemote)
                        {
                            workstationEvent.EventId = WorkstationEventType.RemoteConnect;
                        }
                        else
                        {
                            workstationEvent.EventId = WorkstationEventType.DeviceConnect;
                        }
                        _eventAggregator?.AddNewAsync(workstationEvent);
                    }
                }
            }
            catch (Exception ex)
            {
                Error(ex);
            }
        }

        void Device_Authorized(object sender, EventArgs e)
        {
            if (sender is IDevice device)
            {
                foreach (var session in SessionManager.Sessions)
                {
                    try
                    {
                        session.Callbacks.DeviceAuthorized(new DeviceDTO(device));
                    }
                    catch (Exception ex)
                    {
                        Error(ex);
                    }
                }
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
                Error(ex);
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
                Error(ex);
                ThrowException(ex);

                return new DeviceDTO[0]; // We will never reach this line
            }
        }

        readonly string _hesAddressRegistryValueName = "client_hes_address";
        RegistryKey GetAppRegistryRootKey()
        {
            return RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default)?
                .OpenSubKey("SOFTWARE")?
                .OpenSubKey("Hideez")?
                .OpenSubKey("Client");
        }

        string GetHesAddress()
        {
            var registryKey = GetAppRegistryRootKey();
            if (registryKey == null)
                throw new Exception("Couldn't find Hideez Client registry key. (HKLM\\SOFTWARE\\Hideez\\Client)");

            var value = registryKey.GetValue(_hesAddressRegistryValueName);
            if (value == null)
                throw new ArgumentNullException($"{_hesAddressRegistryValueName} value is null or empty. Please specify HES address in registry under value {_hesAddressRegistryValueName}. Key: HKLM\\SOFTWARE\\Hideez\\Client");

            if (value is string == false)
                throw new FormatException($"{_hesAddressRegistryValueName} could not be cast to string. Check that its value has REG_SZ type");

            var address = value as string;

            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentNullException($"{_hesAddressRegistryValueName} value is null or empty. Please specify HES address in registry under value {_hesAddressRegistryValueName}. Key: HKLM\\SOFTWARE\\Hideez\\Client");

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
                throw new Exception("Couldn't find Hideez Client registry key. (HKLM\\SOFTWARE\\Hideez\\Client)");

            var value = registryKey.GetValue(_ignoreWorkstationOwnershipSecurityValueName);
            if (value == null)
            {
                _log.WriteLine($"{_ignoreWorkstationOwnershipSecurityValueName} value is null or empty.", LogErrorSeverity.Warning);
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
                Error(ex);
                ThrowException(ex);
            }
        }

        public async Task RemoveDeviceAsync(string id)
        {
            try
            {
                var device = _deviceManager.Find(id);
                if (device != null)
                    await _deviceManager.RemoveAll(device.Mac);
            }
            catch (Exception ex)
            {
                Error(ex);
                ThrowException(ex);
            }
        }

        public void PublishEvent(WorkstationEventDTO workstationEvent)
        {
            WorkstationEvent we = new WorkstationEvent
            {
                Version = WorkstationEvent.CurrentVersion,
                Id = workstationEvent.Id,
                Date = workstationEvent.Date,
                WorkstationId = workstationEvent.WorkstationId,
                EventId = (WorkstationEventType)workstationEvent.EventId,
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
                Error(ex);
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

        void RemoteConnection_RssiReceived(object sender, sbyte rssi)
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
                Error(ex);
            }
        }

        void RemoteConnection_BatteryChanged(object sender, sbyte battery)
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
                Error(ex);
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
                Error(ex);
            }
        }

        public async Task<byte[]> RemoteConnection_VerifyCommandAsync(string connectionId, byte[] data)
        {
            try
            {
                var wcfDevice = (IWcfDevice)_deviceManager.Find(connectionId);

                var response = await wcfDevice.OnVerifyCommandAsync(data);

                return response;
            }
            catch (Exception ex)
            {
                Error(ex);
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
                Error(ex);
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
                Error(ex);
                ThrowException(ex);
            }
        }
        #endregion

        #region PIN
        public void SendPin(string deviceId, byte[] pin, byte[] oldPin)
        {
            try
            {
                var s_pin = Encoding.UTF8.GetString(pin);
                var s_oldPin = Encoding.UTF8.GetString(oldPin);

                _clientProxy.EnterPin(deviceId, s_pin, s_oldPin);
            }
            catch (Exception ex)
            {
                _log.WriteDebugLine(ex);
            }
        }

        public void CancelPin()
        {
            try
            {
                _clientProxy.CancelPin();
            }
            catch (Exception ex)
            {
                _log.WriteDebugLine(ex);
            }
        }
        #endregion

        #region Host only
        public static void OnSessionChange(bool sessionLocked)
        {
            try
            {
                var newState = sessionLocked ? "locked" : "unlocked";
                _log.WriteLine($"Session state changed to: {newState} (sessionLocked: {sessionLocked});");

                if (sessionLocked)
                {
                    _advIgnoreList.SetIgnoreList(_deviceManager?.Devices.Where(d => d.IsConnected).Select(d => d.Mac).ToArray());
                    _deviceManager?.Devices.ToList().ForEach(d => d.Disconnect());
                }
            }
            catch (Exception ex)
            {
                Error(ex);
            }
        }

        public static void OnLaunchFromSleep()
        {
            try
            {
                _log.WriteLine("System left suspended mode");
                _log.WriteLine("Restarting connection manager");
                _connectionManager.Restart();
            }
            catch (Exception ex)
            {
                Error(ex);
            }
        }

        public static async Task OnServiceStartedAsync()
        {
            WorkstationEvent workstationEvent = WorkstationEvent.GetBaseInitializedInstance();
            workstationEvent.UserSession = SessionSwitchManager.UserSessionName;
            workstationEvent.Severity = WorkstationEventSeverity.Info;
            workstationEvent.EventId = WorkstationEventType.ServiceStarted;
            await _eventAggregator?.AddNewAsync(workstationEvent); //todo - null reference exception at startup
        }

        public static async Task OnServiceStoppedAsync()
        {
            WorkstationEvent workstationEvent = WorkstationEvent.GetBaseInitializedInstance();
            workstationEvent.UserSession = SessionSwitchManager.UserSessionName;
            workstationEvent.Severity = WorkstationEventSeverity.Info;
            workstationEvent.EventId = WorkstationEventType.ServiceStopped;
            await _eventAggregator?.AddNewAsync(workstationEvent, true);
        }
        #endregion
    }
}
