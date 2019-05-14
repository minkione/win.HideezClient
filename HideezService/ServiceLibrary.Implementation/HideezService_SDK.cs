using Hideez.CsrBLE;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.Proximity;
using HideezMiddleware;
using Microsoft.Win32;
using System;
using System.Linq;

namespace ServiceLibrary.Implementation
{
    public partial class HideezService : IHideezService, IWorkstationLocker
    {
        static ILog _log;
        static BleConnectionManager _connectionManager;
        static BleDeviceManager _deviceManager;
        static CredentialProviderConnection _credentialProviderConnection;
        static WorkstationUnlocker _workstationUnlocker;
        static HesAppConnection _hesConnection;
        static RfidServiceConnection _rfidService;
        static ProximityMonitorManager _proximityMonitorManager;

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
            _workstationUnlocker = new WorkstationUnlocker(_deviceManager, _hesConnection, _credentialProviderConnection, _rfidService, _log);

            // Proximity Monitor ==================================
            _proximityMonitorManager = new ProximityMonitorManager(_deviceManager, this, _log);
            _proximityMonitorManager.Start();

            _connectionManager.Start();
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

        public bool GetAdapterState(Adapter addapter)
        {
            switch (addapter)
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

        public void LockWorkstation()
        {
            foreach (var client in SessionManager.Sessions)
                client.Callbacks.LockWorkstationRequest();
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
    }
}
