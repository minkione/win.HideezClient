using System;
using System.ServiceProcess;
using Hideez.SDK.Communication.Log;

namespace Hideez.RFID
{
    public partial class RfidService : ServiceBase
    {
        RfidConnection _connection;
        PipeConnectionsManager _pipeServer;
        EventLogger _log;

        public RfidService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _log = new EventLogger("RFID");
            _log.WriteDebugLine("Service", "Started");

            _connection = new RfidConnection(_log);
            _connection.RfidReceived += _connection_RfidReceived;
            _connection.ReaderStateChanged += _connection_ReaderStateChanged;
            _connection.Start();

            _pipeServer = new PipeConnectionsManager(_log);
            _pipeServer.OnReaderUpdateRequest += _pipeServer_OnReaderUpdateRequest;
            _pipeServer.Start();

        }

        private void _connection_ReaderStateChanged(object sender, ReaderStateChangedEventArgs e)
        {
            try
            {
                _pipeServer.WriteReaderState(e.IsConnected);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void _connection_RfidReceived(object sender, RfidReceivedEventArgs e)
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

        private void _pipeServer_OnReaderUpdateRequest(object sender, System.EventArgs e)
        {
            try
            {
                _pipeServer.WriteReaderState(_connection.IsConnected);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        protected override void OnStop()
        {
            if (_connection != null)
            {
                _connection.Stop();
                _connection.RfidReceived -= _connection_RfidReceived;
                _connection.ReaderStateChanged -= _connection_ReaderStateChanged;
            }
            if (_pipeServer != null)
            {
                _pipeServer.Stop();
                _pipeServer.OnReaderUpdateRequest -= _pipeServer_OnReaderUpdateRequest;
            }
            _log?.Shutdown();
        }
    }
}
