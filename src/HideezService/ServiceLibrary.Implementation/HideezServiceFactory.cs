using HideezMiddleware;
using HideezMiddleware.ApplicationModeProvider;

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

            var mode = GetApplicationMode();
            if (mode == ApplicationMode.Standalone)
                return director.BuildStandaloneService(builder);
            else
                return director.BuildEnterpriseService(builder);
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
    }
}
