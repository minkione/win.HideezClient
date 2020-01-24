using GalaSoft.MvvmLight.Messaging;
using HideezClient.Messages;
using HideezClient.Models;
using HideezClient.Modules.DeviceManager;
using HideezClient.Mvvm;
using MvvmExtensions.Commands;
using System;
using System.Collections.Specialized;
using System.Reflection;
using System.Windows.Input;
using System.Linq;
using HideezClient.Modules;
using HideezClient.Modules.ServiceProxy;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MvvmExtensions.Attributes;

namespace HideezClient.ViewModels
{
    class MainViewModel : ObservableObject
    {
        string currentPage = "";
        readonly IDeviceManager _deviceManager;
        readonly IMenuFactory _menuFactory;
        readonly IActiveDevice _activeDevice;
        readonly ViewModelLocator _viewModelLocator;
        readonly ISet<MenuItemViewModel> _leftAppMenuItems = new HashSet<MenuItemViewModel>();
        readonly ISet<MenuItemViewModel> _leftDeviceMenuItems = new HashSet<MenuItemViewModel>();
        Uri _displayPage;

        public MainViewModel(IDeviceManager deviceManager, IMenuFactory menuFactory, IActiveDevice activeDevice, ViewModelLocator viewModelLocator)
        {
            _deviceManager = deviceManager;
            _menuFactory = menuFactory;
            _activeDevice = activeDevice;
            _viewModelLocator = viewModelLocator;

            InitMenu();
            
            _deviceManager.DevicesCollectionChanged += Devices_CollectionChanged;
            _activeDevice.ActiveDeviceChanged += ActiveDevice_ActiveDeviceChanged;
        }

        void InitMenu()
        {
            MenuDeviceSettings = new MenuItemViewModel
            {
                Header = "Menu.DeviceSettings",
                Command = OpenDeviceSettingsPageCommand,
            };
            MenuPasswordManager = new MenuItemViewModel
            {
                Header = "Menu.PasswordManager",
                Command = OpenPasswordManagerPageCommand,
            };
            _leftDeviceMenuItems.Add(MenuDeviceSettings);
            _leftDeviceMenuItems.Add(MenuPasswordManager);

            MenuDefaultPage = new MenuItemViewModel
            {
                Header = "Menu.MenuDefaultPage",
                Command = OpenDefaultPageCommand,
                IsChecked = true,
            };
            MenuHelp = new MenuItemViewModel
            {
                Header = "Menu.Help",
                Command = OpenHelpPageCommand,
            };
            MenuSettings = new MenuItemViewModel
            {
                Header = "Menu.Settings",
                Command = OpenSettingsPageCommand,
            };
            _leftAppMenuItems.Add(MenuDefaultPage);
            _leftAppMenuItems.Add(MenuHelp);
            _leftAppMenuItems.Add(MenuSettings);

            foreach (var item in _leftDeviceMenuItems.Concat(_leftAppMenuItems))
            {
                item.PropertyChanged += Item_PropertyChanged;
            }
        }

