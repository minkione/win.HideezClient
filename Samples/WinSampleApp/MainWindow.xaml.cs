using System.Windows;
using WinSampleApp.ViewModel;

namespace WinSampleApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }

        void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var vm = (MainWindowViewModel)DataContext;
            vm.Close();

            Properties.Settings.Default.Save();
        }
    }
}
