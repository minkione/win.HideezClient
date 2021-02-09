using System;

namespace ServiceLibrary.Implementation
{
    public sealed class HideezServiceBuildDirector
    {
        public HideezService BuildEnterpriseService(HideezServiceBuilder builder)
        {
            builder.Begin();

            builder.AddFatalExceptionHandling();
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
            builder.AddStandaloneProximitySettingsSupport();
            builder.AddStandaloneConnectionFlow();
            builder.AddCsrSupport();
            builder.AddWinBleSupport();
            builder.AddWorkstationLock();
            builder.AddClientPipe();
            builder.End();

            return builder.GetService();
        }
    }
}
