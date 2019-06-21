using HideezSafe.HideezServiceReference;
using HideezSafe.Modules;
using HideezSafe.Modules.Localize;
using HideezSafe.Modules.ServiceProxy;
using HideezSafe.Mvvm;
using MvvmExtensions.Commands;
using System;
using System.Windows;
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
        private string serialNo;

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

        public string SerialNo
        {
            get { return serialNo; }
            set { Set(ref serialNo, value); }
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
            SerialNo = dto.SerialNo;
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
                        windowsManager.ShowDialogAddCredential(Name, Id);
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
                        OnDisconnectDevice();
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
                        OnRemoveDevice();
                    },
                };
            }
        }

        #endregion

        private async void OnDisconnectDevice()
        {
            try
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to disconnect {Name}?", 
                    $"Disconnect {Name}", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                    await serviceProxy.GetService().DisconnectDeviceAsync(Id);
            }
            catch (Exception ex)
            {
                windowsManager.ShowError(ex.Message);
            }
        }

        private async void OnRemoveDevice()
        {
            try
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to remove {Name}?{Environment.NewLine}Note: All manually stored data will be lost!", 
                    $"Remove {Name}", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                    await serviceProxy.GetService().RemoveDeviceAsync(Id);
            }
            catch (Exception ex)
            {
                windowsManager.ShowError(ex.Message);
            }
        }

    }
}
