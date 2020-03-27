using HideezClient.Modules;
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

namespace HideezClient.Controls
{
    /// <summary>
    /// Interaction logic for AccountNotFoundNotification.xaml
    /// </summary>
    public partial class AccountNotFoundNotification : NotificationBase
    {
        public AccountNotFoundNotification(NotificationOptions options)
            : base(options)
        {
            InitializeComponent();
            Icon.Content = this.TryFindResource("InfoIco");
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            Confirm();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Cancel();
        }

        private void Confirm()
        {
            Options.TaskCompletionSource?.TrySetResult(true);
            Close();
        }

        private void Cancel()
        {
            base.Close();
        }
    }
}
