using System;

namespace ServiceLibrary.Implementation
{
    public sealed class HideezServiceBuildDirector
    {
        public HideezService BuildEnterpriseService(HideezServiceBuilder builder)
        {
            builder.Begin();

            builder.AddFatalExceptionHandling();
            builder.AddEnterpriseResources();
            builder.AddEnterpriseProximitySettingsSupport();
            builder.AddHES();
            builder.AddEnterpriseConnectionFlow();
            builder.AddCsrSupport();
            builder.AddWinBleSupport();
            builder.AddRfidSupport();
            builder.AddRemoteUnlock();
            builder.AddWorkstationLock();
            builder.AddClientPipe();
            builder.AddAudit();
            builder.End();

            return builder.GetService();
        }

        public HideezService BuildStandaloneService(HideezServiceBuilder builder)
        {
            builder.Begin();

            builder.AddFatalExceptionHandling();
            builder.AddStandaloneResources();
            builder.AddStandaloneProximitySettingsSupport();
            builder.AddStandaloneConnectionFlow();
            builder.AddCsrSupport();
            builder.AddWinBleSupport();
            builder.AddRemoteUnlock();
            builder.AddWorkstationLock();
            builder.AddClientPipe();
            builder.AddUpdateCheck();
            builder.End();

            return builder.GetService();
        }
    }
}
