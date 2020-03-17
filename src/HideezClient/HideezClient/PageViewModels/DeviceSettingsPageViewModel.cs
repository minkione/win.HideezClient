using GalaSoft.MvvmLight.Messaging;
using Hideez.SDK.Communication.Log;
using HideezClient.HideezServiceReference;
using HideezClient.Messages;
using HideezClient.Models;
using HideezClient.Modules;
using HideezClient.Modules.Log;
using HideezClient.Modules.ServiceProxy;
using HideezClient.Mvvm;
using HideezClient.ViewModels;
using MvvmExtensions.Commands;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace HideezClient.PageViewModels
{
    class DeviceSettingsPageViewModel : ReactiveObject, IWeakEventListener
    {
        readonly IServiceProxy serviceProxy;
        readonly IWindowsManager windowsManager;
        readonly IMessenger _messenger;
        readonly Logger log = LogManager.GetCurrentClassLogger(nameof(DeviceSettingsPageViewModel));

        public DeviceSettingsPageViewModel(IServiceProxy serviceProxy, IWindowsManager windowsManager, IMessenger messenger, IActiveDevice activeDevice)
        {
            this.serviceProxy = serviceProxy;
            this.windowsManager = windowsManager;
            _messenger = messenger;

            _messenger.Register<DeviceProximitySettingsChangedMessage>(this, OnDeviceProximitySettingsChanged);
            _messenger.Register<ActiveDeviceChangedMessage>(this, OnActiveDeviceChanged);

            Сonnected = new ConnectionIndicatorViewModel
            {
                Name = "Status.Device.Сonnected",
                HasConnectionText = "",
                NoConnectionText = "",
            };
            Initialized = new ConnectionIndicatorViewModel
            {
                Name = "Status.Device.Initialized",
                HasConnectionText = "",
                NoConnectionText = "",
            };
            Authorized = new ConnectionIndicatorViewModel
            {
                Name = "Status.Device.Authorized",
                HasConnectionText = "",
                NoConnectionText = "",
            };
            StorageLoaded = new ConnectionIndicatorViewModel
            {
                Name = "Status.Device.StorageLoaded",
                HasConnectionText = "",
                NoConnectionText = "",
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

                Сonnected.State = Device.IsConnected;
                Initialized.State = Device.IsInitialized;
                Authorized.State = Device.IsAuthorized;
                StorageLoaded.State = Device.IsStorageLoaded;
            });

            this.WhenAnyValue(x => x.LockProximity, x => x.UnlockProximity).Where(t => t.Item1 != 0 && t.Item2 != 0).Subscribe(o => ProximityHasChanges = true);
            this.WhenAnyValue(x => x.Device).Where(d => d != null).Subscribe(o => Task.Run(LoadCurrentProximitySettings));

            Device = activeDevice.Device != null ? new DeviceViewModel(activeDevice.Device) : null;
        }

        [Reactive] public DeviceViewModel Device { get; set; }
        [Reactive] public ConnectionIndicatorViewModel Сonnected { get; set; }
        [Reactive] public ConnectionIndicatorViewModel Initialized { get; set; }
        [Reactive] public ConnectionIndicatorViewModel Authorized { get; set; }
        [Reactive] public ConnectionIndicatorViewModel StorageLoaded { get; set; }
        [Reactive] public int LockProximity { get; set; }
        [Reactive] public int UnlockProximity { get; set; }
        [Reactive] public bool ProximityHasChanges { get; set; }
        [Reactive] public bool AllowEditProximitySettings { get; set; }


        public ObservableCollection<ConnectionIndicatorViewModel> Indicators { get; } = new ObservableCollection<ConnectionIndicatorViewModel>();

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

        public ICommand CancelEditProximityCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        Task.Run(LoadCurrentProximitySettings);
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
                        Task.Run(async () =>
                       {
                           try
                           {
                               await serviceProxy.GetService().SetProximitySettingsAsync(Device.Mac, LockProximity, UnlockProximity);
                               ProximityHasChanges = false;
                           }
                           catch (Exception ex)
                           {
                               _messenger.Send(new ShowErrorNotificationMessage("An error occured while updating proximity settings"));
                               log.WriteLine(ex);
                           }
                       });
                    }
                };
            }
        }

        #endregion

        private void OnActiveDeviceChanged(ActiveDeviceChangedMessage obj)
        {
            // Todo: ViewModel should be reused instead of being recreated each time active device is changed
            Device = obj.NewDevice != null ? new DeviceViewModel(obj.NewDevice) : null;
        }

        private void OnDeviceProximitySettingsChanged(DeviceProximitySettingsChangedMessage obj)
        {
            Task.Run(LoadCurrentProximitySettings);
        }

        private async Task LoadCurrentProximitySettings()
        {
            // TODO: Race condition and potential NullReferenceException at Device.Mac
            if (Device != null)
            {
                var dto = await this.serviceProxy.GetService().GetCurrentProximitySettingsAsync(Device.Mac);
                AllowEditProximitySettings = dto.AllowEditProximitySettings;
                LockProximity = dto.LockProximity;
                UnlockProximity = dto.UnlockProximity;
                ProximityHasChanges = false;
            }
        }

        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            // We still receive events from previous device, so this check is important
            // to filter events from device relevant/selected device only
            if (Device != null && Device == sender as DeviceViewModel)
            {
                Сonnected.State = Device.IsConnected;
                Initialized.State = Device.IsInitialized;
                Authorized.State = Device.IsAuthorized;
                StorageLoaded.State = Device.IsStorageLoaded;
            }
            return true;
        }
    }
}
