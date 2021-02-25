using Hideez.SDK.Communication;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.Connection;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Enumeration;
using Windows.Devices.Radios;

namespace WinBle
{
    public class WinBleConnectionManager : Logger, IBleConnectionManager, IDisposable
    {
        const int START_TIMEOUT = 50_000;
        const int STOP_TIMEOUT = 100_000;

        readonly WinBleDeviceWatcher _deviceWatcher;
        readonly ConcurrentDictionary<string, BleConnectionController> _controllers = new ConcurrentDictionary<string, BleConnectionController>();
        readonly SemaphoreSlim _startStopLock = new SemaphoreSlim(1, 1);

        Radio _bluetoothRadio = null;
        BluetoothAdapterState _state = BluetoothAdapterState.PoweredOff;

        public event EventHandler<AdvertismentReceivedEventArgs> AdvertismentReceived;
        public event EventHandler<DiscoveredDeviceAddedEventArgs> DiscoveredDeviceAdded;
        public event EventHandler<DiscoveredDeviceRemovedEventArgs> DiscoveredDeviceRemoved;
        public event EventHandler DiscoveryStopped;
        public event EventHandler AdapterStateChanged;

        // when device connected
        public event EventHandler<ControllerAddedEventArgs> ControllerAdded;
        // when device bonding deleted from Windows
        public event EventHandler<ControllerRemovedEventArgs> ControllerRemoved;

        public BluetoothAdapterState State
        {
            get
            {
                return _state;
            }
            private set
            {
                if (value != _state)
                {
                    _state = value;
                    AdapterStateChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public bool SupportsDiscoveryAndDeviceManagement => false;

        public byte Id => (byte)DefaultConnectionIdProvider.WinBle;

        public IReadOnlyCollection<IConnectionController> ConnectionControllers => _controllers.Values.Select(x => (IConnectionController)x).ToList();

        /// <summary>
        /// Provider that is used for unpair operation if UI thread is not available to application
        /// </summary>
        public IUnpairProvider UnpairProvider { get; set; }

        public WinBleConnectionManager(ILog log)
            :base(nameof(WinBleConnectionManager), log)
        {
            _deviceWatcher = new WinBleDeviceWatcher(log);
            _deviceWatcher.Added += DeviceWatcher_Added;
            _deviceWatcher.Removed += DeviceWatcher_Removed;

            //_bluetoothLEAdvertisementWatcher = new BluetoothLEAdvertisementWatcher
            //{
            //    ScanningMode = BluetoothLEScanningMode.Active
            //};
            //_bluetoothLEAdvertisementWatcher.Received += BluetoothLEAdvertisementWatcher_Received;
        }

        #region IDisposable implementation
        bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual async void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                _deviceWatcher.Added -= DeviceWatcher_Added;
                _deviceWatcher.Removed -= DeviceWatcher_Removed;
                _deviceWatcher.Dispose();

                await Stop();
            }

            disposed = true;
        }
        #endregion

        public async Task Restart()
        {
            State = BluetoothAdapterState.Resetting;

            await Stop();
            await Start();
        }

        public async Task Start()
        {
            await _startStopLock.WaitAsync(START_TIMEOUT);

            try
            {
                if (State == BluetoothAdapterState.PoweredOn)
                    return;

                _deviceWatcher.Start();

                if (_bluetoothRadio == null)
                {
                    BluetoothAdapter adapter = await BluetoothAdapter.GetDefaultAsync();
                    if (adapter != null)
                    {
                        _bluetoothRadio = await adapter.GetRadioAsync();
                        _bluetoothRadio.StateChanged += BluetoothRadio_StateChanged;
                    }
                }

                State = _bluetoothRadio.State == RadioState.On ? BluetoothAdapterState.PoweredOn : BluetoothAdapterState.Unknown;
            }
            catch (Exception ex)
            {
                WriteLine(ex);
                await Stop();
            }
            finally
            {
                _startStopLock.Release();
            }
        }

        public async Task Stop()
        {
            await _startStopLock.WaitAsync(STOP_TIMEOUT);

            try
            {
                _deviceWatcher.Stop();

                if (_bluetoothRadio != null)
                {
                    _bluetoothRadio.StateChanged -= BluetoothRadio_StateChanged;
                    _bluetoothRadio = null;
                }

                await ClearConnections();

                if (State != BluetoothAdapterState.Resetting)
                    State = BluetoothAdapterState.PoweredOff;
            }
            catch (Exception ex)
            {
                WriteLine(ex);
            }
            finally
            {
                _startStopLock.Release();
            }
        }

        #region Event handlers
        void BluetoothRadio_StateChanged(Radio sender, object args)
        {
            State = sender.State == RadioState.On ?
                BluetoothAdapterState.PoweredOn :
                BluetoothAdapterState.Unknown;
        }

        void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation deviceInfo)
        {
            WriteDebugLine($"Device added");

            Task.Run(async () => 
            {
                try
                {
                    var controller = CreateConnectionController(deviceInfo);
                    await Connect(controller);
                }
                catch (Exception ex)
                {
                    WriteLine(ex);
                }
            });
        }

