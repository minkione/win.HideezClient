using Hideez.SDK.Communication.Log;
using Microsoft.Win32;
using System;

namespace HideezMiddleware.ApplicationModeProvider
{
    /// <summary>
    /// Implementation of <see cref="IApplicationModeProvider"/> that retrieves
    /// application mode from hideez registry key
    /// </summary>
    public sealed class ApplicationModeRegistryProvider : Logger, IApplicationModeProvider
    {
        const string REG_MODE_NAME = "mode";
        readonly RegistryKey _registryRootKey;

        ApplicationMode _mode;

        public ApplicationModeRegistryProvider(RegistryKey rootKey, ILog log)
            : base(nameof(ApplicationModeRegistryProvider), log)
        {
            _registryRootKey = rootKey ?? throw new ArgumentNullException(nameof(rootKey));

            _mode = ReadModeFromRegistry();
        }

        ApplicationMode ReadModeFromRegistry()
        {
            try
            {
                var modeStr = _registryRootKey.GetValue(REG_MODE_NAME) as string;

                if (int.TryParse(modeStr, out int mode))
                {
                    if (mode == 0)
                        return ApplicationMode.Standalone;

                    if (mode == 1)
                        return ApplicationMode.Enterprise;
                }
            }
            catch (Exception ex)
            {
                WriteLine(ex);
            }

            WriteLine($"Failed to read application mode, defaulting to {ApplicationMode.Standalone}");
            return ApplicationMode.Standalone;
        }

        public ApplicationMode GetApplicationMode()
        {
            return _mode;
        }
    }
}
