using System;
using System.Linq;
using System.Management;
using Hideez.RFID;
using Hideez.SDK.Communication.Log;

namespace HideezMiddleware.COM
{
    public class ComPortManager : Logger
    {
		ManagementEventWatcher _watcher;
        ComConnection _comConnection;

        public bool IsConnected => _comConnection?.IsConnected ?? false;

        public event EventHandler<RfidReceivedEventArgs> RfidReceived;
        public event EventHandler<ReaderStateChangedEventArgs> ReaderStateChanged;

        public ComPortManager(ILog log)
            :base(nameof(ComPortManager), log)
        {
        }

        public void Stop()
        {
            if (_watcher != null)
            {
                _watcher?.Stop();
                _watcher.EventArrived -= new EventArrivedEventHandler(Watcher_EventArrived);
                _watcher = null;
            }

            if (_comConnection != null)
            {
                _comConnection.Disconnect();
                _comConnection.RfidReceived -= ComConnection_RfidReceived;
                _comConnection = null;
            }
        }

        public void Start()
        {
            try
            {
                var query = new WqlEventQuery
                {
                    EventClassName = "__InstanceOperationEvent",
                    WithinInterval = new TimeSpan(0, 0, 3),
                    Condition = @"TargetInstance ISA 'Win32_USBControllerDevice'"
                };

                var scope = new ManagementScope("root\\CIMV2");
                scope.Options.EnablePrivileges = true;

                _watcher = new ManagementEventWatcher(scope, query);
                _watcher.EventArrived += new EventArrivedEventHandler(Watcher_EventArrived);
                _watcher.Start();

                OnPortStateChanged();
            }
            catch (Exception ex)
            {
                WriteLine(ex);
            }
        }

        void Watcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            OnPortStateChanged();
        }

        void OnPortStateChanged()
        {
            var port = GetCH340SerialPort();

            if (port != null)
            {
                if (_comConnection != null && _comConnection.PortName != port)
                {
                    _comConnection.Disconnect();
                    _comConnection.RfidReceived -= ComConnection_RfidReceived;
                    _comConnection = null;
                }

                if (_comConnection == null)
                {
                    _comConnection = new ComConnection(_log, port, 9600);
                    _comConnection.RfidReceived += ComConnection_RfidReceived;
                }

                _comConnection.Connect();
            }
            else
            {
                _comConnection?.Disconnect();
            }

            OnReaderStateChanged();
        }

        void ComConnection_RfidReceived(object sender, RfidReceivedEventArgs e)
        {
            OnRfidReceived(e.Rfid);
        }

        void OnRfidReceived(string rfid)
        {
            RfidReceived?.Invoke(this, new RfidReceivedEventArgs(rfid));
        }
        
        void OnReaderStateChanged()
        {
            ReaderStateChanged?.Invoke(this, new ReaderStateChangedEventArgs(IsConnected));
        }

        static string GetCH340SerialPort()
        {
            using (var searcher = new ManagementObjectSearcher
                ("SELECT * FROM Win32_PnPEntity WHERE Name LIKE 'USB-SERIAL CH340%'"))
            {
                var port = searcher.Get().Cast<ManagementBaseObject>().FirstOrDefault();
                if (port != null)
                {
                    var caption = port.GetPropertyValue("Caption").ToString();
                    if (caption != null)
                    {
                        var ind = caption.LastIndexOf("(COM");
                        if (ind > 0)
                        {
                            return caption.Substring(ind + 1).Replace(")", string.Empty).Trim();
                        }
                    }
                }
            }

            return null;
        }
    }
}
