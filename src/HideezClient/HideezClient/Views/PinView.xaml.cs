using HideezClient.Controls;
using HideezClient.Extension;
using HideezClient.Modules.HotkeyManager.BondTech.HotKeyManagement;
using HideezClient.Utilities;
using HideezClient.ViewModels;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for ConfirmPinView.xaml
    /// </summary>
    public partial class PinView : MetroWindow
    {
        public PinView()
        {
            InitializeComponent();
            this.DataContextChanged += PinView_DataContextChanged;
        }

        private void PinView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is EnterPinViewModel enterViewModel)
            {
                containerWaitUserAction.Children.Add(new EnterPin());
            }
            else if (e.NewValue is SetPinViewModel setViewModel)
            {
                containerWaitUserAction.Children.Add(new SetPin());
            }
            else if (e.NewValue is ChangePinViewModel changeViewModel)
            {
                containerWaitUserAction.Children.Add(new ChangePin());
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            containerWaitUserAction.Children.OfType<IDisposable>().ToList().ForEach(d => d.Dispose());
        }

        private void Button_Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                IInputElement focusedControl = Keyboard.FocusedElement;
                button.Focus();
                focusedControl?.Focus();
            }
        }
    }
}
