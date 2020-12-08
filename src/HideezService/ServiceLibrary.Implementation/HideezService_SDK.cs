using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Hideez.CsrBLE;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.Device;
using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.Proximity;
using Hideez.SDK.Communication.Utils;
using Hideez.SDK.Communication.PipeDevice;
using Hideez.SDK.Communication.Workstation;
using Hideez.SDK.Communication.WorkstationEvents;
using HideezMiddleware;
using HideezMiddleware.Audit;
using HideezMiddleware.DeviceConnection;
using HideezMiddleware.DeviceLogging;
using HideezMiddleware.IPC.DTO;
using HideezMiddleware.IPC.IncommingMessages;
using HideezMiddleware.IPC.Messages;
using HideezMiddleware.Local;
using HideezMiddleware.ReconnectManager;
using HideezMiddleware.ScreenActivation;
using HideezMiddleware.Settings;
using HideezMiddleware.SoftwareVault.UnlockToken;
using Meta.Lib.Modules.PubSub.Messages;
using Microsoft.Win32;
using ServiceLibrary.Implementation.ClientManagement;
using ServiceLibrary.Implementation.RemoteConnectionManagement;
using ServiceLibrary.Implementation.ScreenActivation;
using ServiceLibrary.Implementation.WorkstationLock;
using HideezMiddleware.DeviceConnection.Workflow;
using Hideez.SDK.Communication.HES.DTO;
using WinBle._10._0._18362;
using Hideez.SDK.Communication.Refactored.BLE;

namespace ServiceLibrary.Implementation
{
    public partial class HideezService
    {
        static BleConnectionManager _csrBleConnectionManager;
        static WinBleConnectionManager _winBleConnectionManager;
        static DeviceManager _deviceManager;
        static CredentialProviderProxy _credentialProviderProxy;
        static HesAppConnection _hesConnection;
        static RfidServiceConnection _rfidService;
        static ProximityMonitorManager _proximityMonitorManager;
        static IScreenActivator _screenActivator;
        static PipeDeviceConnectionManager _pipeDeviceConnectionManager;
        static EventSender _eventSender;
        static ServiceClientUiManager _clientProxy;
        static UiProxyManager _uiProxy;
        static StatusManager _statusManager;
        static IWorkstationLocker _workstationLocker;
        static WorkstationLockProcessor _workstationLockProcessor;

        static ISettingsManager<RfidSettings> _rfidSettingsManager;
        static ISettingsManager<ProximitySettings> _proximitySettingsManager;
        static ISettingsManager<ServiceSettings> _serviceSettingsManager;
        static DeviceProximitySettingsHelper _deviceProximitySettingsHelper;
        static WatchingSettingsManager<WorkstationSettings> _workstationSettingsManager;

        static BondManager _bondManager;
        static ConnectionFlowProcessor _connectionFlowProcessor;
        static AdvertisementIgnoreList _advIgnoreList;
        static RfidConnectionProcessor _rfidProcessor;
        static TapConnectionProcessor _tapProcessor;
        static ProximityConnectionProcessor _proximityProcessor;
        static WinBleAutomaticConnectionProcessor _winBleProcessor;
        static WinBleControllersStateMonitor _winBleControllersStateMonitor;
        static SessionUnlockMethodMonitor _sessionUnlockMethodMonitor;
        static SessionSwitchLogger _sessionSwitchLogger;
        static ConnectionManagerRestarter _connectionManagerRestarter;
        private ConnectionManagersCoordinator _connectionManagersCoordinator;
        static ILocalDeviceInfoCache _localDeviceInfoCache;
        static DeviceLogManager _deviceLogManager;

        static DeviceReconnectManager _deviceReconnectManager;

        static IUnlockTokenProvider _unlockTokenProvider;
        static UnlockTokenGenerator _unlockTokenGenerator;
        static RemoteWorkstationUnlocker _remoteWorkstationUnlocker;

        static HesAppConnection _tbHesConnection;
        static IHesAccessManager _hesAccessManager;

        #region Initialization

