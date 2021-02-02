using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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

                        AccountName.Focus();            // Set Logical Focus
                        Keyboard.Focus(AccountName);    // Set Keyboard Focus
                    }
                    catch (Exception) { }
                }));
        }
    }
}
