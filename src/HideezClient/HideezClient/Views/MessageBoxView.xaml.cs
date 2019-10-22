using HideezClient.Modules.Localize;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

namespace HideezClient.Views
{
    /// <summary>
    /// Interaction logic for MessageBoxView.xaml
    /// </summary>
    public partial class MessageBoxView : MetroWindow
    {
        public MessageBoxView(string icoKey, string confirmButtonTextKey = "Button.Ok", string cancelButtonTextKey = "")
        {
            InitializeComponent();

            this.Title = $"Hideez Client ({Assembly.GetExecutingAssembly().GetName().Version.ToString()})";

            this.IcoContainer.Content = this.TryFindResource(icoKey);

            if (!string.IsNullOrWhiteSpace(confirmButtonTextKey))
            {
                BindingOperations.SetBinding(this.ConfirmButton, Button.ContentProperty, new LocalizationExtension(confirmButtonTextKey));
                this.ConfirmButton.Visibility = Visibility.Visible;
            }

            if (!string.IsNullOrWhiteSpace(cancelButtonTextKey))
            {
                BindingOperations.SetBinding(this.CancelButton, Button.ContentProperty, new LocalizationExtension(cancelButtonTextKey));
                this.CancelButton.Visibility = Visibility.Visible;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
