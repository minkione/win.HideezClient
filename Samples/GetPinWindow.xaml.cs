using System;
using System.Windows;
using WinSampleApp.ViewModel;

namespace WinSampleApp
{
    public partial class GetPinWindow : Window
    {
        private readonly Action<string, string> _onPinReceived;
        private readonly GetPinViewModel _vm;

        public GetPinWindow(GetPinViewModel vm, Action<string, string> onPinReceived) 
        {
            _onPinReceived = onPinReceived;
            _vm = vm;
            DataContext = vm;
            InitializeComponent();
        }

        void Button_Click(object sender, RoutedEventArgs e)
        {
            _onPinReceived(_vm.Pin, _vm.OldPin);
            //DialogResult = true;
        }
    }
}
