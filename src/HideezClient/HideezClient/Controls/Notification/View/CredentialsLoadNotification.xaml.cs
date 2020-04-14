using HideezClient.Modules;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

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
                Application.Current.Dispatcher.Invoke(() =>
                {
                    switch (viewModel.State)
                    {
                        case LoadedCredentialsState.Loading:
                            this.StartTimer(TimeSpan.FromSeconds(3));
                            this.Icon.Content = this.TryFindResource("IcoKeyProcess");
                            break;
                        case LoadedCredentialsState.Loaded:
                            this.StartTimer(TimeSpan.FromSeconds(3));
                            this.Icon.Content = this.TryFindResource("IconKeyFinish");
                            break;
                        case LoadedCredentialsState.Fail:
                            this.StartTimer(TimeSpan.FromSeconds(5));
                            // TODO: Add icon fail loading
                            break;
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
