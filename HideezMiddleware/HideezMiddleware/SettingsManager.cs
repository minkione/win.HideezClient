using Hideez.SDK.Communication;
using Hideez.SDK.Communication.HES.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace HideezMiddleware
{
    public class SettingsManager : ISettingsManager
    {
        private readonly object lockObjForUnlockerSettings = new object();
        private readonly string settingsPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)}\Hideez\Service\Settings";
        private readonly string unlockerFilePath;
        private UnlockerSettingsInfo unlockerSettingsInfoCache;

        public SettingsManager()
        {
            unlockerFilePath = $@"{settingsPath}\Unlocker.json";
        }

        public event EventHandler<UnlockerSettingsInfoEventArgs> SettingsUpdated;

        public Task<UnlockerSettingsInfo> GetSettingsAsync()
        {
            return Task.Run(new Func<UnlockerSettingsInfo>(GetSettings));
        }

        public UnlockerSettingsInfo GetSettings()
        {
            lock (lockObjForUnlockerSettings)
            {
                if (unlockerSettingsInfoCache == null)
                {
                    if (File.Exists(unlockerFilePath))
                    {
                        string json = File.ReadAllText(unlockerFilePath);
                        unlockerSettingsInfoCache = JsonConvert.DeserializeObject(json) as UnlockerSettingsInfo;
                    }
                    else
                    {
                        unlockerSettingsInfoCache = new UnlockerSettingsInfo { LockProximity = 30, UnlockProximity = 50, LockTimeoutSeconds = 3, };
                    }
                }
            }

            return unlockerSettingsInfoCache;
        }

        public Task UpdateSettingsAsync(UnlockerSettingsInfo settings)
        {
            if (settings == null)
            {
                Debug.Assert(false);
                throw new ArgumentNullException(nameof(settings));
            }

            var oldSettings = unlockerSettingsInfoCache;

            return Task.Run(() =>
            {
                lock (lockObjForUnlockerSettings)
                {
                    string json = JsonConvert.SerializeObject(settings);

                    if (!Directory.Exists(unlockerFilePath))
                    {
                        Directory.CreateDirectory(settingsPath);
                    }
                    File.WriteAllText(unlockerFilePath, json);

                    unlockerSettingsInfoCache = settings;
                }

                if (SettingsUpdated != null)
                {
                    var @event = SettingsUpdated;
                    Task.Run(() => @event.Invoke(this, new UnlockerSettingsInfoEventArgs(oldSettings, settings)));
                }
            });
        }
    }
}