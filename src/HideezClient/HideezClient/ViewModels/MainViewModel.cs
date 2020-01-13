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

namespace HideezClient.ViewModels
{
    class MainViewModel : ObservableObject
    {
        private string currentPage = "";
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
            leftDeviceMenuItems.Add(MenuDeviceSettings);
            leftDeviceMenuItems.Add(MenuPasswordManager);

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
            leftAppMenuItems.Add(MenuDefaultPage);
            leftAppMenuItems.Add(MenuHelp);
            leftAppMenuItems.Add(MenuSettings);

            foreach (var item in leftDeviceMenuItems.Concat(leftAppMenuItems))
            {
                item.PropertyChanged += Item_PropertyChanged;
            }
        }

        private void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MenuItemViewModel.IsChecked) && sender is MenuItemViewModel menu && menu.IsChecked)
            {
                foreach (var item in leftDeviceMenuItems.Concat(leftAppMenuItems).Where(m => m != menu && m.IsChecked))
                {
                    item.IsChecked = false;
                }
            }
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
            if (device != null && SelectedDevice != null)
            {
                SelectedDevice.PropertyChanged -= Device_PropertyChanged;
                Devices.Remove(SelectedDevice);
                SelectedDevice = null;

                if (!leftAppMenuItems.Any(m => m.IsChecked))
                {
                    MenuDefaultPage.IsChecked = true;
                }
            }
        }

        private void AddedDevice(Device device)
        {
            if (device != null)
            {
                SelectedDevice = new DeviceInfoViewModel(device, menuFactory);
                Devices.Add(SelectedDevice);
                if (MenuDefaultPage.IsChecked)
                {
                    MenuDeviceSettings.IsChecked = true;
                }

                SelectedDevice.PropertyChanged += Device_PropertyChanged;
            }
        }

        private void Device_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DeviceInfoViewModel.CanShowPasswordManager))
            {
                if (SelectedDevice != null && (!SelectedDevice.CanShowPasswordManager && leftDeviceMenuItems.Any(m => m.IsChecked)))
                {
                    MenuDeviceSettings.IsChecked = true;
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
            set
            {
                if (Set(ref selectedDevice, value))
                {
                    viewModelLocator.PasswordManager.Device = selectedDevice;
                    viewModelLocator.DeviceSettingsPageViewModel.Device = selectedDevice;
                }
            }
        }

        public ObservableCollection<DeviceInfoViewModel> Devices { get; } = new ObservableCollection<DeviceInfoViewModel>();

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
