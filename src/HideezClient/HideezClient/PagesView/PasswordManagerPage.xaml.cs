using HideezClient.Mvvm;
using HideezClient.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private Storyboard AnimationHideAccountInfo;
        private Storyboard AnimationShowAccountInfo;
        private BindingRaiseevent bindingEditAccount;

        public PasswordManagerPage()
        {
            this.DataContextChanged += PasswordManagerPage_DataContextChanged;
            InitializeComponent();
            AccountsList.Loaded += AccountsList_Loaded;
        }

        private void PasswordManagerPage_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            bindingEditAccount = new BindingRaiseevent(DataContext, "EditAccount");
            bindingEditAccount.ValueChanged += obj =>
            {
                try
                {
                    this.PasswordBox.Clear();
                    App.Current.Dispatcher.Invoke(AccountName.Focus);
                }
                catch { }
            }; 
        }

        private void AccountsList_Loaded(object sender, RoutedEventArgs e)
        {
            AnimationHideAccountInfo = this.FindResource("AnimationHideAccountInfo") as Storyboard;
            AnimationShowAccountInfo = this.FindResource("AnimationShowAccountInfo") as Storyboard;

            UpdateAnimation();
        }

        private void AccountsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateAnimation();
        }

        private void UpdateAnimation()
        {
            if (AccountsList.SelectedItem == null)
            {
                AnimationShowAccountInfo?.Stop();
                AnimationHideAccountInfo?.Begin();
            }
            else
            {
                AnimationHideAccountInfo?.Stop();
                AnimationShowAccountInfo?.Begin();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PasswordBox.Clear();
        }
    }
}
