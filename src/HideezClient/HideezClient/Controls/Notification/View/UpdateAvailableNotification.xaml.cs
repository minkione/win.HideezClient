using HideezClient.Modules;
using System.Windows;

namespace HideezClient.Controls
{
    /// <summary>
    /// Interaction logic for UpdateAvailableNotification.xaml
    /// </summary>
    public partial class UpdateAvailableNotification : NotificationBase
    {
        public UpdateAvailableNotification(NotificationOptions options)
            : base(options)
        {
            InitializeComponent();
            Icon.Content = this.TryFindResource("KeyIco");
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
