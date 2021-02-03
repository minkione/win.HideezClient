using Hideez.SDK.Communication.Log;
using HideezClient.Messages;
using HideezClient.Modules;
using HideezClient.Modules.Log;
using HideezClient.Modules.ServiceProxy;
using HideezClient.ViewModels;
using HideezClient.ViewModels.Controls;
using HideezMiddleware.Settings;
using Meta.Lib.Modules.PubSub;
using MvvmExtensions.Commands;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HideezClient.PageViewModels
{
    class DeviceSettingsPageViewModel : ReactiveObject, IWeakEventListener
    {
        readonly IServiceProxy serviceProxy;
        readonly IWindowsManager windowsManager;
        readonly Logger log = LogManager.GetCurrentClassLogger(nameof(DeviceSettingsPageViewModel));
        readonly ISettingsManager<UserProximitySettings> _proximitySettingsManager;
        readonly IMetaPubSub _metaMessenger;

        public DeviceSettingsPageViewModel(
            IServiceProxy serviceProxy,
            IWindowsManager windowsManager,
            IActiveDevice activeDevice,
            ISettingsManager<UserProximitySettings> proximitySettingsManager,
            IMetaPubSub metaMessenger)
        {
            this.serviceProxy = serviceProxy;
            this.windowsManager = windowsManager;
            _proximitySettingsManager = proximitySettingsManager;
            _metaMessenger = metaMessenger;

            _metaMessenger.Subscribe<ActiveDeviceChangedMessage>(OnActiveDeviceChanged);

            Сonnected = new StateControlViewModel
            {
                Name = "Status.Device.Сonnected",
                Visible = true,
            };
            Initialized = new StateControlViewModel
            {
                Name = "Status.Device.Initialized",
                Visible = true,
            };
            Authorized = new StateControlViewModel
            {
                Name = "Status.Device.Authorized",
                Visible = true,
            };
            StorageLoaded = new StateControlViewModel
            {
                Name = "Status.Device.StorageLoaded",
                Visible = true,
            };

            Indicators.Add(Сonnected);
            Indicators.Add(Initialized);
            Indicators.Add(Authorized);
            Indicators.Add(StorageLoaded);

            this.WhenAnyValue(x => x.Device).Where(d => d != null).Subscribe(d =>
            {
                PropertyChangedEventManager.AddListener(Device, this, nameof(Device.IsConnected));
                PropertyChangedEventManager.AddListener(Device, this, nameof(Device.IsInitialized));
                PropertyChangedEventManager.AddListener(Device, this, nameof(Device.IsAuthorized));
                PropertyChangedEventManager.AddListener(Device, this, nameof(Device.IsStorageLoaded));

                Сonnected.State = StateControlViewModel.BoolToState(Device.IsConnected);
                Initialized.State = StateControlViewModel.BoolToState(Device.IsInitialized);
                Authorized.State = StateControlViewModel.BoolToState(Device.IsAuthorized);
                StorageLoaded.State = StateControlViewModel.BoolToState(Device.IsStorageLoaded);
            });

            this.WhenAnyValue(x => x.LockProximity, x => x.UnlockProximity).Where(t => t.Item1 != 0 && t.Item2 != 0).Subscribe(o => ProximityHasChanges = true);

            Device = activeDevice.Device != null ? new DeviceViewModel(activeDevice.Device) : null;

            _proximitySettingsManager.LoadSettings();
            TryLoadProximitySettings();
        }

        [Reactive] public DeviceViewModel Device { get; set; }
        [Reactive] public StateControlViewModel Сonnected { get; set; }
        [Reactive] public StateControlViewModel Initialized { get; set; }
        [Reactive] public StateControlViewModel Authorized { get; set; }
        [Reactive] public StateControlViewModel StorageLoaded { get; set; }
        [Reactive] public int LockProximity { get; set; }
        [Reactive] public int UnlockProximity { get; set; }
        [Reactive] public bool EnabledLockByProximity { get; set; }
        [Reactive] public bool EnabledUnlockByProximity { get; set; }
        [Reactive] public bool DisabledDisplayAuto { get; set; }
        [Reactive] public bool ProximityHasChanges { get; set; }
        [Reactive] public bool AllowEditProximitySettings { get; set; }


        public ObservableCollection<StateControlViewModel> Indicators { get; } = new ObservableCollection<StateControlViewModel>();

        #region Command

        public ICommand SelectCSVFileCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {

                    }
                };
            }
        }

        public ICommand ExportCredentialsCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {

                    }
                };
            }
        }

        public ICommand SaveProximityCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        SaveOrUpdateSettings();
                    }
                };
            }
        }

        #endregion

        private Task OnActiveDeviceChanged(ActiveDeviceChangedMessage obj)
        {
            // Todo: ViewModel should be reused instead of being recreated each time active device is changed
            Device = obj.NewDevice != null ? new DeviceViewModel(obj.NewDevice) : null;

            return Task.CompletedTask;
        }

        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            // We still receive events from previous device, so this check is important
            // to filter events from device relevant/selected device only
            if (Device != null && Device == sender as DeviceViewModel)
            {
                Сonnected.State = StateControlViewModel.BoolToState(Device.IsConnected);
                Initialized.State = StateControlViewModel.BoolToState(Device.IsInitialized);
                Authorized.State = StateControlViewModel.BoolToState(Device.IsAuthorized);
                StorageLoaded.State = StateControlViewModel.BoolToState(Device.IsStorageLoaded);
            }
            return true;
        }

        void TryLoadProximitySettings()
        {
            try
            {
                var settings = _proximitySettingsManager.Settings;
                var deviceProximitySettings = settings.GetProximitySettings(Device.Id);
                LockProximity = deviceProximitySettings.LockProximity;
                UnlockProximity = deviceProximitySettings.UnlockProximity;
                EnabledLockByProximity = deviceProximitySettings.EnabledLockByProximity;
                EnabledUnlockByProximity = deviceProximitySettings.EnabledUnlockByProximity;
                DisabledDisplayAuto = deviceProximitySettings.DisabledDisplayAuto;
            }
            catch(Exception ex)
            {
                log.WriteLine($"Failed proximity settings loading: {ex.Message}");
            }
        }

        void SaveOrUpdateSettings()
        {
            var settings = _proximitySettingsManager.Settings;
            var deviceSettings = settings.DevicesProximity.FirstOrDefault(s => s.Id == Device.Id);
            if(deviceSettings!= null)
            {
                deviceSettings.LockProximity = LockProximity;
                deviceSettings.UnlockProximity = UnlockProximity;
                deviceSettings.EnabledLockByProximity = EnabledLockByProximity;
                deviceSettings.EnabledUnlockByProximity = EnabledUnlockByProximity;
                deviceSettings.DisabledDisplayAuto = DisabledDisplayAuto;
            }
            else
            {
                var devicesSettings = settings.DevicesProximity.ToList();
                var newSettings = UserDeviceProximitySettings.DefaultSettings;
                newSettings.Id = Device.Id;
                newSettings.DisabledDisplayAuto = DisabledDisplayAuto;
                newSettings.EnabledLockByProximity = EnabledLockByProximity;
                newSettings.EnabledUnlockByProximity = EnabledUnlockByProximity;
                newSettings.LockProximity = LockProximity;
                newSettings.UnlockProximity = UnlockProximity;
                devicesSettings.Add(newSettings);
                settings.DevicesProximity = devicesSettings.ToArray();
            }
            _proximitySettingsManager.SaveSettings(settings);
        }
    }
}