        void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            WriteDebugLine($"Device removed");

            Task.Run(() =>
            {
                try
                {
                    if (_controllers.TryRemove(deviceInfoUpdate.Id, out BleConnectionController controller))
                    {
                        if (controller.Connection is WinBleConnection connection)
                        {
                            connection.AdvertismentReceived -= Connection_AdvertismentReceived;
                            connection.Dispose();
                        }

                        controller.ConnectionStateChanged -= ConnectionController_ConnectionStateChanged;
                        
                        SafeInvoke(ControllerRemoved, new ControllerRemovedEventArgs(controller));
                    }
                }
                catch (Exception ex)
                {
                    WriteLine(ex);
                }
            });
        }

        void Connection_AdvertismentReceived(object sender, AdvertismentReceivedEventArgs e)
        {
            SafeInvoke(AdvertismentReceived, e);
        }

        void ConnectionController_ConnectionStateChanged(object sender, EventArgs e)
        {
            if (sender is BleConnectionController connectionController &&
                connectionController.State == ConnectionState.Connected)
            {
                SafeInvoke(ControllerAdded, new ControllerAddedEventArgs(connectionController));
            }
        }

        #endregion

        IConnectionController CreateConnectionController(DeviceInformation deviceInfo)
        {
            return _controllers.GetOrAdd(deviceInfo.Id, (id) => 
            {
                var connection = new WinBleConnection(deviceInfo.Id, _log);
                var controller = new BleConnectionController(connection, _log);

                controller.ConnectionStateChanged += ConnectionController_ConnectionStateChanged;
                connection.AdvertismentReceived += Connection_AdvertismentReceived;

                return controller;
            });
        }

        void RemoveConnectionController(string id)
        {
            if (_controllers.TryRemove(id, out BleConnectionController controller))
            {
                if (controller.Connection is WinBleConnection connection)
                {
                    connection.AdvertismentReceived -= Connection_AdvertismentReceived;
                    connection.Dispose();
                }

                controller.ConnectionStateChanged -= ConnectionController_ConnectionStateChanged;

                SafeInvoke(ControllerRemoved, new ControllerRemovedEventArgs(controller));
            }
        }

        async Task ClearConnections()
        {
            foreach (var controller in _controllers.Values.ToList())
            {
                await RemoveConnection(controller);
            }

            _controllers.Clear();
        }

        public Task<IConnectionController> PairAndConnect(ConnectionId id)
        {
            throw new NotSupportedException();
        }

        public async Task<IConnectionController> Connect(IConnectionController connectionController)
        {
            if (connectionController != null
                && connectionController.Connection is WinBleConnection winBleConnection)
            {
                await winBleConnection.Connect();
            }
            return connectionController;
        }

        public Task<IConnectionController> Connect(ConnectionId id)
        {
            if (!_controllers.TryGetValue(id.Id, out BleConnectionController connectionController))
                throw new WinBleException($"Connection '{id.Id}' not found");
            return Connect(connectionController);
        }

        public Task Disconnect(IConnectionController controller)
        {
            return controller is BleConnectionController bleDeviceConnectionController
                ? bleDeviceConnectionController.Disconnect()
                : throw new Exception("Invalid connection object");
        }

        public async Task RemoveConnection(IConnectionController controller)
        {
            if (_controllers.TryRemove(controller.Id, out BleConnectionController bleConnectionController))
            {
                await Disconnect(controller);

                if (controller.Connection is WinBleConnection connection)
                {
                    connection.AdvertismentReceived -= Connection_AdvertismentReceived;
                    connection.Dispose();
                }

                bleConnectionController.ConnectionStateChanged -= ConnectionController_ConnectionStateChanged;

                SafeInvoke(ControllerRemoved, new ControllerRemovedEventArgs(controller));

                WriteLine($"Connection removed {controller.Name} [{controller.Id}]");
            }
        }

        public async Task DeleteBond(IConnectionController controller)
        {
            if (controller.Connection is WinBleConnection winBleConnection)
            {
                try
                {
                    // Unpair can only be called from UI thread
                    if (UnpairProvider != null)
                    {
                        await winBleConnection.Unpair(UnpairProvider);
                        WriteLine($"Unpair vault ({controller.Id}): assumed successful");
                    }
                    else
                    {
                        var unpairResult = await winBleConnection.Unpair();
                        WriteLine($"Unpair vault ({controller.Id}): {unpairResult.Status}");
                    }
                }
                catch (Exception ex) 
                {
                    WriteLine(ex);
                }

                await RemoveConnection(controller);
            }
        }

        public Task DeleteBond(ConnectionId connectionId)
        {
            if (_controllers.TryGetValue(connectionId.Id, out var controller))
                return DeleteBond(controller);

            return Task.CompletedTask;
        }

    }
}
