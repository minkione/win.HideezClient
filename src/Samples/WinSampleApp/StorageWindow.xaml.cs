using System.Windows;
using Hideez.SDK.Communication.Log;
using WinSampleApp.ViewModel;

namespace WinSampleApp
{
    public partial class StorageWindow : Window
    {
        public StorageWindow(DeviceViewModel currentDevice, EventLogger log)
        {
            DataContext = new StorageViewModel(currentDevice, log);
            InitializeComponent();
        }

    }
}
