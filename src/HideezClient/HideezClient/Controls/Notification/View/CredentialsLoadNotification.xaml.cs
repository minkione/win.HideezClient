using HideezClient.Modules;
using MahApps.Metro.IconPacks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for CredentialsLoadNotification.xaml
    /// </summary>
    public partial class CredentialsLoadNotification : NotificationBase
    {
        public CredentialsLoadNotification(NotificationOptions options)
            : base(options)
        {
            InitializeComponent();

            this.DataContextChanged += CredentialsLoadNotification_DataContextChanged;
        }

        private void CredentialsLoadNotification_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(e.NewValue is INotifyPropertyChanged newNotifyPropertyChanged)
            {
                newNotifyPropertyChanged.PropertyChanged += NotifyPropertyChanged_PropertyChanged;
            }

            if(e.OldValue is INotifyPropertyChanged oldNotifyPropertyChanged)
            {
                oldNotifyPropertyChanged.PropertyChanged -= NotifyPropertyChanged_PropertyChanged;
            }
        }

        private void NotifyPropertyChanged_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "State" && sender is CredentialsLoadNotificationViewModel viewModel)
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    switch (viewModel.State)
                    {
                        case LoadedCredentialsState.Loaded:
                            this.StartTimer(TimeSpan.FromSeconds(15));
                            IcoLoadState.Kind = PackIconFontAwesomeKind.CheckCircleSolid;
                            IcoLoadState.Foreground = Brushes.Black;
                            break;
                        case LoadedCredentialsState.Fail:
                        case LoadedCredentialsState.Cancel:
                            this.Close();
                            break;
                    }
                });
            }
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
