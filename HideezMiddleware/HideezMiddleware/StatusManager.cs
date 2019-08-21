using System;
using System.Collections.Generic;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.HES.Client;
using NLog;

namespace HideezMiddleware
{
    public class StatusManager : IDisposable
    {
        readonly ILogger _log = LogManager.GetCurrentClassLogger();

        readonly HesAppConnection _hesConnection;
        readonly RfidServiceConnection _rfidService;
        readonly IBleConnectionManager _connectionManager;
        readonly CredentialProviderProxy _credentialProviderConnection;
        readonly UiProxyManager _ui;

        public StatusManager(HesAppConnection hesConnection,
            RfidServiceConnection rfidService,
            IBleConnectionManager connectionManager,
            CredentialProviderProxy credentialProviderConnection,
            UiProxyManager ui)
        {
            _hesConnection = hesConnection;
            _rfidService = rfidService;
            _connectionManager = connectionManager;
            _credentialProviderConnection = credentialProviderConnection;
            _ui = ui;

            _credentialProviderConnection.OnProviderConnected += CredentialProviderConnection_OnProviderConnected;
            _hesConnection.HubConnectionStateChanged += HesConnection_HubConnectionStateChanged;
            _rfidService.RfidServiceStateChanged += RfidService_RfidServiceStateChanged;
            _rfidService.RfidReaderStateChanged += RfidService_RfidReaderStateChanged;
            _connectionManager.AdapterStateChanged += ConnectionManager_AdapterStateChanged;
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
                _credentialProviderConnection.OnProviderConnected -= CredentialProviderConnection_OnProviderConnected;
                _hesConnection.HubConnectionStateChanged -= HesConnection_HubConnectionStateChanged;
                _rfidService.RfidServiceStateChanged -= RfidService_RfidServiceStateChanged;
                _rfidService.RfidReaderStateChanged -= RfidService_RfidReaderStateChanged;
                _connectionManager.AdapterStateChanged -= ConnectionManager_AdapterStateChanged;
            }

            disposed = true;
        }

        ~StatusManager()
        {
            Dispose(false);
        }
        #endregion

        void CredentialProviderConnection_OnProviderConnected(object sender, EventArgs e)
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
                var bluetoothStatus = GetBluetoothStatus();

                var rfidStatus = GetRfidStatus();
                
                var hesStatus = GetHesStatus();

                await _ui.SendStatus(bluetoothStatus, rfidStatus, hesStatus);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
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
            if (!_rfidService.ServiceConnected)
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
