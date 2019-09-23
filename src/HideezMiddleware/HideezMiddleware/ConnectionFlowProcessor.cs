using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.HES.DTO;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.PasswordManager;
using Hideez.SDK.Communication.Utils;

namespace HideezMiddleware
{
    public class ConnectionFlowProcessor : Logger, IConnectionFlow
    {
        public const string FLOW_FINISHED_PROP = "MainFlowFinished"; 

        const int CONNECT_RETRY_DELAY = 1000;
        readonly BleDeviceManager _deviceManager;
        readonly IWorkstationUnlocker _workstationUnlocker;
        readonly IScreenActivator _screenActivator;
        readonly UiProxyManager _ui;
        readonly HesAppConnection _hesConnection;

        int _isConnecting = 0;
        string _infNid = string.Empty; // Notification Id, which must be the same for the entire duration of MainWorkflow
        string _errNid = string.Empty; // Error Notification Id

        public event EventHandler<IDevice> DeviceFinishedMainFlow;

        public ConnectionFlowProcessor(BleDeviceManager deviceManager,
            HesAppConnection hesConnection,
            IWorkstationUnlocker workstationUnlocker,
            IScreenActivator screenActivator,
            UiProxyManager ui,
            ILog log)
            : base(nameof(ConnectionFlowProcessor), log)
        {
            _deviceManager = deviceManager;
            _workstationUnlocker = workstationUnlocker;
            _screenActivator = screenActivator;
            _ui = ui;
            _hesConnection = hesConnection;
        }

        public async Task<ConnectionFlowResult> ConnectAndUnlock(string mac)
        {
            // ignore, if workflow for any device already initialized
            if (Interlocked.CompareExchange(ref _isConnecting, 1, 0) == 0)
            {
                try
                {
                    return await MainWorkflow(mac);
                }
                finally
                {
                    // this delay allows a user to move away the device from the dongle or RFID
                    // and prevents the repeated call of this method
                    await Task.Delay(1500);

                    Interlocked.Exchange(ref _isConnecting, 0);
                }
            }

            return new ConnectionFlowResult();
        }

