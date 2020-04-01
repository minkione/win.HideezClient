using HideezClient.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace HideezClient.Controls
{
    /// <summary>
    /// Interaction logic for DeviceInfo.xaml
    /// </summary>
    public partial class DeviceInfoControl : UserControl
    {
        public VaultInfoViewModel Device
        {
            get { return (VaultInfoViewModel)GetValue(DeviceProperty); }
            set { SetValue(DeviceProperty, value); }
        }

        public static readonly DependencyProperty DeviceProperty =
            DependencyProperty.Register(
                "Device", 
                typeof(VaultInfoViewModel), 
                typeof(DeviceInfoControl), 
                new PropertyMetadata(null)
                );

        public DeviceInfoControl()
        {
            InitializeComponent();
        }
    }
}
