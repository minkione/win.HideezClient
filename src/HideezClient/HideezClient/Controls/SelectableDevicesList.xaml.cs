using HideezClient.ViewModels;
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
        public IEnumerable<VaultInfoViewModel> Devices
        {
            get { return (IEnumerable<VaultInfoViewModel>)GetValue(DevicesProperty); }
            set { SetValue(DevicesProperty, value); }
        }

        public static readonly DependencyProperty DevicesProperty =
            DependencyProperty.Register(
                "Devices", 
                typeof(IEnumerable<VaultInfoViewModel>), 
                typeof(SelectableDevicesList), 
                new PropertyMetadata(new List<VaultInfoViewModel>())
                );

        public SelectableDevicesList()
        {
            InitializeComponent();
        }
    }
}
