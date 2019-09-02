using HideezClient.ViewModels;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for SetPin.xaml
    /// </summary>
    public partial class SetPin : UserControl, IDisposable
    {
        public SetPin()
        {
            InitializeComponent();
            Loaded += EnterPin_Loaded;
            DataContextChanged += EnterPin_DataContextChanged;
        }

        private void EnterPin_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            BindingOperations.ClearBinding(setPin, SetPin.SetPinProperty);
            BindingOperations.ClearBinding(setPinConfirm, SetPin.SetPinConfirmProperty);

            Binding setPinBinding = new Binding(nameof(SetPinViewModel.SetPin));
            setPinBinding.Source = DataContext;
            setPinBinding.ValidatesOnDataErrors = true;
            BindingOperations.SetBinding(setPin, SetPin.SetPinProperty, setPinBinding);

            Binding setPinConfirmBinding = new Binding(nameof(SetPinViewModel.SetPinConfirm));
            setPinConfirmBinding.Source = DataContext;
            setPinConfirmBinding.ValidatesOnDataErrors = true;
            BindingOperations.SetBinding(setPinConfirm, SetPin.SetPinConfirmProperty, setPinConfirmBinding);
        }

        public static readonly DependencyProperty SetPinProperty = DependencyProperty.Register("SetPin", typeof(SecureString), typeof(SetPin));
        public static readonly DependencyProperty SetPinConfirmProperty = DependencyProperty.Register("SetPinConfirm", typeof(SecureString), typeof(SetPin));

        private void EnterPin_Loaded(object sender, RoutedEventArgs e)
        {
            setPin.Focus();
        }


        private void Pin_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox && DataContext is SetPinViewModel viewModel)
            {
                switch (passwordBox.Name)
                {
                    case "setPin":
                        viewModel.SetPin = passwordBox.SecurePassword;
                        break;
                    case "setPinConfirm":
                        viewModel.SetPinConfirm = passwordBox.SecurePassword;
                        break;
                }
            }
        }

        public void Dispose()
        {
            setPin.SecurePassword.Clear();
            setPin.Clear();

            setPinConfirm.SecurePassword.Clear();
            setPinConfirm.Clear();
        }
    }
}
