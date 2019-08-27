using System;
using System.Collections.Generic;
using System.Linq;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.Log;
using HideezMiddleware.Settings;

namespace HideezMiddleware
{
    public class StatusManager : Logger, IDisposable
    {
        readonly HesAppConnection _hesConnection;
        readonly RfidServiceConnection _rfidService;
        readonly IBleConnectionManager _connectionManager;
        readonly IClientUiProxy _ui;
        readonly ISettingsManager<UnlockerSettings> _unlockerSettingsManager;

        public StatusManager(HesAppConnection hesConnection,
            RfidServiceConnection rfidService,
            IBleConnectionManager connectionManager,
            IClientUiProxy ui,
            ISettingsManager<UnlockerSettings> unlockerSettingsManager,
            ILog log)
            : base(nameof(StatusManager), log)
        {
            _hesConnection = hesConnection;
            _rfidService = rfidService;
            _connectionManager = connectionManager;
            _ui = ui;
            _unlockerSettingsManager = unlockerSettingsManager;

            _ui.ClientConnected += Ui_ClientUiConnected;
            _rfidService.RfidServiceStateChanged += RfidService_RfidServiceStateChanged;
            _rfidService.RfidReaderStateChanged += RfidService_RfidReaderStateChanged;
            _connectionManager.AdapterStateChanged += ConnectionManager_AdapterStateChanged;
            _unlockerSettingsManager.SettingsChanged += UnlockerSettingsManager_SettingsChanged;

            if (_hesConnection != null)
                _hesConnection.HubConnectionStateChanged += HesConnection_HubConnectionStateChanged;
        }

        private void UnlockerSettingsManager_SettingsChanged(object sender, SettingsChangedEventArgs<UnlockerSettings> e)
        {
            SendStatusToUI();
        }

        #region IDisposable
        bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Release managed resources here
                _ui.ClientConnected -= Ui_ClientUiConnected;
                _rfidService.RfidServiceStateChanged -= RfidService_RfidServiceStateChanged;
                _rfidService.RfidReaderStateChanged -= RfidService_RfidReaderStateChanged;
                _connectionManager.AdapterStateChanged -= ConnectionManager_AdapterStateChanged;

                if (_hesConnection != null)
                    _hesConnection.HubConnectionStateChanged -= HesConnection_HubConnectionStateChanged;
            }

            disposed = true;
        }

        ~StatusManager()
        {
            Dispose(false);
        }
        #endregion

        void Ui_ClientUiConnected(object sender, EventArgs e)
        {
            SendStatusToUI();
        }

        void HesConnection_HubConnectionStateChanged(object sender, EventArgs e)
        {
            SendStatusToUI();
        }

        void ConnectionManager_AdapterStateChanged(object sender, EventArgs e)
        {
            SendStatusToUI();
        }

        void RfidService_RfidReaderStateChanged(object sender, EventArgs e)
        {
            SendStatusToUI();
        }

        void RfidService_RfidServiceStateChanged(object sender, EventArgs e)
        {
            SendStatusToUI();
        }

        async void SendStatusToUI()
        {
            try
            {
                var hesStatus = GetHesStatus();

                var rfidStatus = GetRfidStatus();

                var bluetoothStatus = GetBluetoothStatus();

                await _ui.SendStatus(hesStatus, rfidStatus, bluetoothStatus);
            }
            catch (Exception ex)
            {
                WriteLine(ex);
            }
        }

        BluetoothStatus GetBluetoothStatus()
        {
            switch (_connectionManager.State)
            {
                case BluetoothAdapterState.PoweredOn:
                case BluetoothAdapterState.LoadingKnownDevices:
                    return BluetoothStatus.Ok;
                case BluetoothAdapterState.Unknown:
                    return BluetoothStatus.Unknown;
                case BluetoothAdapterState.Resetting:
                    return BluetoothStatus.Resetting;
                case BluetoothAdapterState.Unsupported:
                    return BluetoothStatus.Unsupported;
                case BluetoothAdapterState.Unauthorized:
                    return BluetoothStatus.Unauthorized;
                case BluetoothAdapterState.PoweredOff:
                    return BluetoothStatus.PoweredOff;
                default:
                    return BluetoothStatus.Unknown;
            }
        }

        RfidStatus GetRfidStatus()
        {
            if (_unlockerSettingsManager.Settings != null && !_unlockerSettingsManager.Settings.DeviceUnlockerSettings.Any(ds => ds.AllowRfid))
                return RfidStatus.Disabled;
            else if (!_rfidService.ServiceConnected)
                return RfidStatus.RfidServiceNotConnected;
            else if (!_rfidService.ReaderConnected)
                return RfidStatus.RfidReaderNotConnected;
            else
                return RfidStatus.Ok;
        }

        HesStatus GetHesStatus()
        {
            if (_hesConnection == null || _hesConnection.State == HesConnectionState.Disconnected)
                return HesStatus.HesNotConnected;
            else
                return HesStatus.Ok;
        }
    }
}
