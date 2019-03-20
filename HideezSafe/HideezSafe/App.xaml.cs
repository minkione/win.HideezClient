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
using HideezSafe.Properties;
using HideezSafe.Utils;
using SingleInstanceApp;
using System.Runtime.InteropServices;
using HideezSafe.Modules;
using GalaSoft.MvvmLight.Messaging;
using NLog;

namespace HideezSafe
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstance
    {
        public static Logger logger;
        private IStartupHelper startupHelper;

        public static IUnityContainer Container { get; private set; }

        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += FatalExceptionHandler;

            // LogManager.DisableLogging();
            // LogManager.EnableLogging();
            logger = LogManager.GetCurrentClassLogger();

            logger.Info("Version: {0}", Environment.Version);
            logger.Info("OS: {0}", Environment.OSVersion);
            logger.Info("Command: {0}", Environment.CommandLine);

            InitializeDIContainer();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            logger.Info("Resolve DI container");
            startupHelper = Container.Resolve<IStartupHelper>();
            Container.Resolve<IWorkstationManager>();

            if (Settings.Default.FirstLaunch)
            {
                OnFirstLaunch();

                Settings.Default.FirstLaunch = false;
                Settings.Default.Save();
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

            logger.Info("Handle start of second instance");

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
            logger.Info("First launch");
            // add to startup with windows if first start app
            bool resalt = startupHelper.AddToStartup();
            logger.Info("Add app to startup: {0}", resalt);
        }

        private void InitializeDIContainer()
        {
            Container = new UnityContainer();

            logger.Info("Start initialize DI container");

            Container.RegisterType<IStartupHelper, StartupHelper>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IWorkstationManager, WorkstationManager>(new ContainerControlledLifetimeManager());
            Container.RegisterInstance<IMessenger>(Messenger.Default, new ContainerControlledLifetimeManager());

            logger.Info("Finish initialize DI container");
        }

        private void FatalExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            string message = "Fatal error occured";
            Exception ex = e.ExceptionObject as Exception;

            try
            {
                logger.Fatal(ex, message);
            }
            catch
            {
                Environment.FailFast(message, ex);
            }
        }
    }
}
