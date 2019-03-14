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
using HideezSafe.Utilities;
using SingleInstanceApp;
using System.Runtime.InteropServices;
using HideezSafe.ViewModels;
using HideezSafe.Modules;
using MvvmExtentions.EventAggregator;
using Hardcodet.Wpf.TaskbarNotification;
using System.Globalization;
using GalaSoft.MvvmLight.Messaging;
using HideezSafe.Mvvm.Messages;
using System.Threading;
using HideezSafe.Mvvm;

namespace HideezSafe
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstance
    {
        private IMessenger messenger;

        public static IUnityContainer Container { get; private set; }

        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += FatalExceptionHandler;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            InitializeDIContainer();

            // Init localization
            CultureInfo culture = Settings.Default.Culture;
            TranslationSource.Instance.CurrentCulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            messenger = Container.Resolve<IMessenger>();
            Container.Resolve<IAppMessageHandler>();
            Container.Resolve<ILanguageMessageHandler>();
            Container.Resolve<IWindowMessageHandler>();

            TaskbarIcon taskbarIcon = Container.Resolve<TaskbarIcon>();
            taskbarIcon.DataContext = Container.Resolve<TaskbarIconViewModel>();

            //IBalloonTipNotifyManager balloonTipNotifyManager = Container.Resolve<IBalloonTipNotifyManager>();
            //balloonTipNotifyManager.ShowInfo("Info", "Hideez Safe is started");

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

            Messenger.Default.Send(new ActivateWindowMessage());

            return true;
        }

        private void OnFirstLaunch()
        {
            // add to startup with windows if first start app
            messenger.Send(new InvertStateAutoStartupMessage());
        }

        private void InitializeDIContainer()
        {
            Container = new UnityContainer();

            Container.RegisterType<IStartupHelper, StartupHelper>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IWindowsManager, WindowsManager>(new ContainerControlledLifetimeManager());
            Container.RegisterInstance(FindResource("TaskbarIcon") as TaskbarIcon, new ContainerControlledLifetimeManager());
            Container.RegisterType<IBalloonTipNotifyManager, BalloonTipNotifyManager>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IMenuFactory, MenuFactory>(new ContainerControlledLifetimeManager());
            Container.RegisterType<TaskbarIconViewModel>(new ContainerControlledLifetimeManager());

            // Messenger
            Container.RegisterType<IMessenger, Messenger>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IAppMessageHandler, AppMessageHandler>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ILanguageMessageHandler, LanguageMessageHandler>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IWindowMessageHandler, WindowMessageHandler>(new ContainerControlledLifetimeManager());
        }

        private void FatalExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            // A simple entry in the event log will suffice for the time being
            Environment.FailFast("Fatal error occured", e.ExceptionObject as Exception);
        }
    }
}
