using Hideez.CsrBLE;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.PasswordManager;
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
        static BleConnectionManager _connectionManager;
        static BleDeviceManager _deviceManager;
        static CredentialProviderConnection _credentialProviderConnection;
        static WorkstationUnlocker _workstationUnlocker;
        static HesAppConnection _hesConnection;
        static RfidServiceConnection _rfidService;
        static ProximityMonitorManager _proximityMonitorManager;
        static IWorkstationLocker _workstationLocker;
        static IScreenActivator _screenActivator;

        void InitializeSDK()
        {
            var sdkLogger = new NLogger();

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


            // Named Pipes Server ==============================
            _credentialProviderConnection = new CredentialProviderConnection(sdkLogger);


            // RFID Service Connection ============================
            _rfidService = new RfidServiceConnection(sdkLogger);
            _rfidService.RfidReaderStateChanged += RFIDService_ReaderStateChanged;
            _rfidService.Start();

            try
            {
                // HES ==================================
                // HKLM\SOFTWARE\Hideez\Safe, hs3_hes_address REG_SZ
                _hesConnection = new HesAppConnection(_deviceManager, GetHesAddress(), sdkLogger);
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
            _workstationUnlocker = new WorkstationUnlocker(_deviceManager, _hesConnection,
                _credentialProviderConnection, _rfidService, _connectionManager, _screenActivator, sdkLogger);

            _credentialProviderConnection.Start();

            // WorkstationLocker ==================================
            _workstationLocker = new UiWorkstationLocker(SessionManager);

            // Proximity Monitor 
            _proximityMonitorManager = new ProximityMonitorManager(_deviceManager, _workstationLocker, sdkLogger);
            _proximityMonitorManager.Start();

            _connectionManager.StartDiscovery();
        }

        #region Event Handlers

        void DeviceManager_DeviceAdded(object sender, DeviceCollectionChangedEventArgs e)
        {
            var device = e.AddedDevice;

            if (device != null)
            {
                // event is only subscribed to once
                device.PropertyChanged -= Device_PropertyChanged;
                device.PropertyChanged += Device_PropertyChanged;
            }
        }

        void DeviceManager_DeviceRemoved(object sender, DeviceCollectionChangedEventArgs e)
        {
            var device = e.RemovedDevice;

            if (device != null)
            {
                device.PropertyChanged -= Device_PropertyChanged;
            }
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

        #endregion

        #region Device proximity monitoring

        void Device_ProximityChanged(object sender, EventArgs e)
        {
            if (sender is IDevice device)
            {
                foreach (var c in SessionManager.Sessions
                    // if has key for device id and enabled monitoring for this id
                    .Where(s => s.IsEnabledProximityMonitoring.TryGetValue(device.Id, out bool isEnabled) && isEnabled))
                {
                    c.Callbacks.ProximityChanged(device.Id, device.Proximity);
                }
            }
        }

        public void EnableMonitoringProximity(string deviceId)
        {
            try
            {
                ChangeIsEnabledProximityMonitoring(deviceId, true);
            }
            catch (Exception ex)
            {
                LogException(ex);
                ThrowException(ex);
            }
        }

        public void DisableMonitoringProximity(string deviceId)
        {
            try
            {
                ChangeIsEnabledProximityMonitoring(deviceId, false);
            }
            catch (Exception ex)
            {
                LogException(ex);
                ThrowException(ex);
            }
        }

        void ChangeIsEnabledProximityMonitoring(string deviceId, bool isEnabled)
        {
            if (_client != null)
            {
                _client.IsEnabledProximityMonitoring[deviceId] = isEnabled;
            }
        }

        #endregion

        #region Device Properties Monitoring

        void Device_PropertyChanged(object sender,string propertyName)
        {
            try
            {
                if (sender is IDevice device)
                {
                    foreach (var client in SessionManager.Sessions
                        // if has key for device id and enabled monitoring for this id
                        .Where(s => s.IsEnabledPropertyMonitoring.TryGetValue(device.Id, out bool isEnabled) && isEnabled))
                    {
                        client.Callbacks.PairedDevicePropertyChanged(new DeviceDTO(device));
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
                ThrowException(ex);
            }
        }

        void Device_DeviceInfoChanged(object sender, EventArgs e)
        {
            try
            {
                if (sender is IDevice device)
                {
                    foreach (var client in SessionManager.Sessions
                        .Where(s => s.IsEnabledPropertyMonitoring.TryGetValue(device.Id, out bool isEnabled) && isEnabled))
                    {
                        client.Callbacks.PairedDevicePropertyChanged(new DeviceDTO(device));
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
                ThrowException(ex);
            }
        }

        public void EnableMonitoringDeviceProperties(string deviceId)
        {
            try
            {
                ChangeIsEnabledDevicePropertiesMonitoring(deviceId, true);
            }
            catch (Exception ex)
            {
                LogException(ex);
                ThrowException(ex);
            }
        }

        public void DisableMonitoringDeviceProperties(string deviceId)
        {
            try
            {
                ChangeIsEnabledDevicePropertiesMonitoring(deviceId, false);
            }
            catch (Exception ex)
            {
                LogException(ex);
                ThrowException(ex);
            }
        }

        private void ChangeIsEnabledDevicePropertiesMonitoring(string deviceId, bool isEnabled)
        {
            if (_client != null)
            {
                _client.IsEnabledPropertyMonitoring[deviceId] = isEnabled;
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

        public DeviceDTO[] GetPairedDevices()
        {
            try
            {
                return _deviceManager.Devices.Select(d => new DeviceDTO(d)).ToArray();
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
            try
            {
                _log.Info($"Session change called: {sessionLocked};  client {_client.ClientType.ToString()}");
                // This operation contract can only be used by ServiceHost or TestConsole 
                // Other clients are prohibited from using it
                if (_client.ClientType == ClientType.ServiceHost || _client.ClientType == ClientType.TestConsole)
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
            catch (Exception ex)
            {
                LogException(ex);
                ThrowException(ex);
            }
        }

        public async Task SaveCredentialAsync(string deviceId, string login, string password)
        {
            try
            {
                var device = _deviceManager.Find(deviceId);
                if (device != null)
                {
                    var dpm = new DevicePasswordManager(device);
                    await dpm.SavePcUnlockCredentials(login, password);
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
                ThrowException(ex);
            }
        }

        public void DisconnectDevice(string deviceId)
        {
            try
            {
                _deviceManager.Find(deviceId)?.Disconnect();
            }
            catch (Exception ex)
            {
                LogException(ex);
                ThrowException(ex);
            }
        }

        public async Task RemoveDeviceAsync(string deviceId)
        {
            try
            {
                var device = _deviceManager.Find(deviceId);
                if (device != null)
                    await _deviceManager.Remove(device);
            }
            catch (Exception ex)
            {
                LogException(ex);
                ThrowException(ex);
            }
        }

    }
}
