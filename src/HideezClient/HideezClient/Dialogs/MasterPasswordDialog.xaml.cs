using HideezClient.ViewModels.Dialog;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HideezClient.Dialogs
{
    /// <summary>
    /// Interaction logic for MasterPasswordDialog.xaml
    /// </summary>
    public partial class MasterPasswordDialog : BaseMetroDialog
    {
        public MasterPasswordDialog(MasterPasswordViewModel vm)
        {
            InitializeComponent();

            vm.ViewModelUpdated += PasswordView_ViewModelUpdated;
            vm.PasswordsCleared += PasswordView_PasswordsCleared;
            DataContext = vm;
        }

        public event EventHandler Closed;

        public void Close()
        {
            if (Application.Current.MainWindow is MetroWindow metroWindow)
            {
                metroWindow.HideMetroDialogAsync(this);
                Closed?.Invoke(this, EventArgs.Empty);
            }
        }

        private void PasswordView_PasswordsCleared(object sender, System.EventArgs e)
        {
            if (DataContext != null)
            {
                CurrentPasswordBox.Clear();
                NewPasswordBox.Clear();
                ConfirmPasswordBox.Clear();
            }
        }

        private void CurrentPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
            {
                ((MasterPasswordViewModel)DataContext).SecureCurrentPassword = ((PasswordBox)sender).SecurePassword;
            }
        }

        private void NewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
            {
                ((MasterPasswordViewModel)DataContext).SecureNewPassword = ((PasswordBox)sender).SecurePassword;
            }
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
            {
                ((MasterPasswordViewModel)DataContext).SecureConfirmPassword = ((PasswordBox)sender).SecurePassword;
            }
        }

        void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            FocusFirstVisiblePasswordBox();
        }

        void PasswordView_ViewModelUpdated(object sender, System.EventArgs e)
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
            var passwordBoxes = new PasswordBox[] { CurrentPasswordBox, NewPasswordBox, ConfirmPasswordBox };

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
            //if (!onlyDigitsRegex.IsMatch(e.Text))
            //    e.Handled = true;
        }
    }
}
