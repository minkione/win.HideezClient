using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.Log;
using Microsoft.Win32;
using System;

namespace HideezMiddleware
{
    // Todo: Technical debt due to no test coverage and a quick band-aid style of implementation
    public sealed class HesAccessManager : Logger, IHesAccessManager
    {
        const string ACCESS_VALUE_NAME = "access";
        readonly RegistryKey _rootKey;

        public event EventHandler<EventArgs> AccessGranted;
        public event EventHandler<EventArgs> AccessRetracted;

        public HesAccessManager(RegistryKey rootKey, ILog log)
            : base(nameof(HesAccessManager), log)
        {
            _rootKey = rootKey ?? throw new ArgumentNullException(nameof(rootKey));
        }

        public bool HasAccessKey()
        {
            var value = _rootKey.GetValue(ACCESS_VALUE_NAME);

            return value != null;
        }
        
        public void ClearAccessKey()
        {
            if (HasAccessKey())
            {
                _rootKey.DeleteValue(ACCESS_VALUE_NAME);
                WriteLine("Access key cleared");
                AccessRetracted?.Invoke(this, EventArgs.Empty);
            }
        }

        public void SaveAccessKey()
        {
            if (!HasAccessKey())
            {
                var key = Guid.NewGuid().ToString().Replace("-", "");
                _rootKey.SetValue(ACCESS_VALUE_NAME, key);
                WriteLine("Access key saved");
                AccessGranted?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
