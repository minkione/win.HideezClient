using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezSafe.Modules.ActionHandler
{
    public class Account
    {
        public Account(string deviceId, ushort key, string name, string login, bool hasOtpSecret, string app, string url)
        {
            this.DeviceId = deviceId;
            this.Key = key;
            this.Name = name;

            this.App = app;
            this.Url = url;

            this.Login = login;
            this.HasOtpSecret = hasOtpSecret;
        }

        public string DeviceId { get; }
        public ushort Key { get; }
        public string Name { get; }

        public bool IsForWebApp { get { return !string.IsNullOrWhiteSpace(Url); } }

        public string App { get; }
        public string Url { get; }

        public string Login { get; }
        public bool HasOtpSecret { get; }

        public override bool Equals(object obj)
        {
            if (obj is Account accountObj)
            {
                return this.Name == accountObj.Name
                        && this.App == accountObj.App
                        && this.Url == accountObj.Url
                        && this.Login == accountObj.Login;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return $"{Name}{App}{Url}{Login}".GetHashCode(); 
        }
    }
}
