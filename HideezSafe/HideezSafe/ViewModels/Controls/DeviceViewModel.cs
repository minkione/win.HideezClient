using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Remote;
using HideezSafe.HideezServiceReference;
using HideezSafe.Models;
using HideezSafe.Modules;
using HideezSafe.Modules.Localize;
using HideezSafe.Modules.ServiceProxy;
using HideezSafe.Mvvm;
using MvvmExtensions.Commands;
using NLog;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

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

        public string Id
        {
            get { return device.Id; }
        }

        public string Name
        {
            get { return device.Name; }
        }

        public bool IsConnected
        {
            get { return device.IsConnected; }
        }

        public double Proximity
        {
            get { return device.Proximity; }
        }

        public int Battery
        {
            get { return device.Battery; }
        }

        public string OwnerName
        {
            get { return device.OwnerName; }
        }

        public string SerialNo
        {
            get { return device.SerialNo; }
        }

        public bool IsInitializing
        {
            get { return device.IsInitializing; }
        }

        public bool IsInitialized
        {
            get { return device.IsInitialized; }
        }

        public ObservableCollection<MenuItemViewModel> MenuItems { get; }

        #endregion Property
    }
}
