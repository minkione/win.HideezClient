using HideezClient.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace HideezClient.Controls
{
    /// <summary>
    /// Interaction logic for DeviceInfo.xaml
    /// </summary>
    public partial class SmallDeviceInfoControl : UserControl
    {
        public DeviceInfoViewModel Device
        {
            get { return (DeviceInfoViewModel)GetValue(DeviceProperty); }
            set { SetValue(DeviceProperty, value); }
        }

        public static readonly DependencyProperty DeviceProperty =
            DependencyProperty.Register(
                "Device", 
                typeof(DeviceInfoViewModel), 
                typeof(SmallDeviceInfoControl), 
                new PropertyMetadata(null)
                );

        public SmallDeviceInfoControl()
        {
            InitializeComponent();
        }
    }
}
