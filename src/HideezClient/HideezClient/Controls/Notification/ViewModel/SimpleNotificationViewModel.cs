using HideezClient.Mvvm;
using MvvmExtensions.Commands;
using System.Windows.Input;

namespace HideezClient.Controls
{
    class SimpleNotificationViewModel : ObservableObject
    {
        string _title;
        string _message;

        public string Title
        {
            get { return _title; }
            set
            {
                Set(ref _title, value);
            }
        }

        public string Message
        {
            get { return _message; }
            set
            {
                Set(ref _message, value);
            }
        }

        public ICommand CloseCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {

                    }
                };
            }
        }
    }
}
