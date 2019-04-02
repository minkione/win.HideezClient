using HideezSafe.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HideezSafe.Controls
{
    /// <summary>
    /// Interaction logic for DevicesExpander.xaml
    /// </summary>
    public partial class DevicesExpander : UserControl
    {
        public DevicesExpander()
        {
            InitializeComponent();
        }

        public DeviceViewModel CurrentDevice
        {
            get { return (DeviceViewModel)GetValue(CurrentDeviceProperty); }
            set { SetValue(CurrentDeviceProperty, value); }
        }

        public static readonly DependencyProperty CurrentDeviceProperty =
            DependencyProperty.Register(nameof(CurrentDevice), typeof(DeviceViewModel), typeof(DevicesExpander), new PropertyMetadata(CurrentDevicePropertyChangedCallback));

        private static void CurrentDevicePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DevicesExpander expander)
            {
                InitInfoDevice(expander, e.NewValue as DeviceViewModel);
            }
        }

        private static void InitInfoDevice(DevicesExpander expander, DeviceViewModel device)
        {
            if (device != null)
            {
                expander.deviceType.Text = device.TypeName;
                expander.deviceName.Text = device.Name;
                object icon = expander.FindResource(device.IcoKey);
                expander.deviceIco.Content = icon;
                expander.connectionQuality.Visibility = Visibility.Visible;
            }
            else
            {
                expander.deviceType.Text = "";
                expander.deviceName.Text = expander.NoConnectedDebiceText;
                expander.deviceIco.Content = null;
                expander.connectionQuality.Visibility = Visibility.Collapsed;
            }
        }


        public string NoConnectedDebiceText
        {
            get { return (string)GetValue(NoConnectedDebiceTextProperty); }
            set { SetValue(NoConnectedDebiceTextProperty, value); }
        }

        public static readonly DependencyProperty NoConnectedDebiceTextProperty =
            DependencyProperty.Register(nameof(NoConnectedDebiceText), typeof(string), typeof(DevicesExpander)
                , new PropertyMetadata(PropertyNoConnectedDebiceTextChangedCallback));

        private static void PropertyNoConnectedDebiceTextChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DevicesExpander expander)
            {
                InitInfoDevice(expander, expander.CurrentDevice);
            }
        }
    }
}
