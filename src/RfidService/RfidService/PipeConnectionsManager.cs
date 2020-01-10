using System;
using System.Text;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.NamedPipes;

namespace Hideez.RFID
{
    public class PipeConnectionsManager
    {
        readonly PipeServer _pipeServer;

        public event EventHandler OnReaderUpdateRequest;

        public PipeConnectionsManager(ILog log)
        {
            _pipeServer = new PipeServer("hideezrfid", log);
            _pipeServer.MessageReceivedEvent += PipeServer_MessageReceivedEvent;
        }

        internal void Start()
        {
            _pipeServer.Start();
        }

        internal void Stop()
        {
            _pipeServer.Stop();
        }

        void PipeServer_MessageReceivedEvent(object sender, MessageReceivedEventArgs e)
        {
            try
            {
                var msgBytes = new byte[e.ReadBytes];
                Array.Copy(e.Buffer, msgBytes, e.ReadBytes);

                var message = Encoding.UTF8.GetString(msgBytes);

                if (!string.IsNullOrWhiteSpace(message) &&
                    message.Contains("updatestate"))
                    OnReaderUpdateRequest?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception) { }
        }

        public void WriteRfid(string rfid)
        {
            if (!rfid.EndsWith("" + '\n'))
                rfid += '\n';
            
            _pipeServer.WriteToAll(Encoding.ASCII.GetBytes(rfid));
        }

        public void WriteReaderState(bool isConnected)
        {
            _pipeServer.WriteToAll(Encoding.ASCII.GetBytes("reader_available:" + isConnected + '\n'));
        }
    }
}
