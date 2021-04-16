using HideezMiddleware;
using HideezMiddleware.ApplicationModeProvider;
using HideezMiddleware.ConnectionModeProvider;

namespace ServiceLibrary.Implementation
{
    public sealed class HideezServiceFactory
    {
        /// <summary>
        /// Returns new Hideez Service configured according to the application mode specified in settings
        /// </summary>
        public HideezService GetHideezService()
        {
            var builder = new HideezServiceBuilder();
            var director = new HideezServiceBuildDirector();

            var appMode = GetApplicationMode();
            var features = GetToggleFeaturesList();
            if (appMode == ApplicationMode.Standalone)
                return director.BuildStandaloneService(builder, features);
            else
                return director.BuildEnterpriseService(builder, features);
        }

        private ApplicationMode GetApplicationMode()
        {
            try
            {
                NLog.LogManager.EnableLogging();

                using (var key = HideezClientRegistryRoot.GetRootRegistryKey(false))
                {
                    var sdkLogger = new NLogWrapper();
                    var applicationModeProvider = new ApplicationModeRegistryProvider(key, sdkLogger);
                    return applicationModeProvider.GetApplicationMode();
                }
            }
            finally
            {
                NLog.LogManager.DisableLogging();
            }
        }

        private GlobalConnectionMode GetConnectionMode()
        {
            try
            {
                NLog.LogManager.EnableLogging();

                using (var key = HideezClientRegistryRoot.GetRootRegistryKey(false))
                {
                    var sdkLogger = new NLogWrapper();
                    var connectionModeProvider = new ConnectionModeProvider(key, sdkLogger);
                    return connectionModeProvider.ConnectionMode;
                }
            }
            finally
            {
                NLog.LogManager.DisableLogging();
            }
        }

        private ToggleFeaturesList GetToggleFeaturesList()
        {
            var connectionMode = GetConnectionMode();

            var toggleFeaturesList = new ToggleFeaturesList
            {
                EnableDongleSupport = connectionMode == GlobalConnectionMode.CsrDongle,
                EnableWinBleSupport = connectionMode == GlobalConnectionMode.WindowsBle
            };

            return toggleFeaturesList;
        }
    }
}
