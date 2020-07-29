using System;
using System.Linq;
using System.Threading.Tasks;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using HideezMiddleware.IPC.Messages;
using HideezMiddleware.Settings;
using Meta.Lib.Modules.PubSub;
using Meta.Lib.Modules.PubSub.Messages;

namespace HideezMiddleware
{
    public class StatusManager : Logger, IDisposable
    {
        readonly HesAppConnection _hesConnection;
        readonly HesAppConnection _tbHesConnection;
        readonly RfidServiceConnection _rfidService;
        readonly IBleConnectionManager _connectionManager;
        readonly IClientUiManager _uiClientManager;
        readonly ISettingsManager<RfidSettings> _rfidSettingsManager;
        readonly IWorkstationUnlocker _workstationUnlocker;

        public StatusManager(HesAppConnection hesConnection,
            HesAppConnection tbHesConnection,
            RfidServiceConnection rfidService,
            IBleConnectionManager connectionManager,
            IClientUiManager clientUiManager,
            ISettingsManager<RfidSettings> rfidSettingsManager,
            IWorkstationUnlocker workstationUnlocker,
            ILog log)
            : base(nameof(StatusManager), log)
        {
            _hesConnection = hesConnection;
            _tbHesConnection = tbHesConnection;
            _rfidService = rfidService;
            _connectionManager = connectionManager;
            _uiClientManager = clientUiManager;
            _rfidSettingsManager = rfidSettingsManager;
            _workstationUnlocker = workstationUnlocker;

            _rfidService.RfidServiceStateChanged += RfidService_RfidServiceStateChanged;
            _rfidService.RfidReaderStateChanged += RfidService_RfidReaderStateChanged;
            _connectionManager.AdapterStateChanged += ConnectionManager_AdapterStateChanged;
            _rfidSettingsManager.SettingsChanged += RfidSettingsManager_SettingsChanged;

            if (_hesConnection != null)
                _hesConnection.HubConnectionStateChanged += HesConnection_HubConnectionStateChanged;

            if(_tbHesConnection != null)
                _tbHesConnection.HubConnectionStateChanged += TryAndBuyHesConnection_HubConnectionStateChanged;

            if (_workstationUnlocker != null)
                _workstationUnlocker.Connected += WorkstationUnlocker_Connected;
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
                _rfidService.RfidServiceStateChanged -= RfidService_RfidServiceStateChanged;
                _rfidService.RfidReaderStateChanged -= RfidService_RfidReaderStateChanged;
                _connectionManager.AdapterStateChanged -= ConnectionManager_AdapterStateChanged;

                if (_hesConnection != null)
                    _hesConnection.HubConnectionStateChanged -= HesConnection_HubConnectionStateChanged;

                if (_tbHesConnection != null)
                    _tbHesConnection.HubConnectionStateChanged -= TryAndBuyHesConnection_HubConnectionStateChanged;

                if (_workstationUnlocker != null)
                    _workstationUnlocker.Connected -= WorkstationUnlocker_Connected;
            }

            disposed = true;
        }

        ~StatusManager()
        {
            Dispose(false);
        }
        #endregion

        async void HesConnection_HubConnectionStateChanged(object sender, EventArgs e)
        {
            await SendStatusToUI();
        }

        async void ConnectionManager_AdapterStateChanged(object sender, EventArgs e)
        {
            await SendStatusToUI();
        }

        async void RfidService_RfidReaderStateChanged(object sender, EventArgs e)
        {
            await SendStatusToUI();
        }

        async void RfidService_RfidServiceStateChanged(object sender, EventArgs e)
        {
            await SendStatusToUI();
        }

        async void TryAndBuyHesConnection_HubConnectionStateChanged(object sender, EventArgs e)
        {
            await SendStatusToUI();
        }

        async void RfidSettingsManager_SettingsChanged(object sender, SettingsChangedEventArgs<RfidSettings> e)
        {
            await SendStatusToUI();
        }

        async void WorkstationUnlocker_Connected(object sender, EventArgs e)
        {
            await Task.Delay(200);
            await SendStatusToUI();
        }

        public async Task SendStatusToUI()
        {
            try
            {
                var hesStatus = GetHesStatus();

                var rfidStatus = GetRfidStatus();

                var bluetoothStatus = GetBluetoothStatus();

                var tbHesStatus = GetTBHesStatus();

                await _uiClientManager.SendStatus(hesStatus, tbHesStatus, rfidStatus, bluetoothStatus);
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
            if (!_rfidSettingsManager.Settings.IsRfidEnabled)
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
            if (_hesConnection != null)
            {
                if (_hesConnection.State == HesConnectionState.Connected)
                    return HesStatus.Ok;

                if (_hesConnection.State == HesConnectionState.NotApproved)
                    return HesStatus.NotApproved;
            }
            
            return HesStatus.HesNotConnected;
        }

        HesStatus GetTBHesStatus()
        {
            if (_tbHesConnection != null)
            {
                if (_tbHesConnection.State == HesConnectionState.Connected)
                    return HesStatus.Ok;

                if (_tbHesConnection.State == HesConnectionState.NotApproved)
                    return HesStatus.NotApproved;
            }

            return HesStatus.HesNotConnected;
        }
    }
}
