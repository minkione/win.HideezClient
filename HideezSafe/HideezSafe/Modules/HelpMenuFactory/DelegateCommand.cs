using System;
using System.Windows.Input;

namespace HideezSafe.Modules
{
    internal class DelegateCommand : ICommand
    {
        public System.Func<object, object> CommandAction { get; set; }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            throw new NotImplementedException();
        }

        public void Execute(object parameter)
        {
            throw new NotImplementedException();
        }
    }
}