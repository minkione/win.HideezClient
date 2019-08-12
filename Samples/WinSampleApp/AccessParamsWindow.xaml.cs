using System.Windows;
using Hideez.SDK.Communication.Command;
using WinSampleApp.ViewModel;

namespace WinSampleApp
{
    /// <summary>
    /// Interaction logic for AccessParamsWindow.xaml
    /// </summary>
    public partial class AccessParamsWindow : Window
    {
        public AccessParamsWindow(AccessParams vm)
        {
            this.DataContext = vm;
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
