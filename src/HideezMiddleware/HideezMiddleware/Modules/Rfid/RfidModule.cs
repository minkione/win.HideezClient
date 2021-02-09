using Hideez.SDK.Communication.Log;
using HideezMiddleware.DeviceConnection;
using HideezMiddleware.IPC.Messages;
using Meta.Lib.Modules.PubSub;
using System;

namespace HideezMiddleware.Modules.Rfid
{
    public sealed class RfidModule : ModuleBase
    {
        readonly RfidConnectionProcessor _rfidConnectionProcessor;
        readonly RfidServiceConnection _rfidServiceConnection;

        public RfidModule(RfidConnectionProcessor rfidConnectionProcessor, 
            RfidServiceConnection rfidServiceConnection, 
            IMetaPubSub messenger, 
            ILog log)
            : base(messenger, nameof(RfidModule), log)
        {

            _rfidConnectionProcessor = rfidConnectionProcessor;
            _rfidServiceConnection = rfidServiceConnection;

            _rfidServiceConnection.RfidReaderStateChanged += RfidServiceConnection_RfidReaderStateChanged;

            _rfidServiceConnection.Start();
            _rfidConnectionProcessor.Start();
        }

        private async void RfidServiceConnection_RfidReaderStateChanged(object sender, EventArgs e)
        {
            await _messenger.Publish(new RfidService_RfidReaderStateChangedMessage(sender, e));
        }
    }
}
