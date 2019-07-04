using HideezSafe.Models.Settings;
using HideezSafe.Modules.DeviceManager;
using HideezSafe.Modules.SettingsManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezSafe.Modules
{
    class SupportMailContentGenerator : ISupportMailContentGenerator
    {
        private IDeviceManager deviceManager;
        private ISettingsManager<ApplicationSettings> settingsManager;

        public SupportMailContentGenerator(IDeviceManager deviceManager,
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