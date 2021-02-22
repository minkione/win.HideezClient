using Hideez.SDK.Communication;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.Connection;
using Hideez.SDK.Communication.Device.Exchange;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using Polly;
using Polly.Contrib.DuplicateRequestCollapser;
using Polly.Wrap;
using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;

namespace WinBle
{
    public class WinBleConnection : Logger, IBleConnection, IDisposable
    {
        private readonly string _deviceId;
        
        BluetoothLEDevice _bluetoothLeDevice;
        GattCharacteristic _infoCharacteristic;
        GattCharacteristic _mainCharacteristic;


        bool _isBoot;
        AsyncPolicyWrap _connectPolicy;
        IAsyncRequestCollapserPolicy _disconnectPolicy;
        CancellationTokenSource _connectCancellation = new CancellationTokenSource();

        //IAsyncRequestCollapserPolicy _requestCollapserPolicy;

        public string DeviceName { get; private set; }

        public string DeviceMac { get; private set; }

        public bool IsConnected => State == ConnectionState.Connected;

        public ConnectionState State { get; private set; } = ConnectionState.NotConnected;

        public ushort PduSize => throw new NotImplementedException();

        public ConnectionId ConnectionId { get; private set; }


        public event EventHandler<PacketReceivedEventArgs> PacketReceived;
        public event EventHandler<ConnectionStateChangedEventArgs> ConnectionStateChanged;
        public event EventHandler<PduSizeChangedEventArgs> PduSizeChanged;
        public event EventHandler<AdvertismentReceivedEventArgs> AdvertismentReceived;

        //public WinBleConnection(BluetoothLEDevice bleDevice, string name, ILog log)
        //    : base(name, log)
        //{
        //    _bluetoothLeDevice = bleDevice;

        //    DeviceMac = GetMacAddress(_bluetoothLeDevice.BluetoothAddress);
        //    ConnectionId = new ConnectionId(_bluetoothLeDevice.Name + DeviceMac, (byte)DefaultConnectionIdProvider.WinBle);
        //    DeviceName = _bluetoothLeDevice.Name;

        //    _bluetoothLeDevice.ConnectionStatusChanged -= BluetoothLeDevice_ConnectionStatusChanged;
        //    _bluetoothLeDevice.ConnectionStatusChanged += BluetoothLeDevice_ConnectionStatusChanged;

        //    UpdateState(_bluetoothLeDevice.ConnectionStatus);
        //}

        public WinBleConnection(string deviceId, ILog log) 
            : base(nameof(WinBleConnection), log)
        {
            _deviceId = deviceId;

            ConnectionId = new ConnectionId(deviceId, (byte)DefaultConnectionIdProvider.WinBle);

            ConfigurePolicy();
        }

        #region IDisposable implementation
        bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual async void Dispose(bool disposing)
        {
            WriteDebugLine($">>>>>>>>>>>>>>>>>>>>>> _disposed: {_disposed}, disposing: {disposing}");

            if (_disposed)
                return;

            if (disposing)
            {
                _connectCancellation.Cancel();

                try
                {
                    await Disconnect();
                }
                catch (Exception ex)
                {
                    WriteLine(ex);
                }

                if (_mainCharacteristic != null)
                    _mainCharacteristic.ValueChanged -= MainCharacteristic_ValueChanged;

                if (_bluetoothLeDevice != null)
                    _bluetoothLeDevice.ConnectionStatusChanged -= BluetoothLeDevice_ConnectionStatusChanged;

                _connectCancellation.Dispose();
            }

            _disposed = true;
        }
        #endregion

        void ConfigurePolicy()
        {
            var sleepDurations = new[]
            {
              TimeSpan.FromMilliseconds(100),
              TimeSpan.FromMilliseconds(300),
              TimeSpan.FromMilliseconds(600)
            };

            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(sleepDurations, onRetry: (exception, retryCount, context) =>
                {
                    WriteLine($"################## Retrying to connect Win BLE device '{DeviceName}', delay: {retryCount}, message: '{exception.Message}'.",
                        LogErrorSeverity.Error);
                });

            _connectPolicy = AsyncRequestCollapserPolicy.Create().WrapAsync(retryPolicy);
            _disconnectPolicy = AsyncRequestCollapserPolicy.Create();

        }

