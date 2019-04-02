﻿using HideezSafe.Models.Settings;
using HideezSafe.Modules.SettingsManager;
using HideezSafe.Utilities;
using Unity;
using Unity.Lifetime;
using System;
using System.Windows;
using System.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using HideezSafe.Properties;
using SingleInstanceApp;
using System.Runtime.InteropServices;
using HideezSafe.Modules;
using GalaSoft.MvvmLight.Messaging;
using NLog;
using HideezSafe.ViewModels;
using Hardcodet.Wpf.TaskbarNotification;
using System.Globalization;
using System.Threading;
using System.IO;
using HideezSafe.Modules.FileSerializer;

namespace HideezSafe
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstance
    {
        public static Logger logger;
        private IStartupHelper startupHelper;
        private IMessenger messenger;
        private IWindowsManager windowsManager;

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
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            InitializeDIContainer();

            // Init settings
            ApplicationSettings settings = null;
            ISettingsManager<ApplicationSettings> settingsManager = Container.Resolve<ISettingsManager<ApplicationSettings>>();

            try
            {
                var task = Task.Run(async () => // Off Loading Load Programm Settings to non-UI thread
                {
                    var appSettingsDirectory = Path.GetDirectoryName(settingsManager.SettingsFilePath);
                    if (!Directory.Exists(appSettingsDirectory))
                        Directory.CreateDirectory(appSettingsDirectory);

                    settings = await settingsManager.LoadSettingsAsync();
                });
                task.Wait(); // Block this to ensure that results are usable in next steps of sequence

                // Init localization
                var culture = new CultureInfo(settings.SelectedUiLanguage);
                TranslationSource.Instance.CurrentCulture = culture;
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
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

            messenger = Container.Resolve<IMessenger>();
            Container.Resolve<ITaskbarIconManager>();

            logger.Info("Resolve DI container");
            startupHelper = Container.Resolve<IStartupHelper>();
            Container.Resolve<IWorkstationManager>();
            windowsManager = Container.Resolve<IWindowsManager>();

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

            logger.Info("Handle start of second instance");
            windowsManager.ActivateMainWindow();

            
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
            Container.RegisterInstance(Messenger.Default, new ContainerControlledLifetimeManager());
            logger.Info("Finish initialize DI container");

            Container.RegisterType<IWindowsManager, WindowsManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IAppHelper, AppHelper>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IFileSerializer, XmlFileSerializer>();
            Container.RegisterType<ISettingsManager<ApplicationSettings>, SettingsManager<ApplicationSettings>>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ISettingsManager<HotkeySettings>, SettingsManager<HotkeySettings>>(new ContainerControlledLifetimeManager());

            // Taskbar icon
            Container.RegisterInstance(FindResource("TaskbarIcon") as TaskbarIcon, new ContainerControlledLifetimeManager());
            Container.RegisterType<IBalloonTipNotifyManager, BalloonTipNotifyManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IMenuFactory, MenuFactory>(new ContainerControlledLifetimeManager());
            Container.RegisterType<TaskbarIconViewModel>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ITaskbarIconManager, TaskbarIconManager>(new ContainerControlledLifetimeManager());

            // Messenger
            Container.RegisterType<IMessenger, Messenger>(new ContainerControlledLifetimeManager());
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
