using Hideez.SDK.Communication.Log;
using Microsoft.Win32;
using System;

namespace HideezMiddleware.ConnectionModeProvider
{
    public enum GlobalConnectionMode
    {
        WindowsBle,
        CsrDongle
    }

    public class ConnectionModeProvider : Logger, IConnectionModeProvider
    {
        const string REG_CSR_NAME = "use_hdongle";
        const string REG_WINBLE_NAME = "use_win10";
        readonly RegistryKey _registryRootKey;

        GlobalConnectionMode _mode;

        public bool IsWinBleMode { get => _mode == GlobalConnectionMode.WindowsBle; }
        public bool IsCsrMode { get => _mode == GlobalConnectionMode.CsrDongle; }

        public ConnectionModeProvider(RegistryKey rootKey, ILog log)
            : base(nameof(ConnectionModeProvider), log)
        {
            _registryRootKey = rootKey ?? throw new ArgumentNullException(nameof(rootKey));

            _mode = ReadModesFromRegistry();
        }

        GlobalConnectionMode ReadModesFromRegistry()
        {
            try
            {
                var csrModeStr = _registryRootKey.GetValue(REG_CSR_NAME) as string;

                if (int.TryParse(csrModeStr, out int csrMode))
                {
                    if (csrMode == 1)
                        return GlobalConnectionMode.CsrDongle;
                }

                var winBleModeStr = _registryRootKey.GetValue(REG_WINBLE_NAME) as string;

                if (int.TryParse(winBleModeStr, out int winBleMode))
                {
                    if (winBleMode == 1)
                        return GlobalConnectionMode.WindowsBle;
                }
            }
            catch (Exception ex)
            {
                WriteLine(ex);
            }

            WriteLine($"Failed to read connection mode for application, defaulting to {GlobalConnectionMode.WindowsBle}");
            return GlobalConnectionMode.WindowsBle;
        }

        public GlobalConnectionMode GetConnectionMode()
        {
            return _mode;
        }
    }
}
