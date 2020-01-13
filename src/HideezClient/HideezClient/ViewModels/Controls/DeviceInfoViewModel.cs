using System.Windows;
using HideezClient.Models;
using HideezClient.Modules;
using HideezClient.Modules.Localize;

namespace HideezClient.ViewModels
{
    public class DeviceInfoViewModel : DeviceViewModel
    {
        private MenuItemViewModel disconnectDeviceMenu;
        private MenuItemViewModel removeDeviceMenu;
        private MenuItemViewModel authorizeDeviceAndLoadStorageMenu;

        public DeviceInfoViewModel(Device device, IMenuFactory menuFactory)
            : base(device)
        {
            DisconnectDeviceMenu = menuFactory.GetMenuItem(device, MenuItemType.DisconnectDevice);
            RemoveDeviceMenu = menuFactory.GetMenuItem(device, MenuItemType.RemoveDevice);
            AuthorizeDeviceAndLoadStorageMenu = menuFactory.GetMenuItem(device, MenuItemType.AuthorizeDeviceAndLoadStorage);
        }

        #region Properties

        public string IcoKey { get; } = "HideezKeySimpleIMG";

        [Localization]
        public string TypeName { get { return device.TypeName; } }

        public MenuItemViewModel DisconnectDeviceMenu
        {
            get { return disconnectDeviceMenu; }
            set { Set(ref disconnectDeviceMenu, value); }
        }

        public MenuItemViewModel RemoveDeviceMenu
        {
            get { return removeDeviceMenu; }
            set { Set(ref removeDeviceMenu, value); }
        }

        public MenuItemViewModel AuthorizeDeviceAndLoadStorageMenu
        {
            get { return authorizeDeviceAndLoadStorageMenu; }
            set { Set(ref authorizeDeviceAndLoadStorageMenu, value); }
        }

        #endregion Property
    }
}
