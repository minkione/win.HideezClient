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
    /// Interaction logic for EditAccountControl.xaml
    /// </summary>
    public partial class EditAccountControl : UserControl
    {
        public EditAccountControl()
        {
            InitializeComponent();
            this.DataContextChanged += EditAccountControl_DataContextChanged;
        }

        private void EditAccountControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.PasswordBox.Clear();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PasswordBox.Clear();
        }
    }
}
