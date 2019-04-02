using GalaSoft.MvvmLight.Messaging;
using HideezSafe.Modules;
using HideezSafe.Modules.Localize;
using HideezSafe.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace HideezSafe.Utilities
{
    class AppHelper : IAppHelper
    {
        public void Shutdown()
        {
            Application.Current.Shutdown();
        }

        public void OpenUrl(string url)
        {
            Task.Run(() =>
            {
                try
                {
                    Process.Start(new ProcessStartInfo(url));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    Debug.Assert(false);
                }
            });
        }

        public void ChangeCulture(CultureInfo newCulture)
        {
            Settings.Default.Culture = newCulture;
            Settings.Default.Save();

            TranslationSource.Instance.CurrentCulture = Settings.Default.Culture;

            Thread.CurrentThread.CurrentCulture = newCulture;
            Thread.CurrentThread.CurrentUICulture = newCulture;
        }
    }
}
