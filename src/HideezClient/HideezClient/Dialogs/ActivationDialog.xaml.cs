using HideezClient.ViewModels;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HideezClient.Dialogs
{
    /// <summary>
    /// Interaction logic for ActivationDialog.xaml
    /// </summary>
    public partial class ActivationDialog : BaseMetroDialog
    {
        readonly Regex onlyDigitsRegex = new Regex("[0-9]+");

        public ActivationDialog(ActivationViewModel vm)
        {
            InitializeComponent();

            vm.ViewModelUpdated += ViewModel_ViewModelUpdated;
            vm.PasswordsCleared += ViewModel_PasswordsCleared;
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

        void ViewModel_ViewModelUpdated(object sender, EventArgs e)
        {
            FocusPasswordBox();
        }

        void ViewModel_PasswordsCleared(object sender, EventArgs e)
        {
            if (DataContext != null)
            {
                CodePasswordBox.Clear();
            }
        }

        void CodePasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
            {
                ((ActivationViewModel)DataContext).SecureActivationCode = ((PasswordBox)sender).SecurePassword;
            }
        }

        void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            FocusPasswordBox();
        }

        void PasswordBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            FocusPasswordBox();
        }

        void FocusPasswordBox()
        {
            if (CodePasswordBox.IsVisible)
            {
                CodePasswordBox.Focusable = true;
                FocusManager.SetFocusedElement(this, CodePasswordBox);
                CodePasswordBox.Focus();
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
