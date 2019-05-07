using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Hideez.CsrBLE;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.Log;
using HideezMiddleware;

namespace WinSampleApp.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        readonly EventLogger _log;
        readonly BleConnectionManager _connectionManager;
        readonly BleDeviceManager _deviceManager;
        readonly CredentialProviderConnection _credentialProviderConnection;
        readonly WorkstationUnlocker _workstationUnlocker;
        private HesAppConnection _hesConnection;

        public string BleAdapterState => _connectionManager?.State.ToString();
        public string RfidAdapterState => "NA";

        bool bleAdapterDiscovering;
        public bool BleAdapterDiscovering
        {
            get
            {
                return bleAdapterDiscovering;
            }
            set
            {
                if (bleAdapterDiscovering != value)
                {
                    bleAdapterDiscovering = value;
                    NotifyPropertyChanged(nameof(BleAdapterDiscovering));
                }
            }
        }

        DeviceViewModel currentDevice;
        public DeviceViewModel CurrentDevice
        {
            get
            {
                return currentDevice;
            }
            set
            {
                if (currentDevice != value)
                {
                    currentDevice = value;
                    NotifyPropertyChanged(nameof(CurrentDevice));
                }
            }
        }

        DiscoveredDeviceAddedEventArgs currentDiscoveredDevice;
        private RfidServiceConnection _rfidService;

        public DiscoveredDeviceAddedEventArgs CurrentDiscoveredDevice
        {
            get
            {
                return currentDiscoveredDevice;
            }
            set
            {
                if (currentDiscoveredDevice != value)
                {
                    currentDiscoveredDevice = value;
                    NotifyPropertyChanged(nameof(CurrentDiscoveredDevice));
                }
            }
        }

        public ObservableCollection<DiscoveredDeviceAddedEventArgs> DiscoveredDevices { get; } 
            = new ObservableCollection<DiscoveredDeviceAddedEventArgs>();


        public ObservableCollection<DeviceViewModel> Devices { get; }
            = new ObservableCollection<DeviceViewModel>();

        //public IReadOnlyList<IDevice> Devices => _deviceManager.Devices;

        #region Commands
        public ICommand BleAdapterStartCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () =>
                    {
                        return _connectionManager.State == BluetoothAdapterState.PoweredOn;
                    },
                    CommandAction = (x) =>
                    {
                        BleAdapterStart(x);
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

        public ICommand RemoveAllDevicesCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () =>
                    {
                        return Devices.Count > 0;
                    },
                    CommandAction = (x) =>
                    {
                        RemoveAllDevices(x);
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

        public ICommand InitDeviceCommand
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
                        //InitDevice(CurrentDevice);
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

        public ICommand AddDeviceChannelCommand
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
                        AddDeviceChannel(CurrentDevice);
                    }
                };
            }
        }

        public ICommand Test1Command
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
                        Test();
                    }
                };
            }
        }

        public ICommand SendHesCommand
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
                        SendHes();
                    }
                };
            }
        }
        #endregion

        public MainWindowViewModel()
        {
            _log = new EventLogger("ExampleApp");
            _connectionManager = new BleConnectionManager(_log, "d:\\temp\\bonds"); //todo

            _connectionManager.AdapterStateChanged += ConnectionManager_AdapterStateChanged;
            _connectionManager.DiscoveryStopped += ConnectionManager_DiscoveryStopped;
            _connectionManager.DiscoveredDeviceAdded += ConnectionManager_DiscoveredDeviceAdded;
            _connectionManager.DiscoveredDeviceRemoved += ConnectionManager_DiscoveredDeviceRemoved;

            // COM =============================
            //var port = new ComConnection(log, "COM68", 9600);
            //port.Connect();

            // BLE ============================
            _deviceManager = new BleDeviceManager(_log, _connectionManager);
            _deviceManager.DeviceAdded += DevicesManager_DeviceCollectionChanged;
            _deviceManager.DeviceRemoved += DevicesManager_DeviceCollectionChanged;


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
           // _connectionManager.StartDiscovery();
        }

        internal void Close()
        {
            _connectionManager?.Shutdown();
            _rfidService?.Stop();
            _credentialProviderConnection?.Stop();
        }

        void DevicesManager_DeviceCollectionChanged(object sender, DeviceCollectionChangedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (e.AddedDevice != null)
                    Devices.Add(new DeviceViewModel(e.AddedDevice));
                else if (e.RemovedDevice != null)
                {
                    var item = Devices.FirstOrDefault(x => x.Id == e.RemovedDevice.Id);
                    if (item != null)
                        Devices.Remove(item);
                }
            });
        }

        void ConnectionManager_DiscoveredDeviceAdded(object sender, DiscoveredDeviceAddedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
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

        void BleAdapterStart(object param)
        {
            _connectionManager.Start();
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

        void RemoveAllDevices(object x)
        {
            _deviceManager.RemoveAll();
        }

        void ConnectDiscoveredDevice(DiscoveredDeviceAddedEventArgs e)
        {
            _connectionManager.ConnectDiscoveredDeviceAsync(e.Id);
        }

        void DisconnectDevice(DeviceViewModel device)
        {
           // device.Connection.Disconnect();
        }

        //async void InitDevice(DeviceViewModel device)
        //{
        //    try
        //    {
        //        await device.Device.Authenticate();
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //    }
        //}

        async void PingDevice(DeviceViewModel device)
        {
            try
            {
                //var pingText = $"{device.Id} {DateTime.Now} qwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqer";
                var pingText = $"{device.Id} {DateTime.Now}";
                //pingText = pingText + pingText + pingText;
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

        void AddDeviceChannel(DeviceViewModel currentDevice)
        {
            //BleDevice newDevice = _deviceManager.AddChannel(currentDevice.Device, _nextChannelNo++);
        }

        void Test()
        {
            try
            {
                foreach (var device in _deviceManager.Devices)
                {
                    Task.Run(async () =>
                    {
                        try
                        {
                            for (int i = 0; i < 10000; i++)
                            {
                                //var pingText = $"{device.Id} {DateTime.Now} qwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqerqwerqwerqwer qwer qwe rwer wqe rqqqqqqqqqqqqqqqqqqqqqqqqqqwer qwer qwer qwerwqr wqer";
                                //pingText = pingText + pingText + pingText;
                                var pingText = $"{device.Id}_{i}";
                                var reply = await device.Ping(pingText);
                                if (pingText != reply.ResultAsString)
                                    throw new Exception("Wrong reply");
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    });

                    Task.Run(async () =>
                    {
                        try
                        {
                            //await Task.Delay(1000);
                            for (int i = 0; i < 10000; i++)
                            {
                                var pingText = $"{device.Id}_{i + 5}";
                                var reply = await device.Ping(pingText);
                                if (pingText != reply.ResultAsString)
                                    throw new Exception("Wrong reply");
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SendHes()
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