        public async Task Connect()
        {
            WriteDebugLine("Connect");
            try
            {
                await _connectPolicy.ExecuteAsync(
                    (context, ct) => ConnectInternal(), 
                    new Context("connect"), 
                    _connectCancellation.Token);
            }
            catch (Exception ex)
            {
                WriteDebugLine($">>>>>>>>>>>>>>>>>>>>>> Connect ex: {ex.Message}");

                throw;
            }
        }

        async Task ConnectInternal()
        {
            WriteDebugLine(">>>>>>>>>>>>>>>>>>>>>> ConnectInternal...");

            if (State == ConnectionState.Connected)
            {
                WriteDebugLine(">>>>>>>>>>>>>>>>>>>>>> Already Connected");
                return;
            }

            if (_connectCancellation.IsCancellationRequested)
                return;

            if (_bluetoothLeDevice == null)
            {
                _bluetoothLeDevice = await BluetoothLEDevice.FromIdAsync(_deviceId);
                if (_bluetoothLeDevice == null)
                    throw new WinBleException("Failed to get BluetoothLEDevice from deviceId");

                DeviceMac = WinBleUtils.GetMacAddress(_bluetoothLeDevice.BluetoothAddress);
                DeviceName = _bluetoothLeDevice.Name;

                _bluetoothLeDevice.ConnectionStatusChanged -= BluetoothLeDevice_ConnectionStatusChanged;
                _bluetoothLeDevice.ConnectionStatusChanged += BluetoothLeDevice_ConnectionStatusChanged;
            }

            //GattDeviceServicesResult result =
            //    await new ConnectDeviceProc(_bluetoothLeDevice)
            //        .Run(SdkConfig.ConnectDeviceTimeout);

            await ReadCharacteristics();

            await ReadDeviceInfo();

            await EnableNotifications();

            UpdateState(ConnectionState.Connected);

            WriteDebugLine(">>>>>>>>>>>>>>>>>>>>>>> ConnectInternal...OK");
        }

        async Task ReadCharacteristics()
        {
            if (_connectCancellation.IsCancellationRequested)
                return;

            if (_mainCharacteristic == null)
            {
                WriteDebugLine(">>>>>>>>>>>>>>>>>>>>>>> ReadCharacteristics...");
                GattDeviceServicesResult result = await _bluetoothLeDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached);

                if (result == null)
                    throw new WinBleException("Failed to connect BLE device");

                if (result.Status != GattCommunicationStatus.Success)
                    throw new WinBleException($"Connection BLE device not successful: {result.Status}");

                var services = result.Services;
                var service = services.Where(s => s.Uuid == BleDefines.HIDEEZ_BLESERVICE_UUID).FirstOrDefault();

                if (service == null)
                    throw new WinBleException($"Hideez BLE service not found");

                GattOpenStatus openStatus = await service.OpenAsync(GattSharingMode.SharedReadAndWrite);
                GattCharacteristicsResult characteristicsResult = await service.GetCharacteristicsAsync(BluetoothCacheMode.Uncached);

                _mainCharacteristic = characteristicsResult.Characteristics.FirstOrDefault(c => c.Uuid == BleDefines.CHARACTERISTIC_UUID);
                if (_mainCharacteristic == null)
                    throw new WinBleException($"Hideez mainCharacteristic not found");

                _infoCharacteristic = characteristicsResult.Characteristics.FirstOrDefault(c => c.Uuid == BleDefines.INFO_CHARACTERISTIC_UUID);
                if (_infoCharacteristic == null)
                    throw new WinBleException($"Hideez infoCharacteristic not found");

                WriteDebugLine($">>>>>>>>>>>>>>>>>>>>>>> ReadCharacteristics...OK");
            }
        }

        public Task Disconnect()
        {
            WriteDebugLine(">>>>>>>>>>>>>>>>>>>>>>> Disconnect");
            return _disconnectPolicy.ExecuteAsync(context => DisconnectInternal(), new Context("disconnect"));
        }

        async Task DisconnectInternal()
        {
            WriteDebugLine("DisconnectInternal");
            if (_mainCharacteristic != null)
            {
                _mainCharacteristic.ValueChanged -= MainCharacteristic_ValueChanged;

                try
                {
                    var status = await _mainCharacteristic
                    .WriteClientCharacteristicConfigurationDescriptorAsync(
                        GattClientCharacteristicConfigurationDescriptorValue.None);
                }
                catch (Exception)
                {
                    throw;
                }

            }

            // do not unsubscribe - used to restore the connection
            //_bluetoothLeDevice.ConnectionStatusChanged -= BluetoothLeDevice_ConnectionStatusChanged;

            UpdateState(ConnectionState.NotConnected);
        }

