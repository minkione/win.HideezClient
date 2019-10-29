using System.Windows;
using Hideez.SDK.Communication.Device;

namespace WinSampleApp
{
    public partial class AccessParamsWindow : Window
    {
        public AccessParamsWindow(AccessParams vm)
        {
            DataContext = vm;
            InitializeComponent();
        }

        void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
