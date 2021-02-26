using DeviceMaintenance.Messages;
using DeviceMaintenance.Service;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Connection;
using Hideez.SDK.Communication.Log;
using Meta.Lib.Modules.PubSub;
using Microsoft.Win32;
using MvvmExtensions.Attributes;
using MvvmExtensions.PropertyChangedMonitoring;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DeviceMaintenance.ViewModel
{
    public class MainWindowViewModel : PropertyChangedImplementation
    {
        const string FW_FILE_EXTENSION = "img";
            
        readonly EventLogger _log;
        readonly MetaPubSub _hub;

        bool _automaticallyUploadFirmware = Properties.Settings.Default.AutomaticallyUpload;
        bool _csrUpdate = true;
        bool _winBleUpdate = false;
        string _fileName = Properties.Settings.Default.FirmwareFileName;

        readonly ConcurrentDictionary<string, DeviceViewModel> _devices =
            new ConcurrentDictionary<string, DeviceViewModel>();

        int _advConnectionInterlock = 0;

        public IEnumerable<DeviceViewModel> Devices => _devices.Values.OrderByDescending(x => x.CreatedAt);
        public HideezServiceController HideezServiceController { get; }
        public ConnectionManagerViewModel ConnectionManager { get; }


        #region Properties

        public string Title
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly().GetName();
                return $"{assembly.Name} v{assembly.Version}";
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

        [DependsOn(nameof(FirmwareFilePath))]
        public bool IsFirmwareSelected
        {
            get
            {
                    return !string.IsNullOrWhiteSpace(FirmwareFilePath) &&
                            FirmwareFilePath.EndsWith($".{FW_FILE_EXTENSION}");
            }
        }

        [DependsOn(nameof(IsFirmwareSelected), nameof(IsCsrEnabled))]
        public bool IsTapEnabled
        {
            get
            {
                return IsFirmwareSelected && IsCsrEnabled;
            }
        }

        [DependsOn(nameof(IsFirmwareSelected), nameof(IsWinBleEnabled))]
        public bool IsWinBleUpdateAvailable
        {
            get
            {
                return IsFirmwareSelected && IsWinBleEnabled;
            }
        }

        public bool AutomaticallyUploadFirmware
        {
            get
            {
                return _automaticallyUploadFirmware;
            }
            set
            {
                if (_automaticallyUploadFirmware != value)
                {
                    _automaticallyUploadFirmware = value;
                    Properties.Settings.Default.AutomaticallyUpload = AutomaticallyUploadFirmware;
                    Properties.Settings.Default.Save();
                    NotifyPropertyChanged();
                }
            }
        }

        public bool IsCsrEnabled
        {
            get
            {
                return _csrUpdate;
            }
            set
            {
                if (_csrUpdate != value)
                {
                    if (value)
                    {
                        _devices.Clear();
                        NotifyPropertyChanged(nameof(Devices));
                        ConnectionManager.Initialize(DefaultConnectionIdProvider.Csr);
                    }
                    _csrUpdate = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool IsWinBleEnabled
        {
            get
            {
                return _winBleUpdate;
            }
            set
            {
                if (_winBleUpdate != value)
                {
                    if (value)
                    {
                        _devices.Clear();
                        NotifyPropertyChanged(nameof(Devices));
                        ConnectionManager.Initialize(DefaultConnectionIdProvider.WinBle);

                        AutomaticallyUploadFirmware = false;
                    }
                    _winBleUpdate = value;
                    NotifyPropertyChanged();
                }
            }
        }

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

        public MainWindowViewModel(MetaPubSub hub)
        {
            // default value -33 is to much and picks up devices from very far
            SdkConfig.TapProximityUnlockThreshold = -29;
            SdkConfig.ConnectDeviceTimeout = 8_000;
            SdkConfig.DeviceInitializationTimeout = 5_000;

            _hub = hub;
            _log = new EventLogger("Maintenance");

            _hub.Subscribe<AdvertismentReceivedEvent>(OnAdvertismentReceived);
            _hub.Subscribe<ControllerAddedEvent>(OnControllerAdded);
            _hub.Subscribe<DeviceConnectedEvent>(OnDeviceConnected);
            _hub.Subscribe<ClosingEvent>(OnClosing);

            ConnectionManager = new ConnectionManagerViewModel(_log, _hub);
            ConnectionManager.Initialize(DefaultConnectionIdProvider.Csr);
            HideezServiceController = new HideezServiceController(_log, _hub);

            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;

            if (IsFirmwareSelected)
                _hub.Publish(new StartDiscoveryCommand());
        }

        Task OnClosing(ClosingEvent arg)
        {
            SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;
            return Task.CompletedTask;
        }

        // WinBle only stuff
        async Task OnControllerAdded(ControllerAddedEvent arg)
        {
            await CreateDeviceViewModel(arg.ConnectionId);
        }

        // CSR only stuff
        async Task OnAdvertismentReceived(AdvertismentReceivedEvent arg)
        {
            var connectionId = new ConnectionId(arg.DeviceId, (byte)DefaultConnectionIdProvider.Csr);
            await CreateDeviceViewModel(connectionId);
        }

        async Task CreateDeviceViewModel(ConnectionId connectionId)
        {
            if (Interlocked.CompareExchange(ref _advConnectionInterlock, 1, 0) == 0)
            {
                DeviceViewModel deviceViewModel = null;
                try
                {
                    bool added = false;
                    deviceViewModel = _devices.GetOrAdd(connectionId.Id, (id) =>
                    {
                        added = true;
                        bool isBonded = ConnectionManager.IsBonded(id);
                        return new DeviceViewModel(connectionId, isBonded, _hub);
                    });

                    if (added)
                    {
                        NotifyPropertyChanged(nameof(Devices));
                    }

                    await deviceViewModel.TryConnect();
                }
                catch (Exception ex)
                {
                    if (deviceViewModel != null)
                        deviceViewModel.CustomError = ex.Message;
                }
                finally
                {
                    Interlocked.Exchange(ref _advConnectionInterlock, 0);
                }
            }
        }

        Task OnDeviceConnected(DeviceConnectedEvent arg)
        {
            if (IsFirmwareSelected && (AutomaticallyUploadFirmware || arg.DeviceViewModel.IsBoot))
                return arg.DeviceViewModel.StartFirmwareUpdate(FirmwareFilePath);

            return Task.CompletedTask;
        }

        void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (e.Reason == SessionSwitchReason.SessionLock ||
                e.Reason == SessionSwitchReason.SessionLogoff ||
                e.Reason == SessionSwitchReason.SessionUnlock ||
                e.Reason == SessionSwitchReason.SessionLogon)
            {
                AutomaticallyUploadFirmware = false;
            }
        }

        void SelectFirmware()
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                InitialDirectory = string.IsNullOrWhiteSpace(FirmwareFilePath) ? string.Empty : Path.GetDirectoryName(FirmwareFilePath),
                Filter = "Firmware Image file | *.img"
            };

            if (ofd.ShowDialog() == true)
                FirmwareFilePath = ofd.FileName;

            if (IsFirmwareSelected)
                _hub.Publish(new StartDiscoveryCommand());
        }

    }
}
