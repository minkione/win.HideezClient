using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
    /// Interaction logic for LeftMenuItemControl.xaml
    /// </summary>
    public partial class LeftMenuItemControl : UserControl
    {
        public LeftMenuItemControl()
        {
            InitializeComponent();
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (DataContext is INotifyPropertyChanged)
            {
                try
                {
                    ICommand command = DataContext.GetType().GetProperty("Command").GetValue(DataContext) as ICommand;
                    if (command != null && command.CanExecute(null))
                    {
                        command.Execute(null);
                    }
                }
                catch (Exception ex)
                {
                    Debug.Assert(false);
                }
            }
        }
    }
}
