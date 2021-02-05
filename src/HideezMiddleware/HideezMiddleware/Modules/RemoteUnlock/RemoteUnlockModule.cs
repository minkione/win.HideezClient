using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.Workstation;
using HideezMiddleware.Settings;
using HideezMiddleware.SoftwareVault.UnlockToken;
using HideezMiddleware.Utils.WorkstationHelper;
using Meta.Lib.Modules.PubSub;
using Microsoft.Win32;
using System.Threading.Tasks;

namespace HideezMiddleware.Modules.RemoteUnlock
{
    public sealed class RemoteUnlockModule : ModuleBase
    {
        public RemoteUnlockModule(ISettingsManager<ServiceSettings> serviceSettingsManager,
            RegistryKey rootKey,
            IWorkstationInfoProvider workstationInfoProvider,
            IWorkstationHelper workstationHelper,
            IHesAppConnection tbHesAppConnection,
            IWorkstationUnlocker workstationUnlocker,
            IMetaPubSub messenger,
            ILog log)
            : base(messenger, nameof(RemoteUnlockModule), log)
        {
            var _unlockTokenProvider = new UnlockTokenProvider(rootKey, log);
            var _unlockTokenGenerator = new UnlockTokenGenerator(_unlockTokenProvider, workstationInfoProvider, workstationHelper, log);
            var _tbHesAppConnection = tbHesAppConnection;
            var _remoteWorkstationUnlocker = new RemoteWorkstationUnlocker(_unlockTokenProvider, _tbHesAppConnection, workstationUnlocker, log);

            if (serviceSettingsManager.Settings.EnableSoftwareVaultUnlock)
                Task.Run(_unlockTokenGenerator.Start);

            _tbHesAppConnection.Start("https://testhub.hideez.com/"); // Launch Try&Buy immediatelly to reduce loading time

            if (serviceSettingsManager.Settings.EnableSoftwareVaultUnlock)
                _remoteWorkstationUnlocker.Start();
        }
    }
}
