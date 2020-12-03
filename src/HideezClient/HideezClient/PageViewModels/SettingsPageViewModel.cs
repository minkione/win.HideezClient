using HideezClient.Mvvm;
using HideezClient.ViewModels;

namespace HideezClient.PageViewModels
{
    class SettingsPageViewModel : LocalizedObject
    {
        public ServiceViewModel Service { get; }

        public SoftwareUnlockSettingViewModel SoftwareUnlock { get; }

        public IndicatorsSettingViewModel Indicators { get; }

        public SettingsPageViewModel(ServiceViewModel serviceViewModel, SoftwareUnlockSettingViewModel softwareUnlockModuleSwitchViewModel, IndicatorsSettingViewModel indicatorsSettingViewModel)
        {
            Service = serviceViewModel;
            SoftwareUnlock = softwareUnlockModuleSwitchViewModel;
            Indicators = indicatorsSettingViewModel;
        }
    }
}