        async Task<ConnectionFlowResult> MainWorkflow(string mac)
        {
            // Ignore MainFlow requests for devices that are already connected
            // IsConnected-true indicates that device already finished main flow or is in progress
            var existingDevice = _deviceManager.Find(mac, 1); // Todo: Replace channel magic number <1> with defined const
            if (existingDevice != null && existingDevice.IsConnected)
                return new ConnectionFlowResult();

            Debug.WriteLine(">>>>>>>>>>>>>>> MainWorkflow +++++++++++++++++++++++++");

            bool success = false;
            var flowResult = new ConnectionFlowResult();
            bool fatalError = false;
            string errorMessage = null;
            IDevice device = null;
            _infNid = Guid.NewGuid().ToString();
            _errNid = Guid.NewGuid().ToString();

            try
            {
                await _ui.SendNotification("", _infNid);
                await _ui.SendError("", _errNid);

                _screenActivator?.ActivateScreen();
                device = await ConnectDevice(mac);

                await WaitDeviceInitialization(mac, device);

                if (device.AccessLevel.IsLocked || device.AccessLevel.IsLinkRequired)
                {
                    // request hes to update this device
                    await _hesConnection.FixDevice(device);
                    await device.RefreshDeviceInfo();
                }

                if (device.AccessLevel.IsLocked)
                    throw new HideezException(HideezErrorCode.DeviceIsLocked);

                if (device.AccessLevel.IsLinkRequired)
                    throw new HideezException(HideezErrorCode.DeviceNotAssignedToUser);

                const int timeout = 60_000;

                await MasterKeyWorkflow(device, timeout);

                if (_workstationUnlocker.IsConnected)
                {
                    if (!await ButtonWorkflow(device, timeout))
                        throw new HideezException(HideezErrorCode.ButtonConfirmationTimeout);

                    await PinWorkflow(device, timeout);

                    // check the button again as it may be outdated while PIN workflow was running
                    if (!await ButtonWorkflow(device, timeout))
                        throw new HideezException(HideezErrorCode.ButtonConfirmationTimeout);

                    if (!device.AccessLevel.IsLocked &&
                        !device.AccessLevel.IsButtonRequired &&
                        !device.AccessLevel.IsPinRequired &&
                        !device.AccessLevel.IsNewPinRequired)
                    {
                        success = await TryUnlockWorkstation(device);
                        flowResult.UnlockSuccessful = success;
                    }
                }
                else
                {
                    success = true;
                }
            }
            catch (HideezException ex) when (ex.ErrorCode == HideezErrorCode.DeviceNotAssignedToUser)
            {
                fatalError = true;
                errorMessage = HideezExceptionLocalization.GetErrorAsString(ex);
            }
            catch (HideezException ex)
            {
                errorMessage = HideezExceptionLocalization.GetErrorAsString(ex);
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }


            try
            {
                await _ui.HidePinUi();
                await _ui.SendNotification("", _infNid);

                if (!string.IsNullOrEmpty(errorMessage))
                {
                    WriteLine(errorMessage);
                    await _ui.SendError(errorMessage, _errNid);
                }

                if (device != null)
                {
                    if (fatalError)
                    {
                        await _deviceManager.Remove(device);
                        flowResult.ConnectSuccessful = false;
                    }
                    else if (!success)
                    {
                        await device.Disconnect();
                        flowResult.ConnectSuccessful = false;
                    }
                    else
                    {
                        device.SetUserProperty(FLOW_FINISHED_PROP, true);
                        DeviceFinishedMainFlow?.Invoke(this, device);
                        flowResult.ConnectSuccessful = true;
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLine(ex, LogErrorSeverity.Fatal);
            }
            finally
            {
                _infNid = string.Empty;
                _errNid = string.Empty;
            }

            Debug.WriteLine(">>>>>>>>>>>>>>> MainWorkflow ------------------------------");

            return flowResult;
        }

        async Task<bool> TryUnlockWorkstation(IDevice device)
        {
            await _ui.SendNotification("Reading credentials from the device...", _infNid);

            // read in parallel info from the HES and credentials from the device
            var infoTask = IsNeedUpdateDevice(device);
            var credentialsTask = GetCredentials(device);

            await Task.WhenAll(infoTask, credentialsTask);

            var credentials = credentialsTask.Result;

            // if the device needs to be updated, update and read credentials again
            if (infoTask.Result)
            {
                await WaitForRemoteDeviceUpdate(device.SerialNo);
                credentials = await GetCredentials(device);
            }

            // send credentials to the Credential Provider to unlock the PC
            await _ui.SendNotification("Unlocking the PC...", _infNid);
            var success = await _workstationUnlocker
                .SendLogonRequest(credentials.Login, credentials.Password, credentials.PreviousPassword);

            return success;
        }

        async Task<bool> IsNeedUpdateDevice(IDevice device)
        {
            try
            {
                if (_hesConnection.State == HesConnectionState.Connected)
                {
                    var info = await _hesConnection.GetInfoBySerialNo(device.SerialNo);
                    return info.NeedUpdate;
                }
            }
            catch (Exception ex)
            {
                WriteLine(ex);
            }

            return false;
        }

        async Task MasterKeyWorkflow(IDevice device, int timeout)
        {
            if (!device.AccessLevel.IsMasterKeyRequired)
                return;

            await _ui.SendNotification("Waiting for HES authorization...", _infNid);
            await _hesConnection.FixDevice(device);
            await _ui.SendNotification("", _infNid);
        }

        async Task<bool> ButtonWorkflow(IDevice device, int timeout)
        {
            if (!device.AccessLevel.IsButtonRequired)
                return true;

            await _ui.SendNotification("Please press the Button on your Hideez Key", _infNid);
            await _ui.ShowButtonConfirmUi(device.Id);
            var res = await device.WaitButtonConfirmation(timeout);
            return res;
        }

        Task<bool> PinWorkflow(IDevice device, int timeout)
        {
            if (device.AccessLevel.IsNewPinRequired)
            {
                return SetPinWorkflow(device, timeout);
            }
            else if (device.AccessLevel.IsPinRequired)
            {
                return EnterPinWorkflow(device, timeout);
            }

            return Task.FromResult(true);
        }

        async Task<bool> SetPinWorkflow(IDevice device, int timeout)
        {
            bool res = false;
            while (device.AccessLevel.IsNewPinRequired)
            {
                await _ui.SendNotification("Please create new PIN code for your Hideez Key", _infNid);
                string pin = await _ui.GetPin(device.Id, timeout, withConfirm: true);

                if (pin == null)
                    return false; // finished by timeout from the _ui.GetPin

                if (string.IsNullOrWhiteSpace(pin))
                {
                    // we received an empty PIN from the user. Trying again with the same timeout.
                    Debug.WriteLine($">>>>>>>>>>>>>>> EMPTY PIN");
                    WriteLine("Received empty PIN");
                    continue;
                }

                res = await device.SetPin(pin); //this using default timeout for BLE commands
            }
            return res;
        }

        async Task<bool> EnterPinWorkflow(IDevice device, int timeout)
        {
            Debug.WriteLine(">>>>>>>>>>>>>>> EnterPinWorkflow +++++++++++++++++++++++++++++++++++++++");

            bool res = false;
            while (!device.AccessLevel.IsLocked)
            {
                await _ui.SendNotification("Please enter the PIN code for your Hideez Key", _infNid);
                string pin = await _ui.GetPin(device.Id, timeout);

                if (pin == null)
                    return false; // finished by timeout from the _ui.GetPin

                Debug.WriteLine($">>>>>>>>>>>>>>> PIN: {pin}");
                if (string.IsNullOrWhiteSpace(pin))
                {
                    // we received an empty PIN from the user. Trying again with the same timeout.
                    Debug.WriteLine($">>>>>>>>>>>>>>> EMPTY PIN");
                    WriteLine("Received empty PIN");

                    continue;
                }

                res = await device.EnterPin(pin); //this using default timeout for BLE commands

                if (res)
                {
                    Debug.WriteLine($">>>>>>>>>>>>>>> PIN OK");
                    break;
                }
                else
                {
                    Debug.WriteLine($">>>>>>>>>>>>>>> Wrong PIN ({device.PinAttemptsRemain} attempts left)");
                    if (device.AccessLevel.IsLocked)
                        await _ui.SendError($"Device is locked", _errNid);
                    else
                        await _ui.SendError($"Wrong PIN ({device.PinAttemptsRemain} attempts left)", _errNid);
                }
            }
            Debug.WriteLine(">>>>>>>>>>>>>>> PinWorkflow ------------------------------");
            return res;
        }

        async Task<IDevice> ConnectDevice(string mac)
        {
            await _ui.SendNotification("Connecting to the device...", _infNid);

            var device = await _deviceManager.ConnectByMac(mac, BleDefines.ConnectDeviceTimeout);

            if (device == null)
            {
                await Task.Delay(CONNECT_RETRY_DELAY); // Wait one second before trying to connect device again
                await _ui.SendNotification("Connection failed. Retrying...", _infNid);
                device = await _deviceManager.ConnectByMac(mac, BleDefines.ConnectDeviceTimeout);
            }

            if (device == null)
            {
                // remove the bond and try one more time
                await _deviceManager.RemoveByMac(mac);
                await _ui.SendNotification("Connection failed. Trying re-bond the device...", _infNid);
                await Task.Delay(CONNECT_RETRY_DELAY); // Wait one second before trying to connect device again
                device = await _deviceManager.ConnectByMac(mac, BleDefines.ConnectDeviceTimeout);
            }

            if (device == null)
                throw new Exception($"Failed to connect device '{mac}'.");

            return device;
        }

        async Task WaitDeviceInitialization(string mac, IDevice device)
        {
            await _ui.SendNotification("Waiting for the device initialization...", _infNid);

            if (!await device.WaitInitialization(BleDefines.DeviceInitializationTimeout))
                throw new Exception($"Failed to initialize device connection '{mac}'. Please try again.");

            if (device.IsErrorState)
            {
                await _deviceManager.Remove(device);
                throw new Exception($"Failed to initialize device connection '{mac}'. Please try again.");
            }
        }

        async Task WaitForRemoteDeviceUpdate(string serialNo) //todo - refactor to use callbacks from server
        {
            if (_hesConnection.State != HesConnectionState.Connected)
                throw new Exception("Cannot update device. Not connected to the HES.");

            DeviceInfoDto info;
            for (int i = 0; i < 10; i++)
            {
                info = await _hesConnection.GetInfoBySerialNo(serialNo);
                if (info.NeedUpdate == false)
                    return;
                await Task.Delay(3000);
            }

            throw new Exception($"Remote device update has been timed out");
        }

        async Task<Credentials> GetCredentials(IDevice device)
        {
            ushort primaryAccountKey = await DevicePasswordManager.GetPrimaryAccountKey(device);
            var credentials = await GetCredentials(device, primaryAccountKey);
            return credentials;
        }

        async Task<Credentials> GetCredentials(IDevice device, ushort key)
        {
            Credentials credentials;

            if (key == 0)
            {
                var str = await device.ReadStorageAsString(
                    (byte)StorageTable.BondVirtualTable1,
                    (ushort)BondVirtualTable1Item.PcUnlockCredentials);

                if (str != null)
                {
                    var parts = str.Split('\n');
                    if (parts.Length >= 2)
                    {
                        credentials.Login = parts[0];
                        credentials.Password = parts[1];
                    }
                    if (parts.Length >= 3)
                    {
                        credentials.PreviousPassword = parts[2];
                    }
                }

                if (credentials.IsEmpty)
                    throw new Exception($"Device '{device.SerialNo}' has not a primary account stored");
            }
            else
            {
                // get the login and password from the Hideez Key
                credentials.Login = await device.ReadStorageAsString((byte)StorageTable.Logins, key);
                credentials.Password = await device.ReadStorageAsString((byte)StorageTable.Passwords, key);
                credentials.PreviousPassword = ""; //todo

                if (credentials.IsEmpty)
                    throw new Exception($"Cannot read login or password from the device '{device.SerialNo}'");
            }

            return credentials;
        }

        void ActivateWorkstationScreen()
        {
            Task.Run(() =>
            {
                try
                {
                    _screenActivator?.ActivateScreen();
                }
                catch (Exception ex)
                {
                    WriteLine(ex);
                }
            });
        }
    }
}