        async Task InitializeSDK()
        {
#if DEBUG
            _log.WriteLine($">>>>>> Verifying error codes.");
            var _hideezExceptionLocalization = new HideezExceptionLocalization(_sdkLogger);
            bool isVerified = _hideezExceptionLocalization.VerifyResourcesForErrorCode(new CultureInfo("en"));
            // Debug.Assert(isVerified, $">>>>>> Verifying error codes resalt: {isVerified}");
#endif

            // Collection of module initialization tasks that must be finished to complete service launch
            // but otherwise are not immediatelly required by other modules
            List<Task> serviceInitializationTasks = new List<Task>();

            // Combined path evaluates to '%ProgramData%\\Hideez\\Bonds'
            var commonAppData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            var bondsFolderPath = $"{commonAppData}\\Hideez\\Service\\Bonds";
            var deviceLogsPath = $"{commonAppData}\\Hideez\\Service\\DeviceLogs";

            var bleInitTask = Task.Run(() =>
            {
                // Csr Connection Manager ============================
                Directory.CreateDirectory(bondsFolderPath); // Ensure directory for bonds is created since unmanaged code doesn't do that

                _csrBleConnectionManager = new BleConnectionManager(_sdkLogger, bondsFolderPath);
                _csrBleConnectionManager.AdapterStateChanged += ConnectionManager_AdapterStateChanged;
                _csrBleConnectionManager.DiscoveryStopped += ConnectionManager_DiscoveryStopped;
                _csrBleConnectionManager.DiscoveredDeviceAdded += ConnectionManager_DiscoveredDeviceAdded;
                _csrBleConnectionManager.DiscoveredDeviceRemoved += ConnectionManager_DiscoveredDeviceRemoved;

                // WinBle Connection Manager ============================
                _winBleConnectionManager = new WinBleConnectionManager(_sdkLogger);
                // Todo: subscribtion to WinBleConnectionManager events
                //_winBleConnectionManager.AdapterStateChanged += ConnectionManager_AdapterStateChanged;
                //_winBleConnectionManager.DiscoveryStopped += ConnectionManager_DiscoveryStopped;
                //_winBleConnectionManager.DiscoveredDeviceAdded += ConnectionManager_DiscoveredDeviceAdded;
                //_winBleConnectionManager.DiscoveredDeviceRemoved += ConnectionManager_DiscoveredDeviceRemoved;


                // WinBle Controllers State Monitor ============================
                _winBleControllersStateMonitor = new WinBleControllersStateMonitor((WinBleConnectionManager)_winBleConnectionManager, _sdkLogger);
                _winBleControllersStateMonitor.WinBleControllersCollectionChanged += WinBleControllersStateMonitor_WinBleControllersCollectionChanged;
                _winBleControllersStateMonitor.Start();

                // Connection Managers Coordinator ============================
                _connectionManagersCoordinator = new ConnectionManagersCoordinator();
                //_connectionManagersCoordinator.AddConnectionManager(_csrBleConnectionManager);
                _connectionManagersCoordinator.AddConnectionManager(_winBleConnectionManager);
                _connectionManagersCoordinator.Start();

                _csrBleConnectionManager.StartDiscovery();
                _winBleConnectionManager.StartDiscovery();

                _connectionManagerRestarter = new ConnectionManagerRestarter(_sdkLogger, _csrBleConnectionManager, _winBleConnectionManager);


                // BLE ============================
                _deviceManager = new DeviceManager(_connectionManagersCoordinator, _sdkLogger);

                _deviceManager.DeviceAdded += DevicesManager_DeviceAdded;
                _deviceManager.DeviceRemoved += DevicesManager_DeviceRemoved;
                //_deviceManager.DeviceRemoved += DeviceManager_DeviceRemoved;
                //_deviceManager.DeviceAdded += DeviceManager_DeviceAdded;
                SessionSwitchMonitor.SessionSwitch += SessionSwitchMonitor_SessionSwitch;

                // Pipe Device Connection Manager ============================
                _pipeDeviceConnectionManager = new PipeDeviceConnectionManager(_deviceManager, _messenger, _sdkLogger);
                _sdkLogger.WriteLine(nameof(HideezService), "BLE initialized");
            });

            var pipeInitTask = Task.Run(() =>
            {
                // Named Pipes Server ==============================
                _credentialProviderProxy = new CredentialProviderProxy(_sdkLogger);
                _credentialProviderProxy.Start(); // Faster we connect to the CP, the better

                // RFID Service Connection ============================
                _rfidService = new RfidServiceConnection(_sdkLogger);
                _rfidService.RfidReaderStateChanged += RFIDService_ReaderStateChanged;
                _sdkLogger.WriteLine(nameof(HideezService), "Pipe initialized");
            });


            // Settings
            var settingsDirectory = $@"{commonAppData}\Hideez\Service\Settings\";
            string sdkSettingsPath = Path.Combine(settingsDirectory, "Sdk.xml");
            string rfidSettingsPath = Path.Combine(settingsDirectory, "Rfid.xml");
            string proximitySettingsPath = Path.Combine(settingsDirectory, "Unlock.xml");
            string serviceSettingsPath = Path.Combine(settingsDirectory, "Service.xml");
            string workstationSettingsPath = Path.Combine(settingsDirectory, "Workstation.xml");
            IFileSerializer fileSerializer = new XmlFileSerializer(_sdkLogger);

            List<Task> settingsInitializationTasks = new List<Task>
            {
                Task.Run(async () =>
                {
                    var sdkSettingsManager = new SettingsManager<SdkSettings>(sdkSettingsPath, fileSerializer);
                    sdkSettingsManager.InitializeFileStruct();
                    await SdkConfigLoader.LoadSdkConfigFromFileAsync(sdkSettingsManager).ConfigureAwait(false);
                    _sdkLogger.WriteLine(nameof(HideezService), $"{nameof(SdkSettings)} loaded");
                }),
                Task.Run(async () =>
                {
                    _rfidSettingsManager = new SettingsManager<RfidSettings>(rfidSettingsPath, fileSerializer);
                    _rfidSettingsManager.InitializeFileStruct();
                    await _rfidSettingsManager.LoadSettingsAsync().ConfigureAwait(false);
                    _sdkLogger.WriteLine(nameof(HideezService), $"{nameof(RfidSettings)} loaded");
                }),
                Task.Run(async () =>
                {
                    _proximitySettingsManager = new SettingsManager<ProximitySettings>(proximitySettingsPath, fileSerializer);
                    _proximitySettingsManager.InitializeFileStruct();
                    await _proximitySettingsManager.GetSettingsAsync().ConfigureAwait(false);

                    _deviceProximitySettingsHelper = new DeviceProximitySettingsHelper(_proximitySettingsManager);
                    _sdkLogger.WriteLine(nameof(HideezService), $"{nameof(ProximitySettings)} loaded");
                }),
                Task.Run(async () =>
                {
                    _serviceSettingsManager = new SettingsManager<ServiceSettings>(serviceSettingsPath, fileSerializer);
                    _serviceSettingsManager.InitializeFileStruct();
                    await _serviceSettingsManager.LoadSettingsAsync().ConfigureAwait(false);
                    _sdkLogger.WriteLine(nameof(HideezService), $"{nameof(ServiceSettings)} loaded");
                }),
                Task.Run(async () =>
                {
                    _workstationSettingsManager = new WatchingSettingsManager<WorkstationSettings>(workstationSettingsPath, fileSerializer);
                    _workstationSettingsManager.InitializeFileStruct();
                    await _workstationSettingsManager.LoadSettingsAsync().ConfigureAwait(false);
                    _workstationSettingsManager.AutoReloadOnFileChanges = true;
                    _workstationSettingsManager.SettingsChanged += WorkstationProximitySettingsManager_SettingsChanged;
                    _sdkLogger.WriteLine(nameof(HideezService), $"{nameof(WorkstationSettings)} loaded");
                })
            };
            await Task.WhenAll(settingsInitializationTasks).ConfigureAwait(false);
            _sdkLogger.WriteLine(nameof(HideezService), "Settings loaded");

            // Get HES address from registry ==================================
            // HKLM\SOFTWARE\Hideez\Client, client_hes_address REG_SZ
            string hesAddress = RegistrySettings.GetHesAddress(_log);

            if (!string.IsNullOrEmpty(hesAddress))
            {
                ServicePointManager.ServerCertificateValidationCallback +=
                (sender, cert, chain, error) =>
                {
                    if (sender is HttpWebRequest request)
                    {
                        if (request.Address.AbsoluteUri.StartsWith(hesAddress))
                            return true;
                    }
                    return error == SslPolicyErrors.None;
                };
            }

            // WorkstationInfoProvider ==================================
            WorkstationHelper.Log = _sdkLogger;
            var workstationInfoProvider = new WorkstationInfoProvider(_workstationIdProvider, _serviceSettingsManager, _sdkLogger);

            // HesAccessManager ==================================
            _hesAccessManager = new HesAccessManager(clientRootRegistryKey, _sdkLogger);
            _hesAccessManager.AccessRetractedEvent += HesAccessManager_AccessRetractedEvent;

            // TB & HES Connections ==================================
            await bleInitTask.ConfigureAwait(false);
            await pipeInitTask.ConfigureAwait(false);
            await CreateHubs(_deviceManager, workstationInfoProvider, _proximitySettingsManager, _rfidSettingsManager, _hesAccessManager, _sdkLogger).ConfigureAwait(false);

            serviceInitializationTasks.Add(StartHubs(hesAddress));

            // Software Vault Unlock Mechanism
            _unlockTokenProvider = new UnlockTokenProvider(clientRootRegistryKey, _sdkLogger);
            _unlockTokenGenerator = new UnlockTokenGenerator(_unlockTokenProvider, workstationInfoProvider, _sdkLogger);
            _remoteWorkstationUnlocker = new RemoteWorkstationUnlocker(_unlockTokenProvider, _tbHesConnection, _credentialProviderProxy, _sdkLogger);

            // Start Software Vault unlock modules
            if (_serviceSettingsManager.Settings.EnableSoftwareVaultUnlock)
                serviceInitializationTasks.Add(Task.Run(_unlockTokenGenerator.Start));

            // Audit Log / Event Aggregator =============================
            _eventSender = new EventSender(_hesConnection, _eventSaver, _sdkLogger);

            // ScreenActivator ==================================
            _screenActivator = new MetalibScreenActivator(_messenger);

            // Client Proxy =============================
            _clientProxy = new ServiceClientUiManager(_messenger);

            // UI Proxy =============================
            _uiProxy = new UiProxyManager(_credentialProviderProxy, _clientProxy, _sdkLogger);

            // StatusManager =============================
            _statusManager = new StatusManager(_hesConnection, _tbHesConnection, _rfidService,
                _csrBleConnectionManager, _winBleConnectionManager, _uiProxy, 
                _rfidSettingsManager, _credentialProviderProxy, _sdkLogger);

            // Local device info cache
            _localDeviceInfoCache = new LocalDeviceInfoCache(clientRootRegistryKey, _sdkLogger);

            //BondManager
            _bondManager = new BondManager(bondsFolderPath, _sdkLogger);

            // ConnectionFlowProcessor
            var connectionFlowProcessorfactory = new ConnectionFlowProcessorFactory(
                _deviceManager,
                _bondManager,
                _hesConnection,
                _credentialProviderProxy,
                _screenActivator,
                _uiProxy,
                _hesAccessManager,
                _serviceSettingsManager,
                _localDeviceInfoCache,
                _sdkLogger);
            _connectionFlowProcessor = connectionFlowProcessorfactory.Create();

            _deviceLogManager = new DeviceLogManager(deviceLogsPath, new DeviceLogWriter(), _serviceSettingsManager, _connectionFlowProcessor, _sdkLogger);
            _connectionFlowProcessor.DeviceFinishedMainFlow += ConnectionFlowProcessor_DeviceFinishedMainFlow;
            _advIgnoreList = new AdvertisementIgnoreList(
                _csrBleConnectionManager,
                _workstationSettingsManager,
                _sdkLogger);
            _rfidProcessor = new RfidConnectionProcessor(
                _connectionFlowProcessor,
                _hesConnection,
                _rfidService,
                _rfidSettingsManager,
                _screenActivator,
                _uiProxy,
                _sdkLogger);
            _tapProcessor = new TapConnectionProcessor(
                _connectionFlowProcessor,
                _csrBleConnectionManager,
                _sdkLogger);
            _proximityProcessor = new ProximityConnectionProcessor(
                _connectionFlowProcessor,
                _csrBleConnectionManager,
                _proximitySettingsManager,
                _workstationSettingsManager,
                _advIgnoreList,
                _deviceManager,
                _credentialProviderProxy,
                _hesAccessManager,
                _sdkLogger);
            _winBleProcessor = new WinBleAutomaticConnectionProcessor(
                _connectionFlowProcessor,
                _winBleConnectionManager,
                _workstationSettingsManager,
                _deviceManager,
                _sdkLogger);

            serviceInitializationTasks.Add(Task.Run(() =>
            {
                _proximityProcessor.Start();
                _tapProcessor.Start();
                _rfidProcessor.Start();
                _winBleProcessor.Start();
            }));

            // Proximity Monitor ==================================
            ProximitySettings proximitySettings = _proximitySettingsManager.Settings;
            _proximityMonitorManager = new ProximityMonitorManager(_deviceManager, _workstationSettingsManager.Settings.GetProximityMonitorSettings(), _sdkLogger);

            // Device Reconnect Manager ================================
            _deviceReconnectManager = new DeviceReconnectManager(_proximityMonitorManager, _deviceManager, _connectionFlowProcessor, _sdkLogger);
            _deviceReconnectManager.DeviceReconnected += (s, a) => _log.WriteLine($"Device {a.SerialNo} reconnected successfully");

            // WorkstationLocker ==================================
            _workstationLocker = new UniversalWorkstationLocker(SdkConfig.DefaultLockTimeout * 1000, _messenger, _sdkLogger);

            // WorkstationLockProcessor ==================================
            _workstationLockProcessor = new WorkstationLockProcessor(_connectionFlowProcessor,
                _proximityMonitorManager,
                _deviceManager,
                _workstationLocker,
                _deviceReconnectManager,
                _sdkLogger);
            _workstationLockProcessor.DeviceProxLockEnabled += WorkstationLockProcessor_DeviceProxLockEnabled;

            //SessionUnlockMethodMonitor ==================================
            _sessionUnlockMethodMonitor = new SessionUnlockMethodMonitor(_connectionFlowProcessor,
                 _tapProcessor, _rfidProcessor, _proximityProcessor, _winBleProcessor, _sdkLogger);

            // SessionSwitchLogger ==================================
            _sessionSwitchLogger = new SessionSwitchLogger(_eventSaver, _sessionUnlockMethodMonitor,
                _workstationLockProcessor, _deviceManager, _sdkLogger);

            // SDK initialization finished, start essential components
            if (_serviceSettingsManager.Settings.EnableSoftwareVaultUnlock)
                _remoteWorkstationUnlocker.Start();

            _rfidService.Start();

            _workstationLockProcessor.Start();
            _proximityMonitorManager.Start();

            _csrBleConnectionManager.StartDiscovery();
            _connectionManagerRestarter.Start();

            _deviceReconnectManager.Start();

            _messenger.Subscribe<RemoteClientConnectedEvent>(OnClientConnected);
            _messenger.Subscribe<RemoteClientDisconnectedEvent>(OnClientDisconnected);

            _messenger.Subscribe<GetAvailableChannelsMessage>(GetAvailableChannels);

            _messenger.Subscribe<PublishEventMessage>(PublishEvent);
            _messenger.Subscribe<DisconnectDeviceMessage>(DisconnectDevice);
            _messenger.Subscribe<RemoveDeviceMessage>(RemoveDeviceAsync);

            _messenger.Subscribe<ChangeServerAddressMessage>(ChangeServerAddress);
            _messenger.Subscribe<SetSoftwareVaultUnlockModuleStateMessage>(SetSoftwareVaultUnlockModuleState);

            _messenger.Subscribe<CancelActivationCodeMessage>(CancelActivationCode);
            _messenger.Subscribe<SendActivationCodeMessage>(SendActivationCode);

            _messenger.Subscribe<LoginClientRequestMessage>(OnClientLogin);
            _messenger.Subscribe<RefreshServiceInfoMessage>(OnRefreshServiceInfo);

            await Task.WhenAll(serviceInitializationTasks).ConfigureAwait(false);
        }

