using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.Workstation;
using HideezMiddleware.IPC.IncommingMessages;
using HideezMiddleware.Modules.RemoteUnlock.Messages;
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
        readonly ISettingsManager<ServiceSettings> _serviceSettingsManager;
        readonly UnlockTokenProvider _unlockTokenProvider;
        readonly UnlockTokenGenerator _unlockTokenGenerator;
        readonly IHesAppConnection _tbHesAppConnection;
        readonly RemoteWorkstationUnlocker _remoteWorkstationUnlocker;

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
            _serviceSettingsManager = serviceSettingsManager;

            _unlockTokenProvider = new UnlockTokenProvider(rootKey, log);
            _unlockTokenGenerator = new UnlockTokenGenerator(_unlockTokenProvider, workstationInfoProvider, workstationHelper, log);
            _tbHesAppConnection = tbHesAppConnection;
            _remoteWorkstationUnlocker = new RemoteWorkstationUnlocker(_unlockTokenProvider, _tbHesAppConnection, workstationUnlocker, log);

            _tbHesAppConnection.HubConnectionStateChanged += TBHesAppConnection_HubConnectionStateChanged;

            if (_serviceSettingsManager.Settings.EnableSoftwareVaultUnlock)
                Task.Run(_unlockTokenGenerator.Start);

            _tbHesAppConnection.Start("https://testhub.hideez.com/"); // Launch Try&Buy immediatelly to reduce loading time

            if (_serviceSettingsManager.Settings.EnableSoftwareVaultUnlock)
                _remoteWorkstationUnlocker.Start();

            _messenger.Subscribe(GetSafeHandler<SetSoftwareVaultUnlockModuleStateMessage>(SetSoftwareVaultUnlockModuleState));
        }

        private async void TBHesAppConnection_HubConnectionStateChanged(object sender, System.EventArgs e)
        {
            await SafePublish(new TBConnection_StateChangedMessage(sender, e));
        }

        Task SetSoftwareVaultUnlockModuleState(SetSoftwareVaultUnlockModuleStateMessage args)
        {
            var settings = _serviceSettingsManager.Settings;
            if (settings.EnableSoftwareVaultUnlock != args.Enabled)
            {
                WriteLine($"Client requested to switch software unlock module. New value: {args.Enabled}");
                settings.EnableSoftwareVaultUnlock = args.Enabled;
                _serviceSettingsManager.SaveSettings(settings);

                if (args.Enabled)
                {
                    _unlockTokenGenerator.Start();
                    _remoteWorkstationUnlocker.Start();
                }
                else
                {
                    _unlockTokenGenerator.Stop();
                    _remoteWorkstationUnlocker.Stop();
                    _unlockTokenGenerator.DeleteSavedToken();
                }
            }

            return Task.CompletedTask;
        }
    }
}
