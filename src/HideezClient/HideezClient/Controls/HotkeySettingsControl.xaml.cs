using HideezClient.ViewModels;
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

namespace HideezClient.Controls
{
    /// <summary>
    /// Interaction logic for HotkeySettingsControl.xaml
    /// </summary>
    public partial class HotkeySettingsControl : UserControl
    {
        public event EventHandler<MouseWheelEventArgs> MouseWheelOverListView;
        public HotkeySettingsControl()
        {
            InitializeComponent();
        }

        private async void HotKeyControl_GotFocus(object sender, RoutedEventArgs e)
        {
            await (DataContext as HotkeySettingsViewModel).ChangeHotkeyManagerState(false);
        }

        private async void HotKeyControl_LostFocus(object sender, RoutedEventArgs e)
        {
            await (DataContext as HotkeySettingsViewModel).ChangeHotkeyManagerState(true);
        }

        private void ListView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if(!IsComboBoxDropDownOpened())
                MouseWheelOverListView?.Invoke(this, e);
        }

        List<Control> AllChildren(DependencyObject parent)
        {
            var list = new List<Control>();
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is Control control)
                    list.Add(control);
                list.AddRange(AllChildren(child));
            }
            return list;
        }

        bool IsComboBoxDropDownOpened()
        {
            foreach(var listViewItem in listView.Items)
            {
                var container = listView.ItemContainerGenerator.ContainerFromItem(listViewItem);
                var children = AllChildren(container);
                var comboBoxes = children.Where(c => c is ComboBox).Select(c=>c as ComboBox).ToList();
                foreach(var comboBox in comboBoxes)
                {
                    if (comboBox.IsDropDownOpen)
                        return true;
                }
            }

            return false;
        }

        private async void ComboBox_DropDownOpened(object sender, EventArgs e)
        {
            await(DataContext as HotkeySettingsViewModel).ChangeHotkeyManagerState(false);
        }

        private async void ComboBox_DropDownClosed(object sender, EventArgs e)
        {
            await(DataContext as HotkeySettingsViewModel).ChangeHotkeyManagerState(true);
        }
    }
}
