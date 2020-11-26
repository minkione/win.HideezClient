using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.Messages.RemoteDevice
{
    /// <summary>
    /// Message to notify service about disconnecting pipe device.
    /// </summary>
    public class PipeDeviceDisconnectedMessage: PubSubMessageBase
    {
        public string PipeDeviceId{ get; set; }

        public PipeDeviceDisconnectedMessage(string pipeDeviceId)
        {
            PipeDeviceId = pipeDeviceId;
        }
    }
}
