using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HideezClient.Controls
{
    /// <summary>
    /// Interaction logic for LeftMenuButtonControl.xaml
    /// </summary>
    public partial class LeftMenuButtonControl : UserControl
    {
        public LeftMenuButtonControl()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is INotifyPropertyChanged)
            {
                try
                {
                    ICommand command = DataContext.GetType().GetProperty("Command").GetValue(DataContext) as ICommand;
                    if (command != null && command.CanExecute(null))
                    {
                        command.Execute(null);
                    }
                }
                catch (Exception)
                {
                    Debug.Assert(false);
                }
            }
        }
    }
}
