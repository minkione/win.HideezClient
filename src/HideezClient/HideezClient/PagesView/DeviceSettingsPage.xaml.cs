using HideezClient.PageViewModels;
using MahApps.Metro.IconPacks;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace HideezClient.PagesView
{
    /// <summary>
    /// Interaction logic for DeviceSettingsPage.xaml
    /// </summary>
    public partial class DeviceSettingsPage : Page
    {
        public DeviceSettingsPage()
        {
            InitializeComponent();
            this.Loaded += DeviceSettingsPage_Loaded;
            this.Unloaded += DeviceSettingsPage_Unloaded;
        }

        private void DeviceSettingsPage_Unloaded(object sender, RoutedEventArgs e)
        {
            (DataContext as DeviceSettingsPageViewModel)?.UpdateIsEditableCredentials();
        }

        private void DeviceSettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            SetFocus();
        }

        private void Page_MouseDown(object sender, MouseButtonEventArgs e)
        {
            proximityStackPanel.Focus();
        }

        private void UnlockProximityTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            int.TryParse(UnlockProximityTextBox.Text, out int value);
            if (value > 100)
                UnlockProximityTextBox.Text = "100";
            else
            {
                int.TryParse(LockProximityTextBox.Text, out int minValue);
                minValue += 20;
                if (value < minValue)
                    UnlockProximityTextBox.Text = minValue.ToString();
            }
        }

        private void LockProximityTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            int.TryParse(LockProximityTextBox.Text, out int value);
            if (value < 0)
                UnlockProximityTextBox.Text = "0";
            else
            {
                int.TryParse(UnlockProximityTextBox.Text, out int maxValue);
                maxValue -= 20;
                if (value > maxValue)
                    LockProximityTextBox.Text = maxValue.ToString();
            }
        }

        private void SetFocus()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input,
                new Action(delegate ()
                {
                    try
                    {
                        PasswordBox.Focus();            // Set Logical Focus
                        Keyboard.Focus(PasswordBox);    // Set Keyboard Focus
                    }
                    catch (Exception) { }
                }));
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            PasswordBox.Clear();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var viewModel = DataContext as DeviceSettingsPageViewModel;
                if (viewModel != null)
                    await viewModel.SaveSettings(PasswordBox.SecurePassword);

                PasswordBox.Clear();
                
                ResultText.Text = "Saved";
                ResultIcon.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.CheckOutline;
                ResultIcon.Foreground = new SolidColorBrush(Colors.Green);
            }
            catch
            {
                ResultText.Text = "Error";
                ResultIcon.Kind = PackIconMaterialKind.AlertOctagon;
                ResultIcon.SetResourceReference(Control.ForegroundProperty, "ErrorBrush");
            }
            finally
            {
                ResultStack.Visibility = Visibility.Visible;
                await Task.Delay(5000);
                ResultStack.Visibility = Visibility.Hidden;
            }
        }

        private void EditCredentialsButton_Click(object sender, RoutedEventArgs e)
        {
            PasswordBox.Clear();
            SetFocus();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            (DataContext as DeviceSettingsPageViewModel).CredentialsHasChanges = PasswordBox.Password.Length != 0; 
        }
    }
}
