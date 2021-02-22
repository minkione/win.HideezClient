using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Utils;
using System;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;

namespace WinBle.Tasks
{
    class DisconnectDeviceProc
    {
        readonly BluetoothLEDevice _bluetoothLeDevice;
        readonly TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();

        public DisconnectDeviceProc(BluetoothLEDevice device)
        {
            _bluetoothLeDevice = device;
        }

        public async Task Run(int timeout)
        {
            try
            {
                _bluetoothLeDevice.ConnectionStatusChanged += BLeDevice_ConnectionStatusChanged;

                _bluetoothLeDevice?.Dispose();

                await _tcs.Task.TimeoutAfter(timeout);

            }
            catch (Exception ex)
            {
                throw new HideezException(ex);
            }
            finally
            {
                _bluetoothLeDevice.ConnectionStatusChanged -= BLeDevice_ConnectionStatusChanged;
            }
        }

        private void BLeDevice_ConnectionStatusChanged(BluetoothLEDevice sender, object args)
        {
            if(sender?.ConnectionStatus == BluetoothConnectionStatus.Disconnected)
                _tcs.TrySetResult(true);
        }
    }
}
