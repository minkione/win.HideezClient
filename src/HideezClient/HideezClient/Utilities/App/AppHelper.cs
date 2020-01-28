using GalaSoft.MvvmLight.Messaging;
using HideezMiddleware.Settings;
using HideezClient.Models.Settings;
using HideezClient.Modules;
using HideezClient.Modules.Localize;
using HideezClient.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using SingleInstanceApp;

namespace HideezClient.Utilities
{
    class AppHelper : IAppHelper
    {
        private readonly ISettingsManager<ApplicationSettings> settingsManager;

        public AppHelper(ISettingsManager<ApplicationSettings> settingsManager)
        {
            this.settingsManager = settingsManager;
        }

        public void Shutdown()
        {
            // TODO: Fix repeated calls to AppHelper.Shutdown()
            Application.Current.Shutdown(0);
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
            var settings = settingsManager.Settings;
            settings.SelectedUiLanguage = newCulture.TwoLetterISOLanguageName;
            settingsManager.SaveSettings(settings);

            TranslationSource.Instance.CurrentCulture = newCulture;
            Thread.CurrentThread.CurrentCulture = newCulture;
            Thread.CurrentThread.CurrentUICulture = newCulture;
        }

        public void Restart()
        {
            SingleInstance<App>.Cleanup();
            System.Windows.Forms.Application.Restart();
            Shutdown();
        }
    }
}
