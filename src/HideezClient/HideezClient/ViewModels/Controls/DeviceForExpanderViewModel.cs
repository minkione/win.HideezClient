using HideezClient.Models;
using HideezClient.Modules;
using HideezClient.Modules.Localize;
using HideezClient.Modules.ServiceProxy;
using HideezClient.Mvvm;
using MvvmExtensions.Commands;
using NLog;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace HideezClient.ViewModels
{
    class DeviceForExpanderViewModel : DeviceViewModel
    {
        readonly ViewModelLocator _viewModelLocator;
        readonly IWindowsManager _windowsManager;

        public DeviceForExpanderViewModel(Device device, IWindowsManager windowsManager, IMenuFactory menuFactory, ViewModelLocator viewModelLocator)
            : base(device)
        {
            _windowsManager = windowsManager;
            _viewModelLocator = viewModelLocator;

            MenuItems = new ObservableCollection<MenuItemViewModel>
            {
                menuFactory.GetMenuItem(device, MenuItemType.AddCredential),
                menuFactory.GetMenuItem(device, MenuItemType.AuthorizeDeviceAndLoadStorage),
                menuFactory.GetMenuItem(device, MenuItemType.DisconnectDevice),
                menuFactory.GetMenuItem(device, MenuItemType.RemoveDevice),
                menuFactory.GetMenuItem(device, MenuItemType.AboutDevice),
            };
        }

        #region Properties

        public string IcoKey { get; } = "HideezKeySimpleIMG";

        [Localization]
        public string TypeName { get { return device.TypeName; } }

        public ObservableCollection<MenuItemViewModel> MenuItems { get; }

        public int? CountAccounts
        {
            get
            {
                if (device?.PasswordManager != null)
                {
                    return device.PasswordManager.Accounts.Count;
                }
                else
                {
                    return null;
                }
            }
        }

        #endregion Property


        #region Command

        public ICommand OpenPasswordManagerCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        _viewModelLocator.PasswordManager.Device = device;
                        _windowsManager.OpenPage("PasswordManagerPage");
                    },
                };
            }
        }

        #endregion
    }
}
