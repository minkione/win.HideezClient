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
    public partial class BackupPasswordDialog : BaseMetroDialog
    {
        public BackupPasswordDialog(BackupPasswordViewModel vm)
        {
            InitializeComponent();

            vm.ViewModelUpdated += PasswordView_ViewModelUpdated;
            vm.PasswordsCleared += PasswordView_PasswordsCleared;
            DataContext = vm;
        }

        public event EventHandler Closed;

        public void SetResult(bool isSuccessful, string errorMessage)
        {

            ((BackupPasswordViewModel)DataContext).InProgress = false;
            progressStack.Visibility = Visibility.Hidden;

            if (isSuccessful)
            {
                successfulResultStack.Visibility = Visibility.Visible;
                if (DataContext is BackupPasswordViewModel viewModel && viewModel.IsNewPassword)
                    openFolderButton.Visibility = Visibility.Visible;
            }
            else
            {
                failedResultStack.Visibility = Visibility.Visible;
                if (!string.IsNullOrWhiteSpace(errorMessage))
                    errorMessageText.Text = errorMessage;
            }

        }

        public void SetProgress(string message)
        {
            progressText.Text = message;
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
            if (Application.Current.MainWindow is MetroWindow metroWindow)
            {
                metroWindow.HideMetroDialogAsync(this);
                Closed?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
