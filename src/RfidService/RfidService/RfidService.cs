using System;
using System.ServiceProcess;
using Hideez.SDK.Communication.Log;
using HideezMiddleware.COM;

namespace Hideez.RFID
{
    public partial class RfidService : ServiceBase
    {
        RfidConnection _connection;
        PipeConnectionsManager _pipeServer;
        EventLogger _log;
        ComPortManager _comPortManager;

        public RfidService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _log = new EventLogger("RFID");
            _log.WriteDebugLine("Service", "Started");

            _connection = new RfidConnection(_log);
            _connection.RfidReceived += Connection_RfidReceived;
            _connection.ReaderStateChanged += Connection_ReaderStateChanged;
            _connection.Start();

            _pipeServer = new PipeConnectionsManager(_log);
            _pipeServer.OnReaderUpdateRequest += PipeServer_OnReaderUpdateRequest;
            _pipeServer.Start();

            _comPortManager = new ComPortManager(_log);
            _comPortManager.RfidReceived += Connection_RfidReceived;
            _comPortManager.ReaderStateChanged += Connection_ReaderStateChanged;
            _comPortManager.Start();
        }

        private void Connection_ReaderStateChanged(object sender, ReaderStateChangedEventArgs e)
        {
            try
            {
                _pipeServer.WriteReaderState(_connection.IsConnected || _comPortManager.IsConnected);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void Connection_RfidReceived(object sender, RfidReceivedEventArgs e)
        {
            try
            {
                _pipeServer.WriteRfid(e.Rfid);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void PipeServer_OnReaderUpdateRequest(object sender, EventArgs e)
        {
            try
            {
                _pipeServer.WriteReaderState(_connection.IsConnected || _comPortManager.IsConnected);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        protected override void OnStop()
        {
            if (_comPortManager != null)
            {
                _comPortManager.Stop();
                _comPortManager.RfidReceived -= Connection_RfidReceived;
                _comPortManager.ReaderStateChanged -= Connection_ReaderStateChanged;
            }
            if (_connection != null)
            {
                _connection.Stop();
                _connection.RfidReceived -= Connection_RfidReceived;
                _connection.ReaderStateChanged -= Connection_ReaderStateChanged;
            }
            if (_pipeServer != null)
            {
                _pipeServer.Stop();
                _pipeServer.OnReaderUpdateRequest -= PipeServer_OnReaderUpdateRequest;
            }
            _log?.Shutdown();
        }
    }
}
