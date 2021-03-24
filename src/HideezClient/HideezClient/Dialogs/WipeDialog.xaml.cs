using HideezClient.ViewModels.Dialog;

namespace HideezClient.Dialogs
{
    /// <summary>
    /// Interaction logic for WipeDialog.xaml
    /// </summary>
    public partial class WipeDialog : BaseDialog
    {
        public WipeDialog(WipeViewModel vm)
        {
            InitializeComponent();

            DataContext = vm;
        }
    }
}
