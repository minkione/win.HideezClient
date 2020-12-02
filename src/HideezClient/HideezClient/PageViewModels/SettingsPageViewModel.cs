using HideezClient.Mvvm;
using HideezClient.ViewModels;

namespace HideezClient.PageViewModels
{
    class SettingsPageViewModel : LocalizedObject
    {
        public ServiceViewModel Service { get; }

        public SoftwareUnlockSettingViewModel SoftwareUnlock { get; }
        // TODO: DO NOT COMMIT AND DONT FORGET TO REMOVE
        private ViewModels.Controls.WinBleDeviceManagementListViewModel _placeholderVM;

        public SettingsPageViewModel(ServiceViewModel serviceViewModel, SoftwareUnlockSettingViewModel softwareUnlockModuleSwitchViewModel, ViewModels.Controls.WinBleDeviceManagementListViewModel placeholderVM)
        {
            Service = serviceViewModel;
            SoftwareUnlock = softwareUnlockModuleSwitchViewModel;
            _placeholderVM = placeholderVM;
        }
    }
}
