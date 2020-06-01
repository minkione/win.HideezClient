using HideezClient.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace HideezClient.Controls
{
    /// <summary>
    /// Interaction logic for ServerAddressEditControl.xaml
    /// </summary>
    public partial class ServerAddressEditControl : UserControl
    {
        public ServerAddressEditControl()
        {
            InitializeComponent();
            (DataContext as ServerAddressEditControlViewModel).PropertyChanged += ServerAddressEditControl_PropertyChanged;
        }

        private void ServerAddressEditControl_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ServerAddressEditControlViewModel.CheckingConnection) ||
                e.PropertyName == nameof(ServerAddressEditControlViewModel.ServerAddress))
            {
                Dispatcher.Invoke(CommandManager.InvalidateRequerySuggested);
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (SaveButton.Command.CanExecute(null))
                    SaveButton.Command.Execute(null);

                e.Handled = true;
            }
        }
    }
}
