using System.Windows;
using HideezClient.Models;
using HideezClient.Modules;
using HideezClient.Modules.Localize;

namespace HideezClient.ViewModels
{
    public class VaultInfoViewModel : VaultViewModel
    {
        private MenuItemViewModel disconnectDeviceMenu;
        private MenuItemViewModel removeDeviceMenu;
        private MenuItemViewModel authorizeDeviceAndLoadStorageMenu;
        private MenuItemViewModel setAsActiveDeviceMenu;

        public VaultInfoViewModel(HardwareVaultModel device, IMenuFactory menuFactory)
            : base(device)
        {
            DisconnectDeviceMenu = menuFactory.GetMenuItem(device, MenuItemType.DisconnectDevice);
            RemoveDeviceMenu = menuFactory.GetMenuItem(device, MenuItemType.RemoveDevice);
            AuthorizeDeviceAndLoadStorageMenu = menuFactory.GetMenuItem(device, MenuItemType.AuthorizeDeviceAndLoadStorage);
            SetAsActiveDeviceMenu = menuFactory.GetMenuItem(device, MenuItemType.SetAsActiveDevice);
        }

        #region Properties

        public string IcoKey { get; } = "HideezKeySimpleIMG";

        [Localization]
        public string TypeName { get { return _vault.TypeName; } }

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

        public MenuItemViewModel SetAsActiveDeviceMenu
        {
            get { return setAsActiveDeviceMenu; }
            set { Set(ref setAsActiveDeviceMenu, value); }
        }
        #endregion Property
    }
}
