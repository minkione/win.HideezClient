using GalaSoft.MvvmLight;
using MvvmExtensions.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HideezSafe.Controls
{
    class SimpleNotificationViewModel : ObservableObject
    {
        private string title;
        private string message;

        public string Title
        {
            get { return title; }
            set
            {
                Set(nameof(Title), ref title, value);
            }
        }

        public string Message
        {
            get { return message; }
            set
            {
                Set(nameof(Message), ref message, value);
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
