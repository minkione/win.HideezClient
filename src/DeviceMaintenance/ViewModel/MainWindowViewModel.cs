using DeviceMaintenance.Service;
using Hideez.CsrBLE;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.FW;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.LongOperations;
using HideezMiddleware.DeviceConnection;
using HideezMiddleware.Settings;
using HideezMiddleware.Settings.Manager;
using Microsoft.Win32;
using MvvmExtensions.Attributes;
using MvvmExtensions.PropertyChangedMonitoring;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DeviceMaintenance.ViewModel
{
    public class MainWindowViewModel : PropertyChangedImplementation
    {
        const string FW_FILE_EXTENSION = "img";
            
        readonly EventLogger _log;
        readonly BleConnectionManager _connectionManager;
        readonly BleDeviceManager _deviceManager;
        readonly AdvertisementIgnoreList _advIgnoreList;

        readonly Dictionary<string, Guid> _pendingConnections = new Dictionary<string, Guid>();
        readonly object pendingConnectionsLock = new object();

        bool _restartServiceOnExit = false;
        bool _automaticallyUpdateFirmware = Properties.Settings.Default.AutomaticallyUpdate;
        string _fileName = Properties.Settings.Default.FirmwareFileName;
        DeviceViewModel _currentDevice;
        DiscoveredDeviceAddedEventArgs _currentDiscoveredDevice;

        #region Properties

        public string Title
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly().GetName();
                return $"{assembly.Name} v{assembly.Version.ToString()}";
            }
        }

        public bool BleAdapterAvailable
        {
            get
            {
                return _connectionManager?.State == BluetoothAdapterState.PoweredOn;
            }
        }

        public string FirmwareFilePath
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
                    Properties.Settings.Default.FirmwareFileName = FirmwareFilePath;
                    Properties.Settings.Default.Save();
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
                    Properties.Settings.Default.AutomaticallyUpdate = AutomaticallyUpdateFirmware;
                    Properties.Settings.Default.Save();
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

        public HideezServiceController HideezServiceController { get; }

        #endregion

        #region Commands

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

            HideezServiceController = new HideezServiceController();

            if (HideezServiceController.IsServiceRunning)
            {
                HideezServiceController.StopService();
                _restartServiceOnExit = true;
            }

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

            _advIgnoreList = new AdvertisementIgnoreList(_connectionManager, new VirtualSettingsManager<ProximitySettings>(), null);

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

        void ConnectionManager_AdvertismentReceived(object sender, AdvertismentReceivedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FirmwareFilePath) || !FirmwareFilePath.EndsWith($".{FW_FILE_EXTENSION}"))
                return;

            if (e.Rssi > SdkConfig.TapProximityUnlockThreshold + 4 && !_advIgnoreList.IsIgnored(e.Id)) // -33 is to much and picks up devices from very far
            {
                Task.Run(async () =>
                {
                    try
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
                                {
                                    deviceVM.StartFirmwareUpdate();
                                    _advIgnoreList.Ignore(deviceVM.Device.Mac);
                                }
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
                    catch (Exception)
                    {
                        lock (pendingConnectionsLock)
                        {
                            _pendingConnections.Remove(e.Id);
                        }
                    }
                });
            }
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
                var imageUploader = new FirmwareImageUploader(FirmwareFilePath, _log);
                await imageUploader.RunAsync(false, _deviceManager, device, longOperation);
                _advIgnoreList.Ignore(device.Mac);
            }
            catch (Exception ex)
            {
                sender.CustomError = ex.Message;
            }
            finally
            {
                await _deviceManager.DisconnectDevice(device);
            }
        }

        void SelectFirmware()
        {
            if (string.IsNullOrWhiteSpace(FirmwareFilePath))
                FirmwareFilePath = "Not selected...";

            OpenFileDialog ofd = new OpenFileDialog
            {
                InitialDirectory = Path.GetDirectoryName(FirmwareFilePath),
                Filter = "Firmware Image file | *.img"
            };

            if (ofd.ShowDialog() == true)
                FirmwareFilePath = ofd.FileName;
        }

        internal void OnClosing()
        {
            SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;

            if (_restartServiceOnExit && !HideezServiceController.IsServiceRunning)
                HideezServiceController.StartService();
        }
    }
}
