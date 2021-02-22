using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using System;
using Windows.Devices.Bluetooth;
using Windows.Devices.Radios;

namespace WinBle
{
    internal class AdapterStateMonitor : Logger, IDisposable
    {
        Radio _bluetoothRadio = null;
        BluetoothAdapterState _state = BluetoothAdapterState.PoweredOff;
        
        public event EventHandler AdapterStateChanged;

        public BluetoothAdapterState State
        {
            get
            {
                return _state;
            }
            private set
            {
                if (value != _state)
                {
                    _state = value;
                    AdapterStateChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public AdapterStateMonitor(ILog log)
            :base(nameof(AdapterStateMonitor), log)
        {
        }

        #region IDisposable implementation
        bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                Stop();
            }

            _disposed = true;
        }
        #endregion

        internal async void Start()
        {
            if (_bluetoothRadio == null)
            {
                BluetoothAdapter btAdapter = await BluetoothAdapter.GetDefaultAsync();
                if (btAdapter != null)
                {
                    _bluetoothRadio = await btAdapter.GetRadioAsync();
                    _bluetoothRadio.StateChanged += BluetoothRadio_StateChanged;
                }
            }
        }

        void BluetoothRadio_StateChanged(Radio sender, object args)
        {
            if (sender.State != RadioState.On)
                State = BluetoothAdapterState.Unknown;
        }

        internal void Stop()
        {
            if (_bluetoothRadio != null)
            {
                _bluetoothRadio.StateChanged -= BluetoothRadio_StateChanged;
                _bluetoothRadio = null;
            }
        }
    }
}
