namespace HideezMiddleware
{
    public abstract class WorkstationInfo
    {
        public string AppVersion { get; protected set; }
        public string MachineName { get; protected set; }

        public string OsName { get; set; }
        public string OSVersion { get; protected set; }
        public string OsBuild { get; protected set; }

        public string IPAddress { get; protected set; }
        public string MACAddress { get; protected set; }
        public string Domain { get; protected set; }
        public string[] WindowsUserAccounts { get; protected set; }
    }
}
