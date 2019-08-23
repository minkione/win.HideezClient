/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:WpfApp5"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using HideezSafe.PageViewModels;
using System.ComponentModel;
using System.Diagnostics;
using Unity;
using Unity.Lifetime;

namespace HideezSafe.ViewModels
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    class ViewModelLocator
    {
        private readonly IUnityContainer DIContainer;

        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            // Check for design mode. 
            if ((bool)(DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(System.Windows.DependencyObject)).DefaultValue))
            {
                // in Design mode
                DIContainer = new UnityContainer();
            }
            else
            {
                DIContainer = App.Container;
            }
        }

        public MainViewModel Main
        {
            get { return DIContainer.Resolve<MainViewModel>(); }
        }

        public LoginSystemPageViewModel LoginSystemViewModel
        {
            get { return DIContainer.Resolve<LoginSystemPageViewModel>(); }
        }

        public LockSettingsPageViewModel LockSettingsPage
        {
            get { return DIContainer.Resolve<LockSettingsPageViewModel>(); }
        }

        public IndicatorsViewModel Indicators
        {
            get { return DIContainer.Resolve<IndicatorsViewModel>(); }
        }

        public DevicesExpanderViewModel DevicesExpander
        {
            get { return DIContainer.Resolve<DevicesExpanderViewModel>(); }
        }

        public AddCredentialViewModel AddCredential
        {
            get { return DIContainer.Resolve<AddCredentialViewModel>(); }
        }

        public NotificationsContainerViewModel NotificationsContainer
        {
            get { return DIContainer.Resolve<NotificationsContainerViewModel>(); }
        }
    }
}
