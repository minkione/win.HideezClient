using HideezSafe.Models.Settings;
using HideezSafe.Modules.SettingsManager;
using HideezSafe.Utilities;
using Unity;
using Unity.Lifetime;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Diagnostics;
using System.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using HideezSafe.Utils;
using SingleInstanceApp;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using System.Globalization;
using HideezSafe.Modules.FileSerializer;

namespace HideezSafe
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstance
    {
        private IStartupHelper startupHelper;

        public static IUnityContainer Container { get; private set; }

        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += FatalExceptionHandler;
            InitializeDIContainer();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ApplicationSettings settings = null;
            ISettingsManager settingsManager = Container.Resolve<ISettingsManager>();

            try
            {
                var task = Task.Run(async () => // Off Loading Load Programm Settings to non-UI thread
                {
                    settings = await settingsManager.LoadSettingsAsync();
                });
                task.Wait(); // Block this to ensure that results are usable in next steps of sequence

                Thread.CurrentThread.CurrentCulture = new CultureInfo(settings.SelectedUiLanguage);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(settings.SelectedUiLanguage);
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

            startupHelper = Container.Resolve<IStartupHelper>();

            if (settings.IsFirstLaunch)
            {
                OnFirstLaunch();

                settings.IsFirstLaunch = false;
                settingsManager.SaveSettings(settings);
            }
        }

        /// <summary>
        /// Application Entry Point.
        /// </summary>
        [System.STAThreadAttribute()]
        private static void Main()
        {
            if (SingleInstance<App>.InitializeAsFirstInstance("{EB9E0C35-8DC5-459D-80C2-93DCE0036C91}"))
            {
                var application = new App();
                application.InitializeComponent();
                application.Run();

                // Allow single instance code to perform cleanup operations
                SingleInstance<App>.Cleanup();
            }
        }

        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            // handle command line arguments of second instance
            // ...

            if (this.MainWindow.WindowState == WindowState.Minimized)
            {
                this.MainWindow.WindowState = WindowState.Normal;
            }

            this.MainWindow.Show();
            this.MainWindow.Activate();
            
            return true;
        }

        private void OnFirstLaunch()
        {
            // add to startup with windows if first start app
            startupHelper.AddToStartup();
        }

        private void InitializeDIContainer()
        {
            Container = new UnityContainer();

            Container.RegisterType<IStartupHelper, StartupHelper>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IFileSerializer, XmlFileSerializer>();
            Container.RegisterType<ISettingsManager, SettingsManager>(new ContainerControlledLifetimeManager());
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
