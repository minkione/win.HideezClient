using HideezSafe.Modules;
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

namespace HideezSafe.Controls
{
    /// <summary>
    /// Interaction logic for SimpleNotification.xaml
    /// </summary>
    public partial class SimpleNotification : NotificationBase
    {
        public SimpleNotification(NotificationOptions options, SimpleNotificationType type)
            : base(options)
        {
            InitializeComponent();

            switch (type)
            {
                case SimpleNotificationType.Info:
                    Icon.Kind = PackIconMaterialKind.InformationOutline;
                    Icon.Foreground = Brushes.White;
                    break;
                case SimpleNotificationType.Warn:
                    Icon.Kind = PackIconMaterialKind.AlertOutline;
                    Icon.Foreground = Brushes.Yellow;
                    break;
                case SimpleNotificationType.Error:
                    Icon.Kind = PackIconMaterialKind.CloseCircleOutline;
                    Icon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff3030"));
                    break;
            }

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
