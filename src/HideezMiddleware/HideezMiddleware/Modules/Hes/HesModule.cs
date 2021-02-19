using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.Utils;
using HideezMiddleware.IPC.IncommingMessages;
using HideezMiddleware.Modules.Hes.Messages;
using HideezMiddleware.Modules.ServiceEvents.Messages;
using Meta.Lib.Modules.PubSub;
using System;
using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;

namespace HideezMiddleware.Modules.Hes
{
    public sealed class HesModule : ModuleBase
    {
        readonly IHesAppConnection _hesAppConnection;
        readonly IHesAccessManager _hesAccessManager;
        string _address = string.Empty;

        public HesModule(IHesAppConnection hesAppConnection, IHesAccessManager hesAccessManager, IMetaPubSub messenger, ILog log)
            : base(messenger, nameof(HesModule), log)
        {
            _hesAppConnection = hesAppConnection;
            _hesAccessManager = hesAccessManager;

            // Get HES address from registry ==================================
            // HKLM\SOFTWARE\Hideez\Client, client_hes_address REG_SZ
            var logger = new Logger(nameof(HesModule), log);
            _address = RegistrySettings.GetHesAddress(logger);

            // Allow self-signed SSL certificate for specified address
            EndpointCertificateManager.Enable();
            EndpointCertificateManager.AllowCertificate(_address);

            _hesAccessManager.AccessRetracted += HesAccessManager_AccessRetracted;
            _hesAppConnection.HubProximitySettingsArrived += HesAppConnection_HubProximitySettingsArrived;
            _hesAppConnection.HubRFIDIndicatorStateArrived += HesAppConnection_HubRFIDIndicatorStateArrived;
            _hesAppConnection.HubConnectionStateChanged += HesAppConnection_HubConnectionStateChanged;
            _hesAppConnection.LockHwVaultStorageRequest += HesAppConnection_LockHwVaultStorageRequest;
            _hesAppConnection.LiftHwVaultStorageLockRequest += HesAppConnection_LiftHwVaultStorageLockRequest;
            _hesAppConnection.Alarm += HesAppConnection_Alarm;

            if (!string.IsNullOrWhiteSpace(_address))
                _hesAppConnection.Start(_address); // Launch HES connection immediatelly to save time
            else
                Task.Run(async () =>
                {
                    // We still need to notify about initial HesAppConnection state
                    await SafePublish(
                        new HesAppConnection_HubConnectionStateChangedMessage(_hesAppConnection, EventArgs.Empty));
                });

            _messenger.Subscribe(GetSafeHandler<ChangeServerAddressMessage>(ChangeServerAddress));

            _messenger.Subscribe(GetSafeHandler<PowerEventMonitor_SystemSuspendingMessage>(OnSystemSuspending));
            _messenger.Subscribe(GetSafeHandler<PowerEventMonitor_SystemLeftSuspendedModeMessage>(OnSystemLeftSuspendedMode));
        }

        private async void HesAccessManager_AccessRetracted(object sender, EventArgs e)
        {
            await SafePublish(new HesAccessManager_AccessRetractedMessage(sender, e));
        }

        private async void HesAppConnection_HubProximitySettingsArrived(object sender, IReadOnlyList<DeviceProximitySettings> e)
        {
            await SafePublish(new HesAppConnection_HubProximitySettingsArrivedMessage(sender, e));
        }

        private async void HesAppConnection_HubRFIDIndicatorStateArrived(object sender, bool isEnabled)
        {
            await SafePublish(new HesAppConnection_HUbRFIDIndicatorStateArrivedMessage(sender, isEnabled));
        }
        private async void HesAppConnection_HubConnectionStateChanged(object sender, EventArgs e)
        {
            await SafePublish(new HesAppConnection_HubConnectionStateChangedMessage(sender, e));
        }
        private async void HesAppConnection_LockHwVaultStorageRequest(object sender, string serialNo)
        {
            await SafePublish(new HesAppConnection_LockHwVaultStorageMessage(sender, serialNo));
        }

        private async void HesAppConnection_LiftHwVaultStorageLockRequest(object sender, string serialNo)
        {
            await SafePublish(new HesAppConnection_LiftHwVaultStorageLockMessage(sender, serialNo));
        }

        private async void HesAppConnection_Alarm(object sender, bool isEnabled)
        {
            await SafePublish(new HesAppConnection_AlarmMessage(sender, isEnabled));
        }

        private async Task ChangeServerAddress(ChangeServerAddressMessage args)
        {
            try
            {
                var oldAddress = _address;
                _address = args.ServerAddress;
                WriteLine($"Client requested HES address change to \"{_address}\"");

                if (string.IsNullOrWhiteSpace(_address))
                {
                    WriteLine($"Clearing server address and shutting down connection");
                    RegistrySettings.SetHesAddress(this, _address);
                    await _hesAppConnection.Stop();
                    await SafePublish(new ChangeServerAddressMessageReply(ChangeServerAddressResult.Success));
                }
                else
                {
                    EndpointCertificateManager.AllowCertificate(_address);
                    var connectedOnNewAddress = await HubConnectivityChecker.CheckHubConnectivity(_address, _log).TimeoutAfter(30_000);
                    if (connectedOnNewAddress)
                    {
                        WriteLine($"Passed connectivity check to {_address}");
                        RegistrySettings.SetHesAddress(this, _address);
                        await _hesAppConnection.Stop();
                        EndpointCertificateManager.DisableCertificate(oldAddress);
                        EndpointCertificateManager.AllowCertificate(_address);
                        _hesAppConnection.Start(_address);

                        await SafePublish(new ChangeServerAddressMessageReply(ChangeServerAddressResult.Success));
                    }
                    else
                    {
                        WriteLine($"Failed connectivity check to {_address}");
                        await SafePublish(new ChangeServerAddressMessageReply(ChangeServerAddressResult.ConnectionTimedOut));
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLine(ex.Message, LogErrorSeverity.Information);
                EndpointCertificateManager.DisableCertificate(args.ServerAddress);

                if (ex is TimeoutException)
                    await SafePublish(new ChangeServerAddressMessageReply(ChangeServerAddressResult.ConnectionTimedOut));
                else if (ex is KeyNotFoundException)
                    await SafePublish(new ChangeServerAddressMessageReply(ChangeServerAddressResult.KeyNotFound));
                else if (ex is UnauthorizedAccessException)
                    await SafePublish(new ChangeServerAddressMessageReply(ChangeServerAddressResult.UnauthorizedAccess));
                else if (ex is SecurityException)
                    await SafePublish(new ChangeServerAddressMessageReply(ChangeServerAddressResult.SecurityError));
                else
                    await SafePublish(new ChangeServerAddressMessageReply(ChangeServerAddressResult.UnknownError));
            }
        }

        private async Task OnSystemSuspending(PowerEventMonitor_SystemSuspendingMessage msg)
        {
            try
            {
                await _hesAppConnection.Stop();
            }
            catch (Exception ex)
            {
                WriteLine(ex);
            }
        }

        private async Task OnSystemLeftSuspendedMode(PowerEventMonitor_SystemLeftSuspendedModeMessage msg)
        {
            WriteLine("Starting restore from suspended mode");
            try
            {
                await _hesAppConnection.Stop();
                if (!string.IsNullOrWhiteSpace(_address))
                    _hesAppConnection.Start();
            }
            catch (Exception ex)
            {
                WriteLine(ex);
            }
        }
    }
}
