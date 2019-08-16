using System;
using System.Collections.Generic;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.HES.Client;
using NLog;

namespace HideezMiddleware
{
    public class StatusManager
    {
        readonly ILogger _log = LogManager.GetCurrentClassLogger();

        readonly RfidServiceConnection _rfidService;
        readonly IBleConnectionManager _connectionManager;
        readonly UiProxy _ui;

        //todo = make read only
        HesAppConnection _hesConnection;

        public StatusManager(HesAppConnection hesConnection,
            RfidServiceConnection rfidService,
            IBleConnectionManager connectionManager,
            UiProxy ui)
        {
            _rfidService = rfidService;
            _connectionManager = connectionManager;
            _ui = ui;

            _rfidService.RfidServiceStateChanged += RfidService_RfidServiceStateChanged;
            _rfidService.RfidReaderStateChanged += RfidService_RfidReaderStateChanged;
            _connectionManager.AdapterStateChanged += ConnectionManager_AdapterStateChanged;

            SetHes(hesConnection);
        }

        //todo - remove (replace with HesConnectionManager)
        void SetHes(HesAppConnection hesConnection)
        {
            if (_hesConnection != null)
            {
                _hesConnection.HubConnectionStateChanged -= HesConnection_HubConnectionStateChanged;
                _hesConnection = null;
            }

            if (hesConnection != null)
            {
                _hesConnection = hesConnection;
                _hesConnection.HubConnectionStateChanged += HesConnection_HubConnectionStateChanged;
            }
        }

        void CredentialProviderConnection_OnProviderConnected(object sender, EventArgs e)
        {
            SendStatusToCredentialProvider();
        }

        void HesConnection_HubConnectionStateChanged(object sender, EventArgs e)
        {
            SendStatusToCredentialProvider();
        }

        void ConnectionManager_AdapterStateChanged(object sender, EventArgs e)
        {
            SendStatusToCredentialProvider();
        }

        void RfidService_RfidReaderStateChanged(object sender, EventArgs e)
        {
            SendStatusToCredentialProvider();
        }

        void RfidService_RfidServiceStateChanged(object sender, EventArgs e)
        {
            SendStatusToCredentialProvider();
        }

        async void SendStatusToCredentialProvider()
        {
            try
            {
                var statuses = new List<string>();

                // Bluetooth
                switch (_connectionManager.State)
                {
                    case BluetoothAdapterState.PoweredOn:
                    case BluetoothAdapterState.LoadingKnownDevices:
                        break;
                    default:
                        statuses.Add($"Bluetooth not available (state: {_connectionManager.State})");
                        break;
                }

                // RFID
                if (!_rfidService.ServiceConnected)
                    statuses.Add("RFID service not connected");
                else if (!_rfidService.ReaderConnected)
                    statuses.Add("RFID reader not connected");

                // Server
                if (_hesConnection == null || _hesConnection.State == HesConnectionState.Disconnected)
                    statuses.Add("HES not connected");

                if (statuses.Count > 0)
                    await _ui.SendStatus($"ERROR: {string.Join("; ", statuses)}");
                else
                    await _ui.SendStatus(string.Empty);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
        }
    }
}
