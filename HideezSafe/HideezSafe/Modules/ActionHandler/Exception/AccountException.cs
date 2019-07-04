using Hideez.ARM;

namespace HideezSafe.Modules.ActionHandler
{
    public class AccountException : System.Exception
    {
        public AccountException() { }
        public AccountException(string message) : base(message) { }
        public AccountException(string message, System.Exception inner) : base(message, inner) { }
        public AccountException(AppInfo appInfo, string[] devicesId) : this(null, appInfo, devicesId) { }
        public AccountException(string message, AppInfo appInfo, string[] devicesId) : this(message, null, appInfo, devicesId) { }
        public AccountException(string message, System.Exception inner, AppInfo appInfo, string[] devicesId) : base(message, inner)
        {
            this.AppInfo = appInfo;
            this.DevicesId = devicesId;
        }

        public string[] DevicesId { get; set; }
        public AppInfo AppInfo { get; set; }
    }
}
