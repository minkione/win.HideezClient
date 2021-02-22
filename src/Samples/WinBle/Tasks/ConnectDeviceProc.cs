using Hideez.SDK.Communication.Utils;
using System;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace WinBle.Tasks
{
    class ConnectDeviceProc
    {
        readonly BluetoothLEDevice _bluetoothLeDevice;
        readonly TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();

        public ConnectDeviceProc(BluetoothLEDevice device)
        {
            _bluetoothLeDevice = device;
        }

        public async Task<GattDeviceServicesResult> Run(int timeout)
        {
            try
            {
                _bluetoothLeDevice.ConnectionStatusChanged += BLeDevice_ConnectionStatusChanged;

                if (_bluetoothLeDevice.ConnectionStatus == BluetoothConnectionStatus.Connected)
                    _tcs.TrySetResult(true);
                
                GattDeviceServicesResult result = await _bluetoothLeDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached);

                await _tcs.Task.TimeoutAfter(timeout);

                return result;
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                _bluetoothLeDevice.ConnectionStatusChanged -= BLeDevice_ConnectionStatusChanged;
            }
        }

        private void BLeDevice_ConnectionStatusChanged(BluetoothLEDevice sender, object args)
        {
            if (sender.ConnectionStatus == BluetoothConnectionStatus.Connected)
                _tcs.TrySetResult(true);
        }
    }
}
