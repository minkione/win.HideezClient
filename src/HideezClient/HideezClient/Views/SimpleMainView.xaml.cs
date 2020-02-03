using MahApps.Metro.Controls;
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
using System.Windows.Shapes;

namespace HideezClient.Views
{
    /// <summary>
    /// Interaction logic for SimpleMainWindow.xaml
    /// </summary>
    public partial class SimpleMainView : MetroWindow
    {
        public SimpleMainView()
        {
            InitializeComponent();
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Visibility = Visibility.Hidden;
            e.Cancel = true;
        }

        void MetroWindow_StateChanged(object sender, EventArgs e)
        {
            // When changing state from minimized to normal, this case ensures that window is properly resized
            if (ActualHeight > 0)
                InvalidateVisual();
        }
    }
}
