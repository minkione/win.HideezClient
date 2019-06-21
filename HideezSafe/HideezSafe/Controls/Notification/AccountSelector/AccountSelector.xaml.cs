using HideezSafe.Modules;
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

namespace HideezSafe.Controls
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
            Result = true;
        }

        private void AccountsList_KeyDown(object sender, KeyEventArgs e)
        {
            if ((int)e.Key == 6)
            {
                Result = true;        
            }
        }

        //private void AccountsList_Loaded(object sender, RoutedEventArgs e)
        //{
        //    ListViewItem item = AccountsList.ItemContainerGenerator.ContainerFromIndex(0) as ListViewItem;
        //    item?.Focus();
        //}

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);

            ListViewItem item = AccountsList.ItemContainerGenerator.ContainerFromIndex(AccountsList.SelectedIndex) as ListViewItem;
            item?.Focus();
        }
    }
}
