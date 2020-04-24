using HideezClient.Mvvm;
using HideezClient.ViewModels;

namespace HideezClient.PageViewModels
{
    class SettingsPageViewModel : LocalizedObject
    {
        public ServiceViewModel Service { get; }

        public SettingsPageViewModel(ServiceViewModel serviceViewModel)
        {
            Service = serviceViewModel;
        }
    }
}
