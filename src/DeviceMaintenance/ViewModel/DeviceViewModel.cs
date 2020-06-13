using DeviceMaintenance.Messages;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.LongOperations;
using Meta.Lib.Modules.PubSub;
using MvvmExtensions.Attributes;
using MvvmExtensions.PropertyChangedMonitoring;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DeviceMaintenance.ViewModel
{
    public class DeviceViewModel : PropertyChangedImplementation
    {
        //  None -> Bonding|Connecting -> Connected -> EnteringBoot -> Updating -> Success
        public enum State
        {
            None,
            Bonding,
            Connecting,
            Connected,
            EnteringBoot,
            Updating,
            Success,
            Error
        }

        readonly object _stateLocker = new object();
        readonly MetaPubSub _hub;
        readonly string _mac;
        readonly bool _isBonded;
        readonly LongOperation _longOperation = new LongOperation(1);

        IDevice _device = null;
        string _customError = string.Empty;
        State _state;


        public DateTime CreatedAt = DateTime.Now;
        public bool IsConnected => _device?.IsConnected ?? false;
        public bool IsBoot => _device?.IsBoot ?? false;
        public string SerialNo => _device?.SerialNo != null ? $"{_device.SerialNo} (v{_device.FirmwareVersion}, b{_device.BootloaderVersion})" : _mac;

        public double Progress => _longOperation.Progress;
        public bool InProgress => _longOperation.IsRunning;

        public string CustomError
        {
            get
            {
                return _customError;
            }
            set
            {
                if (_customError != value)
                {
                    _customError = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public State CurrentState
        {
            get
            {
                return _state;
            }
            set
            {
                _state = value;
                NotifyPropertyChanged();
            }
        }


        #region Visual States
        [DependsOn(nameof(CurrentState))]
        public bool BondingState => CurrentState == State.Bonding;

        [DependsOn(nameof(CurrentState))]
        public bool ConnectingState => CurrentState == State.Connecting;

        [DependsOn(nameof(CurrentState))]
        public bool ReadyToUpdateState => CurrentState == State.Connected;

        [DependsOn(nameof(CurrentState))]
        public bool EnteringBootModeState => CurrentState == State.EnteringBoot;

        [DependsOn(nameof(CurrentState))]
        public bool UpdatingState => CurrentState == State.Updating;

        [DependsOn(nameof(CurrentState))]
        public bool SuccessState => CurrentState == State.Success;

        [DependsOn(nameof(CurrentState))]
        public bool ErrorState => CurrentState == State.Error;
        #endregion

        public ICommand UpdateDevice
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = async (x) =>
                    {
                        await StartFirmwareUpdate((string)x);
                    }
                };
            }
        }

        public DeviceViewModel(string mac, bool isBonded, MetaPubSub hub)
        {
            RegisterDependencies();

            _hub = hub;
            _mac = mac;
            _isBonded = isBonded;

            _longOperation.StateChanged += (object sender, EventArgs e) =>
            {
                NotifyPropertyChanged(nameof(InProgress));
                NotifyPropertyChanged(nameof(Progress));
            };
        }

        public void SetDevice(IDevice device)
        {
            _device = device;

            _device.ConnectionStateChanged += (object sender, EventArgs e) =>
            {
                NotifyPropertyChanged(nameof(IsConnected));
            };

            _device.PropertyChanged += (object sender, string e) =>
            {
                NotifyPropertyChanged(e);
            };

            NotifyPropertyChanged(nameof(SerialNo));
        }

        //  None -> Bonding|Connecting -> Connected -> EnteringBoot -> Updating -> Success
        internal async Task TryConnect()
        {
            lock (_stateLocker)
            {
                if (CurrentState != State.None && CurrentState != State.Error)
                    return;
                CurrentState = _isBonded ? State.Connecting : State.Bonding;
            }

            try
            {
                var res = await _hub.Process<ConnectDeviceResponse>(
                    new ConnectDeviceCommand(_mac),
                    SdkConfig.ConnectDeviceTimeout * 2 + SdkConfig.DeviceInitializationTimeout,
                    x => x.Mac == _mac);

                if (res.Device == null)
                    throw new Exception("Failed to connect device");

                CurrentState = State.Connected;
                SetDevice(res.Device);
                await _hub.Publish(new DeviceConnectedEvent(this));
            }
            catch (Exception ex)
            {
                CustomError = ex.FlattenMessage();
                CurrentState = State.Error;
            }
        }

        public async Task StartFirmwareUpdate(string filePath)
        {
            try
            {
                CurrentState = State.EnteringBoot;

                var res = await _hub.Process<EnterBootResponse>(
                    new EnterBootCommand(this, filePath, _device, _longOperation), 5_000);

                CurrentState = State.Updating;

                await res.ImageUploader.Run(false);

                CurrentState = State.Success;
            }
            catch (Exception ex)
            {
                CustomError = ex.FlattenMessage();
                CurrentState = State.Error;
            }
        }
    }
}
