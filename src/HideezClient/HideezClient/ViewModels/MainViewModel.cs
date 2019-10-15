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

namespace HideezClient.ViewModels
{
    class MainViewModel : ObservableObject
    {
        private readonly IDeviceManager deviceManager;
        private readonly IMenuFactory menuFactory;
        private readonly ViewModelLocator viewModelLocator;
        private readonly ISet<MenuItemViewModel> leftAppMenuItems = new HashSet<MenuItemViewModel>();
        private readonly ISet<MenuItemViewModel> leftDeviceMenuItems = new HashSet<MenuItemViewModel>();

        public MainViewModel(IDeviceManager deviceManager, IMenuFactory menuFactory, ViewModelLocator viewModelLocator)
        {
            this.deviceManager = deviceManager;
            this.menuFactory = menuFactory;
            this.viewModelLocator = viewModelLocator;

            InitMenu();

            AddedDevice(deviceManager.Devices.FirstOrDefault());
            deviceManager.DevicesCollectionChanged += Devices_CollectionChanged;
        }

        private void InitMenu()
        {
            MenuAboutDevice = new MenuItemViewModel
            {
                Header = "Menu.AboutDevice",
                Command = OpenAboutDevicePageCommand,
            };
            MenuPasswordManager = new MenuItemViewModel
            {
                Header = "Menu.PasswordManager",
                Command = OpenPasswordManagerPageCommand,
            };
            leftDeviceMenuItems.Add(MenuAboutDevice);
            leftDeviceMenuItems.Add(MenuPasswordManager);

            MenuDefaultPage = new MenuItemViewModel
            {
                Header = "Menu.MenuDefaultPage",
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
            leftAppMenuItems.Add(MenuDefaultPage);
            leftAppMenuItems.Add(MenuHelp);
            leftAppMenuItems.Add(MenuSettings);
        }

        private void Devices_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            App.Current.Dispatcher.Invoke((System.Action)(() =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    AddedDevice(deviceManager.Devices.FirstOrDefault());
                }
                else if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    RemovedDevice(e.OldItems[0] as Device);
                }
            }));
        }

        private void RemovedDevice(Device device)
        {
            SelectedDevice = null;
            UpdateMenu();
            if (device != null)
            {
                device.PropertyChanged -= Device_PropertyChanged;
            }
        }

        private void AddedDevice(Device device)
        {
            if (device != null)
            {
                SelectedDevice = new DeviceInfoViewModel(device, menuFactory);
                viewModelLocator.AboutDevicePageViewModel.Device = SelectedDevice;
                device.PropertyChanged += Device_PropertyChanged;
            }

            UpdateMenu();
        }

        private void Device_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Device.IsConnected) || e.PropertyName == nameof(Device.IsAuthorized) || e.PropertyName == nameof(Device.IsStorageLoaded))
            {
                UpdateMenu();
            }
        }

        private void UpdateMenu()
        {
            if (SelectedDevice == null)
            {
                if (leftDeviceMenuItems.Any(m => m.IsChecked) || !leftAppMenuItems.Any(m => m.IsChecked))
                {
                    MenuDefaultPage.IsChecked = true;
                    OnOpenDefaultPage();
                }
            }
            else 
            {
                if (MenuDefaultPage.IsChecked || (MenuPasswordManager.IsChecked && !SelectedDevice.IsAuthorized))
                {
                    MenuAboutDevice.IsChecked = true;
                    OnOpenAboutDevicePage();
                }
            }
        }

        #region Properties

        private Uri displayPage;
        private DeviceInfoViewModel selectedDevice;

        public string Version
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public Uri DisplayPage
        {
            get { return displayPage; }
            set { Set(ref displayPage, value); }
        }

        public DeviceInfoViewModel SelectedDevice
        {
            get { return selectedDevice; }
            set { Set(ref selectedDevice, value); }
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

        public MenuItemViewModel MenuAboutDevice
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
                        viewModelLocator.PasswordManager.Device = SelectedDevice;
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

        public ICommand OpenAboutDevicePageCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        OnOpenAboutDevicePage();
                    },
                };
            }
        }

        #endregion Command

        #region Navigation

        public void ProcessNavRequest(string page)
        {
            DisplayPage = new Uri($"pack://application:,,,/HideezClient;component/PagesView/{page}.xaml", UriKind.Absolute);
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

        private void OnOpenAboutDevicePage()
        {
            ProcessNavRequest("AboutDevicePage");
        }

        #endregion
    }
}
