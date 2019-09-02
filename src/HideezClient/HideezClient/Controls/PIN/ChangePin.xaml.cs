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
    /// Interaction logic for ChangePin.xaml
    /// </summary>
    public partial class ChangePin : UserControl, IDisposable
    {
        public ChangePin()
        {
            InitializeComponent();
            Loaded += EnterPin_Loaded;
            DataContextChanged += EnterPin_DataContextChanged;
        }

        private void EnterPin_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            BindingOperations.ClearBinding(changePinOld, ChangePin.ChangePinOldProperty);
            BindingOperations.ClearBinding(changePinNew, ChangePin.ChangePinNeProperty);
            BindingOperations.ClearBinding(changePinConfirm, ChangePin.ChangePinConfirmProperty);


            Binding changePinOldBinding = new Binding(nameof(ChangePinViewModel.ChangePinOld));
            changePinOldBinding.Source = DataContext;
            changePinOldBinding.ValidatesOnDataErrors = true;
            BindingOperations.SetBinding(changePinOld, ChangePin.ChangePinOldProperty, changePinOldBinding);

            Binding changePinNewBinding = new Binding(nameof(ChangePinViewModel.ChangePinNew));
            changePinNewBinding.Source = DataContext;
            changePinNewBinding.ValidatesOnDataErrors = true;
            BindingOperations.SetBinding(changePinNew, ChangePin.ChangePinNeProperty, changePinNewBinding);

            Binding changePinConfirmBinding = new Binding(nameof(ChangePinViewModel.ChangePinConfirm));
            changePinConfirmBinding.Source = DataContext;
            changePinConfirmBinding.ValidatesOnDataErrors = true;
            BindingOperations.SetBinding(changePinConfirm, ChangePin.ChangePinConfirmProperty, changePinConfirmBinding);
        }

        public static readonly DependencyProperty ChangePinOldProperty = DependencyProperty.Register("ChangePinOld", typeof(SecureString), typeof(ChangePin));
        public static readonly DependencyProperty ChangePinNeProperty = DependencyProperty.Register("ChangePinNew", typeof(SecureString), typeof(ChangePin));
        public static readonly DependencyProperty ChangePinConfirmProperty = DependencyProperty.Register("ChangePinConfirm", typeof(SecureString), typeof(ChangePin));

        private void EnterPin_Loaded(object sender, RoutedEventArgs e)
        {
            changePinOld.Focus();
        }

        private void Pin_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox && DataContext is ChangePinViewModel viewModel)
            {
                switch (passwordBox.Name)
                {
                    case "changePinOld":
                        viewModel.ChangePinOld = passwordBox.SecurePassword;
                        break;
                    case "changePinNew":
                        viewModel.ChangePinNew = passwordBox.SecurePassword;
                        break;
                    case "changePinConfirm":
                        viewModel.ChangePinConfirm = passwordBox.SecurePassword;
                        break;
                }
            }
        }

        public void Dispose()
        {
            changePinOld.SecurePassword.Clear();
            changePinOld.Clear();
            changePinNew.SecurePassword.Clear();
            changePinNew.Clear();
            changePinConfirm.SecurePassword.Clear();
            changePinConfirm.Clear();
        }
    }
}
