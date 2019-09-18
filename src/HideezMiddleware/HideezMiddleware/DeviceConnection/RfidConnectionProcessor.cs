using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.Log;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HideezMiddleware.DeviceConnection
{
    public class RfidConnectionProcessor : Logger, IDisposable
    {
        readonly ConnectionFlowProcessor _connectionFlowProcessor;
        readonly IClientUiManager _clientUiManager;
        readonly HesAppConnection _hesConnection;
        readonly RfidServiceConnection _rfidService;
        readonly IScreenActivator _screenActivator;

        int _isConnecting = 0;

        public event EventHandler<string> WorkstationUnlockPerformed;

        public RfidConnectionProcessor(
            ConnectionFlowProcessor connectionFlowProcessor, 
            HesAppConnection hesConnection,
            RfidServiceConnection rfidService, 
            IScreenActivator screenActivator,
            IClientUiManager clientUiManager, 
            ILog log) 
            : base(nameof(RfidConnectionProcessor), log)
        {
            _connectionFlowProcessor = connectionFlowProcessor ?? throw new ArgumentNullException(nameof(connectionFlowProcessor));
            _hesConnection = hesConnection ?? throw new ArgumentNullException(nameof(hesConnection));
            _rfidService = rfidService ?? throw new ArgumentNullException(nameof(rfidService));
            _clientUiManager = clientUiManager ?? throw new ArgumentNullException(nameof(clientUiManager));
            _screenActivator = screenActivator;

            _rfidService.RfidReceivedEvent += RfidService_RfidReceivedEvent;
        }

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed = false;
        void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                _rfidService.RfidReceivedEvent -= RfidService_RfidReceivedEvent;
            }

            disposed = true;
        }

        ~RfidConnectionProcessor()
        {
            Dispose(false);
        }
        #endregion

        // Todo: Maybe add Start/Stop methods to RfidConnectionProcessor

        async void RfidService_RfidReceivedEvent(object sender, RfidReceivedEventArgs e)
        {
            await UnlockByRfid(e.Rfid);
        }

        async Task UnlockByRfid(string rfid)
        {
            if (Interlocked.CompareExchange(ref _isConnecting, 1, 1) == 1)
                return;

            try
            {
                _screenActivator?.ActivateScreen();
                await _clientUiManager.SendNotification("Connecting to the HES server...");

                if (_hesConnection == null)
                    throw new Exception("Cannot connect device. Not connected to the HES.");

                // get MAC address from the HES
                var info = await _hesConnection.GetInfoByRfid(rfid);

                if (info == null)
                    throw new Exception($"Device not found");

                if (Interlocked.CompareExchange(ref _isConnecting, 1, 0) == 0)
                {
                    try
                    {
                        var result = await _connectionFlowProcessor.ConnectAndUnlock(info.DeviceMac);

                        if (result.UnlockSuccessful)
                            WorkstationUnlockPerformed?.Invoke(this, info.DeviceMac);
                    }
                    catch (Exception)
                    {
                        // Silent handling. Log is already printed inside of _connectionFlowProcessor.ConnectAndUnlock()
                    }
                    finally
                    {
                        Interlocked.Exchange(ref _isConnecting, 0);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLine(ex);
                await _clientUiManager.SendNotification("");
                await _clientUiManager.SendError(ex.Message);
            }
        }

    }
}
