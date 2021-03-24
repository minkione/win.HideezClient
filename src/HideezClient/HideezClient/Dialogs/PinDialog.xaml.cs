using HideezClient.ViewModels;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HideezClient.Dialogs
{
    /// <summary>
    /// Interaction logic for PinDialog.xaml
    /// </summary>
    public partial class PinDialog : BaseDialog
    {
        readonly Regex onlyDigitsRegex = new Regex("[0-9]+");

        public PinDialog(PinViewModel vm)
        {
            InitializeComponent();

            vm.ViewModelUpdated += PinView_ViewModelUpdated;
            vm.PasswordsCleared += PinView_PasswordsCleared;
            Closed += PinDialog_Closed;
            DataContext = vm;
        }

        private void PinDialog_Closed(object sender, System.EventArgs e)
        {
            (DataContext as PinViewModel).OnClose();
        }

        private void PinView_PasswordsCleared(object sender, System.EventArgs e)
        {
            if (DataContext != null)
            {
                CurrentPinPasswordBox.Clear();
                NewPinPasswordBox.Clear();
                ConfirmPinPasswordBox.Clear();
            }
        }

        private void CurrentPinPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
            {
                ((PinViewModel)DataContext).SecureCurrentPin = ((PasswordBox)sender).SecurePassword;
            }
        }

        private void NewPinPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
            {
                ((PinViewModel)DataContext).SecureNewPin = ((PasswordBox)sender).SecurePassword;
            }
        }

        private void ConfirmPinPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
            {
                ((PinViewModel)DataContext).SecureConfirmPin = ((PasswordBox)sender).SecurePassword;
            }
        }

        void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            FocusFirstVisiblePasswordBox();
        }

        void PinView_ViewModelUpdated(object sender, System.EventArgs e)
        {
            FocusFirstVisiblePasswordBox();
        }

        void PasswordBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            FocusFirstVisiblePasswordBox();
        }

        void FocusFirstVisiblePasswordBox()
        {
            // Set focus to the first password box that is visible
            var passwordBoxes = new PasswordBox[] { CurrentPinPasswordBox, NewPinPasswordBox, ConfirmPinPasswordBox };

            foreach (var pb in passwordBoxes)
            {
                if (pb.IsVisible)
                {
                    pb.Focusable = true;
                    FocusManager.SetFocusedElement(this, pb);
                    pb.Focus();
                    break;
                }
            }
        }

        void PasswordBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Ignore all entered symbols except digits
            if (!onlyDigitsRegex.IsMatch(e.Text))
                e.Handled = true;
        }
    }
}
