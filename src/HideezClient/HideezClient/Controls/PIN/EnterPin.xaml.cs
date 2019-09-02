using HideezClient.Mvvm;
using HideezClient.ViewModels;
using HideezClient.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security;
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
    /// Interaction logic for EnterPin.xaml
    /// </summary>
    public partial class EnterPin : UserControl, IDisposable
    {
        private BindingRaiseevent<ViewPinState> bindingRaiseevent;

        public EnterPin()
        {
            InitializeComponent();
            Loaded += EnterPin_Loaded;
            DataContextChanged += EnterPin_DataContextChanged;
        }

        private void EnterPin_Loaded(object sender, RoutedEventArgs e)
        {
            enterPin.Focus();
        }

        private void EnterPin_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            BindingOperations.ClearBinding(enterPin, EnterPin.SecurePinProperty);

            Binding enterPinBinding = new Binding(nameof(EnterPinViewModel.EnterPin));
            enterPinBinding.Source = DataContext;
            enterPinBinding.ValidatesOnDataErrors = true;
            BindingOperations.SetBinding(enterPin, EnterPin.SecurePinProperty, enterPinBinding);

            bindingRaiseevent = new BindingRaiseevent<ViewPinState>(DataContext, nameof(PinViewModelBase.State));
            bindingRaiseevent.ValueChanged += BindingRaiseevent_ValueChanged;
        }

        private void BindingRaiseevent_ValueChanged(ViewPinState obj)
        {
            if (obj == ViewPinState.WaitUserAction)
            {
                enterPin.Focus();
            }
        }

        public static readonly DependencyProperty SecurePinProperty = DependencyProperty.Register("SecurePin", typeof(SecureString), typeof(EnterPin));

        private void EnterPin_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox && DataContext is EnterPinViewModel viewModel)
            {
                viewModel.EnterPin = passwordBox.SecurePassword;
            }
        }

        public void Dispose()
        {
            enterPin.SecurePassword.Clear();
            enterPin.Clear();
        }
    }
}