        Task<HesAppConnection> CreateHesHub(DeviceManager deviceManager,
            WorkstationInfoProvider workstationInfoProvider,
            ISettingsManager<ProximitySettings> proximitySettingsManager,
            ISettingsManager<RfidSettings> rfidSettingsManager,
            IHesAccessManager hesAccessManager,
            ILog log)
        {
            return Task.Run(() =>
            {
                var hesConnection = new HesAppConnection(deviceManager, workstationInfoProvider, hesAccessManager, log);

                hesConnection.HubProximitySettingsArrived += async (sender, receivedSettings) =>
                {
                    var settings = await proximitySettingsManager.GetSettingsAsync().ConfigureAwait(false);
                    settings.DevicesProximity = receivedSettings.ToArray();
                    proximitySettingsManager.SaveSettings(settings);
                };
                hesConnection.HubRFIDIndicatorStateArrived += async (sender, isEnabled) =>
                {
                    var settings = await rfidSettingsManager.GetSettingsAsync().ConfigureAwait(false);
                    settings.IsRfidEnabled = isEnabled;
                    rfidSettingsManager.SaveSettings(settings);
                };
                hesConnection.HubConnectionStateChanged += HES_ConnectionStateChanged;
                hesConnection.LockHwVaultStorageRequest += HES_LockDeviceStorageRequest;
                hesConnection.LiftHwVaultStorageLockRequest += HES_LiftDeviceStorageLockRequest;
                hesConnection.Alarm += HesConnection_Alarm;

                return hesConnection;
            });
        }

