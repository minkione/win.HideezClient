using Hideez.SDK.Communication.Device;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Utils;
using HideezMiddleware.DeviceConnection.Workflow.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HideezMiddleware.DeviceConnection.Workflow
{
    public class StandaloneVaultAuthorizationProcessor : IVaultAuthorizationProcessor
    {
        public async Task AuthVault(IDevice device, CancellationToken ct)
        {
            var masterkey = Encoding.UTF8.GetBytes("passphrase");
            var code = Encoding.UTF8.GetBytes("123456");
            if (device.AccessLevel.IsLinkRequired)
            {
                await device.Link(masterkey, code, 5);
                await device.Access(DateTime.Now, masterkey, new AccessParams()
                {
                    MasterKey_Bond = true
                });
            }

            if (device.AccessLevel.IsLocked)
            {
                await device.UnlockDeviceCode(code);
                await device.RefreshDeviceInfo();
            }

            if (device.AccessLevel.IsMasterKeyRequired)
                await device.CheckPassphrase(masterkey);
        }
    }
}
