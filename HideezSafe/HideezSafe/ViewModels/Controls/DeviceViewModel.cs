using HideezSafe.Models;
using HideezSafe.Modules;
using HideezSafe.Modules.Localize;
using HideezSafe.Modules.ServiceProxy;
using HideezSafe.Mvvm;
using NLog;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace HideezSafe.ViewModels
{
    public class DeviceForExpanderViewModel : DeviceViewModel
    {
        readonly IWindowsManager _windowsManager;

        public DeviceForExpanderViewModel(Device device, IWindowsManager windowsManager, IMenuFactory menuFactory)
            : base(device)
        {
            _windowsManager = windowsManager;

            MenuItems = new ObservableCollection<MenuItemViewModel>
            {
                menuFactory.GetMenuItem(device, MenuItemType.AddCredential),
                menuFactory.GetMenuItem(device, MenuItemType.DisconnectDevice),
                menuFactory.GetMenuItem(device, MenuItemType.RemoveDevice),
                menuFactory.GetMenuItem(device, MenuItemType.AboutDevice),
            };
        }

        #region Properties

        public string IcoKey { get; } = "HedeezKeySimpleIMG";

        [Localization]
        public string TypeName { get { return device.TypeName; } }

        public ObservableCollection<MenuItemViewModel> MenuItems { get; }

        #endregion Property
    }
}
