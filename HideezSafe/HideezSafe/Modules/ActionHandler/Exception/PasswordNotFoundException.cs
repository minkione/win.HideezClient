using Hideez.ARS;

namespace HideezSafe.Modules.ActionHandler
{
    public class PasswordNotFoundException : AccountException
    {
        public PasswordNotFoundException() { }
        public PasswordNotFoundException(string message) : base(message) { }
        public PasswordNotFoundException(string message, System.Exception inner) : base(message, inner) { }
        public PasswordNotFoundException(AppInfo appInfo, string[] devicesId) : base(appInfo, devicesId) { }
        public PasswordNotFoundException(string message, AppInfo appInfo, string[] devicesId) : base(message, appInfo, devicesId) { }
        public PasswordNotFoundException(string message, System.Exception inner, AppInfo appInfo, string[] devicesId) : base(message, inner, appInfo, devicesId) { }
    }
}
