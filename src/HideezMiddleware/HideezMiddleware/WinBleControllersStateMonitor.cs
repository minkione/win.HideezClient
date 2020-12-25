using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using WinBle._10._0._18362;

namespace HideezMiddleware
{
    public sealed class WinBleControllerState
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Mac { get; set; }
        /// <summary>
        /// Shows whether controller is currently connected
        /// </summary>
        public bool IsConnected { get; set; }
        /// <summary>
        /// Shows whether controller advertisements were recently discovered
        /// </summary>
        public bool IsDiscovered { get; set; }
    }

    public sealed class WinBleControllersCollectionChangedEventArgs : EventArgs
    {
        public IEnumerable<WinBleControllerState> WinBleControllers { get; }

        public WinBleControllersCollectionChangedEventArgs(IEnumerable<WinBleControllerState> winBleControllers)
        {
            WinBleControllers = winBleControllers;
        }
    }

    /// <summary>
    /// Provides a dynamic list of win ble connection controllers with information about their connection and discovery state
    /// </summary>
    public sealed class WinBleControllersStateMonitor : Logger
    {
        readonly WinBleConnectionManager _winBleConnectionManager;
        readonly object _lock = new object();
        bool isRunning = false;

        /// <summary>
        /// Invoked when either WinBleControllers collection changes or changes one of the controllers properties
        /// </summary>
        public event EventHandler<WinBleControllersCollectionChangedEventArgs> WinBleControllersCollectionChanged;

        public WinBleControllersStateMonitor(WinBleConnectionManager winBleConnectionManager, ILog log)
            : base(nameof(WinBleControllersStateMonitor), log)
        {
            _winBleConnectionManager = winBleConnectionManager;
        }

        public void Start()
        {
            lock (_lock)
            {
                if (!isRunning)
                {
                    SubscribeToEvents();
                    isRunning = true;
                    WriteLine("Started");
                    NotifySubscribers();
                }
            }
        }

        public void Stop()
        {
            lock (_lock)
            {
                isRunning = false;
                UnsubscribeFromEvents();
                WriteLine("Stopped");
            }
        }

        void SubscribeToEvents()
        {
            _winBleConnectionManager.DiscoveredDeviceAdded += WinBleConnectionManager_DiscoveredDeviceAdded;
            _winBleConnectionManager.DiscoveredDeviceRemoved += WinBleConnectionManager_DiscoveredDeviceRemoved;
            _winBleConnectionManager.BondedControllerAdded += WinBleConnectionManager_BondedControllerAdded;
            _winBleConnectionManager.BondedControllerRemoved += WinBleConnectionManager_BondedControllerRemoved;
        }

        void UnsubscribeFromEvents()
        {
            _winBleConnectionManager.DiscoveredDeviceAdded -= WinBleConnectionManager_DiscoveredDeviceAdded;
            _winBleConnectionManager.DiscoveredDeviceRemoved -= WinBleConnectionManager_DiscoveredDeviceRemoved;
            _winBleConnectionManager.BondedControllerAdded -= WinBleConnectionManager_BondedControllerAdded;
            _winBleConnectionManager.BondedControllerRemoved -= WinBleConnectionManager_BondedControllerRemoved;
        }

        void WinBleConnectionManager_DiscoveredDeviceRemoved(object sender, DiscoveredDeviceRemovedEventArgs e)
        {
            NotifySubscribers();
        }

        void WinBleConnectionManager_DiscoveredDeviceAdded(object sender, DiscoveredDeviceAddedEventArgs e)
        {
            NotifySubscribers();
        }
        
        void WinBleConnectionManager_BondedControllerRemoved(object sender, ControllerRemovedEventArgs e)
        {
            NotifySubscribers();
        }

        void WinBleConnectionManager_BondedControllerAdded(object sender, ControllerAddedEventArgs e)
        {
            NotifySubscribers();
        }

        public void NotifySubscribers()
        {
            WinBleControllersCollectionChangedEventArgs args = null;

            lock (_lock)
            {
                if (isRunning)
                {
                    var controllersCollection = GenerateControllersStateCollection();
                    args = new WinBleControllersCollectionChangedEventArgs(controllersCollection);
                }
            }

            if (args != null)
                SafeInvoke(WinBleControllersCollectionChanged, args);
        }

        List<WinBleControllerState> GenerateControllersStateCollection()
        {
            var bondedControllers = _winBleConnectionManager.BondedControllers;
            var discoveredDevices = _winBleConnectionManager.DiscoveredDevices;

            List<WinBleControllerState> controllers = bondedControllers.Select(bc => new WinBleControllerState()
                {
                    Id = bc.Id,
                    Name = bc.Name,
                    Mac = bc.Mac,
                    IsConnected = bc.State == Hideez.SDK.Communication.ConnectionState.Connected,
                }).ToList();

            foreach (var controller in controllers)
            {
                controller.IsDiscovered = discoveredDevices.Contains(controller.Id);
            }

            return controllers;
        }
    }
}
