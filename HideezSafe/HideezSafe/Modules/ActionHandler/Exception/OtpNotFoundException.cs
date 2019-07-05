using Hideez.ARM;
using System;

namespace HideezSafe.Modules.ActionHandler
{
    public class OtpNotFoundException : AccountException
    {
        public OtpNotFoundException() { }
        public OtpNotFoundException(string message) : base(message) { }
        public OtpNotFoundException(string message, Exception inner) : base(message, inner) { }
        public OtpNotFoundException(AppInfo appInfo, string[] devicesId) : base(appInfo, devicesId) { }
        public OtpNotFoundException(string message, AppInfo appInfo, string[] devicesId) : base(message, appInfo, devicesId) { }
        public OtpNotFoundException(string message, Exception inner, AppInfo appInfo, string[] devicesId) : base(message, inner, appInfo, devicesId) { }
    }
}
