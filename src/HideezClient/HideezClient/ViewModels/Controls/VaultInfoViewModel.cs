using System.Windows;
using HideezClient.Models;
using HideezClient.Modules;
using HideezClient.Mvvm;

namespace HideezClient.ViewModels
{
    public class VaultInfoViewModel : LocalizedObject
    {
        private MenuItemViewModel disconnectDeviceMenu;
        private MenuItemViewModel removeDeviceMenu;
        private MenuItemViewModel authorizeDeviceAndLoadStorageMenu;
        private MenuItemViewModel setAsActiveDeviceMenu;

        public VaultInfoViewModel(IVaultModel vault, IMenuFactory menuFactory)
        {
            Vault = vault;

            DisconnectDeviceMenu = menuFactory.GetMenuItem(Vault, MenuItemType.DisconnectDevice);
            RemoveDeviceMenu = menuFactory.GetMenuItem(Vault, MenuItemType.RemoveDevice);
            AuthorizeDeviceAndLoadStorageMenu = menuFactory.GetMenuItem(Vault, MenuItemType.AuthorizeDeviceAndLoadStorage);
            SetAsActiveDeviceMenu = menuFactory.GetMenuItem(Vault, MenuItemType.SetAsActiveDevice);
        }

        #region Properties
        public IVaultModel Vault { get; }

        // TODO: Separate icon for software, hardware and file vaults. 
        // Bind to Vault then apply converter to get IcoKey based on implementation class type
        public string IcoKey { get; } = "HideezKeySimpleIMG"; 

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
