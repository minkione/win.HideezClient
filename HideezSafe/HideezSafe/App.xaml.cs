using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
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
        }

        private void InitializeDIContainer()
        {
            Container = new UnityContainer();
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
