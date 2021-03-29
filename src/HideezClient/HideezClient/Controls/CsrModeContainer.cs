using HideezClient.ViewModels;
using System.Windows.Controls;

namespace HideezClient.Controls
{
    public class CsrModeContainer: UserControl
    {
        public CsrModeContainer()
        {
            var locator = (ViewModelLocator)FindResource("Locator");
            DataContext = locator.ConnectionModeContainerViewModel;
        }
    }
}
