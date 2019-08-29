using Hideez.SDK.Communication.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezMiddleware.DeviceConnection
{
    public struct ShortDeviceInfo
    {
        public string SerialNo { get; set; }

        public string Mac { get; set; }
    }

    public class BlacklistConnectionProcessor : BaseConnectionProcessor
    {
        readonly List<string> _deviceMacBlackList = new List<string>();
        readonly object _blackListLock = new object();

        List<ShortDeviceInfo> _accessList = new List<ShortDeviceInfo>();
        bool _ignoreAccessList;

        public List<ShortDeviceInfo> AccessList
        {
            get
            {
                return _accessList;
            }
            set
            {
                _accessList = value;
                ClearBlacklist();
            }
        }

        public bool IgnoreAccessList
        {
            get
            {
                return _ignoreAccessList;
            }
            set
            {
                _ignoreAccessList = value;
                ClearBlacklist();
            }
        }

        public BlacklistConnectionProcessor(ConnectionFlowProcessor connectionFlowProcessor, string logSource, ILog log)
            : base(connectionFlowProcessor, logSource, log)
        {
        }

        void BlacklistDevice(string mac)
        {
            lock (_blackListLock)
            {
                _deviceMacBlackList.Add(mac);
                WriteLine($"Blacklisted {mac}");
            }
        }

        bool IsDeviceBlacklisted(string mac)
        {
            lock (_blackListLock)
            {
                return _deviceMacBlackList.Any(m => m == mac);
            }
        }

        public void ClearBlacklist()
        {
            lock (_blackListLock)
            {
                if (_deviceMacBlackList.Count > 0)
                {
                    _deviceMacBlackList.Clear();
                    WriteLine("Blacklist cleared");
                }
            }
        }

        public override async Task ConnectDeviceByMac(string mac)
        {
            if (!IgnoreAccessList)
            {
                // Blacklisted devices are ignored without message
                if (IsDeviceBlacklisted(mac))
                    return;

                // Connection attempts from unauthorized devices produce an error message for user
                if (!AccessList.Any(d => d.Mac == mac))
                {
                    BlacklistDevice(mac);
                    throw new AccessDeniedAuthException();
                }
            }

            await base.ConnectDeviceByMac(mac);
        }
    }
}
