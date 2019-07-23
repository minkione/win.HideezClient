using System.ComponentModel;
using System.Windows;
using DeviceMaintenance.ViewModel;

namespace DeviceMaintenance
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }

        void Window_Closing(object sender, CancelEventArgs e)
        {
            var vm = (MainWindowViewModel)DataContext;
            vm.Close();
        }
    }
}
