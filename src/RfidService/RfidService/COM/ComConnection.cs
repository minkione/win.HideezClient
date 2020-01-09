using System;
using System.IO.Ports;
using System.Text;
using Hideez.RFID;
using Hideez.SDK.Communication.Log;

namespace HideezMiddleware.COM
{
    public class ComConnection : Logger
    {
        readonly byte[] _buf = new byte[1024];
        readonly object _locker = new object();

        SerialPort _port;

        public event EventHandler<RfidReceivedEventArgs> RfidReceived;

        public string PortName { get; }
        public int BaudRate { get; }

        public bool IsConnected => _port?.IsOpen ?? false;

        public ComConnection(ILog log, string portName, int baudRate)
            : base(nameof(ComConnection), log)
        {
            PortName = portName;
            BaudRate = baudRate;
        }

        public void Connect()
        {
            lock (_locker)
            {
                try
                {
                    _port = new SerialPort
                    {
                        PortName = PortName,
                        BaudRate = BaudRate
                    };

                    _port.DataReceived += new SerialDataReceivedEventHandler(OnDataReceived);
                    _port.ErrorReceived += new SerialErrorReceivedEventHandler(OnErrorReceived);

                    _port.Open();
                }
                catch (Exception ex)
                {
                    WriteLine(ex);
                    throw;
                }
            }
        }

        public void Disconnect()
        {
            lock (_locker)
            {
                if (_port != null)
                {
                    if (_port.IsOpen)
                        _port.Close();

                    _port.DataReceived -= new SerialDataReceivedEventHandler(OnDataReceived);
                    _port.ErrorReceived -= new SerialErrorReceivedEventHandler(OnErrorReceived);

                    _port = null;
                }
            }
        }

        void OnErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
        }

        void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                while (_port.BytesToRead > 0)
                {
                    int bytesToRead = _port.BytesToRead <= _buf.Length ? _port.BytesToRead : _buf.Length;
                    int actuallyRead = _port.Read(_buf, 0, bytesToRead);


                    if (actuallyRead > 0)
                    {
                        var data = Encoding.ASCII.GetString(_buf, 0, actuallyRead);
                        RfidReceived?.Invoke(this, new RfidReceivedEventArgs(data));
                        WriteDebugLine(data);
                    }

                    //if (actuallyRead > 0)
                    //{
                    //    int etx = -1;
                    //    for (int i = 0; i < actuallyRead; i++)
                    //    {
                    //        if (_buf[i] == STX)
                    //        {
                    //            _stx = i;
                    //            _snIndex = 0;
                    //        }
                    //        else if (_buf[i] == ETX)
                    //        {
                    //            if (_stx >= 0)
                    //            {
                    //                etx = i;
                    //                break;
                    //            }
                    //        }
                    //        else if (_stx >= 0)
                    //        {
                    //            _sn[_snIndex++] = _buf[i];
                    //        }
                    //    }

                    //    if (_stx >= 0 && etx >= 0)
                    //    {
                    //        var data = Encoding.ASCII.GetString(_sn, 0, _snIndex);
                    //        WriteDebugLine(data);
                    //        _stx = -1;
                    //        _snIndex = 0;
                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {
                WriteLine(ex);
            }
        }


    }
}
