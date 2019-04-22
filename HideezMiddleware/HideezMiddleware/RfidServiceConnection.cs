using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.NamedPipes;
using System;

namespace HideezMiddleware
{
    public class RfidReceivedEventArgs
    {
        public string Rfid { get; set; }
    }

    public class RfidServiceConnection : Logger
    {
        readonly PipeClient _pipeClient;

        public event EventHandler<RfidReceivedEventArgs> RfidReceivedEvent;

        public RfidServiceConnection(ILog log)
            : base(nameof(RfidServiceConnection), log)
        {
            _pipeClient = new PipeClient("hideezrfid", log);
        }

        public void Start()
        {
            _pipeClient.Run();
            _pipeClient.MessageReceivedEvent += PipeClient_MessageReceivedEvent;
        }

        public void Stop()
        {
            if (_pipeClient != null)
            {
                _pipeClient?.Stop();
                _pipeClient.MessageReceivedEvent -= PipeClient_MessageReceivedEvent;
            }
        }

        void PipeClient_MessageReceivedEvent(object sender, MessageReceivedEventArgs e)
        {
            RfidReceivedEvent?.Invoke(this, new RfidReceivedEventArgs() { Rfid = e.Message });
        }
    }
}
