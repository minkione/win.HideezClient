using HideezClient.Modules;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HideezClient.Controls
{
    /// <summary>
    /// Interaction logic for AccountSelector.xaml
    /// </summary>
    public partial class AccountSelector : NotificationBase
    {
        public AccountSelector(NotificationOptions options)
            : base(options)
        {
            InitializeComponent();
        }

        private void AccountsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SelectedAccount();
        }

        private void AccountsList_KeyDown(object sender, KeyEventArgs e)
        {
            if ((int)e.Key == 6) // Key.Enter, Key.Return
            {
                SelectedAccount();
            }
            else if ((int)e.Key == 13) // Key.Escape
            {
                Cancel();
            }
        }

        private void AccountsList_Loaded(object sender, RoutedEventArgs e)
        {
            ListViewItem item = AccountsList.ItemContainerGenerator.ContainerFromIndex(AccountsList.SelectedIndex) as ListViewItem;
            item?.Focus();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Cancel();
        }

        private void SelectedAccount()
        {
            Options.TaskCompletionSource?.TrySetResult(true);
            Close();
        }

        private void Cancel()
        {
            Options.TaskCompletionSource?.TrySetCanceled();
            Close();
        }
    }
}
