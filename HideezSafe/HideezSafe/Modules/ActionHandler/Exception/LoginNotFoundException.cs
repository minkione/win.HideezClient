using Hideez.ARS;
using System;

namespace HideezSafe.Modules.ActionHandler
{
    public class LoginNotFoundException : AccountException
    {
        public LoginNotFoundException() { }
        public LoginNotFoundException(string message) : base(message) { }
        public LoginNotFoundException(string message, Exception inner) : base(message, inner) { }
        public LoginNotFoundException(AppInfo appInfo, string[] devicesId) : base(appInfo, devicesId) { }
        public LoginNotFoundException(string message, AppInfo appInfo, string[] devicesId) : base(message, appInfo, devicesId) { }
        public LoginNotFoundException(string message, Exception inner, AppInfo appInfo, string[] devicesId) : base(message, inner, appInfo, devicesId) { }
    }
}
