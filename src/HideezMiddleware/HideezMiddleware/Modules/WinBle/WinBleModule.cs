using Hideez.SDK.Communication.Connection;
using Hideez.SDK.Communication.Log;
using HideezMiddleware.CredentialProvider;
using HideezMiddleware.DeviceConnection;
using HideezMiddleware.IPC.Messages;
using Meta.Lib.Modules.PubSub;
using System;
using System.Threading.Tasks;
using WinBle._10._0._18362;

namespace HideezMiddleware.Modules.WinBle
{
    public sealed class WinBleModule : ModuleBase
    {
        readonly AdvertisementIgnoreList _advertisementIgnoreList;
        readonly WinBleConnectionManager _winBleConnectionManager;
        readonly WinBleAutomaticConnectionProcessor _winBleAutomaticConnectionProcessor;
        readonly CommandLinkVisibilityController _commandLinkVisibilityController;
        readonly ConnectionManagerRestarter _connectionManagerRestarter;

        public WinBleModule(ConnectionManagersCoordinator connectionManagersCoordinator,
            ConnectionManagerRestarter connectionManagerRestarter,
            AdvertisementIgnoreList advertisementIgnoreList,
            WinBleConnectionManager winBleConnectionManager,
            WinBleAutomaticConnectionProcessor winBleAutomaticConnectionProcessor,
            CommandLinkVisibilityController commandLinkVisibilityController,
            IMetaPubSub messenger,
            ILog log)
            : base(messenger, nameof(WinBleModule), log)
        {
            _advertisementIgnoreList = advertisementIgnoreList;
            _winBleConnectionManager = winBleConnectionManager;
            _winBleAutomaticConnectionProcessor = winBleAutomaticConnectionProcessor;
            _commandLinkVisibilityController = commandLinkVisibilityController;
            _connectionManagerRestarter = connectionManagerRestarter;

            _winBleConnectionManager.AdapterStateChanged += WinBleConnectionManager_AdapterStateChanged;

            _connectionManagerRestarter.AddManager(_winBleConnectionManager);
            connectionManagersCoordinator.AddConnectionManager(_winBleConnectionManager);

            _messenger.Subscribe<CredentialProvider_CommandLinkPressedMessage>(CredentialProvider_CommandLinkPressedHandler);

            _winBleAutomaticConnectionProcessor.Start();
            _connectionManagerRestarter.Start();
        }

        private async void WinBleConnectionManager_AdapterStateChanged(object sender, EventArgs e)
        {
            await _messenger.Publish(new BleAdapterStateChangedMessage(_winBleConnectionManager, _winBleConnectionManager.State));
        }

        private Task CredentialProvider_CommandLinkPressedHandler(CredentialProvider_CommandLinkPressedMessage msg)
        {
            _advertisementIgnoreList.Clear();

            return Task.CompletedTask;
        }
    }
}