        Task<HesAppConnection> CreateTbHesHub(WorkstationInfoProvider workstationInfoProvider, ILog log)
        {
            return Task.Run(() =>
            {
                return new HesAppConnection(workstationInfoProvider, _sdkLogger);
            });
        }

        Task CreateHubs(DeviceManager deviceManager,
            WorkstationInfoProvider workstationInfoProvider,
            ISettingsManager<ProximitySettings> proximitySettingsManager,
            ISettingsManager<RfidSettings> rfidSettingsManager,
            IHesAccessManager hesAccessManager,
            ILog log)
        {
            return Task.Run(async () =>
            {
                var hubTask = CreateHesHub(deviceManager, workstationInfoProvider, proximitySettingsManager, rfidSettingsManager, hesAccessManager, log);
                var tbHubTask = CreateTbHesHub(workstationInfoProvider, log);

                await Task.WhenAll(hubTask, tbHubTask).ConfigureAwait(false);

                _hesConnection = hubTask.Result;
                _tbHesConnection = tbHubTask.Result;
            });
        }

        Task StartHubs(string hesAddress)
        {
            return Task.Run(async () =>
            {
                var tasks = new List<Task>
                {
                    StartHesHub(hesAddress),
                    StartTbHesHub()
                };

                await Task.WhenAll(tasks).ConfigureAwait(false);
            });
        }

        Task StartHesHub(string hesAddress)
        {
            return Task.Run(async () =>
            {
                if (!string.IsNullOrWhiteSpace(hesAddress))
                {
                    _hesConnection.Start(hesAddress); // Launch HES connection immediatelly to save time
                    if (_hesConnection.State == HesConnectionState.Error)
                    {
                        await _hesConnection.Stop().ConfigureAwait(false);

                        Error("Hideez Service has encountered an error during HES connection initialization"
                            + Environment.NewLine
                            + "New connection establishment will be attempted after service restart"
                            + Environment.NewLine
                            + _hesConnection.ErrorMessage);
                    }
                }
                else
                {
                    Error("HES connection initialization aborted. HES address is not specified."
                        + Environment.NewLine
                        + "New connection establishment will be attempted after service restart");
                }
            });
        }

