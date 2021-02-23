using HideezClient.ViewModels;
using System.Windows.Controls;

namespace HideezClient.Controls
{
    // Due to way WPF handles named controls inside UserControls, this control's style is defined in ResourceDictionary
    public class StandaloneModeContainer : UserControl
    {
        public StandaloneModeContainer()
        {
            var locator = (ViewModelLocator)FindResource("Locator");
            DataContext = locator.AppModeRestrictedContainerViewModel;
        }
    }
}
