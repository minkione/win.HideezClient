using Hideez.SDK.Communication.Connection;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using HideezMiddleware.CredentialProvider;
using HideezMiddleware.DeviceConnection;
using HideezMiddleware.IPC.Messages;
using HideezMiddleware.Modules.ServiceEvents.Messages;
using HideezMiddleware.Modules.WinBle.Messages;
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

            _messenger.Subscribe(GetSafeHandler<CredentialProvider_CommandLinkPressedMessage>(CredentialProvider_CommandLinkPressedHandler));
            _messenger.Subscribe(GetSafeHandler<PowerEventMonitor_SystemSuspendingMessage>(OnSystemSuspending));
            _messenger.Subscribe(GetSafeHandler<PowerEventMonitor_SystemLeftSuspendedModeMessage>(OnSystemLeftSuspendedMode));

            _winBleAutomaticConnectionProcessor.Start();
        }

        private async void WinBleConnectionManager_AdapterStateChanged(object sender, EventArgs e)
        {
            BluetoothStatus status;
            switch (_winBleConnectionManager.State)
            {
                case BluetoothAdapterState.PoweredOn:
                case BluetoothAdapterState.LoadingKnownDevices:
                    status = BluetoothStatus.Ok;
                    break;
                case BluetoothAdapterState.Unknown:
                    status = BluetoothStatus.Unknown;
                    break;
                case BluetoothAdapterState.Resetting:
                    status = BluetoothStatus.Resetting;
                    break;
                case BluetoothAdapterState.Unsupported:
                    status = BluetoothStatus.Unsupported;
                    break;
                case BluetoothAdapterState.Unauthorized:
                    status = BluetoothStatus.Unauthorized;
                    break;
                case BluetoothAdapterState.PoweredOff:
                    status = BluetoothStatus.PoweredOff;
                    break;
                default:
                    status = BluetoothStatus.Unknown;
                    break;
            }

            await SafePublish(new WinBleStatusChangedMessage(sender, status));
        }

        private Task CredentialProvider_CommandLinkPressedHandler(CredentialProvider_CommandLinkPressedMessage msg)
        {
            _advertisementIgnoreList.Clear();

            return Task.CompletedTask;
        }

        private Task OnSystemSuspending(PowerEventMonitor_SystemSuspendingMessage arg)
        {
            _winBleAutomaticConnectionProcessor.Stop();
            return Task.CompletedTask;
        }

        private Task OnSystemLeftSuspendedMode(PowerEventMonitor_SystemLeftSuspendedModeMessage msg)
        {
            WriteLine("Starting restore from suspended mode");

            _winBleConnectionManager.Stop();
            _winBleAutomaticConnectionProcessor.Stop();

            _winBleAutomaticConnectionProcessor.Start();
            _winBleConnectionManager.Start();

            return Task.CompletedTask;
        }
    }
}
