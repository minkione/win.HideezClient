using HideezSafe.Models.Settings;
using HideezSafe.Modules.SettingsManager;
using HideezSafe.Utilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Unity;

namespace HideezSafe
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IUnityContainer Container { get; private set; }

        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += FatalExceptionHandler;
            InitializeDIContainer();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ISettings settings = null;
            ISettingsManager settingsManager = null;
            try
            {
                // Resolve SettingsManager to retrieve app settings/session data
                // and start with correct parameters from last session
                // Todo: uncomment when DI container is merged into develop
                //settingsManager = сontainer.Resolve<ISettingsManager>();

                var task = Task.Run(async () => // Off Loading Load Programm Settings to non-UI thread
                {
                    settings = await settingsManager.LoadSettingsAsync(Path.Combine(Constants.DefaultSettingsFolderPath, Constants.DefaultSettingsFileName));
                });
                task.Wait(); // Block this to ensure that results are usable in next steps of sequence

                Thread.CurrentThread.CurrentCulture = new CultureInfo(settings.SelectedLanguage);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(settings.SelectedLanguage);
            }
            catch (Exception exp)
            {
                // Todo: log error with logger
                Console.WriteLine("");
                Console.WriteLine("Unexpected Error 1 in App.Application_Startup()");
                Console.WriteLine("   Message:{0}", exp.Message);
                Console.WriteLine("StackTrace:{0}", exp.StackTrace);
                Console.WriteLine("");
            }

            // Todo: Resolve main window view & viewmodel

            if (settings.FirstLaunch)
            {
                settings.FirstLaunch = false;
            }
        }

        private void InitializeDIContainer()
        {
            Container = new UnityContainer();
        }

        /// <summary>
        /// Check if configuration file can be opened. File is deleted on error.
        /// </summary>
        private void HandleBrokenConfig()
        {
            try
            {
                ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
            }
            catch (ConfigurationErrorsException ex)
            {
                string filename = ex.Filename;

                if (File.Exists(filename))
                    File.Delete(filename);
            }
        }

        private void FatalExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            // A simple entry in the event log will suffice for the time being
            Environment.FailFast("Fatal error occured", e.ExceptionObject as Exception);
        }
    }
}
