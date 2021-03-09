using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Device;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.Utils;
using HideezMiddleware.DeviceConnection.Workflow.Interfaces;
using HideezMiddleware.Localize;
using HideezMiddleware.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HideezMiddleware.DeviceConnection.Workflow
{
    public class StandaloneVaultAuthorizationProcessor : Logger, IVaultAuthorizationProcessor
    {
        readonly IClientUiManager _ui;

        public StandaloneVaultAuthorizationProcessor(IClientUiManager ui, ILog log)
            : base(nameof(VaultAuthorizationProcessor), log)
        {
            _ui = ui;
        }

        public async Task AuthVault(IDevice device, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            if(device.AccessLevel.IsMasterKeyRequired)
                await EnterMasterkeyWorkflow(device, ct);
        }

        async Task EnterMasterkeyWorkflow(IDevice device, CancellationToken ct)
        {
            Debug.WriteLine(">>>>>>>>>>>>>>> EnterMasterkeyWorkflow +++++++++++++++++++++++++++++++++++++++");

            await _ui.SendNotification(TranslationSource.Instance["ConnectionFlow.MasterPassword.EnterMPMessage"], device.DeviceConnection.Connection.ConnectionId.Id);
            while (device.AccessLevel.IsMasterKeyRequired)
            {
                var inputResult = await _ui.GetPassword(device.Id, SdkConfig.MainWorkflowTimeout, ct);

                ct.ThrowIfCancellationRequested();
               
                if (string.IsNullOrWhiteSpace(inputResult))
                {
                    // we received an empty PIN from the user. Trying again with the same timeout.
                    Debug.WriteLine($">>>>>>>>>>>>>>> EMPTY Masterkey");
                    WriteLine("Received empty Masterkey");

                    continue;
                }
                await _ui.SendError(string.Empty, device.DeviceConnection.Connection.ConnectionId.Id);

                var masterkey = MasterPasswordConverter.GetMasterKey(inputResult, device.SerialNo);

                try
                {
                    await device.CheckPassphrase(masterkey); //this using default timeout for BLE commands
                    ct.ThrowIfCancellationRequested();

                    await device.RefreshDeviceInfo();
                }
                catch(HideezException ex)
                {
                    Debug.WriteLine($">>>>>>>>>>>>>>> Wrong masterkey ");
                    if(ex.ErrorCode == HideezErrorCode.ERR_KEY_WRONG)
                        await _ui.SendError(TranslationSource.Instance["ConnectionFlow.MasterPassword.Error.InvalidMP"], device.DeviceConnection.Connection.ConnectionId.Id);
                    else
                        await _ui.SendError(ex.Message);
                    continue;
                }
                catch (Exception ex)
                {
                    await _ui.SendError(ex.Message);
                    continue;
                }

                WriteLine(">>>>>>>>>>>>>>> Masterkey OK");
                break;
            }
            Debug.WriteLine(">>>>>>>>>>>>>>> EnterMasterkeyWorkflow ------------------------------");
        }
    }
}
