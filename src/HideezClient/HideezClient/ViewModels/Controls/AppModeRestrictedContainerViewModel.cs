using HideezClient.Mvvm;
using HideezMiddleware.ApplicationModeProvider;

namespace HideezClient.ViewModels.Controls
{
    internal sealed class AppModeRestrictedContainerViewModel : LocalizedObject
    {
        readonly ApplicationMode _applicationMode;

        public bool IsEnterprise
        {
            get
            {
                return _applicationMode == ApplicationMode.Enterprise;
            }
        }

        public bool IsStandalone
        {
            get
            {
                return _applicationMode == ApplicationMode.Standalone;
            }
        }

        public AppModeRestrictedContainerViewModel(IApplicationModeProvider applicationModeProvider)
        {
            _applicationMode = applicationModeProvider.GetApplicationMode();
        }
    }
}
