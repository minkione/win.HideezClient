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
                // Todo: This entire class will have to be changed to adapt to win ble
                var con = await _deviceManager.ConnectionManager.Connect(mac).TimeoutAfter(SdkConfig.ConnectDeviceTimeout);
                device = _deviceManager.Find(con.Id, (byte)DefaultDeviceChannel.Main);
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
                    var con = await _deviceManager.ConnectionManager.Connect(mac).TimeoutAfter(SdkConfig.ConnectDeviceTimeout / 2);
                    device = _deviceManager.Find(con.Id, (byte)DefaultDeviceChannel.Main);
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
                    await _deviceManager.ConnectionManager.DeleteBond(mac);

                    if (ltkErrorOccured)
                        await _ui.SendNotification(TranslationSource.Instance["ConnectionFlow.Connection.Stage3.LtkError.PressButton"], mac); // TODO: Fix LTK error in CSR
                    else
                        await _ui.SendNotification(TranslationSource.Instance["ConnectionFlow.Connection.Stage3.PressButton"], mac);

                    var con = await _deviceManager.ConnectionManager.Connect(mac).TimeoutAfter(SdkConfig.ConnectDeviceTimeout);
                    device = _deviceManager.Find(con.Id, (byte)DefaultDeviceChannel.Main);
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
                await _deviceManager.ConnectionManager.RemoveConnection(device.DeviceConnection);
                throw new WorkflowException(TranslationSource.Instance.Format("ConnectionFlow.Initialization.DeviceInitializationError", mac, device.ErrorMessage));
            }
        }
    }
}
