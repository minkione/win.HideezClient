using Hideez.CsrBLE;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.Log;
using HideezMiddleware;
using System;
using System.Threading.Tasks;

namespace ServiceLibrary.Implementation
{
    public partial class HideezService : IHideezService
    {
        private EventLogger _log;
        private BleConnectionManager _connectionManager;
        private BleDeviceManager _deviceManager;
        private CredentialProviderConnection _credentialProviderConnection;
        private WorkstationUnlocker _workstationUnlocker;
        private HesAppConnection _hesConnection;
        private RfidServiceConnection _rfidService;

        public string BleAdapterState => _connectionManager?.State.ToString();
        public string RfidAdapterState => "NA";

        private void InitializeSDK()
        {
            _log = new EventLogger("ExampleApp");
            //_connectionManager = new BleConnectionManager(_log, "d:\\temp\\bonds"); //todo

            //_connectionManager.AdapterStateChanged += ConnectionManager_AdapterStateChanged;
            //_connectionManager.DiscoveryStopped += ConnectionManager_DiscoveryStopped;
            //_connectionManager.DiscoveredDeviceAdded += ConnectionManager_DiscoveredDeviceAdded;
            //_connectionManager.DiscoveredDeviceRemoved += ConnectionManager_DiscoveredDeviceRemoved;

            // COM =============================
            //var port = new ComConnection(log, "COM68", 9600);
            //port.Connect();

            // BLE ============================
            //_deviceManager = new BleDeviceManager(_log, _connectionManager);
            //_deviceManager.DeviceAdded += DevicesManager_DeviceCollectionChanged;
            //_deviceManager.DeviceRemoved += DevicesManager_DeviceCollectionChanged;


            // Named Pipes Server ==============================
            _credentialProviderConnection = new CredentialProviderConnection(_log);
            _credentialProviderConnection.Start();


            // RFID Service Connection ============================
            _rfidService = new RfidServiceConnection(_log);
            _rfidService.Start();


            // WorkstationUnlocker ==================================
            _workstationUnlocker = new WorkstationUnlocker(_deviceManager, _credentialProviderConnection, _rfidService, _log);


            // HES
            _hesConnection = new HesAppConnection(_deviceManager, "https://localhost:44371", _log);
            _hesConnection.Connect();

            _connectionManager.Start();
            //_connectionManager.StartDiscovery();
        }

        void DevicesManager_DeviceCollectionChanged(object sender, DeviceCollectionChangedEventArgs e)
        {
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

        void ConnectionManager_AdapterStateChanged(object sender, EventArgs e)
        {
        }
    }
}
