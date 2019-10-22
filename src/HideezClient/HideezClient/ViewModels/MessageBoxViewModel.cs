using HideezClient.Modules.Localize;
using HideezClient.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezClient.ViewModels
{
    class MessageBoxViewModel : LocalizedObject
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

        [Localization]
        public string Caption { get { return string.Format(L(caption), captionArgs); } }
        [Localization]
        public string Message { get { return string.Format(L(message), messageArgs); } }
    }
}
