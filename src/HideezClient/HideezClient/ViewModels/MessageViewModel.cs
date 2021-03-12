using HideezMiddleware.Localize;
using HideezClient.Mvvm;
using MvvmExtensions.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using HideezClient.Modules.Localize;

namespace HideezClient.ViewModels
{
    class MessageViewModel : LocalizedObject
    {
        private string caption;
        private object[] captionArgs;
        private string message;
        private object[] messageArgs;

        public void SetCaptionFormat(string formatKey, params object[] args)
        {
            caption = formatKey;
            captionArgs = args;
        }

        public void SetMessageFormat(string formatKey, params object[] args)
        {
            message = formatKey;
            messageArgs = args;
        }

        public TaskCompletionSource<bool> Tcs { get; set; }

        [Localization]
        public string Caption { get { return string.Format(L(caption), captionArgs); } }
        [Localization]
        public string Message { get { return string.Format(L(message), messageArgs); } }
        [Localization]
        public string ConfirmButtonTextKey { get; set; }


        public ICommand CancelCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        Tcs?.TrySetResult(false);
                    }
                };
            }
        }

        public ICommand ConfirmCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        Tcs?.TrySetResult(true);
                    }
                };
            }
        }
    }
}
