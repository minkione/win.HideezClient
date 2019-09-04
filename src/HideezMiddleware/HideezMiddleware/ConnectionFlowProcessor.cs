using System;
using System.Collections.Generic;
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
    public class ConnectionFlowProcessor : Logger
    {
        const int CONNECT_RETRY_DELAY = 1000;
        readonly BleDeviceManager _deviceManager;
        readonly IWorkstationUnlocker _workstationUnlocker;
        readonly IScreenActivator _screenActivator;
        readonly UiProxyManager _ui;
        readonly HesAppConnection _hesConnection;

        int _isConnecting = 0;

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

        public async Task<bool> ConnectAndUnlock(string mac)
        {
            bool res = false;
            // ignore, if workflow for any device already initialized
            if (Interlocked.CompareExchange(ref _isConnecting, 1, 0) == 0)
            {
                res = true;
                try
                {
                    await MainWorkflow(mac);
                }
                finally
                {
                    // this delay allows a user to move away the device from the dongle or RFID
                    // and prevents the repeated call of this method
                    await Task.Delay(1500);

                    Interlocked.Exchange(ref _isConnecting, 0);
                }
            }
            return res;
        }

        async Task MainWorkflow(string mac)
        {
            Debug.WriteLine(">>>>>>>>>>>>>>> MainWorkflow +++++++++++++++++++++++++");

            bool success = false;
            IDevice device = null;
            try
            {
                await _ui.SendNotification("");
                await _ui.SendError("");

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
                    throw new HideezException(HideezErrorCode.DeviceRequiresLink);

                const int timeout = 30_000;

                await MasterKeyWorkflow(device, timeout);

                if (!await ButtonWorkflow(device, timeout))
                    throw new HideezException(HideezErrorCode.ButtonConfirmationTimeout);

                var showStatusTask = ShowWaitStatus(device, timeout);
                var pinTask = PinWorkflow(device, timeout);

                await Task.WhenAll(showStatusTask, pinTask);

                if (!device.AccessLevel.IsLocked &&
                    !device.AccessLevel.IsButtonRequired &&
                    !device.AccessLevel.IsPinRequired &&
                    !device.AccessLevel.IsNewPinRequired)
                {
                    if (_workstationUnlocker.IsConnected)
                        success = await TryUnlockWorkstation(device);
                    else
                        success = true;
                }
            }
            catch (HideezException ex)
            {
                var message = HideezExceptionLocalization.GetErrorAsString(ex);
                WriteLine(message);
                await _ui.SendNotification("");
                await _ui.SendError(message);
                throw;
            }
            catch (Exception ex)
            {
                WriteLine(ex);
                await _ui.SendNotification("");
                await _ui.SendError(ex.Message);
                throw;
            }
            finally
            {
                await _ui.HidePinUi();

                if (!success)
                    await device?.Disconnect();
            }
            Debug.WriteLine(">>>>>>>>>>>>>>> MainWorkflow ------------------------------");
        }

        async Task<bool> TryUnlockWorkstation(IDevice device)
        {
            await _ui.SendNotification("Reading credentials from the device...");

            // read in parallel info from the HES and credentials from the device
            var infoTask = IsNeedUpdateDevice(device);
            var credentialsTask = GetCredentials(device);

            await Task.WhenAll(infoTask, credentialsTask);

            var credentials = credentialsTask.Result;

            // if the device needs to be updated, update and read credentials again
            if (infoTask.Result)
            {
                await WaitForRemoteDeviceUpdate(device.Mac);
                credentials = await GetCredentials(device);
            }

            // send credentials to the Credential Provider to unlock the PC
            await _ui.SendNotification("Unlocking the PC...");
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
                    var info = await _hesConnection.GetInfoByMac(device.Mac);
                    return info.NeedUpdate;
                }
            }
            catch (Exception ex)
            {
                WriteLine(ex);
            }

            return false;
        }

        async Task ShowWaitStatus(IDevice device, int timeout)
        {
            Debug.WriteLine(">>>>>>>>>>>>>>> ShowWaitStatus ++++++++++++++++++++++++++++++++++++");
            var startedTime = DateTime.Now;
            var elapsed = startedTime - DateTime.Now;

            while ( elapsed.TotalMilliseconds < timeout && 
                    !device.AccessLevel.IsLocked &&
                    (device.AccessLevel.IsButtonRequired || device.AccessLevel.IsPinRequired || device.AccessLevel.IsNewPinRequired))
            {
                var statuses = new List<string>();

                if (device.AccessLevel.IsButtonRequired)
                    statuses.Add("Waiting for the BUTTON confirmation...");

                if (device.AccessLevel.IsPinRequired)
                {
                    await _ui.ShowPinUi(device.Id);
                    statuses.Add("Waiting for the PIN...");
                }
                else if (device.AccessLevel.IsNewPinRequired)
                {
                    await _ui.ShowPinUi(device.Id, withConfirm: true);
                    statuses.Add("Waiting for the PIN...");
                }

                await _ui.SendNotification(string.Join("; ", statuses));

                await Task.Delay(300);
                elapsed = DateTime.Now - startedTime;
            }
            Debug.WriteLine(">>>>>>>>>>>>>>> ShowWaitStatus ------------------------------");
        }

        async Task MasterKeyWorkflow(IDevice device, int timeout)
        {
            if (!device.AccessLevel.IsMasterKeyRequired)
                return;

            await _ui.SendNotification("Waiting for HES authorization...");
            await _hesConnection.FixDevice(device);
            await _ui.SendNotification("");
        }

        async Task<bool> ButtonWorkflow(IDevice device, int timeout)
        {
            if (!device.AccessLevel.IsButtonRequired)
                return true;

            await _ui.SendNotification("Please press the Button on your Hideez Key");
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
            string pin = await _ui.GetPin(device.Id, timeout, withConfirm: true);
            if (string.IsNullOrWhiteSpace(pin))
                return false;
            bool res = await device.SetPin(pin); //this using default timeout for BLE commands

            // тут почему-то метка просит нажать кнопку

            return res;
        }

        async Task<bool> EnterPinWorkflow(IDevice device, int timeout)
        {
            Debug.WriteLine(">>>>>>>>>>>>>>> PinWorkflow +++++++++++++++++++++++++++++++++++++++");

            bool res = false;
            while (!device.AccessLevel.IsLocked)
            {
                string pin = await _ui.GetPin(device.Id, timeout);
                Debug.WriteLine($">>>>>>>>>>>>>>> PIN: {pin}");
                if (string.IsNullOrWhiteSpace(pin))
                    break;

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
                        await _ui.SendError($"Device is locked");
                    else
                        await _ui.SendError($"Wrong PIN ({device.PinAttemptsRemain} attempts left)");
                }
            }
            Debug.WriteLine(">>>>>>>>>>>>>>> PinWorkflow ------------------------------");
            return res;
        }

        //------------------------------------

        async Task<IDevice> ConnectDevice(string mac, int tryCount = 2)
        {
            IDevice device = null;

            await _ui.SendNotification("Connecting to the device...");

            do
            {
                device = await _deviceManager.ConnectByMac(mac, BleDefines.ConnectDeviceTimeout);
                if (device == null)
                {
                    await _deviceManager.RemoveByMac(mac);
                    await _ui.SendNotification("Connection failed. Retrying...");
                    await Task.Delay(CONNECT_RETRY_DELAY); // Wait one second before trying to connect device again
                }
            }
            while (--tryCount > 0 && device == null);

            if (device == null)
                throw new Exception($"Failed to connect device '{mac}' after {tryCount} attempts. Please try again.");

            return device;
        }

        async Task WaitDeviceInitialization(string mac, IDevice device)
        {
            await _ui.SendNotification("Waiting for the device initialization...");
            await device.WaitInitialization(BleDefines.DeviceInitializationTimeout);
            if (device.IsErrorState)
            {
                await _deviceManager.Remove(device);
                throw new Exception($"Failed to initialize device connection '{mac}'. Please try again.");
            }
        }

        async Task WaitForRemoteDeviceUpdate(string mac) //todo - refactor to use callbacks from server
        {
            if (_hesConnection.State != HesConnectionState.Connected)
                throw new Exception("Cannot update device. Not connected to the HES.");

            DeviceInfoDto info;
            for (int i = 0; i < 10; i++)
            {
                //todo - GetInfoBySerialNo
                info = await _hesConnection.GetInfoByMac(mac);
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
