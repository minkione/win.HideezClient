using HideezClient.Models.Settings;
using HideezClient.Utilities;
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
using HideezClient.Properties;
using SingleInstanceApp;
using System.Runtime.InteropServices;
using HideezClient.Modules;
using GalaSoft.MvvmLight.Messaging;
using NLog;
using HideezClient.ViewModels;
using Hardcodet.Wpf.TaskbarNotification;
using System.Globalization;
using System.Threading;
using System.IO;
using HideezClient.Mvvm;
using HideezClient.Modules.ServiceProxy;
using HideezClient.Modules.Localize;
using HideezClient.HideezServiceReference;
using HideezClient.Modules.ServiceCallbackMessanger;
using HideezClient.Modules.ServiceWatchdog;
using HideezClient.Modules.DeviceManager;
using HideezClient.Modules.SessionStateMonitor;
using HideezClient.Modules.ActionHandler;
using Hideez.ISM;
using WindowsInput;
using HideezClient.PageViewModels;
using HideezClient.Modules.HotkeyManager;
using System.Text;
using HideezMiddleware.Settings;
using Unity.Injection;
using Hideez.SDK.Communication.Log;
using HideezMiddleware;
using HideezClient.Messages;
using HideezClient.Controls;
using System.Reflection;

namespace HideezClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstance
    {
        public static ILogger logger;
        private IStartupHelper startupHelper;
        private IWorkstationManager workstationManager;
        private IMessenger messenger;
        private IWindowsManager windowsManager;
        private IServiceWatchdog serviceWatchdog;
        private IDeviceManager deviceManager;
        private UserActionHandler userActionHandler;
        private IHotkeyManager hotkeyManager;

        public static IUnityContainer Container { get; private set; }

        public App()
        {
            SetupExceptionHandling();

            LogManager.EnableLogging();
            logger = LogManager.GetCurrentClassLogger();

            logger.Info("App version: {0}", Assembly.GetEntryAssembly().GetName().Version);
            logger.Info("Version: {0}", Environment.Version);
            logger.Info("OS: {0}", Environment.OSVersion);
            logger.Info("Command: {0}", Environment.CommandLine);
        }

        void SetupExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            LogUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

            DispatcherUnhandledException += (s, e) =>
                LogUnhandledException(e.Exception, "Application.Current.DispatcherUnhandledException");

            TaskScheduler.UnobservedTaskException += (s, e) =>
                LogUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException");
        }

        void LogUnhandledException(Exception e, string source)
        {
            try
            {
                LogManager.EnableLogging();

                var fatalLogger = logger ?? LogManager.GetCurrentClassLogger();
                var assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName();

                fatalLogger.Fatal($"Unhandled exception in {assemblyName.Name} v{assemblyName.Version}");
                fatalLogger.Fatal(e);
                LogManager.Flush();
            }
            catch (Exception)
            {
                try
                {
                    Environment.FailFast("An error occured while handling fatal error", e as Exception);
                }
                catch (Exception exc)
                {
                    Environment.FailFast("An error occured while handling an error during fatal error handling", exc);
                }
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            // Unity-Container call Dispose on all instances implementing the IDisposable interface registered by ContainerControlledLifetimeManager or HierarchicalLifetimeManager.
            Container.Dispose();
            LogManager.Flush();
            LogManager.Shutdown();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            InitializeDIContainer();

            // Init settings
            ApplicationSettings settings = null;
            ISettingsManager<ApplicationSettings> appSettingsManager = Container.Resolve<ISettingsManager<ApplicationSettings>>();

            try
            {
                var appSettingsDirectory = Path.GetDirectoryName(appSettingsManager.SettingsFilePath);
                if (!Directory.Exists(appSettingsDirectory))
                    Directory.CreateDirectory(appSettingsDirectory);

                settings = await appSettingsManager.LoadSettingsAsync().ConfigureAwait(true);

                // Init localization
                var culture = new CultureInfo(settings.SelectedUiLanguage);
                TranslationSource.Instance.CurrentCulture = culture;
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
            }
            catch (Exception exp)
            {
                var sb = new StringBuilder();
                sb.AppendLine();
                sb.AppendLine("Unexpected Error 1 in App.Application_Startup()");
                sb.AppendLine($"   Message:{exp.Message}");
                sb.AppendLine($"StackTrace:{exp.StackTrace}");
                sb.AppendLine();
                logger.Error(sb.ToString());
            }

            messenger = Container.Resolve<IMessenger>();
            Container.Resolve<ITaskbarIconManager>();

            logger.Info("Resolve DI container");
            startupHelper = Container.Resolve<IStartupHelper>();
            workstationManager = Container.Resolve<IWorkstationManager>();
            windowsManager = Container.Resolve<IWindowsManager>();
            Container.Resolve<IHideezServiceCallback>();
            serviceWatchdog = Container.Resolve<IServiceWatchdog>();
            serviceWatchdog.Start();
            deviceManager = Container.Resolve<IDeviceManager>();
            userActionHandler = Container.Resolve<UserActionHandler>();
            hotkeyManager = Container.Resolve<IHotkeyManager>();
            hotkeyManager.Enabled = true;

            if (settings.IsFirstLaunch)
            {
                OnFirstLaunch();

                settings.IsFirstLaunch = false;
                appSettingsManager.SaveSettings(settings);
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
            logger.Info("First Hideez Client launch");
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
            Container.RegisterType<NotificationsContainerViewModel>();
            Container.RegisterType<DeviceNotAuthorizedNotificationViewModel>();
            Container.RegisterType<PinViewModel>();

            #endregion ViewModels

            Container.RegisterType<IStartupHelper, StartupHelper>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IWorkstationManager, WorkstationManager>(new ContainerControlledLifetimeManager());
            Container.RegisterInstance<IMessenger>(Messenger.Default, new ContainerControlledLifetimeManager());

            Container.RegisterType<ILog, NLogWrapper>(new ContainerControlledLifetimeManager());

            Container.RegisterType<IWindowsManager, WindowsManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IAppHelper, AppHelper>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IDialogManager, DialogManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IFileSerializer, XmlFileSerializer>();
            Container.RegisterType<IDeviceManager, DeviceManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ISessionStateMonitor, SessionStateMonitor>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ISupportMailContentGenerator, SupportMailContentGenerator>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IHotkeyManager, HotkeyManager>(new ContainerControlledLifetimeManager());

            // Settings
            Container.RegisterType<ISettingsManager<ApplicationSettings>, HSSettingsManager<ApplicationSettings>>(new ContainerControlledLifetimeManager()
                , new InjectionConstructor(Path.Combine(Constants.DefaultSettingsFolderPath, Constants.ApplicationSettingsFileName), typeof(IFileSerializer), typeof(IMessenger)));
            Container.RegisterType<ISettingsManager<HotkeySettings>, HSSettingsManager<HotkeySettings>>(new ContainerControlledLifetimeManager()
                , new InjectionConstructor(Path.Combine(Constants.DefaultSettingsFolderPath, Constants.HotkeySettingsFileName), typeof(IFileSerializer), typeof(IMessenger)));

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

            Container.RegisterType<IEventPublisher, EventPublisher>(new ContainerControlledLifetimeManager());

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
    }
}
