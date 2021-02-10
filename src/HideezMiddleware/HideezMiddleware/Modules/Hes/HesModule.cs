using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.Utils;
using HideezMiddleware.IPC.IncommingMessages;
using HideezMiddleware.Modules.Hes.Messages;
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

        public HesModule(IHesAppConnection hesAppConnection, IHesAccessManager hesAccessManager, IMetaPubSub messenger, ILog log)
            : base(messenger, nameof(HesModule), log)
        {
            _hesAppConnection = hesAppConnection;
            _hesAccessManager = hesAccessManager;

            // Get HES address from registry ==================================
            // HKLM\SOFTWARE\Hideez\Client, client_hes_address REG_SZ
            var logger = new Logger(nameof(HesModule), log);
            string hesAddress = RegistrySettings.GetHesAddress(logger);

            // Allow self-signed SSL certificate for specified address
            EndpointCertificateManager.Enable();
            EndpointCertificateManager.AllowCertificate(hesAddress);

            _hesAccessManager.AccessRetracted += HesAccessManager_AccessRetracted;
            _hesAppConnection.HubProximitySettingsArrived += HesAppConnection_HubProximitySettingsArrived; // todo: handler in settings module
            _hesAppConnection.HubRFIDIndicatorStateArrived += HesAppConnection_HubRFIDIndicatorStateArrived; // todo: handler in settings module
            _hesAppConnection.HubConnectionStateChanged += HesAppConnection_HubConnectionStateChanged;
            _hesAppConnection.LockHwVaultStorageRequest += HesAppConnection_LockHwVaultStorageRequest;
            _hesAppConnection.LiftHwVaultStorageLockRequest += HesAppConnection_LiftHwVaultStorageLockRequest;
            _hesAppConnection.Alarm += HesAppConnection_Alarm;

            if (!string.IsNullOrWhiteSpace(hesAddress))
                _hesAppConnection.Start(hesAddress); // Launch HES connection immediatelly to save time

            _messenger.Subscribe<ChangeServerAddressMessage>(ChangeServerAddress);
        }

        private async void HesAccessManager_AccessRetracted(object sender, EventArgs e)
        {
            await _messenger.Publish(new HesAccessManager_AccessRetractedMessage(sender, e));
        }

        private async void HesAppConnection_HubProximitySettingsArrived(object sender, IReadOnlyList<DeviceProximitySettings> e)
        {
            await _messenger.Publish(new HesAppConnection_HubProximitySettingsArrivedMessage(sender, e));
        }

        private async void HesAppConnection_HubRFIDIndicatorStateArrived(object sender, bool isEnabled)
        {
            await _messenger.Publish(new HesAppConnection_HUbRFIDIndicatorStateArrivedMessage(sender, isEnabled));
        }
        private async void HesAppConnection_HubConnectionStateChanged(object sender, EventArgs e)
        {
            await _messenger.Publish(new HesAppConnection_HubConnectionStateChangedMessage(sender, e));
        }
        private async void HesAppConnection_LockHwVaultStorageRequest(object sender, string serialNo)
        {
            await _messenger.Publish(new HesAppConnection_LockHwVaultStorageMessage(sender, serialNo));
        }

        private async void HesAppConnection_LiftHwVaultStorageLockRequest(object sender, string serialNo)
        {
            await _messenger.Publish(new HesAppConnection_LiftHwVaultStorageLockMessage(sender, serialNo));
        }

        private async void HesAppConnection_Alarm(object sender, bool isEnabled)
        {
            await _messenger.Publish(new HesAppConnection_AlarmMessage(sender, isEnabled));
        }

        private async Task ChangeServerAddress(ChangeServerAddressMessage args)
        {
            try
            {
                var address = args.ServerAddress;
                WriteLine($"Client requested HES address change to \"{address}\"");

                if (string.IsNullOrWhiteSpace(address))
                {
                    WriteLine($"Clearing server address and shutting down connection");
                    RegistrySettings.SetHesAddress(this, address);
                    await _hesAppConnection.Stop();
                    await SafePublish(new ChangeServerAddressMessageReply(ChangeServerAddressResult.Success));
                }
                else
                {
                    EndpointCertificateManager.AllowCertificate(address);
                    var connectedOnNewAddress = await HubConnectivityChecker.CheckHubConnectivity(address, _log).TimeoutAfter(30_000);
                    if (connectedOnNewAddress)
                    {
                        WriteLine($"Passed connectivity check to {address}");
                        RegistrySettings.SetHesAddress(this, address);
                        await _hesAppConnection.Stop();
                        EndpointCertificateManager.DisableAllCertificates();
                        EndpointCertificateManager.AllowCertificate(address);
                        _hesAppConnection.Start(address);

                        await SafePublish(new ChangeServerAddressMessageReply(ChangeServerAddressResult.Success));
                    }
                    else
                    {
                        WriteLine($"Failed connectivity check to {address}");
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
    }
}
