using MahApps.Metro.Controls;
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
using System.Windows.Shapes;

namespace HideezSafe.Views
{
    /// <summary>
    /// Interaction logic for AddCredential.xaml
    /// </summary>
    public partial class AddCredentialView : MetroWindow
    {
        public AddCredentialView()
        {
            InitializeComponent();
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                if (passwordBox.SecurePassword.Length > 0)
                {
                    passwordBox.BorderBrush = null;
                }
                else
                {
                    passwordBox.BorderBrush = passwordBox.TryFindResource("ErrorBorderBrush") as Brush;
                }
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (sender is ComboBox comboBox)
            {
                if (string.IsNullOrWhiteSpace(comboBox.SelectedItem as string))
                {
                    comboBox.BorderBrush = passwordBox.TryFindResource("ErrorBorderBrush") as Brush;
                }
                else
                {
                    comboBox.BorderBrush = null;
                }
            }
        }
    }
}
