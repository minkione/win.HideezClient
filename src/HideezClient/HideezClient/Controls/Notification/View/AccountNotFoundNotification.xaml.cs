using HideezClient.Modules;
using System.Windows;

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
            Close();
        }
    }
}
