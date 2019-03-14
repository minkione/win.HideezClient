using GalaSoft.MvvmLight.Messaging;
using HideezSafe.Modules;
using HideezSafe.Mvvm.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezSafe.Mvvm
{
    class WindowMessageHandler : IWindowMessageHandler
    {
        private readonly IWindowsManager windowsManager;

        public WindowMessageHandler(IWindowsManager windowsManager, IMessenger messenger)
        {
            this.windowsManager = windowsManager;

            messenger.Register<ActivateWindowMessage>(this, OnMessageReceived);
        }

        private void OnMessageReceived(ActivateWindowMessage eventObject)
        {
            windowsManager.ActivateMainWindow();
        }
    }
}