        async Task ReadDeviceInfo()
        {
            if (_connectCancellation.IsCancellationRequested)
                return;

            GattReadResult result = await _infoCharacteristic.ReadValueAsync(BluetoothCacheMode.Uncached);
            if (result?.Status == GattCommunicationStatus.Success)
            {
                var data = result.Value?.ToArray();
                if (data.Length > 0)
                {
                    _isBoot = (data[0] == 0x01) || (data[0] == 0x64);
                    WriteDebugLine($">>>>>>>>>>>>>>>>>>>>>>> _isBoot: {_isBoot}");
                }
                else
                    throw new WinBleException("Cannot read info characteristic");
            }
        }

        async Task EnableNotifications()
        {
            if (_connectCancellation.IsCancellationRequested)
                return;

            _mainCharacteristic.ValueChanged -= MainCharacteristic_ValueChanged;
            _mainCharacteristic.ValueChanged += MainCharacteristic_ValueChanged;

            var status = await _mainCharacteristic
                .WriteClientCharacteristicConfigurationDescriptorAsync(
                GattClientCharacteristicConfigurationDescriptorValue.Notify);

            if (status != GattCommunicationStatus.Success)
                throw new WinBleException("Failed to enable notifications on the main characteristic");

            // Hideez Key will not send notifications until the first command has arrived. 
            // Sending the Cancel command
            WriteDebugLine("------------ reset ----------------------");
            byte[] data = new byte[2] { 0x02, 0x01 };
            status = await _mainCharacteristic.WriteValueAsync(data.AsBuffer());

            if (status != GattCommunicationStatus.Success)
                throw new WinBleException("Failed to write to the main characteristic");
        }

        void BluetoothLeDevice_ConnectionStatusChanged(BluetoothLEDevice sender, object args)
        {
            WriteDebugLine($">>>>>>>>>>>>>>>>>>>>>>>  BluetoothLeDevice_ConnectionStatusChanged {State} -> {sender.ConnectionStatus}");

            if (sender.ConnectionStatus == BluetoothConnectionStatus.Disconnected)
            {
                UpdateState(ConnectionState.NotConnected);
            }
            else if (sender.ConnectionStatus == BluetoothConnectionStatus.Connected
                     && State != ConnectionState.Connected)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        if (!_connectCancellation.IsCancellationRequested)
                            await Connect();
                    }
                    catch (Exception ex)
                    {
                        WriteDebugLine(ex.Message);
                    }
                });
            }
        }

        void UpdateState(ConnectionState newState)
        {
            if (newState != State)
            {
                WriteDebugLine($"!!! UpdateState {State} -> {newState}");
                State = newState;
                SafeInvoke(ConnectionStateChanged, new ConnectionStateChangedEventArgs(newState));
            }
        }

        void MainCharacteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            if (args.CharacteristicValue == null)
                return;

            byte[] data = args.CharacteristicValue.ToArray();

            if (data[1] == (byte)FwControlCommand.STATUS)
            {
                var rssi = (sbyte)data[3];
                SafeInvoke(AdvertismentReceived, new AdvertismentReceivedEventArgs(DeviceName, _deviceId, rssi));
                WriteDebugLine($"!!! RSSI {rssi}");
            }

            SafeInvoke(PacketReceived, new PacketReceivedEventArgs(data));
        }

        public bool IsBoot()
        {
            return _isBoot;
        }

        public async Task SendPacket(byte[] data)
        {
            if (_mainCharacteristic == null || _connectCancellation.IsCancellationRequested)
                throw new Exception("Cannot send packet - the connection has been closed");

            GattCommunicationStatus status = GattCommunicationStatus.Unreachable;
            try
            {
                status = await _mainCharacteristic.WriteValueAsync(data.AsBuffer(), GattWriteOption.WriteWithoutResponse);
            }
            catch (Exception) { }

            if (status != GattCommunicationStatus.Success)
                throw new HideezException(HideezErrorCode.BleCharacteristicWriteError);
        }

        internal async Task<DeviceUnpairingResult> Unpair()
        {
            return await _bluetoothLeDevice.DeviceInformation.Pairing.UnpairAsync();
        }

        internal async Task Unpair(IUnpairProvider unpairProvider)
        {
            await unpairProvider.UnpairAsync(_bluetoothLeDevice.DeviceInformation.Id);
        }

    }
}
