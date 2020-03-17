using HideezClient.Modules;
using MahApps.Metro.IconPacks;
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
    /// Interaction logic for SimpleNotification.xaml
    /// </summary>
    public partial class SimpleNotification : NotificationBase
    {
        public SimpleNotification(NotificationOptions options, NotificationIconType type)
            : base(options)
        {
            InitializeComponent();
            string icoKey = "";
            switch (type)
            {
                case NotificationIconType.Info:
                    icoKey = "InfoIco";
                    break;
                case NotificationIconType.Warn:
                    icoKey = "WarnIco";
                    break;
                case NotificationIconType.Error:
                    icoKey = "ErrorIco";
                    break;
                case NotificationIconType.Lock:
                    icoKey = "LockIco";
                    break;
            }

            Icon.Content = this.TryFindResource(icoKey);
            this.PreviewKeyDown += HandleEsc;
        }

        private void HandleEsc(object sender, KeyEventArgs e)
        {
            if (e.Key.ToString() == "Escape")
            {
                Close();
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            base.Close();
        }
    }
}
