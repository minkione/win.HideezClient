using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.Log;
using HideezMiddleware.Settings;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HideezMiddleware.DeviceConnection
{
    class RfidConnectionProcessor : BlacklistConnectionProcessor, IDisposable
    {
        readonly IClientUi _clientUi;
        readonly HesAppConnection _hesConnection;
        readonly RfidServiceConnection _rfidService;
        readonly IScreenActivator _screenActivator;
        readonly ISettingsManager<UnlockerSettings> _unlockerSettingsManager;

        int isConnecting = 0;

        public RfidConnectionProcessor(
            ConnectionFlowProcessor connectionFlowProcessor, 
            HesAppConnection hesConnection,
            RfidServiceConnection rfidService, 
            IScreenActivator screenActivator,
            IClientUi clientUi, 
            ISettingsManager<UnlockerSettings> unlockerSettingsManager,
            ILog log) 
            : base(connectionFlowProcessor, nameof(RfidConnectionProcessor), log)
        {
            _hesConnection = hesConnection;
            _rfidService = rfidService;
            _screenActivator = screenActivator;
            _clientUi = clientUi;
            _unlockerSettingsManager = unlockerSettingsManager;

            _rfidService.RfidReceivedEvent += RfidService_RfidReceivedEvent;
            _unlockerSettingsManager.SettingsChanged += UnlockerSettingsManager_SettingsChanged;

            SetAccessListFromSettings(_unlockerSettingsManager.Settings);
        }

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed = false;
        void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                _rfidService.RfidReceivedEvent -= RfidService_RfidReceivedEvent;
                _unlockerSettingsManager.SettingsChanged -= UnlockerSettingsManager_SettingsChanged;
            }

            disposed = true;
        }

        ~RfidConnectionProcessor()
        {
            Dispose(false);
        }
        #endregion

        // Todo: Maybe add Start/Stop methods to RfidConnectionProcessor

        void SetAccessListFromSettings(UnlockerSettings settings)
        {
            AccessList = settings.DeviceUnlockerSettings
                .Where(s => s.AllowRfid)
                .Select(s => new ShortDeviceInfo()
                {
                    Mac = s.Mac,
                    SerialNo = s.SerialNo
                })
                .ToList();
        }

        void UnlockerSettingsManager_SettingsChanged(object sender, SettingsChangedEventArgs<UnlockerSettings> e)
        {
            SetAccessListFromSettings(e.NewSettings);
        }

        async void RfidService_RfidReceivedEvent(object sender, RfidReceivedEventArgs e)
        {
            await UnlockByRfid(e.Rfid);
        }

        async Task UnlockByRfid(string rfid)
        {
            if (Interlocked.CompareExchange(ref isConnecting, 1, 1) == 1)
                return;

            try
            {
                _screenActivator?.ActivateScreen();
                await _clientUi.SendNotification("Connecting to the HES server...");

                if (_hesConnection == null)
                    throw new Exception("Cannot connect device. Not connected to the HES.");

                // get MAC address from the HES
                var info = await _hesConnection.GetInfoByRfid(rfid);

                if (info == null)
                    throw new Exception($"Device not found");

                if (Interlocked.CompareExchange(ref isConnecting, 1, 0) == 0)
                {
                    try
                    {
                        await ConnectDeviceByMac(info.DeviceMac);
                    }
                    finally
                    {
                        Interlocked.Exchange(ref isConnecting, 0);
                    }
                }
            }
            catch (AccessDeniedAuthException ex)
            {
                WriteLine(ex);
                await _clientUi.SendNotification("");
                await _clientUi.SendError(ex.Message);
            }
            catch (Exception ex)
            {
                WriteLine(ex);
            }
        }

    }
}
