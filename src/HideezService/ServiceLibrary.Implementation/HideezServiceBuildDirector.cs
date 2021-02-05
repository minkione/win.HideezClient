using System;

namespace ServiceLibrary.Implementation
{
    public sealed class HideezServiceBuildDirector
    {
        public HideezService BuildEnterpriseService(HideezServiceBuilder builder)
        {
            builder.Begin();

            builder.AddFatalExceptionHandling();
            builder.AddHES();
            builder.AddEnterpriseConnectionFlow();
            builder.AddCsrSupport();
            builder.AddWinBleSupport();
            builder.AddRfidSupport();
            builder.AddRemoteUnlock();
            //builder.AddClientPipe();
            builder.AddAudit();

            return builder.GetService();
        }

        public HideezService BuildStandaloneService(HideezServiceBuilder builder)
        {
            builder.Begin();

            builder.AddFatalExceptionHandling();
            builder.AddStandaloneConnectionFlow();
            builder.AddCsrSupport();
            builder.AddWinBleSupport();
            //builder.AddClientPipe();

            return builder.GetService();
        }
    }
}
