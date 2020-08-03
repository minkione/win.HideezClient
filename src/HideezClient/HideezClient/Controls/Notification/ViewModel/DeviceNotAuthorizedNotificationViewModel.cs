using HideezClient.Models;
using HideezClient.Modules;
using HideezClient.Mvvm;
using MvvmExtensions.Attributes;
using MvvmExtensions.Commands;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HideezClient.Controls
{
    class DeviceNotAuthorizedNotificationViewModel : ObservableObject
    {
        readonly IWindowsManager _windowsManager;

        Device device;

        public DeviceNotAuthorizedNotificationViewModel(IWindowsManager windowsManager)
        {
            _windowsManager = windowsManager;
        }

        public Device Device
        {
            get { return device; }
            set
            {
                if (value != device)
                {
                    Set(ref device, value);
                }
            }
        }

        [DependsOn(nameof(Device))]
        public string DeviceSN
        {
            get { return Device?.SerialNo; }
        }

        public ICommand OpenLinkCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        Task.Run(async () => { await device.InitRemoteAndLoadStorageAsync(); });
                        _windowsManager.CloseWindow(ObservableId); // Todo: Is this the correct way to close notification?
                    }
                };
            }
        }
    }
}
