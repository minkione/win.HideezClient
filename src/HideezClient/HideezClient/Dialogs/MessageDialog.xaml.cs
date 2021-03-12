using HideezClient.Modules.Localize;
using HideezMiddleware.Localize;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
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

namespace HideezClient.Dialogs
{
    public partial class MessageDialog : BaseMetroDialog
    {
        public MessageDialog(string icoKey, string confirmButtonTextKey = "Button.Ok", string cancelButtonTextKey = "")
        {
            InitializeComponent();

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
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MetroWindow metroWindow)
            {
                metroWindow.HideMetroDialogAsync(this);
            }
        }
    }
}
