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
using HideezSafe.Mvvm;
using HideezSafe.Modules.ServiceProxy;
using HideezSafe.Modules.Localize;
using HideezSafe.HideezServiceReference;
using HideezSafe.Modules.ServiceCallbackMessanger;
using HideezSafe.Modules.ServiceWatchdog;
using HideezSafe.Modules.DeviceManager;
using HideezSafe.Modules.SessionStateMonitor;
using HideezSafe.Modules.ActionHandler;
using Hideez.ISM;
using WindowsInput;
using HideezSafe.PageViewModels;
using HideezSafe.Modules.HotkeyManager;

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
        private IServiceWatchdog serviceWatchdog;
        private IDeviceManager deviceManager;
        private IHotkeyManager hotkeyManager;

        public static IUnityContainer Container { get; private set; }

        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            LogManager.EnableLogging();
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
            Container.Resolve<IHideezServiceCallback>();
            serviceWatchdog = Container.Resolve<IServiceWatchdog>();
            serviceWatchdog.Start();
            deviceManager = Container.Resolve<IDeviceManager>();
            Container.Resolve<UserActionHandler>();
            hotkeyManager = Container.Resolve<IHotkeyManager>();
            hotkeyManager.Enabled = true;

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
            logger.Info("First Hideez Safe 3 launch");
        }

        private void InitializeDIContainer()
        {
            Container = new UnityContainer();
#if DEBUG
            Container.AddExtension(new Diagnostic());
#endif
            logger.Info("Start initialize DI container");

            #region ViewModels

            Container.RegisterType<MainViewModel>(new ContainerControlledLifetimeManager());
            Container.RegisterType<LoginSystemPageViewModel>();
            Container.RegisterType<LockSettingsPageViewModel>();
            Container.RegisterType<IndicatorsViewModel>();
            Container.RegisterType<DevicesExpanderViewModel>();
            Container.RegisterType<AddCredentialViewModel>();
            Container.RegisterType<NotificationsContainerViewModel>(new ContainerControlledLifetimeManager());

            #endregion ViewModels

            Container.RegisterType<IStartupHelper, StartupHelper>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IWorkstationManager, WorkstationManager>(new ContainerControlledLifetimeManager());
            Container.RegisterInstance<IMessenger>(Messenger.Default, new ContainerControlledLifetimeManager());

            logger.Info("Finish initialize DI container");

            Container.RegisterType<IWindowsManager, WindowsManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IAppHelper, AppHelper>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IDialogManager, DialogManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IFileSerializer, XmlFileSerializer>();
            Container.RegisterType<IDeviceManager, DeviceManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ISessionStateMonitor, SessionStateMonitor>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ISupportMailContentGenerator, SupportMailContentGenerator>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IHotkeyManager, HotkeyManager>(new ContainerControlledLifetimeManager());

            // Settings
            Container.RegisterType<ISettingsManager<ApplicationSettings>, SettingsManager<ApplicationSettings>>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ISettingsManager<HotkeySettings>, SettingsManager<HotkeySettings>>(new ContainerControlledLifetimeManager());

            // Service
            Container.RegisterType<IServiceProxy, ServiceProxy>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IHideezServiceCallback, ServiceCallbackMessanger>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IServiceWatchdog, ServiceWatchdog>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IRemoteDeviceFactory, RemoteDeviceFactory>(new ContainerControlledLifetimeManager());

            // Taskbar icon
            Container.RegisterInstance(FindResource("TaskbarIcon") as TaskbarIcon, new ContainerControlledLifetimeManager());
            Container.RegisterType<IBalloonTipNotifyManager, BalloonTipNotifyManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IMenuFactory, MenuFactory>(new ContainerControlledLifetimeManager());
            Container.RegisterType<TaskbarIconViewModel>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ITaskbarIconManager, TaskbarIconManager>(new ContainerControlledLifetimeManager());


            // Messenger
            Container.RegisterType<IMessenger, Messenger>(new ContainerControlledLifetimeManager());

            // Input
            Container.RegisterType<UserActionHandler>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IInputCache, InputCache>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IInputHandler, InputHandler>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IInputSimulator, InputSimulator>(new ContainerControlledLifetimeManager());
            Container.RegisterInstance(typeof(ITemporaryCacheAccount), new TemporaryCacheAccount(TimeSpan.FromMinutes(1)), new ContainerControlledLifetimeManager());
            Container.RegisterType<InputOtp>();
            Container.RegisterType<InputPassword>();
            Container.RegisterType<InputLogin>();

            Container.RegisterType<INotifier, Notifier>(new ContainerControlledLifetimeManager());

            logger.Info("Finish initialize DI container");

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

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                LogManager.EnableLogging();

                var fatalLogger = logger ?? LogManager.GetCurrentClassLogger();

                fatalLogger.Fatal(e.ExceptionObject as Exception);
                LogManager.Flush();
            }
            catch (Exception)
            {
                try
                {
                    Environment.FailFast("An error occured while handling fatal error", e.ExceptionObject as Exception);
                }
                catch (Exception exc)
                {
                    Environment.FailFast("An error occured while handling an error during fatal error handling", exc);
                }
            }
        }
    }
}
