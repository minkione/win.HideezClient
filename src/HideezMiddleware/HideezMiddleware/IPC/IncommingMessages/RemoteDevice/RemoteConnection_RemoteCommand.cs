namespace HideezMiddleware.IPC.IncommingMessages.RemoteDevice
{
    public sealed class RemoteConnection_RemoteCommand
    {
        public string Connectionid { get; set; }

        public byte[] Data { get; set; }

        public RemoteConnection_RemoteCommand(string connectionid, byte[] data)
        {
            Connectionid = connectionid;
            Data = data;
        }
    }
}
