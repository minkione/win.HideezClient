using HideezClient.Models;
using HideezClient.Modules;
using HideezClient.Mvvm;
using HideezClient.ViewModels;
using MvvmExtensions.Attributes;
using MvvmExtensions.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HideezClient.Controls
{
    class PinNotVerifiedNotificationViewModel : ObservableObject
    {
        private Device device;
        private readonly IWindowsManager windowsManager;
        private readonly ViewModelLocator viewModelLocator;

        public PinNotVerifiedNotificationViewModel(IWindowsManager windowsManager, ViewModelLocator viewModelLocator)
        {
            this.windowsManager = windowsManager;
            this.viewModelLocator = viewModelLocator;
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
                        EnterPinViewModel viewModel = viewModelLocator.EnterPinViewModel;
                        viewModel.Device = Device;
                        viewModel.State = ViewPinState.WaitUserAction;
                        viewModel.ButtonState = ConfirmButtonState.None;
                        windowsManager.ShowDialogEnterPinAsync(viewModel);
                    }
                };
            }
        }
    }
}
