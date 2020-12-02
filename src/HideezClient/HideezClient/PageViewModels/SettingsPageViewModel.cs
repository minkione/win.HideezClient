using HideezClient.Mvvm;
using HideezClient.ViewModels;

namespace HideezClient.PageViewModels
{
    class SettingsPageViewModel : LocalizedObject
    {
        public ServiceViewModel Service { get; }

        public SoftwareUnlockSettingViewModel SoftwareUnlock { get; }

        public SettingsPageViewModel(ServiceViewModel serviceViewModel, SoftwareUnlockSettingViewModel softwareUnlockModuleSwitchViewModel)
        {
            Service = serviceViewModel;
            SoftwareUnlock = softwareUnlockModuleSwitchViewModel;
        }
    }
}