        Task StartTbHesHub()
        {
            return Task.Run(async () =>
            {
                _tbHesConnection.Start("https://testhub.hideez.com/"); // Launch Try&Buy immediatelly to reduce loading time
                if (_tbHesConnection.State == HesConnectionState.Error)
                {
                    await _tbHesConnection.Stop().ConfigureAwait(false);

                    Error("Try & Buy server is not available"
                        + Environment.NewLine
                        + "New connection establishment will be attempted after service restart"
                        + Environment.NewLine
                        + _tbHesConnection.ErrorMessage);
                }
            });
        }

        #endregion

        #region Event Handlers

        Task OnClientConnected(RemoteClientConnectedEvent arg)
        {
            _log.WriteLine($">>>>>> AttachClient, {arg.TotalClientsCount} remaining clients", LogErrorSeverity.Debug);
            return Task.CompletedTask;
        }

        Task OnClientDisconnected(RemoteClientDisconnectedEvent arg)
        {
            _log.WriteLine($">>>>>> DetachClient, {arg.TotalClientsCount} remaining clients", LogErrorSeverity.Debug);
            return Task.CompletedTask;
        }

        // This message is sent by client when its ready to receive messages from server
        // Upon receiving LoginClientRequestMessage we sent to the client all currently available information
        // Previously the client had to ask for each bit separatelly. Now we instead send it upon connection establishment
        // The updates in sent information are sent separatelly
        async Task OnClientLogin(LoginClientRequestMessage arg)
        {
            _log.WriteLine($"Service client login");

            await RefreshServiceInfo();

            // Client may have only been launched after we already sent an event about workstation unlock
            // This may happen if we are login into the new session where application is not running during unlock but loads afterwards
            // To avoid the confusion, we resend the event about latest unlock method to every client that connects to service
            if (_deviceManager.Devices.Count() == 0)
                await _messenger.Publish(new WorkstationUnlockedMessage(_sessionUnlockMethodMonitor.GetUnlockMethod() == SessionSwitchSubject.NonHideez));

        }

        async Task OnRefreshServiceInfo(RefreshServiceInfoMessage arg)
        {
            await RefreshServiceInfo();
        }

        async void HesAccessManager_AccessRetractedEvent(object sender, EventArgs e)
        {
            // Disconnect all connected devices
            _deviceReconnectManager.DisableAllDevicesReconnect();
            foreach (var device in _deviceManager.Devices)
            {
                await _deviceManager.Disconnect(device.DeviceConnection).ConfigureAwait(false);
            }
        }

        void WorkstationProximitySettingsManager_SettingsChanged(object sender, SettingsChangedEventArgs<WorkstationSettings> e)
        {
            try
            {
                if (_proximityMonitorManager != null)
                {
                    _log.WriteLine("Updating proximity monitors with new settings");
                    _proximityMonitorManager.MonitorSettings = e.NewSettings.GetProximityMonitorSettings();
                }
            }
            catch (Exception ex)
            {
                _log.WriteLine(ex);
            }
        }

        //void DeviceManager_DeviceAdded(object sender, DeviceCollectionChangedEventArgs e)
        //{
        //    var device = e.AddedDevice;

        //    if (device != null)
        //    {
        //        device.ConnectionStateChanged += Device_ConnectionStateChanged;
        //        device.Initialized += Device_Initialized;
        //        device.Disconnected += Device_Disconnected;
        //        device.OperationCancelled += Device_OperationCancelled;
        //        device.ProximityChanged += Device_ProximityChanged;
        //        device.BatteryChanged += Device_BatteryChanged;
        //        device.WipeFinished += Device_WipeFinished;
        //        device.AccessLevelChanged += Device_AccessLevelChanged;
        //    }
        //}

        //async void DeviceManager_DeviceRemoved(object sender, DeviceCollectionChangedEventArgs e)
        //{
        //    var device = e.RemovedDevice;

        //    if (device != null)
        //    {
        //        device.ConnectionStateChanged -= Device_ConnectionStateChanged;
        //        device.Initialized -= Device_Initialized;
        //        device.Disconnected -= Device_Disconnected;
        //        device.OperationCancelled -= Device_OperationCancelled;
        //        device.ProximityChanged -= Device_ProximityChanged;
        //        device.BatteryChanged -= Device_BatteryChanged;
        //        device.WipeFinished -= Device_WipeFinished;
        //        device.AccessLevelChanged -= Device_AccessLevelChanged;

        //        if (device is IPipeDevice pipeDevice)
        //            _pipeDeviceConnectionManager.RemovePipeDevice(pipeDevice);

        //        if (!device.IsRemote && device.IsInitialized)
        //        {
        //            var workstationEvent = _eventSaver.GetWorkstationEvent();
        //            workstationEvent.EventId = WorkstationEventType.DeviceDeleted;
        //            workstationEvent.Severity = WorkstationEventSeverity.Warning;
        //            workstationEvent.DeviceId = device.SerialNo;
        //            await _eventSaver.AddNewAsync(workstationEvent);
        //        }
        //    }
        //}

        async void Device_Disconnected(object sender, EventArgs e)
        {
            if (sender is IDevice device && device.IsInitialized && (!(device is IRemoteDeviceProxy) || device.ChannelNo > 2))
            {
                var workstationEvent = _eventSaver.GetWorkstationEvent();
                workstationEvent.Severity = WorkstationEventSeverity.Info;
                workstationEvent.DeviceId = device.SerialNo;
                if (device is IRemoteDeviceProxy)
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
            if (_csrBleConnectionManager.State == BluetoothAdapterState.Unknown || _csrBleConnectionManager.State == BluetoothAdapterState.PoweredOn)
            {
                var we = _eventSaver.GetWorkstationEvent();
                if (_csrBleConnectionManager.State == BluetoothAdapterState.PoweredOn)
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

                    // Clear all devices on disconnect
                    await _messenger.Publish(new LiftDeviceStorageLockMessage(string.Empty));
                }

                await _eventSaver.AddNewAsync(we, sendImmediately);
            }
        }

        async void HES_LockDeviceStorageRequest(object sender, string serialNo)
        {
            try
            {
                await _messenger.Publish(new LockDeviceStorageMessage(serialNo));
            }
            catch (Exception)
            {
                // Silent handling
            }
        }

        async void HES_LiftDeviceStorageLockRequest(object sender, string serialNo)
        {
            try
            {
                await _messenger.Publish(new LiftDeviceStorageLockMessage(serialNo));
            }
            catch (Exception)
            {
                // Silent handling
            }
        }

