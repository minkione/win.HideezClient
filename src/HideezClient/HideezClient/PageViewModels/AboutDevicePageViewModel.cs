using HideezClient.Mvvm;
using HideezClient.ViewModels;
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

namespace HideezClient.PageViewModels
{
    class AboutDevicePageViewModel : ReactiveObject, IWeakEventListener
    {
        public AboutDevicePageViewModel()
        {
            Сonnected = new ConnectionIndicatorViewModel
            {
                Name = "Сonnected",
                HasConnectionText = "Status.Tooltip.ConectedService",
                NoConnectionText = "Status.Tooltip.DisconectedService",
            };
            Initialized = new ConnectionIndicatorViewModel
            {
                Name = "Initialized",
                HasConnectionText = "Status.Tooltip.ConectedService",
                NoConnectionText = "Status.Tooltip.DisconectedService",
            };
            Authorized = new ConnectionIndicatorViewModel
            {
                Name = "Authorized",
                HasConnectionText = "Status.Tooltip.ConectedService",
                NoConnectionText = "Status.Tooltip.DisconectedService",
            };
            StorageLoaded = new ConnectionIndicatorViewModel
            {
                Name = "Storage loaded",
                HasConnectionText = "Status.Tooltip.ConectedService",
                NoConnectionText = "Status.Tooltip.DisconectedService",
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
        }

        [Reactive] public DeviceViewModel Device { get; set; }
        [Reactive] public ConnectionIndicatorViewModel Сonnected { get; set; }
        [Reactive] public ConnectionIndicatorViewModel Initialized { get; set; }
        [Reactive] public ConnectionIndicatorViewModel Authorized { get; set; }
        [Reactive] public ConnectionIndicatorViewModel StorageLoaded { get; set; }


        public ObservableCollection<ConnectionIndicatorViewModel> Indicators { get; } = new ObservableCollection<ConnectionIndicatorViewModel>();

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
