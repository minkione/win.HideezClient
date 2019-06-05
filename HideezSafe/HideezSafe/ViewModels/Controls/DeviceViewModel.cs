using HideezSafe.HideezServiceReference;
using HideezSafe.Modules;
using HideezSafe.Modules.Localize;
using HideezSafe.Modules.ServiceProxy;
using HideezSafe.Mvvm;
using MvvmExtensions.Commands;
using System;
using System.Windows.Input;

namespace HideezSafe.ViewModels
{
    public class DeviceViewModel : LocalizedObject
    {
        readonly IWindowsManager windowsManager;
        readonly IServiceProxy serviceProxy;

        public DeviceViewModel(DeviceDTO device, IWindowsManager windowsManager, IServiceProxy serviceProxy)
        {
            this.windowsManager = windowsManager;
            this.serviceProxy = serviceProxy;
            LoadFrom(device);
        }

        #region Property

        private string id;
        private bool isConnected;
        private double proximity;

        public string IcoKey { get; } = "HedeezKeySimpleIMG";

        public string Id
        {
            get { return id; }
            set { Set(ref id, value); }
        }

        public bool IsConnected
        {
            get { return isConnected; }
            set
            {
                Set(ref isConnected, value);
                if (!isConnected)
                {
                    Proximity = 0;
                }
            }
        }

        public double Proximity
        {
            get { return proximity; }
            set { Set(ref proximity, value); }
        }

        #region Text

        private string name;
        private string typeNameKey = "Hideez key";
        private string ownerName;

        public string OwnerName
        {
            get { return ownerName; }
            set { Set(ref ownerName, value); }
        }

        [Localization]
        public string Name
        {
            get { return name; }
            set { Set(ref name, value); }
        }

        [Localization]
        public string TypeName
        {
            get { return L(typeNameKey); }
            set { Set(ref typeNameKey, value); }
        }

        #endregion Text

        #endregion Property

        public void LoadFrom(DeviceDTO dto)
        {
            Id = dto.Id;
            Name = dto.Name;
            Proximity = dto.Proximity;
            OwnerName = dto.Owner ?? "...unspecified...";
            this.IsConnected = dto.IsConnected;
        }

        #region Command

        public ICommand AddCredentialCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        windowsManager.ShowDialogAddCredential(this.Name, this.Id);
                    },
                };
            }
        }

        public ICommand DisconnectDeviceCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        try
                        {
                            serviceProxy.GetService().DisconnectDevice(Id);
                        }
                        catch (Exception) { }
                    },
                };
            }
        }

        public ICommand RemoveDeviceCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        try
                        {
                            serviceProxy.GetService().RemoveDevice(Id);
                        }
                        catch (Exception) { }
                    },
                };
            }
        }

        #endregion
    }
}