        async void DevicesManager_DeviceAdded(object sender, DeviceAddedEventArgs e)
        {
            await _messenger.Publish(new DevicesCollectionChangedMessage(GetDevices()));

            var device = e.Device;

            if (device != null)
            {
                device.ConnectionStateChanged += Device_ConnectionStateChanged;
                device.Initialized += Device_Initialized;
                device.Disconnected += Device_Disconnected;
                device.OperationCancelled += Device_OperationCancelled;
                device.ProximityChanged += Device_ProximityChanged;
                device.BatteryChanged += Device_BatteryChanged;
                device.WipeFinished += Device_WipeFinished;
                device.AccessLevelChanged += Device_AccessLevelChanged;
            }
        }

        async void DevicesManager_DeviceRemoved(object sender, DeviceRemovedEventArgs e)
        {
            await _messenger.Publish(new DevicesCollectionChangedMessage(GetDevices()));
            var device = e.Device;

            if (device != null)
            {
                device.ConnectionStateChanged -= Device_ConnectionStateChanged;
                device.Initialized -= Device_Initialized;
                device.Disconnected -= Device_Disconnected;
                device.OperationCancelled -= Device_OperationCancelled;
                device.ProximityChanged -= Device_ProximityChanged;
                device.BatteryChanged -= Device_BatteryChanged;
                device.WipeFinished -= Device_WipeFinished;
                device.AccessLevelChanged -= Device_AccessLevelChanged;

                if (device is PipeRemoteDeviceProxy pipeDevice)
                    _pipeDeviceConnectionManager.RemovePipeDeviceConnection(pipeDevice);

                if (!(device is IRemoteDeviceProxy) && device.IsInitialized)
                {
                    var workstationEvent = _eventSaver.GetWorkstationEvent();
                    workstationEvent.EventId = WorkstationEventType.DeviceDeleted;
                    workstationEvent.Severity = WorkstationEventSeverity.Warning;
                    workstationEvent.DeviceId = device.SerialNo;
                    await _eventSaver.AddNewAsync(workstationEvent);
                }
            }
        }

