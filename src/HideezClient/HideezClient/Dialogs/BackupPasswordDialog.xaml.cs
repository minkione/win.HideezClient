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
    /// Interaction logic for BackupPasswordDialog.xaml
    /// </summary>
    public partial class BackupPasswordDialog : BaseDialog
    {
        public BackupPasswordDialog(BackupPasswordViewModel vm)
        {
            InitializeComponent();

            vm.ViewModelUpdated += PasswordView_ViewModelUpdated;
            vm.PasswordsCleared += PasswordView_PasswordsCleared;
            Closed += Dialog_Closed;
            DataContext = vm;
        }

        private void Dialog_Closed(object sender, System.EventArgs e)
        {
            (DataContext as BackupPasswordViewModel).OnClose();
        }

        private void PasswordView_PasswordsCleared(object sender, System.EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (DataContext != null)
                {
                    CurrentPasswordBox.Clear();
                    NewPasswordBox.Clear();
                    ConfirmPasswordBox.Clear();
                }
            });
        }

        private void CurrentPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
            {
                ((BackupPasswordViewModel)DataContext).SecureCurrentPassword = ((PasswordBox)sender).SecurePassword;
            }
        }

        private void NewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
            {
                ((BackupPasswordViewModel)DataContext).SecureNewPassword = ((PasswordBox)sender).SecurePassword;
            }
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
            {
                ((BackupPasswordViewModel)DataContext).SecureConfirmPassword = ((PasswordBox)sender).SecurePassword;
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
