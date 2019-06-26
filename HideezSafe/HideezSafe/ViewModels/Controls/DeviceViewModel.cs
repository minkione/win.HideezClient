using HideezSafe.Models;
using HideezSafe.Modules;
using HideezSafe.Modules.Localize;
using HideezSafe.Modules.ServiceProxy;
using HideezSafe.Mvvm;
using NLog;
using System.Collections.ObjectModel;
using System.Windows;

namespace HideezSafe.ViewModels
{
    public class DeviceViewModel : LocalizedObject
    {
        private readonly Device device;
        readonly ILogger _log = LogManager.GetCurrentClassLogger();
        readonly IWindowsManager _windowsManager;
        readonly IServiceProxy _serviceProxy;

        public DeviceViewModel(Device device, IWindowsManager windowsManager, IServiceProxy serviceProxy, IMenuFactory menuFactory)
        {
            _windowsManager = windowsManager;
            _serviceProxy = serviceProxy;
            this.device = device;
            device.PropertyChanged += (sender, e) => RaisePropertyChanged(e.PropertyName);

            MenuItems = new ObservableCollection<MenuItemViewModel>
            {
                menuFactory.GetMenuItem(device, MenuItemType.AddCredential),
                menuFactory.GetMenuItem(device, MenuItemType.DisconnectDevice),
                menuFactory.GetMenuItem(device, MenuItemType.RemoveDevice),
            };
        }

        #region Properties

        public string IcoKey { get; } = "HedeezKeySimpleIMG";

        [Localization]
        public string TypeName { get { return device.TypeName; } }

        public string Id => device.Id;
        public string Name => device.Name;
        public bool IsConnected => device.IsConnected;
        public double Proximity => device.Proximity;
        public int Battery => device.Battery;
        public string OwnerName => device.OwnerName;
        public string SerialNo => device.SerialNo;
        public bool IsInitializing => device.IsInitializing;
        public bool IsInitialized => device.IsInitialized;

        public ObservableCollection<MenuItemViewModel> MenuItems { get; }

        #endregion Property
    }
}