        private async void HesConnection_Alarm(object sender, bool isEnabled)
        {
            try
            {
                if (_serviceSettingsManager.Settings.AlarmTurnOn != isEnabled)
                {
                    var settings = _serviceSettingsManager.Settings;
                    settings.AlarmTurnOn = isEnabled;
                    _serviceSettingsManager.SaveSettings(settings);

                    if (isEnabled)
                    {
                        foreach (var device in _deviceManager.Devices)
                            await _deviceManager.RemoveConnection(device.DeviceConnection);
                        //_deviceManager.RemoveAll() remove only bonds for devices that are connected
                        //So the left files need to be deleted manually
                        await _bondManager.RemoveAll();

                        var wsLocker = new WcfWorkstationLocker(_messenger, _sdkLogger);
                        wsLocker.LockWorkstation();

                        _connectionManagerRestarter.Stop();
                        await Task.Run(async () =>
                        {
                            await Task.Delay(1000);
                            _log.WriteLine("Restarting connection manager");
                            _csrBleConnectionManager.Restart();
                            _connectionManagerRestarter.Start();
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Error(ex);
            }
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

        async void Device_ConnectionStateChanged(object sender, EventArgs e)
        {
            try
            {
                if (sender is IDevice device)
                {
                    if (!device.IsConnected)
                        device.SetUserProperty(CustomProperties.HW_CONNECTION_STATE_PROP, HwVaultConnectionState.Offline);

                    await _messenger.Publish(new DeviceConnectionStateChangedMessage(new DeviceDTO(device)));
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
                    // Separate error handling block for each callback ensures we try to notify 
                    // every session, even if an error occurs
                    try
                    {
                        await _messenger.Publish(new DeviceInitializedMessage(new DeviceDTO(device)));
                    }
                    catch (Exception ex)
                    {
                        Error(ex);
                    }

                    if (!(device is IRemoteDeviceProxy) || device.ChannelNo > 2)
                    {
                        var workstationEvent = _eventSaver.GetWorkstationEvent();
                        workstationEvent.Severity = WorkstationEventSeverity.Info;
                        workstationEvent.DeviceId = device.SerialNo;
                        if (device is IRemoteDeviceProxy)
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

        async void Device_OperationCancelled(object sender, EventArgs e)
        {
            try
            {
                if (sender is IDevice device)
                {
                    await _messenger.Publish(new DeviceOperationCancelledMessage(new DeviceDTO(device)));
                }
            }
            catch (Exception ex)
            {
                Error(ex);
            }
        }

        async void Device_ProximityChanged(object sender, double e)
        {
            if (sender is IDevice device)
            {
                try
                {
                    await _messenger.Publish(new DeviceProximityChangedMessage(device.Id, e));
                }
                catch (Exception ex)
                {
                    Error(ex);
                }
            }
        }

        async void Device_BatteryChanged(object sender, sbyte e)
        {
            if (sender is IDevice device)
            {
                try
                {
                    await _messenger.Publish(new DeviceBatteryChangedMessage(device.Id, device.Mac, e));
                }
                catch (Exception ex)
                {
                    Error(ex);
                }
            }

        }

        void Device_WipeFinished(object sender, WipeFinishedEventtArgs e)
        {
            if (e.Status == FwWipeStatus.WIPE_OK)
            {
                var device = (IDevice)sender;
                _log?.WriteLine($"({device.SerialNo}) Wipe finished. Disabling automatic reconnect");
                _deviceReconnectManager.DisableDeviceReconnect(device);
                device.Disconnected += async (s, a) =>
                {
                    try
                    {
                        await _deviceManager.RemoveConnection(device.DeviceConnection);
                    }
                    catch (Exception ex)
                    {
                        Error(ex);
                    }
                };

            }
        }

        async void Device_AccessLevelChanged(object sender, AccessLevel e)
        {
            var device = (IDevice)sender;
            if (device.ChannelNo == (int)DefaultDeviceChannel.Main)
            {
                try
                {
                    await device.RefreshDeviceInfo();
                }
                catch { }
            }
        }

        async void ConnectionFlowProcessor_DeviceFinishedMainFlow(object sender, IDevice device)
        {
            try
            {
                await _messenger.Publish(new DeviceFinishedMainFlowMessage(new DeviceDTO(device)));
            }
            catch (Exception ex)
            {
                Error(ex);
            }
        }

        async void WorkstationLockProcessor_DeviceProxLockEnabled(object sender, IDevice device)
        {
            try
            {
                await _messenger.Publish(new DeviceProximityLockEnabledMessage(new DeviceDTO(device)));
            }
            catch (Exception ex)
            {
                Error(ex);
            }
        }

        async void SessionManager_SessionClosed()
        {
            // TODO: Disconnect remote devices when client disconnects
        }

        async void SessionSwitchMonitor_SessionSwitch(int sessionId, SessionSwitchReason reason)
        {
            try
            {
                if (reason == SessionSwitchReason.SessionUnlock || reason == SessionSwitchReason.SessionLogon)
                    await _messenger.Publish(new WorkstationUnlockedMessage(_sessionUnlockMethodMonitor.GetUnlockMethod() == SessionSwitchSubject.NonHideez));

                if (reason == SessionSwitchReason.SessionLogoff || reason == SessionSwitchReason.SessionLock)
                {
                    // Disconnect all connected devices
                    _deviceReconnectManager.DisableAllDevicesReconnect();
                    foreach (var device in _deviceManager.Devices)
                        await _deviceManager.Disconnect(device.DeviceConnection);
                }
            }
            catch (Exception ex)
            {
                Error(ex);
            }
        }

        async void WinBleControllersStateMonitor_WinBleControllersCollectionChanged(object sender, WinBleControllersCollectionChangedEventArgs e)
        {
            // Todo:
        }
        #endregion

        #region  Incomming Messages Handlers
        DeviceDTO[] GetDevices()
        {
            try
            {
                return _deviceManager.Devices.Select(d => new DeviceDTO(d)).ToArray();
            }
            catch (Exception ex)
            {
                Error(ex);
                throw;
            }
        }

        async Task GetAvailableChannels(GetAvailableChannelsMessage args)
        {
            try
            {
                var devices = _deviceManager.Devices.Where(d => d.SerialNo == args.SerialNo).ToList();
                if (devices.Count == 0)
                    throw new HideezException(HideezErrorCode.DeviceNotFound, args.SerialNo);

                // Channels range from 1 to 6 
                List<byte> freeChannels = new List<byte>() { 1, 2, 3, 4, 5, 6 };

                // These channels are reserved by system, the rest is available to clients
                freeChannels.Remove((byte)DefaultDeviceChannel.Main);
                freeChannels.Remove((byte)DefaultDeviceChannel.HES);

                // Filter out taken channels
                var channelsInUse = devices.Select(d => d.ChannelNo).ToList();
                freeChannels.RemoveAll(c => channelsInUse.Contains(c));

                await _messenger.Publish(new GetAvailableChannelsMessageReply(freeChannels.ToArray()));
            }
            catch (Exception ex)
            {
                Error(ex);
                throw;
            }
        }

        async Task DisconnectDevice(DisconnectDeviceMessage args)
        {
            try
            {
                var device = _deviceManager.Devices.FirstOrDefault(d => d.Id == args.DeviceId);
                if (device != null)
                {

                    _deviceReconnectManager.DisableDeviceReconnect(device);
                    await _deviceManager.Disconnect(device.DeviceConnection);
                }
            }
            catch (Exception ex)
            {
                Error(ex);
                throw;
            }
        }

        async Task RemoveDeviceAsync(RemoveDeviceMessage args)
        {
            try
            {
                var device = _deviceManager.Devices.FirstOrDefault(d=>d.Id == args.DeviceId);
                if (device != null)
                {
                    _deviceReconnectManager.DisableDeviceReconnect(device);
                    await _deviceManager.RemoveConnection(device.DeviceConnection);
                }
            }
            catch (Exception ex)
            {
                Error(ex);
                throw;
            }
        }

        async Task PublishEvent(PublishEventMessage args)
        {
            var workstationEvent = args.WorkstationEvent;
            var we = _eventSaver.GetWorkstationEvent();
            we.Version = WorkstationEvent.ClassVersion;
            we.Id = workstationEvent.Id;
            we.Date = workstationEvent.Date;
            we.TimeZone = workstationEvent.TimeZone;
            we.EventId = (WorkstationEventType)workstationEvent.EventId;
            we.Severity = (WorkstationEventSeverity)workstationEvent.Severity;
            we.Note = workstationEvent.Note;
            we.DeviceId = workstationEvent.DeviceId;
            we.AccountName = workstationEvent.AccountName;
            we.AccountLogin = workstationEvent.AccountLogin;
            await _eventSaver.AddNewAsync(we);
        }

        public void SetProximitySettings(string mac, int lockProximity, int unlockProximity)
        {
            _deviceProximitySettingsHelper?.SetClientProximity(mac, lockProximity, unlockProximity);
        }

        public ProximitySettingsDTO GetCurrentProximitySettings(string mac)
        {
            return new ProximitySettingsDTO
            {
                Mac = mac,
                SerialNo = string.Empty,
                LockProximity = _workstationSettingsManager.Settings.LockProximity,
                UnlockProximity = _workstationSettingsManager.Settings.UnlockProximity,
                AllowEditProximitySettings = _deviceProximitySettingsHelper?.GetAllowEditProximity(mac) ?? false,
            };
        }

        public string GetServerAddress()
        {
            return RegistrySettings.GetHesAddress(_log);
        }

        public async Task ChangeServerAddress(ChangeServerAddressMessage args)
        {
            try
            {
                var address = args.ServerAddress;
                _log.WriteLine($"Client requested HES address change to \"{address}\"");

                if (string.IsNullOrWhiteSpace(address))
                {
                    _log.WriteLine($"Clearing server address and shutting down connection");
                    RegistrySettings.SetHesAddress(_log, address);
                    await _hesConnection.Stop();
                    await _messenger.Publish(new ChangeServerAddressMessageReply(ChangeServerAddressResult.Success));
                }
                else
                {
                    var connectedOnNewAddress = await HubConnectivityChecker.CheckHubConnectivity(address, _sdkLogger).TimeoutAfter(5_000);
                    if (connectedOnNewAddress)
                    {
                        _log.WriteLine($"Passed connectivity check to {address}");
                        RegistrySettings.SetHesAddress(_log, address);
                        await _hesConnection.Stop();
                        _hesConnection.Start(address);

                        await _messenger.Publish(new ChangeServerAddressMessageReply(ChangeServerAddressResult.Success));
                    }
                    else
                    {
                        _log.WriteLine($"Failed connectivity check to {address}");
                        await _messenger.Publish(new ChangeServerAddressMessageReply(ChangeServerAddressResult.ConnectionTimedOut));
                    }
                }
            }
            catch (Exception ex)
            {
                _log.WriteLine(ex.Message, LogErrorSeverity.Information);

                if (ex is TimeoutException)
                    await _messenger.Publish(new ChangeServerAddressMessageReply(ChangeServerAddressResult.ConnectionTimedOut));
                else if (ex is KeyNotFoundException)
                    await _messenger.Publish(new ChangeServerAddressMessageReply(ChangeServerAddressResult.KeyNotFound));
                else if (ex is UnauthorizedAccessException)
                    await _messenger.Publish(new ChangeServerAddressMessageReply(ChangeServerAddressResult.UnauthorizedAccess));
                else if (ex is SecurityException)
                    await _messenger.Publish(new ChangeServerAddressMessageReply(ChangeServerAddressResult.SecurityError));
                else
                    await _messenger.Publish(new ChangeServerAddressMessageReply(ChangeServerAddressResult.UnknownError));
            }
        }

        async Task SetSoftwareVaultUnlockModuleState(SetSoftwareVaultUnlockModuleStateMessage args)
        {
            var settings = _serviceSettingsManager.Settings;
            if (settings.EnableSoftwareVaultUnlock != args.Enabled)
            {
                _log.WriteLine($"Client requested to switch software unlock module. New value: {args.Enabled}");
                settings.EnableSoftwareVaultUnlock = args.Enabled;
                _serviceSettingsManager.SaveSettings(settings);

                if (args.Enabled)
                {
                    _unlockTokenGenerator.Start();
                    _remoteWorkstationUnlocker.Start();
                    await RefreshServiceInfo();
                }
                else
                {
                    _unlockTokenGenerator.Stop();
                    _remoteWorkstationUnlocker.Stop();
                    _unlockTokenGenerator.DeleteSavedToken();
                    await RefreshServiceInfo();
                }
            }
        }

        async Task RefreshServiceInfo() // Todo: error handling
        {
            await _statusManager.SendStatusToUI();

            await _messenger.Publish(new DevicesCollectionChangedMessage(GetDevices()));

            await _messenger.Publish(new ServiceSettingsChangedMessage(_serviceSettingsManager.Settings.EnableSoftwareVaultUnlock, RegistrySettings.GetHesAddress(_log)));

            _winBleControllersStateMonitor.NotifySubscribers();
        }
        #endregion

        #region Activation Code
        public Task SendActivationCode(SendActivationCodeMessage args)
        {
            try
            {
                _clientProxy.EnterActivationCode(args.DeviceId, args.ActivationCode);
            }
            catch (Exception ex)
            {
                _log.WriteDebugLine(ex);
            }
            return Task.CompletedTask;
        }

        public Task CancelActivationCode(CancelActivationCodeMessage args)
        {
            try
            {
                _clientProxy.CancelActivationCode(args.DeviceId);
            }
            catch (Exception ex)
            {
                _log.WriteDebugLine(ex);
            }
            return Task.CompletedTask;
        }
        #endregion

        #region Host only

        static bool _restoringFromSleep = false;
        /* Prevents multiple restored from occuring when multiple resumes from suspend happen 
         * within a short frame of each other due to inconsistent behavior caused by SystemPowerEvent implementation
         */
        static bool _alreadyRestored = false; 
        public async Task OnLaunchFromSuspend()
        {
            _log.WriteLine("System left suspended mode");
            if (!_restoringFromSleep && !_alreadyRestored)
            {
                _restoringFromSleep = true;
                _log.WriteLine("Starting restore from suspended mode");

                try
                {
                    _log.WriteLine("Stopping connection processors");
                    _proximityProcessor.Stop();
                    _tapProcessor.Stop();
                    _rfidProcessor.Stop();

                    await _hesConnection.Stop();



                    _connectionManagerRestarter.Stop();
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(1000);
                        _log.WriteLine("Restarting connection manager");
                        _csrBleConnectionManager.Restart();
                        _connectionManagerRestarter.Start();

                        _log.WriteLine("Starting connection processors");
                        _proximityProcessor.Start();
                        _tapProcessor.Start();
                        _rfidProcessor.Start();
                    });

                    _hesConnection.Start();
                    _alreadyRestored = true;
                }
                catch (Exception ex)
                {
                    Error(ex);
                }
                finally
                {
                    _restoringFromSleep = false;
                    _log.WriteLine($"Restore from suspended mode finished at {DateTime.UtcNow}");
                }
            }
        }

        // It looks like windows never sends this particular event
        public async Task OnPreparingToSuspend()
        {
            try
            {
                _log.WriteLine("System query suspend");
                await _eventSender.SendEventsAsync(true);
            }
            catch (Exception ex)
            {
                _log.WriteLine($"An error occured suspend query");
                _log.WriteLine(ex);
            }
        }

        public async Task OnSuspending()
        {
            try
            {
                _log.WriteLine("System going into suspended mode");
                _alreadyRestored = false;

                _log.WriteLine("Stopping connection processors");
                _proximityProcessor.Stop();
                _tapProcessor.Stop();
                _rfidProcessor.Stop();

                _log.WriteLine("Disconnecting all connected vaults");
                foreach (var device in _deviceManager.Devices)
                    await _deviceManager.Disconnect(device.DeviceConnection);

                _log.WriteLine("Sending all events");
                await _eventSender.SendEventsAsync(true);

                await _hesConnection.Stop();
            }
            catch (Exception ex)
            {
                Error(ex);
            }
        }
        #endregion
    }
}
