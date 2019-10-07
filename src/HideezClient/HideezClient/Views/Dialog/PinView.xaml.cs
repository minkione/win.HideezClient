using HideezClient.ViewModels;
using MahApps.Metro.Controls;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HideezClient.Views
{
    /// <summary>
    /// Interaction logic for ConfirmPinView.xaml
    /// </summary>
    public partial class PinView : MetroWindow
    {
        readonly Regex onlyDigitsRegex = new Regex("[0-9]+");

        public PinView()
        {
            InitializeComponent();

            if (DataContext != null)
            {
                ((PinViewModel)DataContext).ViewModelUpdated += PinView_ViewModelUpdated;
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
            ActivateWindowAndBringToTop();
        }

        void PinView_ViewModelUpdated(object sender, System.EventArgs e)
        {
            FocusFirstVisiblePasswordBox();
            ActivateWindowAndBringToTop();
        }

        void PasswordBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            FocusFirstVisiblePasswordBox();
            ActivateWindowAndBringToTop();
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

        void ActivateWindowAndBringToTop()
        {
            if (WindowState == WindowState.Minimized)
                WindowState = WindowState.Normal;

            Activate();

            Topmost = true;
            Topmost = false;

            Focus();
        }

        void PasswordBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Ignore all entered symbols except digits
            if (!onlyDigitsRegex.IsMatch(e.Text))
                e.Handled = true;
        }
    }
}
