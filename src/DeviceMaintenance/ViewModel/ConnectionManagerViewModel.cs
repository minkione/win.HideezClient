using DeviceMaintenance.Messages;
using Hideez.CsrBLE;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.FW;
using Hideez.SDK.Communication.Log;
using Meta.Lib.Modules.PubSub;
using MvvmExtensions.PropertyChangedMonitoring;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DeviceMaintenance.ViewModel
{
    public class ConnectionManagerViewModel : PropertyChangedImplementation
    {
        readonly BleConnectionManager _connectionManager;
        readonly BleDeviceManager _deviceManager;
        readonly EventLogger _log;
        readonly MetaPubSub _hub;

        public bool BleAdapterAvailable => _connectionManager?.State == BluetoothAdapterState.PoweredOn;

        public ConnectionManagerViewModel(EventLogger log, MetaPubSub hub)
        {
            _log = log;
            _hub = hub;
            _hub.Subscribe<ConnectDeviceCommand>(OnConnectDeviceCommand);
            _hub.Subscribe<StartDiscoveryCommand>(OnStartDiscoveryCommand);
            _hub.Subscribe<EnterBootCommand>(OnEnterBootCommand);

            var commonAppData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            var bondsFilePath = Path.Combine(commonAppData, @"Hideez\bonds");

            // ConnectionManager ============================
            _connectionManager = new BleConnectionManager(log, bondsFilePath);
            _connectionManager.AdapterStateChanged += ConnectionManager_AdapterStateChanged;
            _connectionManager.AdvertismentReceived += ConnectionManager_AdvertismentReceived;

            // DeviceManager ============================
            _deviceManager = new BleDeviceManager(log, _connectionManager);
        }

        void ConnectionManager_AdapterStateChanged(object sender, EventArgs e)
        {
            NotifyPropertyChanged(nameof(BleAdapterAvailable));
        }

        void ConnectionManager_AdvertismentReceived(object sender, AdvertismentReceivedEventArgs e)
        {
            if (e.Rssi > SdkConfig.TapProximityUnlockThreshold)
                _hub.Publish(new AdvertismentReceivedEvent(e));
        }

        Task OnStartDiscoveryCommand(StartDiscoveryCommand arg)
        {
            _connectionManager.StartDiscovery();
            return Task.CompletedTask;
        }

        async Task OnConnectDeviceCommand(ConnectDeviceCommand arg)
        {
            var device = await _deviceManager.ConnectDevice(arg.Mac, SdkConfig.ConnectDeviceTimeout);

            if (device == null)
                device = await _deviceManager.ConnectDevice(arg.Mac, SdkConfig.ConnectDeviceTimeout / 2);

            if (device != null)
                await device.WaitInitialization(SdkConfig.DeviceInitializationTimeout, default);

            await _hub.Publish(new ConnectDeviceResponse(device, arg.Mac));
        }

        async Task OnEnterBootCommand(EnterBootCommand cmd)
        {
            var imageUploader = new FirmwareImageUploader(
                cmd.FirmwareFilePath, _deviceManager, cmd.Device, cmd.LongOperation, _log);
            await imageUploader.EnterBoot();
            await _hub.Publish(new EnterBootResponse(imageUploader));
        }

        public bool IsBonded(string id)
        {
            return _connectionManager.IsBonded(id);
        }
    }
}
