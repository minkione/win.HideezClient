using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.BLE;
using HideezMiddleware.DeviceConnection.Workflow.Interfaces;
using HideezMiddleware.Localize;
using Meta.Lib.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;
using Hideez.SDK.Communication.Interfaces;

namespace HideezMiddleware.DeviceConnection.Workflow
{
    public class VaultConnectionProcessor : Logger, IVaultConnectionProcessor
    {
        readonly IClientUiManager _ui;
        readonly BondManager _bondManager;
        readonly DeviceManager _deviceManager;

        public VaultConnectionProcessor(IClientUiManager ui, BondManager bondManager, DeviceManager deviceManager, ILog log)
            : base(nameof(VaultConnectionProcessor), log)
        {
            _ui = ui;
            _bondManager = bondManager;
            _deviceManager = deviceManager;
        }

        public async Task<IDevice> ConnectVault(string mac, bool rebondOnFail, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            if (_bondManager.Exists(mac))
                await _ui.SendNotification(TranslationSource.Instance["ConnectionFlow.Connection.Stage1"], mac);
            else await _ui.SendNotification(TranslationSource.Instance["ConnectionFlow.Connection.Stage1.PressButton"], mac);

            bool ltkErrorOccured = false;
            IDevice device = null;
            try
            {
                device = await _deviceManager.Connect(mac).TimeoutAfter(SdkConfig.ConnectDeviceTimeout);
            }
            catch (Exception ex) // Thrown when LTK error occurs in csr
            {
                WriteLine(ex);
                ltkErrorOccured = true;
            }

            if (device == null)
            {
                ct.ThrowIfCancellationRequested();

                string ltk = "";
                if (ltkErrorOccured)
                {
                    ltk = "LTK error.";
                    ltkErrorOccured = false;
                }
                if (_bondManager.Exists(mac))
                    await _ui.SendNotification(ltk + TranslationSource.Instance["ConnectionFlow.Connection.Stage2"], mac);
                else await _ui.SendNotification(ltk + TranslationSource.Instance["ConnectionFlow.Connection.Stage2.PressButton"], mac);

                try
                {
                    device = await _deviceManager.Connect(mac).TimeoutAfter(SdkConfig.ConnectDeviceTimeout / 2);
                }
                catch (Exception ex) // Thrown when LTK error occurs in csr
                {
                    WriteLine(ex);
                    ltkErrorOccured = true;
                }

                if (device == null && rebondOnFail)
                {
                    ct.ThrowIfCancellationRequested();

                    // remove the bond and try one more time
                    await _deviceManager.DeleteBond(mac);

                    if (ltkErrorOccured)
                        await _ui.SendNotification(TranslationSource.Instance["ConnectionFlow.Connection.Stage3.LtkError.PressButton"], mac); // TODO: Fix LTK error in CSR
                    else
                        await _ui.SendNotification(TranslationSource.Instance["ConnectionFlow.Connection.Stage3.PressButton"], mac);

                    device = await _deviceManager.Connect(mac).TimeoutAfter(SdkConfig.ConnectDeviceTimeout);
                }
            }

            if (device == null)
                throw new WorkflowException(TranslationSource.Instance.Format("ConnectionFlow.Connection.ConnectionFailed", mac));

            return device;
        }

        public async Task WaitVaultInitialization(string mac, IDevice device, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            await _ui.SendNotification(TranslationSource.Instance["ConnectionFlow.Initialization.WaitingInitializationMessage"], mac);

            if (!await device.WaitInitialization(SdkConfig.DeviceInitializationTimeout, ct))
                throw new WorkflowException(TranslationSource.Instance.Format("ConnectionFlow.Initialization.InitializationFailed", mac));

            if (device.IsErrorState)
            {
                await _deviceManager.RemoveConnection(device.DeviceConnection);
                throw new WorkflowException(TranslationSource.Instance.Format("ConnectionFlow.Initialization.DeviceInitializationError", mac, device.ErrorMessage));
            }
        }
    }
}
