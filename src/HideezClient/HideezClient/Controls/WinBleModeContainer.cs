using HideezClient.ViewModels;
using System.Windows.Controls;

namespace HideezClient.Controls
{
    public class WinBleModeContainer: UserControl
    {
        public WinBleModeContainer()
        {
            var locator = (ViewModelLocator)FindResource("Locator");
            DataContext = locator.ConnectionModeContainerViewModel;
        }
    }
}
