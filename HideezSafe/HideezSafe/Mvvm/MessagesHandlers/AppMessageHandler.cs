using GalaSoft.MvvmLight.Messaging;
using HideezSafe.Mvvm.Messages;
using HideezSafe.Properties;
using HideezSafe.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HideezSafe.Mvvm
{
    class AppMessageHandler : IAppMessageHandler
    {
        private readonly IStartupHelper startupHelper;
        private readonly IMessenger messenger;

        public AppMessageHandler(IStartupHelper startupHelper, IMessenger messenger)
        {
            this.startupHelper = startupHelper;
            this.messenger = messenger;

            this.messenger.Register<InvertStateAutoStartupMessage>(this, OnMessageReceived);
            this.messenger.Register<ShutdownAppMessage>(this, OnMessageReceived);
            this.messenger.Register<OpenUrlMessage>(this, OnMessageReceived);
        }

        private void OnMessageReceived(InvertStateAutoStartupMessage eventObject)
        {
            if (startupHelper.IsInStartup())
            {
                startupHelper.RemoveFromStartup();
            }
            else
            {
                startupHelper.AddToStartup();
            }
        }

        private void OnMessageReceived(ShutdownAppMessage shutdownAppMessage)
        {
            Application.Current.Shutdown();
        }

        private void OnMessageReceived(OpenUrlMessage openUrlMessage)
        {
            Task.Run(() =>
            {
                try
                {
                    Process.Start(new ProcessStartInfo(openUrlMessage.Url));
                }
                catch (Exception ex)
                {
                    // Assumed that exceptions occur only during development because of invalid address
                    Debug.WriteLine(ex);
                    Debug.Assert(false);
                }
            });
        }
    }
}
