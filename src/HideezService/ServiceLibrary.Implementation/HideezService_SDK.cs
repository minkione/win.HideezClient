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
using HideezMiddleware.Audit;
using Microsoft.Win32;
using ServiceLibrary.Implementation.ScreenActivation;
using ServiceLibrary.Implementation.ClientManagement;
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
using System.Reflection;

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
        static EventSender _eventSender;
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
        static SessionSwitchLogger _sessionSwitchLogger;
        static SessionTimestampLogger _sessionTimestampLogger;

        void InitializeSDK()
        {
#if DEBUG
            _log.WriteLine($">>>>>> Verifying error codes.");
            var _hideezExceptionLocalization = new HideezExceptionLocalization(_sdkLogger);
            bool isVerified = _hideezExceptionLocalization.VerifyResourcesForErrorCode(new CultureInfo("en"));
            // Debug.Assert(isVerified, $">>>>>> Verifying error codes resalt: {isVerified}");
#endif

            // Combined path evaluates to '%ProgramData%\\Hideez\\Bonds'
            var commonAppData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            var bondsFilePath = $"{commonAppData}\\Hideez\\bonds";
            var sessionTimestampPath = $@"{commonAppData}\Hideez\Service\Timestamp\timestamp.dat";
            string settingsDirectory = $@"{commonAppData}\Hideez\Service\Settings\";

            // Session Timestamp Logger ============================
            _sessionTimestampLogger = new SessionTimestampLogger(sessionTimestampPath, _sessionInfoProvider, _eventSaver, _sdkLogger);
            Task.Run(_sessionTimestampLogger.Initialize);

            // Connection Manager ============================
            _connectionManager = new BleConnectionManager(_sdkLogger, bondsFilePath);
            _connectionManager.AdapterStateChanged += ConnectionManager_AdapterStateChanged;
            _connectionManager.DiscoveryStopped += ConnectionManager_DiscoveryStopped;
            _connectionManager.DiscoveredDeviceAdded += ConnectionManager_DiscoveredDeviceAdded;
            _connectionManager.DiscoveredDeviceRemoved += ConnectionManager_DiscoveredDeviceRemoved;

            // BLE ============================
            _deviceManager = new BleDeviceManager(_sdkLogger, _connectionManager);
            _deviceManager.DeviceAdded += DevicesManager_DeviceCollectionChanged;
            _deviceManager.DeviceRemoved += DevicesManager_DeviceCollectionChanged;
            _deviceManager.DeviceRemoved += DeviceManager_DeviceRemoved;
            _deviceManager.DeviceAdded += DeviceManager_DeviceAdded;

            // WCF ============================
            _wcfDeviceManager = new WcfDeviceFactory(_deviceManager, _sdkLogger);

            // Named Pipes Server ==============================
            _credentialProviderProxy = new CredentialProviderProxy(_sdkLogger);

            // RFID Service Connection ============================
            _rfidService = new RfidServiceConnection(_sdkLogger);
            _rfidService.RfidReaderStateChanged += RFIDService_ReaderStateChanged;

            // Settings
            if (!Directory.Exists(settingsDirectory))
            {
                Directory.CreateDirectory(settingsDirectory);
            }
            string proximitySettingsPath = Path.Combine(settingsDirectory, "Proximity.xml");
            IFileSerializer fileSerializer = new XmlFileSerializer(_sdkLogger);
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
            WorkstationHelper.Log = _sdkLogger;
            var workstationInfoProvider = new WorkstationInfoProvider(hesAddress, _sdkLogger);

            // HES Connection ==================================
            _hesConnection = new HesAppConnection(_deviceManager, workstationInfoProvider, _sdkLogger)
            {
                ReconnectDelayMs = 10_000 // Todo: remove hes recoonect delay overwrite in stable version
            };
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

            // Audit Log / Event Aggregator =============================
            _eventSender = new EventSender(_hesConnection, _eventSaver, _sdkLogger);

            // ScreenActivator ==================================
            _screenActivator = new WcfScreenActivator(sessionManager);

            // Client Proxy =============================
            _clientProxy = new ServiceClientUiManager(sessionManager);

            // UI Proxy =============================
            _uiProxy = new UiProxyManager(_credentialProviderProxy, _clientProxy, _sdkLogger);

            // StatusManager =============================
            _statusManager = new StatusManager(_hesConnection, _rfidService, _connectionManager, _uiProxy, _proximitySettingsManager, _sdkLogger);

            // ConnectionFlowProcessor
            _connectionFlowProcessor = new ConnectionFlowProcessor(
                _deviceManager,
                _hesConnection,
                _credentialProviderProxy,
                _screenActivator,
                _uiProxy,
                _sdkLogger);
            _connectionFlowProcessor.DeviceFinishedMainFlow += ConnectionFlowProcessor_DeviceFinishedMainFlow;
            _advIgnoreList = new AdvertisementIgnoreList(
                _connectionManager,
                _deviceManager,
                _proximitySettingsManager,
                _sdkLogger);
            _rfidProcessor = new RfidConnectionProcessor(
                _connectionFlowProcessor,
                _hesConnection,
                _rfidService,
                _screenActivator,
                _uiProxy,
                _sdkLogger);
            _tapProcessor = new TapConnectionProcessor(
                _connectionFlowProcessor,
                _connectionManager,
                _sdkLogger);
            _proximityProcessor = new ProximityConnectionProcessor(
                _connectionFlowProcessor,
                _connectionManager,
                _proximitySettingsManager,
                _advIgnoreList,
                _deviceManager,
                _credentialProviderProxy,
                _sdkLogger);

            // Proximity Monitor ==================================
            ProximitySettings proximitySettings = _proximitySettingsManager.GetSettingsAsync().Result;
            _proximityMonitorManager = new ProximityMonitorManager(_deviceManager, _sdkLogger, proximitySettings.DevicesProximity);

            // WorkstationLocker ==================================
            _workstationLocker = new WcfWorkstationLocker(sessionManager, _sdkLogger);

            // WorkstationLockProcessor ==================================
            _workstationLockProcessor = new WorkstationLockProcessor(_connectionFlowProcessor, _proximityMonitorManager, 
                _deviceManager, _workstationLocker, _sdkLogger);

            _sessionSwitchLogger = new SessionSwitchLogger(_eventSaver,
                _tapProcessor, _rfidProcessor, _proximityProcessor, 
                _workstationLockProcessor, _deviceManager, _sdkLogger);

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

        void ProximitySettingsManager_SettingsChanged(object sender, SettingsChangedEventArgs<ProximitySettings> e)
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
                device.Disconnected += Device_Disconnected;
            }
        }

        async void DeviceManager_DeviceRemoved(object sender, DeviceCollectionChangedEventArgs e)
        {
            var device = e.RemovedDevice;

            if (device != null)
            {
                device.ConnectionStateChanged -= Device_ConnectionStateChanged;
                device.Initialized -= Device_Initialized;
                device.Disconnected -= Device_Disconnected;

                if (device is IWcfDevice wcfDevice)
                    UnsubscribeFromWcfDeviceEvents(wcfDevice);

                if (!device.IsRemote && device.IsInitialized)
                {
                    var workstationEvent = _eventSaver.GetWorkstationEvent();
                    workstationEvent.EventId = WorkstationEventType.DeviceDeleted;
                    workstationEvent.Severity = WorkstationEventSeverity.Warning;
                    workstationEvent.DeviceId = device.SerialNo;
                    await _eventSaver.AddNewAsync(workstationEvent);
                }
            }
        }

        async void Device_Disconnected(object sender, EventArgs e)
        {
            if (sender is IDevice device && device.IsInitialized && (!device.IsRemote || device.ChannelNo > 2))
            {
                var workstationEvent = _eventSaver.GetWorkstationEvent();
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
                await _eventSaver.AddNewAsync(workstationEvent);
            }
        }

        async void ConnectionManager_AdapterStateChanged(object sender, EventArgs e)
        {
            if (_connectionManager.State == BluetoothAdapterState.Unknown || _connectionManager.State == BluetoothAdapterState.PoweredOn)
            {
                var we = _eventSaver.GetWorkstationEvent();
                if (_connectionManager.State == BluetoothAdapterState.PoweredOn)
                {
                    we.EventId = WorkstationEventType.DonglePlugged;
                    we.Severity = WorkstationEventSeverity.Info;
                }
                else
                {
                    we.EventId = WorkstationEventType.DongleUnplugged;
                    we.Severity = WorkstationEventSeverity.Warning;
                }

                await _eventSaver.AddNewAsync(we);
            }
        }

        //todo - if RFID is not present, do not monitor this event
        bool prevRfidIsConnectedState = false;
        async void RFIDService_ReaderStateChanged(object sender, EventArgs e)
        {
            var isConnected = _rfidService.ServiceConnected && _rfidService.ReaderConnected;
            if (prevRfidIsConnectedState != isConnected)
            {
                prevRfidIsConnectedState = isConnected;

                var we = _eventSaver.GetWorkstationEvent();
                we.EventId = isConnected ? WorkstationEventType.RFIDAdapterPlugged : WorkstationEventType.RFIDAdapterUnplugged;
                we.Severity = isConnected ? WorkstationEventSeverity.Info : WorkstationEventSeverity.Warning;

                await _eventSaver.AddNewAsync(we);
            }
        }

        bool prevHesIsConnectedState = false;
        async void HES_ConnectionStateChanged(object sender, EventArgs e)
        {
            var isConnected = _hesConnection.State == HesConnectionState.Connected;
            if (prevHesIsConnectedState != isConnected)
            {
                prevHesIsConnectedState = isConnected;
                bool sendImmediately = false;
                var we = _eventSaver.GetWorkstationEvent();
                if (_hesConnection.State == HesConnectionState.Connected)
                {
                    we.EventId = WorkstationEventType.HESConnected;
                    we.Severity = WorkstationEventSeverity.Info;
                    sendImmediately = true;
                }
                else
                {
                    we.EventId = WorkstationEventType.HESDisconnected;
                    we.Severity = WorkstationEventSeverity.Warning;
                }

                await _eventSaver.AddNewAsync(we, sendImmediately);
            }
        }

        void DevicesManager_DeviceCollectionChanged(object sender, DeviceCollectionChangedEventArgs e)
        {
            foreach (var client in sessionManager.Sessions)
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
                    foreach (var client in sessionManager.Sessions)
                    {
                        if (!device.IsConnected)
                            device.SetUserProperty(ConnectionFlowProcessor.FLOW_FINISHED_PROP, false);

                        client.Callbacks.DeviceConnectionStateChanged(new DeviceDTO(device));
                    }
                }
            }
            catch (Exception ex)
            {
                Error(ex);
            }
        }

        async void Device_Initialized(object sender, EventArgs e)
        {
            try
            {
                if (sender is IDevice device)
                {
                    foreach (var session in sessionManager.Sessions)
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
                        var workstationEvent = _eventSaver.GetWorkstationEvent();
                        workstationEvent.Severity = WorkstationEventSeverity.Info;
                        workstationEvent.DeviceId = device.SerialNo;
                        if (device.IsRemote)
                        {
                            workstationEvent.EventId = WorkstationEventType.RemoteConnect;
                        }
                        else
                        {
                            workstationEvent.EventId = WorkstationEventType.DeviceConnect;
                        }
                        await _eventSaver.AddNewAsync(workstationEvent);
                    }
                }
            }
            catch (Exception ex)
            {
                Error(ex);
            }
        }

        void ConnectionFlowProcessor_DeviceFinishedMainFlow(object sender, IDevice device)
        {
            foreach (var session in sessionManager.Sessions)
            {
                try
                {
                    session.Callbacks.DeviceFinishedMainFlow(new DeviceDTO(device));
                }
                catch (Exception ex)
                {
                    Error(ex);
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

        public async void PublishEvent(WorkstationEventDTO workstationEvent)
        {
            var we = _eventSaver.GetWorkstationEvent();
            we.Version = WorkstationEvent.ClassVersion;
            we.Id = workstationEvent.Id;
            we.Date = workstationEvent.Date;
            we.EventId = (WorkstationEventType)workstationEvent.EventId;
            we.Severity = (WorkstationEventSeverity)workstationEvent.Severity;
            we.Note = workstationEvent.Note;
            we.DeviceId = workstationEvent.DeviceId;
            we.AccountName = workstationEvent.AccountName;
            we.AccountLogin = workstationEvent.AccountLogin;
            await _eventSaver.AddNewAsync(we);
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
            wcfDevice.DeviceStateChanged += RemoteConnection_DeviceStateChanged;
        }

        void UnsubscribeFromWcfDeviceEvents(IWcfDevice wcfDevice)
        {
            wcfDevice.DeviceStateChanged -= RemoteConnection_DeviceStateChanged;
            RemoteWcfDevices.Remove(wcfDevice);
        }

        void RemoteConnection_DeviceStateChanged(object sender, DeviceStateEventArgs e)
        {
            try
            {
                if (RemoteWcfDevices.Count > 0)
                {
                    if (sender is IWcfDevice wcfDevice)
                    {
                        _client.Callbacks.RemoteConnection_DeviceStateChanged(wcfDevice.Id, new DeviceStateDTO(e.State));
                    }
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
        #endregion
    }
}
