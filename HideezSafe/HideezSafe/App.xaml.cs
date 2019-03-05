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

namespace HideezSafe
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
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

            startupHelper = Container.Resolve<IStartupHelper>();

            if (Settings.Default.FirstLaunch)
            {
                OnFirstLaunch();

                Settings.Default.FirstLaunch = false;
                Settings.Default.Save();
            }
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
        }

        private void FatalExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            /*
             * The intent of this method is to gather as much information as we can
             * Even if something goes wrong in the information gathering part
             * We know we are doomed for the fatal error crash
             * But we can still gather some useful information 
             * So that those who come after us
             * May fix our mistakes
             * And make this application great again
             */
            try
            {
                // Todo: Send fatal exception report
                Debug.WriteLine((e.ExceptionObject as Exception)?.ToString());
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
