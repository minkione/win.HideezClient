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

namespace HideezClient.PagesView
{
    /// <summary>
    /// Interaction logic for LockSettingsPage.xaml
    /// </summary>
    public partial class LockSettingsPage : Page
    {
        public LockSettingsPage()
        {
            InitializeComponent();
        }

        private void ProximitySlider_LayoutUpdated(object sender, EventArgs e)
        {
            this.ProximitySlider.MinRangeWidth = this.ProximitySlider.ActualWidth / 5;
        }

        private void ProximitySlider_UpperValueChanged(object sender, MahApps.Metro.Controls.RangeParameterChangedEventArgs e)
        {
            //this.scaleRadius.CenterX = -this.ProximitySlider.UpperValue;
            //this.scaleRadius.CenterY = -this.ProximitySlider.UpperValue;

            //this.scaleRadius.ScaleX = this.ProximitySlider.UpperValue / 3.45;
            //this.scaleRadius.ScaleY = this.ProximitySlider.UpperValue / 3.45;
        }
    }
}
