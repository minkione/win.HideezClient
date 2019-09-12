using HideezClient.ViewModels;
using MahApps.Metro.Controls;
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
        public PinView()
        {
            InitializeComponent();
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

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            FocusFirstVisiblePasswordBox();
        }

        private void PasswordBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
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
                    FocusManager.SetFocusedElement(this, pb);
                    break;
                }
            }
        }
    }
}