        void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MenuItemViewModel.IsChecked) && sender is MenuItemViewModel menu && menu.IsChecked)
            {
                foreach (var item in _leftDeviceMenuItems.Concat(_leftAppMenuItems).Where(m => m != menu && m.IsChecked))
                {
                    item.IsChecked = false;
                }
            }
        }

        void Devices_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_deviceManager.Devices.Count() > 0 && MenuDefaultPage.IsChecked)
            {
                MenuDeviceSettings.IsChecked = true;
            }
            else if (!_leftAppMenuItems.Any(m => m.IsChecked))
            {
                MenuDefaultPage.IsChecked = true;
            }

            NotifyPropertyChanged(nameof(Devices));
        }

        void ActiveDevice_ActiveDeviceChanged(object sender, ActiveDeviceChangedEventArgs args)
        {
            if (args.PreviousDevice != null)
                args.PreviousDevice.PropertyChanged -= ActiveDevice_PropertyChanged;

            if (args.NewDevice != null)
                args.NewDevice.PropertyChanged += ActiveDevice_PropertyChanged;

            NotifyPropertyChanged(nameof(ActiveDevice));
        }

        void ActiveDevice_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DeviceInfoViewModel.CanShowPasswordManager))
            {
                if (ActiveDevice != null && (!ActiveDevice.CanShowPasswordManager && _leftDeviceMenuItems.Any(m => m.IsChecked)))
                {
                    MenuDeviceSettings.IsChecked = true;
                }
            }
        }

        #region Properties

        public string Version
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public Uri DisplayPage
        {
            get { return _displayPage; }
            set { Set(ref _displayPage, value); }
        }

        public DeviceInfoViewModel ActiveDevice
        {
            get 
            {
                if (_activeDevice.Device == null)
                    return null;

                return new DeviceInfoViewModel(_activeDevice.Device, _menuFactory); 
            }
        }

        [DependsOn(nameof(ActiveDevice))]
        public  List<DeviceInfoViewModel> Devices
        {
            get
            {
                // Todo: cache ViewModels instead of recreating them each time the device collection changes.
                return _deviceManager.Devices
                    .Where(d => d.Id != _activeDevice.Device.Id)
                    .Select(v => new DeviceInfoViewModel(v, _menuFactory))
                    .Reverse()
                    .ToList();
            }
        }
        #endregion Properties

        #region MenuItems

        private MenuItemViewModel menuPasswordManager;
        private MenuItemViewModel menuHelp;
        private MenuItemViewModel menuSettings;
        private MenuItemViewModel menuAboutDevice;
        private MenuItemViewModel menuDefaultPage;

        public MenuItemViewModel MenuPasswordManager
        {
            get { return menuPasswordManager; }
            set { Set(ref menuPasswordManager, value); }
        }

        public MenuItemViewModel MenuHelp
        {
            get { return menuHelp; }
            set { Set(ref menuHelp, value); }
        }

        public MenuItemViewModel MenuSettings
        {
            get { return menuSettings; }
            set { Set(ref menuSettings, value); }
        }

        public MenuItemViewModel MenuDeviceSettings
        {
            get { return menuAboutDevice; }
            set { Set(ref menuAboutDevice, value); }
        }

        public MenuItemViewModel MenuDefaultPage
        {
            get { return menuDefaultPage; }
            set { Set(ref menuDefaultPage, value); }
        }

        #endregion

        #region Command

        public ICommand OpenPasswordManagerPageCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        OnOpenPasswordManager();
                    }
                };
            }
        }

        public ICommand OpenHelpPageCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x => OnOpenHelp(),
                };
            }
        }

        public ICommand OpenSettingsPageCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x => OnOpenSettings(),
                };
            }
        }

        public ICommand OpenDeviceSettingsPageCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        OnOpenDeviceSettingsPage();
                    },
                };
            }
        }

        public ICommand OpenDefaultPageCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        OnOpenDefaultPage();
                    },
                };
            }
        }

        #endregion Command

        #region Navigation

        public void ProcessNavRequest(string page)
        {
            if (currentPage != page)
            {
                DisplayPage = new Uri($"pack://application:,,,/HideezClient;component/PagesView/{page}.xaml", UriKind.Absolute);
                currentPage = page;
            }
        }

        private void OnOpenSettings()
        {
            ProcessNavRequest("SettingsPage");
        }

        private void OnOpenHelp()
        {
            ProcessNavRequest("HelpPage");
        }

        private void OnOpenPasswordManager()
        {
            ProcessNavRequest("PasswordManagerPage");
        }

        private void OnOpenDefaultPage()
        {
            ProcessNavRequest("DefaultPage");
        }

        private void OnOpenDeviceSettingsPage()
        {
            ProcessNavRequest("DeviceSettingsPage");
        }

        #endregion
    }
}
