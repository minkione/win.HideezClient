using HideezMiddleware.Settings;
using HideezClient.Models.Settings;
using HideezClient.Modules.VaultManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezClient.Modules
{
    class SupportMailContentGenerator : ISupportMailContentGenerator
    {
        private IVaultManager deviceManager;
        private ISettingsManager<ApplicationSettings> settingsManager;

        public SupportMailContentGenerator(IVaultManager deviceManager,
            ISettingsManager<ApplicationSettings> settingsManager)
        {
            this.deviceManager = deviceManager;
            this.settingsManager = settingsManager;
        }

        public Task<string> GenerateSupportMail(string address)
        {
            return Task.FromResult($"mailto:{address}");

            // TODO: Implement mail generate
        }
    }
}