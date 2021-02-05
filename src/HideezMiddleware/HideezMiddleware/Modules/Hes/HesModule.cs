using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.Log;
using HideezMiddleware.Modules.Hes.Messages;
using Meta.Lib.Modules.PubSub;
using System;
using System.Collections.Generic;

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


            // TODO: Event handling
            //hesConnection.HubProximitySettingsArrived += async (s, e) => await _service.Messenger.Publish(new HES_HubProximitySettingsArrivedMessage());

            /*
            hesConnection.HubProximitySettingsArrived += async (sender, receivedSettings) =>
            {
                var settings = await proximitySettingsManager.GetSettingsAsync().ConfigureAwait(false);
                settings.DevicesProximity = receivedSettings.ToArray();
                proximitySettingsManager.SaveSettings(settings);
            };
            hesConnection.HubRFIDIndicatorStateArrived += async (sender, isEnabled) =>
            {
                var settings = await rfidSettingsManager.GetSettingsAsync().ConfigureAwait(false);
                settings.IsRfidEnabled = isEnabled;
                rfidSettingsManager.SaveSettings(settings);
            };
            hesConnection.HubConnectionStateChanged += HES_ConnectionStateChanged;
            hesConnection.LockHwVaultStorageRequest += HES_LockDeviceStorageRequest;
            hesConnection.LiftHwVaultStorageLockRequest += HES_LiftDeviceStorageLockRequest;
            hesConnection.Alarm += HesConnection_Alarm;
            */

            if (!string.IsNullOrWhiteSpace(hesAddress))
                _hesAppConnection.Start(hesAddress); // Launch HES connection immediatelly to save time
        }

        private async void HesAccessManager_AccessRetracted(object sender, EventArgs e)
        {
            await _messenger.Publish(new HesAccessManager_AccessRetractedMessage(sender, e));
        }

        private async void HesAppConnection_HubProximitySettingsArrived(object sender, IReadOnlyList<DeviceProximitySettings> e)
        {
            await _messenger.Publish(new HesAppConnection_HubProximitySettingsArrivedMessage(sender, e));
        }

        private async void HesAppConnection_HubRFIDIndicatorStateArrived(object sender, bool e)
        {
            await _messenger.Publish(new HesAppConnection_HUbRFIDIndicatorStateArrivedMessage(sender, e));
        }
        private async void HesAppConnection_HubConnectionStateChanged(object sender, EventArgs e)
        {
            await _messenger.Publish(new HesAppConnection_HubConnectionStateChangedMessage(sender, e));
        }
        private async void HesAppConnection_LockHwVaultStorageRequest(object sender, string e)
        {
            await _messenger.Publish(new HesAppConnection_LockHwVaultStorageMessage(sender, e));
        }

        private async void HesAppConnection_LiftHwVaultStorageLockRequest(object sender, string e)
        {
            await _messenger.Publish(new HesAppConnection_LiftHwVaultStorageLockMessage(sender, e));
        }

        private async void HesAppConnection_Alarm(object sender, bool e)
        {
            await _messenger.Publish(new HesAppConnection_AlarmMessage(sender, e));
        }
    }
}
