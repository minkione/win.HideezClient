using Hideez.SDK.Communication;
using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.HES.DTO;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using HideezMiddleware.Localize;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HideezMiddleware.DeviceConnection.Workflow
{
    public class LicensingProcessor : Logger
    {
        UiProxyManager _ui;
        IHesAppConnection _hesConnection;

        public LicensingProcessor(IHesAppConnection hesConnection, UiProxyManager ui, ILog log)
            : base(nameof(LicensingProcessor), log)
        {
            _hesConnection = hesConnection;
            _ui = ui;
        }

        public async Task CheckLicense(IDevice device, HwVaultInfoFromHesDto vaultInfo, CancellationToken ct)
        {
            if (vaultInfo.NeedUpdateLicense)
            {
                await _ui.SendNotification(TranslationSource.Instance["ConnectionFlow.License.UpdatingLicenseMessage"], device.Mac);
                var licenses = await _hesConnection.GetNewDeviceLicenses(device.SerialNo, ct);
                WriteLine($"Received {licenses.Count} new licenses from HES");

                ct.ThrowIfCancellationRequested();

                if (licenses.Count > 0)
                {
                    for (int i = 0; i < licenses.Count; i++)
                    {
                        var license = licenses[i];

                        ct.ThrowIfCancellationRequested();

                        if (license.Data == null)
                            throw new Exception(TranslationSource.Instance.Format("ConnectionFlow.License.Error.EmptyLicenseData", device.SerialNo));

                        if (license.Id == null)
                            throw new Exception(TranslationSource.Instance.Format("ConnectionFlow.License.Error.EmptyLicenseId", device.SerialNo));

                        try
                        {
                            await device.LoadLicense(license.Data, SdkConfig.DefaultCommandTimeout);
                            WriteLine($"Loaded license ({license.Id}) into vault ({device.SerialNo}) in available slot");
                        }
                        catch (HideezException ex) when (ex.ErrorCode == HideezErrorCode.ERR_DEVICE_LOCKED_BY_CODE || ex.ErrorCode == HideezErrorCode.ERR_DEVICE_LOCKED_BY_PIN)
                        { 
                            await device.LoadLicense(i, license.Data, SdkConfig.DefaultCommandTimeout);
                            WriteLine($"Loaded license ({license.Id}) into vault ({device.SerialNo}) into slot {i}");
                        }

                        await _hesConnection.OnDeviceLicenseApplied(device.SerialNo, license.Id);
                    }

                    await device.RefreshDeviceInfo();
                }
            }
        }
    }
}
