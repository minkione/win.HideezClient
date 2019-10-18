using HideezClient.HideezServiceReference;
using HideezClient.Modules;
using HideezClient.Modules.ServiceProxy;
using HideezClient.Mvvm;
using HideezClient.ViewModels;
using MvvmExtensions.Commands;
using NLog;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HideezClient.PageViewModels
{
    class DeviceSettingsPageViewModel : ReactiveObject, IWeakEventListener
    {
        private readonly IServiceProxy serviceProxy;
        private readonly IWindowsManager windowsManager;
        protected readonly ILogger log = LogManager.GetCurrentClassLogger();

        public DeviceSettingsPageViewModel(IServiceProxy serviceProxy, IWindowsManager windowsManager)
        {
            this.serviceProxy = serviceProxy;
            this.windowsManager = windowsManager;

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

            this.WhenAnyValue(x => x.LockProximity, x => x.UnlockProximity).Subscribe(o => ProximityHasChanges = true);
        }

        [Reactive] public DeviceViewModel Device { get; set; }
        [Reactive] public ConnectionIndicatorViewModel Сonnected { get; set; }
        [Reactive] public ConnectionIndicatorViewModel Initialized { get; set; }
        [Reactive] public ConnectionIndicatorViewModel Authorized { get; set; }
        [Reactive] public ConnectionIndicatorViewModel StorageLoaded { get; set; }
        [Reactive] public int LockProximity { get; set; } = 35;
        [Reactive] public int UnlockProximity { get; set; } = 70;
        [Reactive] public bool ProximityHasChanges { get; set; }
        [Reactive] public bool CanChangeProximitySettings { get; set; } = true;


        public ObservableCollection<ConnectionIndicatorViewModel> Indicators { get; } = new ObservableCollection<ConnectionIndicatorViewModel>();

        #region Command

        public ICommand CancelEditProximityCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        LockProximity = 35;
                        UnlockProximity = 70;
                        ProximityHasChanges = false;
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
                               windowsManager.ShowError("Error seva proximity settings.");
                               log.Error(ex);
                           }
                       });
                    }
                };
            }
        }

        #endregion

        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            Сonnected.State = Device.IsConnected;
            Initialized.State = Device.IsInitialized;
            Authorized.State = Device.IsAuthorized;
            StorageLoaded.State = Device.IsStorageLoaded;
            return true;
        }
    }
}
