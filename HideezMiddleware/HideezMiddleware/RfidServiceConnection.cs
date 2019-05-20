using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.NamedPipes;
using System;
using System.Threading.Tasks;

namespace HideezMiddleware
{
    public class RfidReceivedEventArgs
    {
        public string Rfid { get; set; }
    }

    public class RfidServiceConnection : Logger
    {
        const string readerStateParameter = "reader_available:";
        readonly PipeClient _pipeClient;

        private bool _readerConnected = false;

        public event EventHandler<RfidReceivedEventArgs> RfidReceivedEvent;
        public event EventHandler<EventArgs> RfidServiceStateChanged;
        public event EventHandler<EventArgs> RfidReaderStateChanged;

        public RfidServiceConnection(ILog log)
            : base(nameof(RfidServiceConnection), log)
        {
            _pipeClient = new PipeClient("hideezrfid", log);
        }

        public bool ServiceConnected => _pipeClient.Connected;

        public bool ReaderConnected
        {
            get
            {
                return ServiceConnected && _readerConnected;
            }
            set
            {
                _readerConnected = value;
                RfidReaderStateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Start()
        {
            _pipeClient.MessageReceivedEvent += PipeClient_MessageReceivedEvent;
            _pipeClient.PipeConnectionStateChanged += PipeClient_PipeStateChanged;
            _pipeClient.Run();
        }

        public void Stop()
        {
            if (_pipeClient != null)
            {
                _pipeClient?.Stop();
                ReaderConnected = false;
                _pipeClient.MessageReceivedEvent -= PipeClient_MessageReceivedEvent;
                _pipeClient.PipeConnectionStateChanged -= PipeClient_PipeStateChanged;
            }
        }

        void PipeClient_MessageReceivedEvent(object sender, MessageReceivedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.Message))
            {
                if (e.Message.StartsWith(readerStateParameter))
                {
                    var stringValue = GetValue(readerStateParameter, e.Message);
                    if (bool.TryParse(stringValue, out bool receivedReaderState))
                    {
                        ReaderConnected = receivedReaderState;
                        return;
                    }
                }
                else
                {
                    RfidReceivedEvent?.Invoke(this, new RfidReceivedEventArgs() { Rfid = e.Message });
                }
            }
        }

        void PipeClient_PipeStateChanged(object sender, EventArgs args)
        {
            if (_pipeClient.Connected)
            {
                try { _pipeClient.SendMessage("updatestate" + '\n'); }
                catch (Exception) { }
            }

            RfidServiceStateChanged?.Invoke(this, EventArgs.Empty);
            RfidReaderStateChanged?.Invoke(this, EventArgs.Empty);
        }

        string GetValue(string parameter, string message)
        {
            if (!message.Contains(parameter))
                return string.Empty;

            return message.Substring(message.IndexOf(parameter) + parameter.Length);
        }
    }
}
