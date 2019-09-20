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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HideezClient.PagesView
{
    /// <summary>
    /// Interaction logic for PasswordManagerPage.xaml
    /// </summary>
    public partial class PasswordManagerPage : Page
    {
        Storyboard AnimationHideAccountInfo;
        Storyboard AnimationShowAccountInfo;

        public PasswordManagerPage()
        {
            InitializeComponent();
            Loaded += PasswordManagerPage_Loaded;   
        }

        private void PasswordManagerPage_Loaded(object sender, RoutedEventArgs e)
        {
            AnimationHideAccountInfo = this.FindResource("AnimationHideAccountInfo") as Storyboard;
            AnimationShowAccountInfo = this.FindResource("AnimationShowAccountInfo") as Storyboard;
        }

        private void AccountsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AccountsList.SelectedItem == null)
            {
                AnimationShowAccountInfo.Stop();
                AnimationHideAccountInfo.Begin();
            }
            else
            {
                AnimationHideAccountInfo.Stop();
                AnimationShowAccountInfo.Begin();
            }
        }
    }
}
