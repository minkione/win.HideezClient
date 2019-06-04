using MahApps.Metro.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
                    passwordBox.ClearValue(BorderBrushProperty);
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
                    comboBox.ClearValue(BorderBrushProperty);
                }
            }
        }
    }
}
