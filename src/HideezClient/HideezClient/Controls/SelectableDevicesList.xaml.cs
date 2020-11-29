using HideezClient.ViewModels;
using HideezClient.ViewModels.Controls;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace HideezClient.Controls
{
    /// <summary>
    /// Interaction logic for DevicesExpander.xaml
    /// </summary>
    public partial class SelectableDevicesList : UserControl
    {
        public IEnumerable<DeviceInfoViewModel> Devices
        {
            get { return (IEnumerable<DeviceInfoViewModel>)GetValue(DevicesProperty); }
            set { SetValue(DevicesProperty, value); }
        }

        public static readonly DependencyProperty DevicesProperty =
            DependencyProperty.Register(
                "Devices", 
                typeof(IEnumerable<DeviceInfoViewModel>), 
                typeof(SelectableDevicesList), 
                new PropertyMetadata(new List<DeviceInfoViewModel>())
                );

        public SelectableDevicesList()
        {
            InitializeComponent();
        }
    }
}
