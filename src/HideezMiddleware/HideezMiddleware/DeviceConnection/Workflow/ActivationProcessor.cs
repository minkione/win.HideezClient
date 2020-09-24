using Hideez.SDK.Communication;
using Hideez.SDK.Communication.HES.DTO;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using HideezMiddleware.Localize;
using System.Threading;
using System.Threading.Tasks;

namespace HideezMiddleware.DeviceConnection.Workflow
{
    public class ActivationProcessor : Logger
    {
        UiProxyManager _ui;

        public ActivationProcessor(UiProxyManager ui, ILog log)
            : base(nameof(ActivationProcessor), log)
        {
            _ui = ui;
        }

        public async Task ActivateVault(IDevice device, CancellationToken ct)
        {
            try
            {
                while (device.IsLocked)
                {
                    ct.ThrowIfCancellationRequested();

                    var code = await _ui.GetActivationCode(device.Id, 30_000, ct); // Todo: activation timeout should not be a magic number

                    ct.ThrowIfCancellationRequested();

                    if (code.Length < 6)
                    {
                        await _ui.SendError(TranslationSource.Instance["ConnectionFlow.ActivationCode.Error.CodeToShort"], device.Mac);
                        continue;
                    }

                    if (code.Length > 8)
                    {
                        await _ui.SendError(TranslationSource.Instance["ConnectionFlow.ActivationCode.Error.CodeToLong"], device.Mac);
                        continue;
                    }

                    try
                    {
                        await device.UnlockDeviceCode(code);
                    }
                    catch (HideezException ex) when (ex.ErrorCode == HideezErrorCode.ERR_PIN_WRONG) // Entered invalid activation code
                    {
                    }
                    catch (HideezException ex) when (ex.ErrorCode == HideezErrorCode.ERR_DEVICE_LOCKED_BY_CODE) // Unlock attempts == 0
                    {
                        throw new HideezException(HideezErrorCode.DeviceIsLocked);
                    }

                    ct.ThrowIfCancellationRequested();

                    await device.RefreshDeviceInfo();

                    ct.ThrowIfCancellationRequested();

                    if (!device.IsLocked)
                    {
                        WriteLine($"({device.SerialNo}) unlocked with activation code");
                        return;
                    }
                    else if (device.UnlockAttemptsRemain > 0)
                    {
                        await _ui.SendNotification(TranslationSource.Instance.Format("ConnectionFlow.ActivationCode.Error.InvalidCode", device.UnlockAttemptsRemain), device.Mac);
                    }
                    else
                    {
                        // We won't reach this line, but will leave it just in case
                        throw new HideezException(HideezErrorCode.DeviceIsLocked);
                    }
                }
            }
            finally
            {
                await _ui.HideActivationCodeUi();
            }
        }
    }
}
