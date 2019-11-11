using System;
using System.Windows;

namespace WinSampleApp
{
    public partial class GetPinWindow : Window
    {
        private readonly Action<string, string> _onPinReceived;

        public GetPinWindow(Action<string, string> onPinReceived) 
        {
            _onPinReceived = onPinReceived;
            InitializeComponent();
            myPin.Focus();
        }

        void Button_Click(object sender, RoutedEventArgs e)
        {
            _onPinReceived(myPin.Text, myOldPin.Text);
            //DialogResult = true;
        }
    }
}
