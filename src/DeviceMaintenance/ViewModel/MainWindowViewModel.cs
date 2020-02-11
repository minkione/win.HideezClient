using Hideez.CsrBLE;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.FW;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.LongOperations;
using Microsoft.Win32;
using MvvmExtensions.Attributes;
using MvvmExtensions.PropertyChangedMonitoring;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace DeviceMaintenance.ViewModel
{
    public class MainWindowViewModel : PropertyChangedImplementation
    {
        readonly object pendingConnectionsLock = new object();
        const string SERVICE_NAME = "Hideez Service";

        readonly EventLogger _log;
        readonly BleConnectionManager _connectionManager;
        readonly BleDeviceManager _deviceManager;

        readonly Dictionary<string, Guid> _pendingConnections = new Dictionary<string, Guid>();

        ServiceController _hideezServiceController;
        Timer _serviceStateRefreshTimer;

        bool _restartServiceOnExit = false;
        bool _automaticallyUpdateFirmware = true;
        string _fileName = Properties.Settings.Default.FirmwareFileName;
        DeviceViewModel _currentDevice;
        DiscoveredDeviceAddedEventArgs _currentDiscoveredDevice;

        private ServiceController HideezServiceController
        {
            get
            {
                return _hideezServiceController;
            }
            set
            {
                _hideezServiceController = value;
                NotifyPropertyChanged();
            }
        }

        public string Title
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly().GetName();
                return $"{assembly.Name} v{assembly.Version.ToString()}";
            }
        }

        [DependsOn(nameof(HideezServiceController))]
        public bool CanInteractWithService
        {
            get
            {
                return _hideezServiceController != null;
            }
        }

        public bool HideezServiceOnline
        {
            get
            {
                return HideezServiceController?.Status == ServiceControllerStatus.Running;
            }
        }

        public bool BleAdapterAvailable
        {
            get
            {
                return _connectionManager?.State == BluetoothAdapterState.PoweredOn;
            }
        }

        public string FileName
        {
            get
            {
                return _fileName;
            }
            set
            {
                if (_fileName != value)
                {
                    _fileName = value;
                    Properties.Settings.Default.FirmwareFileName = FileName;
                    Properties.Settings.Default.Save();
                    NotifyPropertyChanged();
                }
            }
        }

        public bool RestartServiceOnExit
        {
            get
            {
                return _restartServiceOnExit;
            }
            set
            {
                if (_restartServiceOnExit != value)
                {
                    _restartServiceOnExit = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool AutomaticallyUpdateFirmware
        {
            get
            {
                return _automaticallyUpdateFirmware;
            }
            set
            {
                if (_automaticallyUpdateFirmware != value)
                {
                    _automaticallyUpdateFirmware = value;
                    NotifyPropertyChanged();
                }
            }
        }

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
                    NotifyPropertyChanged();
                }
            }
        }

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
                    NotifyPropertyChanged();
                }
            }
        }

        public ObservableCollection<DiscoveredDeviceAddedEventArgs> DiscoveredDevices { get; }
            = new ObservableCollection<DiscoveredDeviceAddedEventArgs>();

        public ObservableCollection<DeviceViewModel> Devices { get; }
            = new ObservableCollection<DeviceViewModel>();

        /// <summary>
        /// Returns true if any device is currently undergoing firmware update
        /// </summary>
        [DependsOn(nameof(Devices))]
        public bool IsFirmwareUpdateInProgress
        {
            get
            {
                return Devices.Any(d => d.InProgress);
            }
        }


        #region Commands

        public ICommand StopServiceCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = (x) =>
                    {
                        StopService();
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

        #endregion

        public MainWindowViewModel()
        {
            _log = new EventLogger("ExampleApp");

            InitializeHideezServiceController();

            var commonAppData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            var bondsFilePath = $"{commonAppData}\\Hideez\\bonds";

            _connectionManager = new BleConnectionManager(_log, bondsFilePath);

            _connectionManager.AdapterStateChanged += ConnectionManager_AdapterStateChanged;
            _connectionManager.DiscoveredDeviceAdded += ConnectionManager_DiscoveredDeviceAdded;
            _connectionManager.DiscoveredDeviceRemoved += ConnectionManager_DiscoveredDeviceRemoved;
            _connectionManager.AdvertismentReceived += ConnectionManager_AdvertismentReceived;

            // BLE ============================
            _deviceManager = new BleDeviceManager(_log, _connectionManager);
            _deviceManager.DeviceAdded += DevicesManager_DeviceCollectionChanged;
            _deviceManager.DeviceRemoved += DevicesManager_DeviceCollectionChanged;

            _connectionManager.StartDiscovery();

            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
        }

        void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (e.Reason == SessionSwitchReason.SessionLock ||
                e.Reason == SessionSwitchReason.SessionLogoff ||
                e.Reason == SessionSwitchReason.SessionUnlock ||
                e.Reason == SessionSwitchReason.SessionLogon)
            {
                AutomaticallyUpdateFirmware = false;
            }
        }

        void InitializeHideezServiceController()
        {
            try
            {
                _serviceStateRefreshTimer = new Timer(2000);
                _serviceStateRefreshTimer.Elapsed += ServiceStateCheckTimer_Elapsed;
                _serviceStateRefreshTimer.AutoReset = true;
                _serviceStateRefreshTimer.Start();

                var controller = new ServiceController(SERVICE_NAME);
                var st = controller.Status; // Will trigger InvalidOperationException if service is not installed
                HideezServiceController = controller;

                NotifyPropertyChanged(nameof(HideezServiceOnline));
            }
            catch (InvalidOperationException)
            {
                // The most probable reason is that service is not installed. It is ok.
            }
            catch (ArgumentException)
            {
                // The most probable reason is that service is not installed. It is ok.
            }
        }

        void ServiceStateCheckTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (HideezServiceController == null)
                {
                    var controller = new ServiceController(SERVICE_NAME);
                    var st = controller.Status; // Will trigger InvalidOperationException if service is not installed
                    HideezServiceController = controller;
                }

                HideezServiceController?.Refresh();

                NotifyPropertyChanged(nameof(HideezServiceOnline));
            }
            catch (InvalidOperationException)
            {
                // The most probable reason is that service is not installed. It is ok.
            }
            catch (ArgumentException)
            {
                // The most probable reason is that service is not installed. It is ok.
            }
            catch (Exception ex)
            {
                _log.WriteLine(nameof(ServiceStateCheckTimer_Elapsed), ex);
            }
        }

        void DevicesManager_DeviceCollectionChanged(object sender, DeviceCollectionChangedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                //if (e.AddedDevice != null)
                //{
                //    var deviceViewModel = new DeviceViewModel(e.AddedDevice);
                //    Devices.Add(deviceViewModel);
                //    if (CurrentDevice == null)
                //        CurrentDevice = deviceViewModel;
                //}
                //else 
                if (e.RemovedDevice != null)
                {
                    var item = Devices.FirstOrDefault(x => x.Id == e.RemovedDevice.Id && x.ChannelNo == e.RemovedDevice.ChannelNo);

                    if (item != null)
                    {
                        Devices.Remove(item);
                        item.FirmwareUpdateRequest -= Device_FirmwareUpdateRequest;
                    }
                }
            });
        }

        void ConnectionManager_AdvertismentReceived(object sender, AdvertismentReceivedEventArgs e)
        {
            Task.Run(async () =>
            {
                try
                {
                    if (e.Rssi > -25)
                    {
                        lock (pendingConnectionsLock)
                        {
                            // Prevent reconnect of devices that neither finished nor failed firmware update
                            if (Devices.ToList().FirstOrDefault(d =>
                                !d.ErrorState &&
                                !d.SuccessState &&
                                d.Device != null &&
                                BleUtils.MacToConnectionId(d.Device.Mac).Equals(e.Id)) != null)
                            {
                                return;
                            }

                            if (_pendingConnections.ContainsKey(e.Id))
                                return;
                            else
                                _pendingConnections.Add(e.Id, Guid.NewGuid());
                        }

                        var deviceVM = await ConnectDeviceByMac(e.Id);
                        try
                        {
                            if (deviceVM?.Device != null)
                            {
                                await deviceVM.Device.WaitInitialization(timeout: 10_000, System.Threading.CancellationToken.None);
                                if (AutomaticallyUpdateFirmware || deviceVM.Device.IsBoot)
                                    deviceVM.StartFirmwareUpdate();
                            }

                            lock (pendingConnectionsLock)
                            {
                                _pendingConnections.Remove(e.Id);
                            }
                        }
                        catch (Exception ex)
                        {
                            deviceVM.CustomError = ex.Message;
                            throw;
                        }
                    }
                }
                catch (Exception)
                {
                    lock (pendingConnectionsLock)
                    {
                        _pendingConnections.Remove(e.Id);
                    }
                }
            });
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

        void ConnectionManager_AdapterStateChanged(object sender, EventArgs e)
        {
            NotifyPropertyChanged(nameof(BleAdapterAvailable));
        }

        void ConnectDiscoveredDevice(DiscoveredDeviceAddedEventArgs e)
        {
            _connectionManager.ConnectDiscoveredDeviceAsync(e.Id);
        }

        async Task<DeviceViewModel> ConnectDeviceByMac(string mac)
        {
            _log.WriteLine("MainVM", $"Waiting Device connectin {mac} ..........................");
            var dvm = new DeviceViewModel(mac);

            var prevDvm = Devices.FirstOrDefault(d => d.Device != null && d.Device.Mac.Replace(":","") == mac);
            if (prevDvm != null)
            {
                prevDvm.FirmwareUpdateRequest -= Device_FirmwareUpdateRequest;
                await _deviceManager.Remove(prevDvm.Device);
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                Devices.Add(dvm);
                dvm.FirmwareUpdateRequest += Device_FirmwareUpdateRequest;
            });

            var device = await _deviceManager.ConnectDevice(mac, SdkConfig.ConnectDeviceTimeout);

            if (device == null)
            {
                device = await _deviceManager.ConnectDevice(mac, SdkConfig.ConnectDeviceTimeout / 2);
            }
            
            if (device == null)
            {
                await _deviceManager.RemoveByMac(mac);
                device = await _deviceManager.ConnectDevice(mac, SdkConfig.ConnectDeviceTimeout);
            }

            if (device != null)
            {
                dvm.SetDevice(device);
                _log.WriteLine("MainVM", $"Device connected {device.Name} ++++++++++++++++++++++++");
            }
            else
            {
                _log.WriteLine("MainVM", $"Device NOT connected --------------------------");
                dvm.CustomError = "Connection failed";
            }

            return dvm;
        }

        async void Device_FirmwareUpdateRequest(DeviceViewModel sender, IDevice device, LongOperation longOperation)
        {
            try
            {
                var imageUploader = new FirmwareImageUploader(FileName, _log);
                await imageUploader.RunAsync(false, _deviceManager, device, longOperation);
            }
            catch (Exception ex)
            {
                sender.CustomError = ex.Message;
            }
        }

        void StopService()
        {
            try
            {
                HideezServiceController?.Refresh();
                NotifyPropertyChanged(nameof(HideezServiceOnline));

                if (HideezServiceController.CanStop)
                {
                    HideezServiceController?.Stop();
                    HideezServiceController?.Refresh();
                    NotifyPropertyChanged(nameof(HideezServiceOnline));
                }
            }
            catch (Exception ex)
            {
                _log.WriteLine(nameof(StopService), ex);
            }
        }

        void StartService()
        {
            try
            {
                HideezServiceController?.Start();
            }
            catch (Exception ex)
            {
                _log.WriteLine(nameof(StopService), ex);
            }
        }

        void SelectFirmware()
        {
            if (string.IsNullOrWhiteSpace(FileName))
                FileName = "Not selected...";

            OpenFileDialog ofd = new OpenFileDialog
            {
                InitialDirectory = Path.GetDirectoryName(FileName),
                Filter = "Firmware Image file | *.img"
            };

            if (ofd.ShowDialog() == true)
                FileName = ofd.FileName;
        }

        internal void OnClosing()
        {
            SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;


            if (RestartServiceOnExit && !HideezServiceOnline)
                StartService();
        }
    }
}
