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
using Hideez.SDK.Communication.Refactored.BLE;

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

        public async Task<IDevice> ConnectVault(ConnectionId connectionId, bool rebondOnFail, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            //if (_bondManager.Exists(mac))
            //    await _ui.SendNotification(TranslationSource.Instance["ConnectionFlow.Connection.Stage1"], connectionId.Id);
            //else 
                await _ui.SendNotification(TranslationSource.Instance["ConnectionFlow.Connection.Stage1.PressButton"], connectionId.Id);

            //var id = BleUtils.MacToConnectionId(mac);

            bool ltkErrorOccured = false;
            IDevice device = null;
            try
            {
                device = await _deviceManager.Connect(connectionId).TimeoutAfter(SdkConfig.ConnectDeviceTimeout);
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
                //if (_bondManager.Exists(mac))
                //    await _ui.SendNotification(ltk + TranslationSource.Instance["ConnectionFlow.Connection.Stage2"], connectionId.Id);
                //else 
                    await _ui.SendNotification(ltk + TranslationSource.Instance["ConnectionFlow.Connection.Stage2.PressButton"], connectionId.Id);

                try
                {
                    device = await _deviceManager.Connect(connectionId).TimeoutAfter(SdkConfig.ConnectDeviceTimeout / 2);
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
                    await _deviceManager.DeleteBond(connectionId.Id);

                    if (ltkErrorOccured)
                        await _ui.SendNotification(TranslationSource.Instance["ConnectionFlow.Connection.Stage3.LtkError.PressButton"], connectionId.Id); // TODO: Fix LTK error in CSR
                    else
                        await _ui.SendNotification(TranslationSource.Instance["ConnectionFlow.Connection.Stage3.PressButton"], connectionId.Id);

                    device = await _deviceManager.Connect(connectionId).TimeoutAfter(SdkConfig.ConnectDeviceTimeout);
                }
            }

            if (device == null)
                throw new WorkflowException(TranslationSource.Instance.Format("ConnectionFlow.Connection.ConnectionFailed", connectionId.Id));

            return device;
        }

        public async Task WaitVaultInitialization(IDevice device, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            var connectionId = device.DeviceConnection.Connection.ConnectionId.Id;

            await _ui.SendNotification(TranslationSource.Instance["ConnectionFlow.Initialization.WaitingInitializationMessage"], connectionId);

            if (!await device.WaitInitialization(SdkConfig.DeviceInitializationTimeout, ct))
                throw new WorkflowException(TranslationSource.Instance.Format("ConnectionFlow.Initialization.InitializationFailed", connectionId));

            if (device.IsErrorState)
            {
                await _deviceManager.RemoveConnection(device.DeviceConnection);
                throw new WorkflowException(TranslationSource.Instance.Format("ConnectionFlow.Initialization.DeviceInitializationError", connectionId, device.ErrorMessage));
            }
        }
    }
}
