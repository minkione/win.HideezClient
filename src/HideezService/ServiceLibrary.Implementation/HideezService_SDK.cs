using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Runtime.Remoting.Messaging;
using System.Security;
using System.Text;
using System.Threading;
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
using Hideez.SDK.Communication.WCF;
using Hideez.SDK.Communication.Workstation;
using Hideez.SDK.Communication.WorkstationEvents;
using HideezMiddleware;
using HideezMiddleware.Audit;
using HideezMiddleware.DeviceConnection;
using HideezMiddleware.DeviceLogging;
using HideezMiddleware.Local;
using HideezMiddleware.ReconnectManager;
using HideezMiddleware.ScreenActivation;
using HideezMiddleware.Settings;
using HideezMiddleware.SoftwareVault.UnlockToken;
using HideezMiddleware.Tasks;
using HideezMiddleware.Workstation;
using Microsoft.Win32;
using ServiceLibrary.Implementation.ClientManagement;
using ServiceLibrary.Implementation.ScreenActivation;
using ServiceLibrary.Implementation.WorkstationLock;

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
        static WcfDeviceFactory _wcfDeviceFactory;
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

        static ConnectionFlowProcessor _connectionFlowProcessor;
        static AdvertisementIgnoreList _advIgnoreList;
        static RfidConnectionProcessor _rfidProcessor;
        static TapConnectionProcessor _tapProcessor;
        static ProximityConnectionProcessor _proximityProcessor;
        static SessionUnlockMethodMonitor _sessionUnlockMethodMonitor;
        static SessionSwitchLogger _sessionSwitchLogger;
        static ConnectionManagerRestarter _connectionManagerRestarter;
        static ILocalDeviceInfoCache _localDeviceInfoCache;
        static DeviceLogManager _deviceLogManager;

        static DeviceReconnectManager _deviceReconnectManager;

        static IUnlockTokenProvider _unlockTokenProvider;
        static UnlockTokenGenerator _unlockTokenGenerator;
        static RemoteWorkstationUnlocker _remoteWorkstationUnlocker;

        static HesAppConnection _tbHesConnection;
        static IWorkstationIdProvider _workstationIdProvider;

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
            var bondsFilePath = $"{commonAppData}\\Hideez\\bonds";
            var deviceLogsPath = $"{commonAppData}\\Hideez\\Service\\DeviceLogs";

            var bleInitTask = Task.Run(() =>
            {
                // Connection Manager ============================
                _connectionManager = new BleConnectionManager(_sdkLogger, bondsFilePath);
                _connectionManager.AdapterStateChanged += ConnectionManager_AdapterStateChanged;
                _connectionManager.DiscoveryStopped += ConnectionManager_DiscoveryStopped;
                _connectionManager.DiscoveredDeviceAdded += ConnectionManager_DiscoveredDeviceAdded;
                _connectionManager.DiscoveredDeviceRemoved += ConnectionManager_DiscoveredDeviceRemoved;
                _connectionManagerRestarter = new ConnectionManagerRestarter(_connectionManager, _sdkLogger);

                // BLE ============================
                _deviceManager = new BleDeviceManager(_sdkLogger, _connectionManager);
                _deviceManager.DeviceAdded += DevicesManager_DeviceCollectionChanged;
                _deviceManager.DeviceRemoved += DevicesManager_DeviceCollectionChanged;
                _deviceManager.DeviceRemoved += DeviceManager_DeviceRemoved;
                _deviceManager.DeviceAdded += DeviceManager_DeviceAdded;
                SessionSwitchMonitor.SessionSwitch += SessionSwitchMonitor_SessionSwitch;

                // WCF ============================
                _wcfDeviceFactory = new WcfDeviceFactory(_deviceManager, _sdkLogger);
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

            // Hideez Client root registry key ============================
            RegistryKey clientRootRegistryKey = HideezClientRegistryRoot.GetRootRegistryKey(true);

            // Workstation Id ============================
            _workstationIdProvider = new WorkstationIdProvider(clientRootRegistryKey, _sdkLogger);
            if (string.IsNullOrWhiteSpace(_workstationIdProvider.GetWorkstationId()))
                _workstationIdProvider.SaveWorkstationId(Guid.NewGuid().ToString());

            // WorkstationInfoProvider ==================================
            WorkstationHelper.Log = _sdkLogger;
            var workstationInfoProvider = new WorkstationInfoProvider(_workstationIdProvider, _sdkLogger);


            // TB & HES Connections ==================================
            await pipeInitTask.ConfigureAwait(false); 
            await CreateHubs(_deviceManager, workstationInfoProvider, _proximitySettingsManager, _rfidSettingsManager, _sdkLogger).ConfigureAwait(false);

            serviceInitializationTasks.Add(StartHubs(hesAddress));

            // Software Vault Unlock Mechanism
            _unlockTokenProvider = new UnlockTokenProvider(clientRootRegistryKey, _sdkLogger);
            _unlockTokenGenerator = new UnlockTokenGenerator(_unlockTokenProvider, workstationInfoProvider, _sdkLogger);
            await pipeInitTask.ConfigureAwait(false);
            _remoteWorkstationUnlocker = new RemoteWorkstationUnlocker(_unlockTokenProvider, _tbHesConnection, _credentialProviderProxy, _sdkLogger);

            // Start Software Vault unlock modules
            if (_serviceSettingsManager.Settings.EnableSoftwareVaultUnlock)
                serviceInitializationTasks.Add(Task.Run(_unlockTokenGenerator.Start));

            // Audit Log / Event Aggregator =============================
            _eventSender = new EventSender(_hesConnection, _eventSaver, _sdkLogger);

            // ScreenActivator ==================================
            _screenActivator = new WcfScreenActivator(sessionManager);

            // Client Proxy =============================
            _clientProxy = new ServiceClientUiManager(sessionManager);

            // UI Proxy =============================
            _uiProxy = new UiProxyManager(_credentialProviderProxy, _clientProxy, _sdkLogger);

            // StatusManager =============================
            _statusManager = new StatusManager(_hesConnection, _tbHesConnection, _rfidService, 
                _connectionManager, _uiProxy, _rfidSettingsManager, _credentialProviderProxy, _sdkLogger);

            // Local device info cache
            _localDeviceInfoCache = new LocalDeviceInfoCache(clientRootRegistryKey, _sdkLogger);

            // ConnectionFlowProcessor
            _connectionFlowProcessor = new ConnectionFlowProcessor(
                _connectionManager,
                _deviceManager,
                _hesConnection,
                _credentialProviderProxy,
                _screenActivator,
                _uiProxy,
                _localDeviceInfoCache,
                _sdkLogger);

            _deviceLogManager = new DeviceLogManager(deviceLogsPath, new DeviceLogWriter(), _serviceSettingsManager, _connectionFlowProcessor, _sdkLogger);
            _connectionFlowProcessor.DeviceFinishedMainFlow += ConnectionFlowProcessor_DeviceFinishedMainFlow;
            _advIgnoreList = new AdvertisementIgnoreList(
                _connectionManager,
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
                _connectionManager,
                _sdkLogger);
            _proximityProcessor = new ProximityConnectionProcessor(
                _connectionFlowProcessor,
                _connectionManager,
                _proximitySettingsManager,
                _workstationSettingsManager,
                _advIgnoreList,
                _deviceManager,
                _credentialProviderProxy,
                _sdkLogger);

            serviceInitializationTasks.Add(Task.Run(() =>
            {
                _proximityProcessor.Start();
                _tapProcessor.Start();
                _rfidProcessor.Start();
            }));

            // Proximity Monitor ==================================
            ProximitySettings proximitySettings = _proximitySettingsManager.Settings;
            _proximityMonitorManager = new ProximityMonitorManager(_deviceManager, _workstationSettingsManager.Settings.GetProximityMonitorSettings(), _sdkLogger);

            // Device Reconnect Manager ================================
            _deviceReconnectManager = new DeviceReconnectManager(_proximityMonitorManager, _deviceManager, _connectionFlowProcessor, _sdkLogger);
            _deviceReconnectManager.DeviceReconnected += (s, a) => _log.WriteLine($"Device {a.SerialNo} reconnected successfully");

            // WorkstationLocker ==================================
            _workstationLocker = new UniversalWorkstationLocker(SdkConfig.DefaultLockTimeout * 1000, sessionManager, _sdkLogger);

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
                 _tapProcessor, _rfidProcessor, _proximityProcessor, _sdkLogger);

            // SessionSwitchLogger ==================================
            _sessionSwitchLogger = new SessionSwitchLogger(_eventSaver, _sessionUnlockMethodMonitor,
                _workstationLockProcessor, _deviceManager, _sdkLogger);

            // SDK initialization finished, start essential components
            if (_serviceSettingsManager.Settings.EnableSoftwareVaultUnlock)
                _remoteWorkstationUnlocker.Start();

            _rfidService.Start();

            _workstationLockProcessor.Start();
            _proximityMonitorManager.Start();

            _connectionManager.StartDiscovery();
            _connectionManagerRestarter.Start();

            _deviceReconnectManager.Start();

            await Task.WhenAll(serviceInitializationTasks).ConfigureAwait(false);
        }

        Task<HesAppConnection> CreateHesHub(BleDeviceManager deviceManager, 
            WorkstationInfoProvider workstationInfoProvider, 
            ISettingsManager<ProximitySettings> proximitySettingsManager, 
            ISettingsManager<RfidSettings> rfidSettingsManager, 
            ILog log)
        {
            return Task.Run(() =>
            {
                var hesConnection = new HesAppConnection(deviceManager, workstationInfoProvider, log);

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

        Task CreateHubs(BleDeviceManager deviceManager, 
            WorkstationInfoProvider workstationInfoProvider,
            ISettingsManager<ProximitySettings> proximitySettingsManager,
            ISettingsManager<RfidSettings> rfidSettingsManager,
            ILog log)
        {
            return Task.Run(async () =>
            {
                var hubTask = CreateHesHub(deviceManager, workstationInfoProvider, proximitySettingsManager, rfidSettingsManager, log);
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

        void DeviceManager_DeviceAdded(object sender, DeviceCollectionChangedEventArgs e)
        {
            var device = e.AddedDevice;

            if (device != null)
            {
                device.ConnectionStateChanged += Device_ConnectionStateChanged;
                device.Initialized += Device_Initialized;
                device.Disconnected += Device_Disconnected;
                device.OperationCancelled += Device_OperationCancelled;
                device.ProximityChanged += Device_ProximityChanged;
                device.BatteryChanged += Device_BatteryChanged;
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
                device.OperationCancelled -= Device_OperationCancelled;
                device.ProximityChanged -= Device_ProximityChanged;
                device.BatteryChanged -= Device_BatteryChanged;

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
                    if (!device.IsConnected)
                        device.SetUserProperty(ConnectionFlowProcessor.FLOW_FINISHED_PROP, false);

                    foreach (var client in sessionManager.Sessions)
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

        void Device_OperationCancelled(object sender, EventArgs e)
        {
            try
            {
                if (sender is IDevice device)
                {
                    foreach (var client in sessionManager.Sessions)
                    {
                        client.Callbacks.DeviceOperationCancelled(new DeviceDTO(device));
                    }
                }
            }
            catch (Exception ex)
            {
                Error(ex);
            }
        }

        void Device_ProximityChanged(object sender, double e)
        {
            if (sender is IDevice device)
            {
                foreach (var client in sessionManager.Sessions)
                {
                    try
                    {
                        client.Callbacks.DeviceProximityChanged(device.Id, e);
                    }
                    catch (Exception ex)
                    {
                        Error(ex);
                    }
                }
            }
        }

        void Device_BatteryChanged(object sender, sbyte e)
        {
            if (sender is IDevice device)
            {
                foreach (var client in sessionManager.Sessions)
                {
                    try
                    {
                        client.Callbacks.DeviceBatteryChanged(device.Id, e);
                    }
                    catch (Exception ex)
                    {
                        Error(ex);
                    }
                }
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

        void WorkstationLockProcessor_DeviceProxLockEnabled(object sender, IDevice device)
        {
            foreach (var session in sessionManager.Sessions)
            {
                try
                {
                    session.Callbacks.DeviceProximityLockEnabled(new DeviceDTO(device));
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

        async void SessionSwitchMonitor_SessionSwitch(int sessionId, SessionSwitchReason reason)
        {
            try
            {
                if (reason == SessionSwitchReason.SessionUnlock || reason== SessionSwitchReason.SessionLogon)
                    foreach (var client in sessionManager.Sessions)
                        client.Callbacks.WorkstationUnlocked(_sessionUnlockMethodMonitor.GetUnlockMethod() == SessionSwitchSubject.NonHideez);

                if (reason == SessionSwitchReason.SessionLogoff || reason == SessionSwitchReason.SessionLock)
                {
                    // Disconnect all connected devices
                    _deviceReconnectManager.DisableAllDevicesReconnect();
                    await _deviceManager.DisconnectAllDevices();
                }
            }
            catch (Exception ex)
            {
                Error(ex);
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
                        return _connectionManager?.State == BluetoothAdapterState.PoweredOn || _connectionManager?.State == BluetoothAdapterState.Resetting;
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

                return Array.Empty<DeviceDTO>(); // We will never reach this line
            }
        }

        public byte[] GetAvailableChannels(string serialNo)
        {
            try
            {
                var devices = _deviceManager.Devices.Where(d => d.SerialNo == serialNo).ToList();
                if (devices.Count == 0)
                    throw new HideezException(HideezErrorCode.DeviceNotFound, serialNo);

                // Channels range from 1 to 6 
                List<byte> freeChannels = new List<byte>() { 1, 2, 3, 4, 5, 6 };

                // These channels are reserved by system, the rest is available to clients
                freeChannels.Remove((byte)DefaultDeviceChannel.Main);
                freeChannels.Remove((byte)DefaultDeviceChannel.HES);

                // Filter out taken channels
                var channelsInUse = devices.Select(d => d.ChannelNo).ToList();
                freeChannels.RemoveAll(c => channelsInUse.Contains(c));

                return freeChannels.ToArray();
            }
            catch (Exception ex)
            {
                Error(ex);
                ThrowException(ex);
                return null; // this line is unreachable
            }
        }

        public async Task DisconnectDevice(string id)
        {
            try
            {
                _deviceReconnectManager.DisableDeviceReconnect(id);
                await _deviceManager.DisconnectDevice(id);
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
                {
                    _deviceReconnectManager.DisableDeviceReconnect(device);
                    await _deviceManager.Remove(device);
                }
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

        public async Task<bool> ChangeServerAddress(string address)
        {
            try
            {
                _log.WriteLine($"Client requested HES address change to \"{address}\"");

                if (string.IsNullOrWhiteSpace(address))
                {
                    _log.WriteLine($"Clearing server address and shutting down connection");
                    RegistrySettings.SetHesAddress(_log, address);
                    await _hesConnection.Stop();
                    return true;
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

                        return true;
                    }
                    else
                    {
                        _log.WriteLine($"Failed connectivity check to {address}");
                        return false;
                    }
                }
            }
            catch (TimeoutException)
            {
                return false;
            }
        }

        public bool IsSoftwareVaultUnlockModuleEnabled()
        {
            return _serviceSettingsManager.Settings.EnableSoftwareVaultUnlock;
        }

        public void SetSoftwareVaultUnlockModuleState(bool enabled)
        {
            var settings = _serviceSettingsManager.Settings;
            if (settings.EnableSoftwareVaultUnlock != enabled)
            {
                _log.WriteLine($"Client requested to switch software unlock module. New value: {enabled}");
                settings.EnableSoftwareVaultUnlock = enabled;
                _serviceSettingsManager.SaveSettings(settings);

                if (enabled)
                {
                    _unlockTokenGenerator.Start();
                    _remoteWorkstationUnlocker.Start();
                }
                else
                {
                    _unlockTokenGenerator.Stop();
                    _remoteWorkstationUnlocker.Stop();
                    _unlockTokenGenerator.DeleteSavedToken();
                }
            }
        }

        #region Remote device management
        // This collection is unique for each client
        readonly List<IWcfDevice> RemoteWcfDevices = new List<IWcfDevice>();

        public async Task<string> EstablishRemoteDeviceConnection(string serialNo, byte channelNo)
        {
            try
            {
                var wcfDevice = (IWcfDevice)_deviceManager.FindBySerialNo(serialNo, 2);
                if (wcfDevice == null)
                {
                    var device = _deviceManager.FindBySerialNo(serialNo, 1);

                    if (device == null)
                        throw new HideezException(HideezErrorCode.DeviceNotFound, serialNo);

                    wcfDevice = await _wcfDeviceFactory.EstablishRemoteDeviceConnection(device.Mac, channelNo);

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
                var wcfDevice = _deviceManager.Find(connectionId) as IWcfDevice;

                if (wcfDevice == null)
                    throw new HideezException(HideezErrorCode.RemoteDeviceNotFound, connectionId);

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
                var wcfDevice = _deviceManager.Find(connectionId) as IWcfDevice;

                if (wcfDevice == null)
                    throw new HideezException(HideezErrorCode.RemoteDeviceNotFound, connectionId);

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
                var wcfDevice = _deviceManager.Find(connectionId) as IWcfDevice;

                if (wcfDevice == null)
                    throw new HideezException(HideezErrorCode.RemoteDeviceNotFound, connectionId);

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

        static bool _restoringFromSleep = false;
        /* Prevents multiple restored from occuring when multiple resumes from suspend happen 
         * within a short frame of each other due to inconsistent behavior caused by SystemPowerEvent implementation
         */
        static bool _alreadyRestored = false; 
        public static async Task OnLaunchFromSuspend()
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
                        _connectionManager.Restart();
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
        public static async Task OnPreparingToSuspend()
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

        public static async Task OnSuspending()
        {
            try
            {
                _log.WriteLine("System going into suspended mode");
                _alreadyRestored = false;

                _log.WriteLine("Stopping connection processors");
                _proximityProcessor.Stop();
                _tapProcessor.Stop();
                _rfidProcessor.Stop();

                _log.WriteLine("Disconnecting all connected devices");
                await _deviceManager.DisconnectAllDevices();

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
