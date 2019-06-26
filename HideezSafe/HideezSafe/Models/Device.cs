﻿using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.PasswordManager;
using Hideez.SDK.Communication.Remote;
using HideezSafe.HideezServiceReference;
using HideezSafe.Modules;
using HideezSafe.Modules.ServiceProxy;
using HideezSafe.Mvvm;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HideezSafe.Models
{
    public class Device : ObservableObject
    {
        readonly ILogger _log = LogManager.GetCurrentClassLogger();
        readonly IServiceProxy _serviceProxy;
        readonly IRemoteDeviceFactory _remoteDeviceFactory;
        RemoteDevice _remoteDevice;

        string id;
        string name;
        string ownerName;
        bool isConnected;
        string serialNo;
        double proximity;
        int battery;
        bool isInitializing;
        bool isInitialized;

        public Device(IServiceProxy serviceProxy, IRemoteDeviceFactory remoteDeviceFactory)
        {
            this._serviceProxy = serviceProxy;
            this._remoteDeviceFactory = remoteDeviceFactory;
        }

        public IDeviceStorage Storage
        {
            get
            {
                return _remoteDevice;
            }
        }

        public DevicePasswordManager PasswordManager { get; private set; }

        public string TypeName { get; } = "Hideez key";

        public string Id
        {
            get { return id; }
            set { Set(ref id, value); }
        }

        public string Name
        {
            get { return name; }
            set { Set(ref name, value); }
        }

        public bool IsConnected
        {
            get { return isConnected; }
            set
            {
                Set(ref isConnected, value);
                if (!isConnected)
                {
                    Proximity = 0;
                    CloseRemoteDeviceConnection();
                }
            }
        }

        public double Proximity
        {
            get { return proximity; }
            set { Set(ref proximity, value); }
        }

        public int Battery
        {
            get { return battery; }
            set { Set(ref battery, value); }
        }

        public string OwnerName
        {
            get { return ownerName; }
            set { Set(ref ownerName, value); }
        }

        public string SerialNo
        {
            get { return serialNo; }
            set { Set(ref serialNo, value); }
        }

        public bool IsInitializing
        {
            get { return isInitializing; }
            private set { Set(ref isInitializing, value); }
        }

        public bool IsInitialized
        {
            get { return isInitialized; }
            private set { Set(ref isInitialized, value); }
        }

        int remoteConnectionEstablishment = 0;
        public async Task EstablishRemoteDeviceConnection()
        {
            if (IsInitialized)
                return;

            if (Interlocked.CompareExchange(ref remoteConnectionEstablishment, 1, 0) == 0)
            {
                IsInitializing = true;

                const int AUTH_CHANNEL = 2;
                const int AUTH_WAIT = 20_000;
                const int INIT_WAIT = 5_000;
                const int RETRY_DELAY = 2_500;

                try
                {
                    while (IsInitializing)
                    {
                        try
                        {
                            _remoteDevice = await _remoteDeviceFactory.CreateRemoteDeviceAsync(SerialNo, AUTH_CHANNEL);
                            if (_remoteDevice == null)
                            {
                                if (IsInitializing)
                                    await Task.Delay(RETRY_DELAY);

                                continue;
                            }

                            await _remoteDevice.Authenticate(AUTH_CHANNEL, null);
                            await _remoteDevice.WaitAuthentication(AUTH_WAIT);
                            await _remoteDevice.Initialize(INIT_WAIT);

                            if (_remoteDevice.SerialNo != SerialNo)
                            {
                                _serviceProxy.GetService().RemoveDevice(_remoteDevice.DeviceId);
                                throw new Exception("Remote device serial number does not match enumerated serial number");
                            }

                            _remoteDevice.ProximityChanged += RemoteDevice_ProximityChanged;
                            _remoteDevice.BatteryChanged += RemoteDevice_BatteryChanged;

                            Proximity = _remoteDevice.Proximity;
                            Battery = _remoteDevice.Battery;

                            PasswordManager = new DevicePasswordManager(_remoteDevice, null);
                            await PasswordManager.Load();

                            IsInitialized = true;
                            break;
                        }
                        catch (Exception ex)
                        {
                            _log.Error(ex);

                            if (IsInitializing)
                                await Task.Delay(RETRY_DELAY);
                        }
                    }
                }
                finally
                {
                    Interlocked.Exchange(ref remoteConnectionEstablishment, 0);
                    IsInitializing = false;
                }
            }
        }

        public void CloseRemoteDeviceConnection()
        {
            if (_remoteDevice != null)
            {
                _remoteDevice.ProximityChanged -= RemoteDevice_ProximityChanged;
                _remoteDevice.BatteryChanged -= RemoteDevice_BatteryChanged;
                _remoteDevice = null;
                PasswordManager = null;

                Battery = 0;
                Proximity = 0;

                IsInitialized = false;
                isInitializing = false;
            }
        }

        void RemoteDevice_ProximityChanged(object sender, int proximity)
        {
            Proximity = proximity;
        }

        void RemoteDevice_BatteryChanged(object sender, int battery)
        {
            Battery = battery;
        }

        public void LoadFrom(DeviceDTO dto)
        {
            id = dto.Id;
            Name = dto.Name;
            OwnerName = dto.Owner ?? "...unspecified...";
            IsConnected = dto.IsConnected;
            SerialNo = dto.SerialNo;
        }
    }
}
