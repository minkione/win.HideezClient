using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.Log;
using Microsoft.Win32;

namespace HideezMiddleware.Local
{
    public class LocalDeviceInfoCache : Logger, ILocalDeviceInfoCache
    {
        readonly RegistryKey _registryRootKey;
        readonly RegistryKey _cacheRootKey; 

        public LocalDeviceInfoCache(RegistryKey rootKey, ILog log) : base(nameof(LocalDeviceInfoCache), log)
        {
            _registryRootKey = rootKey;
            if (_registryRootKey != null)
                _cacheRootKey = _registryRootKey.CreateSubKey("DeviceCache");
        }

        public LocalDeviceInfo GetLocalInfo(string deviceMac)
        {
            if (_cacheRootKey == null)
                return null;

            var keyName = BleUtils.MacToConnectionId(deviceMac);

            var cacheKey = _cacheRootKey.OpenSubKey(keyName);

            if (cacheKey == null)
                return null;

            var info = new LocalDeviceInfo();
            info.SerialNo = (string)cacheKey.GetValue(nameof(info.SerialNo));
            info.RFID = (string)cacheKey.GetValue(nameof(info.RFID));
            info.OwnerName = (string)cacheKey.GetValue(nameof(info.OwnerName));
            info.OwnerEmail = (string)cacheKey.GetValue(nameof(info.OwnerEmail));
            info.Mac = deviceMac;

            return info;
        }

        public void RemoveLocalInfo(string deviceMac)
        {
            if (_cacheRootKey == null)
                return;

            var keyName = BleUtils.MacToConnectionId(deviceMac);
            _cacheRootKey.DeleteSubKeyTree(keyName);

        }

        public void SaveLocalInfo(LocalDeviceInfo info)
        {
            if (_cacheRootKey == null)
                return;

            var keyName = BleUtils.MacToConnectionId(info.Mac);

            var cacheKey = _cacheRootKey.CreateSubKey(keyName);

            cacheKey.SetValue(nameof(info.SerialNo), info.SerialNo ?? string.Empty, RegistryValueKind.String);
            cacheKey.SetValue(nameof(info.RFID), info.RFID ?? string.Empty, RegistryValueKind.String);
            cacheKey.SetValue(nameof(info.OwnerName), info.OwnerName ?? string.Empty, RegistryValueKind.String);
            cacheKey.SetValue(nameof(info.OwnerEmail), info.OwnerEmail ?? string.Empty, RegistryValueKind.String);
        }

    }
}
