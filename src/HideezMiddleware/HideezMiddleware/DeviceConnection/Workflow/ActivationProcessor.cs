using Hideez.SDK.Communication;
using Hideez.SDK.Communication.HES.Client;
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
        IHesAppConnection _hesConnection;

        public ActivationProcessor(UiProxyManager ui, HesAppConnection hesConnection, ILog log)
            : base(nameof(ActivationProcessor), log)
        {
            _ui = ui;
            _hesConnection = hesConnection;
        }

        public async Task<HwVaultInfoFromHesDto> ActivateVault(IDevice device, HwVaultInfoFromHesDto vaultInfo, CancellationToken ct)
        {
            if (device.IsLocked && device.IsCanUnlock)
            {
                try
                {
                    do
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
                            throw new WorkflowException(TranslationSource.Instance["ConnectionFlow.ActivationCode.Error.LockedByInvalidAttempts"]);
                        }

                        ct.ThrowIfCancellationRequested();

                        await device.RefreshDeviceInfo();

                        ct.ThrowIfCancellationRequested();

                        if (!device.IsLocked)
                        {
                            WriteLine($"({device.SerialNo}) unlocked with activation code");
                        }
                        else if (device.UnlockAttemptsRemain > 0)
                        {
                            await _ui.SendNotification(TranslationSource.Instance.Format("ConnectionFlow.ActivationCode.Error.InvalidCode", device.UnlockAttemptsRemain), device.Mac);
                        }
                        else
                        {
                            // We won't reach this line, but will leave it just in case
                            throw new WorkflowException(TranslationSource.Instance["ConnectionFlow.ActivationCode.Error.LockedByInvalidAttempts"]);
                        }
                    }
                    while (device.IsLocked);
                }
                finally
                {
                    await _ui.HideActivationCodeUi();
#pragma warning disable IDE0059 // Unnecessary assignment of a value
                    vaultInfo = await _hesConnection.UpdateDeviceProperties(new HwVaultInfoFromClientDto(device), true);
#pragma warning restore IDE0059 // Unnecessary assignment of a value
                }
            }

            if (device.IsLocked && !device.IsCanUnlock)
            {
                if (_hesConnection.State == HesConnectionState.Connected)
                    throw new WorkflowException(TranslationSource.Instance["ConnectionFlow.ActivationCode.Error.VaultIsLocked"]);
                else
                    throw new WorkflowException(TranslationSource.Instance["ConnectionFlow.ActivationCode.Error.VaultIsLockedNoNetwork"]);
            }

            return vaultInfo;
        }
    }
}
