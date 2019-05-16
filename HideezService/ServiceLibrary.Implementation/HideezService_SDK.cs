using Hideez.CsrBLE;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.Proximity;
using HideezMiddleware;
using Microsoft.Win32;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceLibrary.Implementation
{
    public partial class HideezService : IHideezService
    {
        static ILog _log;
        static BleConnectionManager _connectionManager;
        static BleDeviceManager _deviceManager;
        static CredentialProviderConnection _credentialProviderConnection;
        static WorkstationUnlocker _workstationUnlocker;
        static HesAppConnection _hesConnection;
        static RfidServiceConnection _rfidService;
        static ProximityMonitorManager _proximityMonitorManager;
        static IWorkstationLocker _workstationLocker;

        private void InitializeSDK()
        {
            //_log = new EventLogger("ExampleApp");
            _log = new NLogger();

            // Combined path evaluates to '%ProgramData%\\Hideez\\Bonds'
            var commonAppData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            var bondsFilePath = $"{commonAppData}\\Hideez\\bonds";

            _connectionManager = new BleConnectionManager(_log, bondsFilePath);
            _connectionManager.AdapterStateChanged += ConnectionManager_AdapterStateChanged;
            _connectionManager.DiscoveryStopped += ConnectionManager_DiscoveryStopped;
            _connectionManager.DiscoveredDeviceAdded += ConnectionManager_DiscoveredDeviceAdded;
            _connectionManager.DiscoveredDeviceRemoved += ConnectionManager_DiscoveredDeviceRemoved;

            // BLE ============================
            _deviceManager = new BleDeviceManager(_log, _connectionManager);
            _deviceManager.DeviceAdded += DevicesManager_DeviceCollectionChanged;
            _deviceManager.DeviceRemoved += DevicesManager_DeviceCollectionChanged;
            _deviceManager.DeviceRemoved += _deviceManager_DeviceRemoved;
            _deviceManager.DeviceAdded += _deviceManager_DeviceAdded;


            // Named Pipes Server ==============================
            _credentialProviderConnection = new CredentialProviderConnection(_log);
            _credentialProviderConnection.Start();


            // RFID Service Connection ============================
            _rfidService = new RfidServiceConnection(_log);
            _rfidService.RfidReaderStateChanged += RFIDService_ReaderStateChanged;
            _rfidService.Start();

            try
            {
                // HES ==================================
                // HKLM\SOFTWARE\Hideez\Safe, hs3_hes_address REG_SZ
                _hesConnection = new HesAppConnection(_deviceManager, GetHesAddress(), _log);
                _hesConnection.HubConnectionStateChanged += HES_ConnectionStateChanged;
                _hesConnection.Connect();
            }
            catch (Exception ex)
            {
                log.Error("Hideez Service has encountered an error during HES connection init." +
                    Environment.NewLine +
                    "New connection establishment will be attempted after service restart");
                log.Error(ex);
            }

            // WorkstationUnlocker ==================================
            _workstationUnlocker = new WorkstationUnlocker(_deviceManager, _hesConnection,
                _credentialProviderConnection, _rfidService, _connectionManager, _log);

            // WorkstationLocker
            _workstationLocker = new WorkstationWtsapiLocker();

            // Proximity Monitor ==================================
            _proximityMonitorManager = new ProximityMonitorManager(_deviceManager, _workstationLocker, _log);
            _proximityMonitorManager.Start();

            _connectionManager.Start();
            _connectionManager.StartDiscovery();
        }

        private void _deviceManager_DeviceAdded(object sender, DeviceCollectionChangedEventArgs e)
        {
            var bleDevice = e.AddedDevice;

            if (bleDevice != null)
            {
                // event is only subscribed to once
                bleDevice.ProximityChanged -= BleDevice_ProximityChanged;
                bleDevice.ProximityChanged += BleDevice_ProximityChanged;

                // event is only subscribed to once
                bleDevice.PropertyChanged -= BleDevice_PropertyChanged;
                bleDevice.PropertyChanged += BleDevice_PropertyChanged;
            }
        }

        private void _deviceManager_DeviceRemoved(object sender, DeviceCollectionChangedEventArgs e)
        {
            var bleDevice = e.RemovedDevice;

            if (bleDevice != null)
            {
                bleDevice.ProximityChanged -= BleDevice_ProximityChanged;
                bleDevice.PropertyChanged -= BleDevice_PropertyChanged;
            }
        }

        private void _deviceManager_DevicePropertyChanged(object sender, DevicePropertyChangedEventArgs e)
        {
            foreach (var client in SessionManager.Sessions)
                client.Callbacks.PairedDevicePropertyChanged(new BleDeviceDTO(e.Device));
        }

        void ConnectionManager_AdapterStateChanged(object sender, EventArgs e)
        {
            foreach (var client in SessionManager.Sessions)
                client.Callbacks.DongleConnectionStateChanged(_connectionManager?.State == BluetoothAdapterState.PoweredOn);
        }

        void RFIDService_ReaderStateChanged(object sender, EventArgs e)
        {
            foreach (var client in SessionManager.Sessions)
                client.Callbacks.RFIDConnectionStateChanged(_rfidService != null ?
                    _rfidService.ServiceConnected && _rfidService.ReaderConnected : false);
        }

        void HES_ConnectionStateChanged(object sender, EventArgs e)
        {
            foreach (var client in SessionManager.Sessions)
                client.Callbacks.HESConnectionStateChanged(_hesConnection?.State == HesConnectionState.Connected);
        }

        void DevicesManager_DeviceCollectionChanged(object sender, DeviceCollectionChangedEventArgs e)
        {
            foreach (var client in SessionManager.Sessions)
                client.Callbacks.PairedDevicesCollectionChanged(GetPairedDevices());
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

        #region proximity monitoring

        private void BleDevice_ProximityChanged(object sender, EventArgs e)
        {
            if (sender is BleDevice bleDevice)
            {
                foreach (var c in SessionManager.Sessions
                    // if has key for device id and enabled monitoring for this id
                    .Where(s => s.IsEnabledProximityMonitoring.TryGetValue(bleDevice.Id, out bool isEnabled) && isEnabled))
                {
                    c.Callbacks.ProximityChanged(bleDevice.Id, bleDevice.Proximity);
                }
            }
        }

        public void EnableMonitoringProximity(string deviceId)
        {
            ChangeIsEnabledProximityMonitoring(deviceId, true);
        }

        public void DisableMonitoringProximity(string deviceId)
        {
            ChangeIsEnabledProximityMonitoring(deviceId, false);
        }

        private void ChangeIsEnabledProximityMonitoring(string deviceId, bool isEnabled)
        {
            if (client != null)
            {
                client.IsEnabledProximityMonitoring[deviceId] = isEnabled;
            }
        }

        #endregion

        #region Device Properties Monitoring


        private void BleDevice_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is BleDevice bleDevice)
            {
                foreach (var c in SessionManager.Sessions
                    // if has key for device id and enabled monitoring for this id
                    .Where(s => s.IsEnabledPropertyMonitoring.TryGetValue(bleDevice.Id, out bool isEnabled) && isEnabled))
                {
                    c.Callbacks.PairedDevicePropertyChanged(new BleDeviceDTO(bleDevice));
                }
            }
        }

        public void EnableMonitoringDeviceProperties(string deviceId)
        {
            ChangeIsEnabledDevicePropertiesMonitoring(deviceId, true);
        }

        public void DisableMonitoringDeviceProperties(string deviceId)
        {
            ChangeIsEnabledDevicePropertiesMonitoring(deviceId, false);
        }

        private void ChangeIsEnabledDevicePropertiesMonitoring(string deviceId, bool isEnabled)
        {
            if (client != null)
            {
                client.IsEnabledPropertyMonitoring[deviceId] = isEnabled;
            }
        }

        #endregion

        public bool GetAdapterState(Adapter adapter)
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

        public BleDeviceDTO[] GetPairedDevices()
        {
            var dto = _deviceManager.Devices.Select(d => new BleDeviceDTO(d)).ToArray();
            return dto;
        }

        readonly string _hesAddressRegistryValueName = "hs3_hes_address";
        private RegistryKey GetAppRegistryRootKey()
        {
            return RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default)?
                .OpenSubKey("SOFTWARE")?
                .OpenSubKey("Hideez")?
                .OpenSubKey("Safe");
        }

        private string GetHesAddress()
        {
            var registryKey = GetAppRegistryRootKey();
            if (registryKey == null)
                throw new Exception("Couldn't find Hideez Safe registry key. (HKLM\\SOFTWARE\\Hideez\\Safe)");

            var value = registryKey.GetValue(_hesAddressRegistryValueName);
            if (value == null)
                throw new ArgumentNullException($"{_hesAddressRegistryValueName} value is null or empty. Please specify HES address in registry under value {_hesAddressRegistryValueName}. Key: HKLM\\SOFTWARE\\Hideez\\Safe ");

            if (value is string == false)
                throw new FormatException($"{_hesAddressRegistryValueName} could not be cast to string. Check that its value has REG_SZ type");

            var address = value as string;

            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentNullException($"{_hesAddressRegistryValueName} value is null or empty. Please specify HES address in registry under value {_hesAddressRegistryValueName}. Key: HKLM\\SOFTWARE\\Hideez\\Safe ");

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

        public void OnSessionChange(bool sessionLocked)
        {
            log.Info($"Session change called: {sessionLocked};  client {client.ClientType.ToString()}");
            // This operation contract can only be used by ServiceHost or TestConsole 
            // Other clients are prohibited from using it
            if (client.ClientType == ClientType.ServiceHost || client.ClientType == ClientType.TestConsole)
            {
                /*
                Task.Run(async () =>
                {
                    if (sessionLocked)
                    {
                        // Todo: disconnect all devices
                    }
                });
                */
            }
            else
            {
                /*
                throw new NotSupportedException();
                */
            }
        }
    }
}
