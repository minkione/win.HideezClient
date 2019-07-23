using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Hideez.CsrBLE;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.FW;
using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.LongOperations;
using Microsoft.Win32;

namespace DeviceMaintenance.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        readonly EventLogger _log;
        readonly BleConnectionManager _connectionManager;
        readonly BleDeviceManager _deviceManager;
        //readonly CredentialProviderConnection _credentialProviderConnection;
        //readonly WorkstationUnlocker _workstationUnlocker;
        //readonly RfidServiceConnection _rfidService;

        //HesAppConnection _hesConnection;
        //byte _nextChannelNo = 2;
        readonly ConcurrentDictionary<string, Guid> _pendingConnections =
            new ConcurrentDictionary<string, Guid>();

        public string BleAdapterState => _connectionManager?.State.ToString();
        public string ConectByMacAddress { get; set; } = "D0:A8:9E:6B:CD:8D";

        //public string RfidAdapterState => "NA";
        //public string RfidAddress { get; set; }

        //public string HesAddress { get; set; }
        //public string HesState => _hesConnection?.State.ToString();


        private string _fileName;
        public string FileName
        {
            get { return _fileName; }
            set
            {
                if (_fileName != value)
                {
                    _fileName = value;
                    NotifyPropertyChanged(nameof(FileName));
                }
            }
        }

        bool _bleAdapterDiscovering;
        public bool BleAdapterDiscovering
        {
            get
            {
                return _bleAdapterDiscovering;
            }
            set
            {
                if (_bleAdapterDiscovering != value)
                {
                    _bleAdapterDiscovering = value;
                    NotifyPropertyChanged(nameof(BleAdapterDiscovering));
                }
            }
        }

        DeviceViewModel _currentDevice;
        public DeviceViewModel CurrentDevice
        {
            get
            {
                return _currentDevice;
            }
            set
            {
                if (_currentDevice != value)
                {
                    _currentDevice = value;
                    NotifyPropertyChanged(nameof(CurrentDevice));
                }
            }
        }

        DiscoveredDeviceAddedEventArgs _currentDiscoveredDevice;
        public DiscoveredDeviceAddedEventArgs CurrentDiscoveredDevice
        {
            get
            {
                return _currentDiscoveredDevice;
            }
            set
            {
                if (_currentDiscoveredDevice != value)
                {
                    _currentDiscoveredDevice = value;
                    NotifyPropertyChanged(nameof(CurrentDiscoveredDevice));
                }
            }
        }

        public ObservableCollection<DiscoveredDeviceAddedEventArgs> DiscoveredDevices { get; } 
            = new ObservableCollection<DiscoveredDeviceAddedEventArgs>();


        public ObservableCollection<DeviceViewModel> Devices { get; }
            = new ObservableCollection<DeviceViewModel>();


        #region Commands

        public ICommand BleAdapterResetCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () =>
                    {
                        return true;
                    },
                    CommandAction = (x) =>
                    {
                        ResetBleAdapter();
                    }
                };
            }
        }

        public ICommand StartDiscoveryCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () =>
                    {
                        return _connectionManager.State == BluetoothAdapterState.PoweredOn && !BleAdapterDiscovering;
                    },
                    CommandAction = (x) =>
                    {
                        StartDiscovery();
                    }
                };
            }
        }

        public ICommand StopDiscoveryCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () =>
                    {
                        return _connectionManager.State == BluetoothAdapterState.PoweredOn && BleAdapterDiscovering;
                    },
                    CommandAction = (x) =>
                    {
                        StopDiscovery();
                    }
                };
            }
        }

        public ICommand ClearDiscoveredDeviceListCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () =>
                    {
                        return true;
                    },
                    CommandAction = (x) =>
                    {
                        ClearDiscoveredDeviceList();
                    }
                };
            }
        }

        public ICommand ConnectDiscoveredDeviceCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () =>
                    {
                        return CurrentDiscoveredDevice != null;
                    },
                    CommandAction = (x) =>
                    {
                        ConnectDiscoveredDevice(CurrentDiscoveredDevice);
                    }
                };
            }
        }

        public ICommand ConnectDeviceCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () =>
                    {
                        return CurrentDevice != null;
                    },
                    CommandAction = (x) =>
                    {
                        ConnectDevice(CurrentDevice);
                    }
                };
            }
        }

        public ICommand DisconnectDeviceCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () =>
                    {
                        return CurrentDevice != null;
                    },
                    CommandAction = (x) =>
                    {
                        DisconnectDevice(CurrentDevice);
                    }
                };
            }
        }

        public ICommand PingDeviceCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () =>
                    {
                        return CurrentDevice != null;
                    },
                    CommandAction = (x) =>
                    {
                        PingDevice(CurrentDevice);
                    }
                };
            }
        }

        public ICommand SelectFirmwareCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = (x) =>
                    {
                        SelectFirmware();
                    }
                };
            }
        }

        public ICommand FirmwareAutoupdateCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = (x) =>
                    {
                        FirmwareAutoupdate();
                    }
                };
            }
        }

        public bool IsFirmwareAutoupdateOn { get; private set; }

        //public ObservableCollection<FirmwareUpdateViewModel> CurrentFirmwareUpdates { get; } = new ObservableCollection<FirmwareUpdateViewModel>();

        #endregion

        public MainWindowViewModel()
        {
            //HesAddress = "https://localhost:44371";

            FileName = Properties.Settings.Default.FirmwareFileName;


            _log = new EventLogger("ExampleApp");
            _connectionManager = new BleConnectionManager(_log, "d:\\temp\\bonds"); //todo

            _connectionManager.AdapterStateChanged += ConnectionManager_AdapterStateChanged;
            _connectionManager.DiscoveryStopped += ConnectionManager_DiscoveryStopped;
            _connectionManager.DiscoveredDeviceAdded += ConnectionManager_DiscoveredDeviceAdded;
            _connectionManager.DiscoveredDeviceRemoved += ConnectionManager_DiscoveredDeviceRemoved;
            _connectionManager.AdvertismentReceived += ConnectionManager_AdvertismentReceived;

            // BLE ============================
            _deviceManager = new BleDeviceManager(_log, _connectionManager);
            _deviceManager.DeviceAdded += DevicesManager_DeviceCollectionChanged;
            _deviceManager.DeviceRemoved += DevicesManager_DeviceCollectionChanged;
            
            _connectionManager.StartDiscovery();
        }

        internal void Close()
        {
            Properties.Settings.Default.FirmwareFileName = FileName;

            Properties.Settings.Default.Save();
        }

        void DevicesManager_DeviceCollectionChanged(object sender, DeviceCollectionChangedEventArgs e)
        {
            //Application.Current.Dispatcher.Invoke(() =>
            //{
            //    if (e.AddedDevice != null)
            //    {
            //        var deviceViewModel = new DeviceViewModel(e.AddedDevice);
            //        Devices.Add(deviceViewModel);
            //        if (CurrentDevice == null)
            //            CurrentDevice = deviceViewModel;
            //    }
            //    else if (e.RemovedDevice != null)
            //    {
            //        var item = Devices.FirstOrDefault(x => x.Id == e.RemovedDevice.Id &&
            //                                               x.ChannelNo == e.RemovedDevice.ChannelNo);

            //        if (item != null)
            //            Devices.Remove(item);
            //    }
            //});
        }

        async void ConnectionManager_AdvertismentReceived(object sender, AdvertismentReceivedEventArgs e)
        {
            try
            {
                if (IsFirmwareAutoupdateOn && e.Rssi > -27)
                {
                    var newGuid = Guid.NewGuid();
                    var guid = _pendingConnections.GetOrAdd(e.Id, newGuid);

                    if (guid == newGuid)
                    {
                        var deviceVM = await ConnectDeviceByMac(e.Id);
                        if (deviceVM?.Device != null)
                        {
                            await deviceVM.Device.WaitInitialization(timeout: 10_000);
                            await deviceVM.StartFirmwareUpdate(FileName);
                        }

                        _pendingConnections.TryRemove(e.Id, out Guid removed);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                _pendingConnections.TryRemove(e.Id, out Guid removed);
            }
        }

        void ConnectionManager_DiscoveredDeviceAdded(object sender, DiscoveredDeviceAddedEventArgs e)
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                DiscoveredDevices.Add(e);
            });
        }

        void ConnectionManager_DiscoveredDeviceRemoved(object sender, DiscoveredDeviceRemovedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var item = DiscoveredDevices.FirstOrDefault(x => x.Id == e.Id);
                if (item != null)
                    DiscoveredDevices.Remove(item);
            });
        }

        void ConnectionManager_DiscoveryStopped(object sender, EventArgs e)
        {
            BleAdapterDiscovering = false;
        }

        void ConnectionManager_AdapterStateChanged(object sender, EventArgs e)
        {
            NotifyPropertyChanged(nameof(BleAdapterState));
        }

        void ResetBleAdapter()
        {
            _connectionManager.Restart();
        }

        void StartDiscovery()
        {
            //DiscoveredDevices.Clear();
            _connectionManager.StartDiscovery();
            BleAdapterDiscovering = true;
        }

        void StopDiscovery()
        {
            _connectionManager.StopDiscovery();
            BleAdapterDiscovering = false;
            DiscoveredDevices.Clear();
        }

        void ClearDiscoveredDeviceList()
        {
            DiscoveredDevices.Clear();
        }

        async void RemoveAllDevices()
        {
            try
            {
                await _deviceManager.RemoveAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void ConnectDiscoveredDevice(DiscoveredDeviceAddedEventArgs e)
        {
            _connectionManager.ConnectDiscoveredDeviceAsync(e.Id);
        }

        async Task<DeviceViewModel> ConnectDeviceByMac(string mac)
        {
            _log.WriteLine("MainVM", $"Waiting Device connectin {mac} ..........................");
            var dvm = new DeviceViewModel(mac, _log);

            Application.Current.Dispatcher.Invoke(() =>
            {
                Devices.Add(dvm);
            });

            var device = await _deviceManager.ConnectByMac(mac, timeout: 10_000);

            if (device != null)
            {
                dvm.SetDevice(device);
                _log.WriteLine("MainVM", $"Device connected {device.Name} ++++++++++++++++++++++++");
            }
            else
                _log.WriteLine("MainVM", $"Device NOT connected --------------------------");

            return dvm;
        }

        void ConnectDevice(DeviceViewModel device)
        {
            try
            {
                device.Device.Connection.Connect();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void DisconnectDevice(DeviceViewModel device)
        {
            device.Device.Disconnect();
        }

        async void PingDevice(DeviceViewModel device)
        {
            try
            {
                var pingText = $"{device.Id} {DateTime.Now}";
                var reply = await device.Device.Ping(pingText);
                if (pingText != reply.ResultAsString)
                    throw new Exception("Wrong reply: " + reply.ResultAsString);
                else
                    MessageBox.Show(reply.ResultAsString);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SelectFirmware()
        {
            if (string.IsNullOrWhiteSpace(FileName))
                FileName = "c:\\";

            OpenFileDialog ofd = new OpenFileDialog
            {
                InitialDirectory = Path.GetDirectoryName(FileName),
                Filter = "Firmware Image file | *.img"
            };

            if (ofd.ShowDialog() == true)
                FileName = ofd.FileName;
        }

        void FirmwareAutoupdate()
        {
            IsFirmwareAutoupdateOn = !IsFirmwareAutoupdateOn;
        }

    }
}
