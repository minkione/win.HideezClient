using System;
using System.Runtime.InteropServices;
using System.Threading;
using Hideez.SDK.Communication.Log;

namespace Hideez.RFID
{
    public class RfidReceivedEventArgs : EventArgs
    {
        public RfidReceivedEventArgs(string rfid)
            : base()
        {
            Rfid = rfid;
        }

        public string Rfid { get; set; }
    }

    public class ReaderStateChangedEventArgs : EventArgs
    {
        public ReaderStateChangedEventArgs(bool isConnected)
            : base()
        {
            IsConnected = isConnected;
        }

        public bool IsConnected { get; set; }
    }

    class RfidConnection : Logger
    {
        public event EventHandler<RfidReceivedEventArgs> RfidReceived;
        public event EventHandler<ReaderStateChangedEventArgs> ReaderStateChanged;

        int h_Reader;
        bool _stopped;

        int Reader
        {
            get
            {
                return h_Reader;
            }
            set
            {
                // When we manually stop reader on error, value is set to 0
                // When reader is unavailable, value is a random negative nubmer
                // This check prevents OnReaderStateChanged from being triggered two times when reader is plugged off
                if (value < 0)
                    value = 0;

                if (h_Reader != value)
                {
                    h_Reader = value;
                    OnReaderStateChanged();
                }
            }
        }

        #region DllImport
        [DllImport("SRF32.dll")]
        private static extern int s_init(
            ushort PortNum,
            int combaud,
            ushort DTR_State,
            ushort RTS_State);

        [DllImport("SRF32.dll")]
        private static extern ushort s_exit(int m_hUSB);

        [DllImport("SRF32.dll")]
        private static extern ushort s_bell(int m_hUSB, ushort bell_time);

        [DllImport("SRF32.dll")]
        private static extern ushort s_emid_read(int m_hUSB, byte[] emid_number);

        [DllImport("SRF32.dll")]
        private static extern ushort s_emid_WriteT5557(
          int m_hUSB,
          byte[] emid_number,
          ushort type,
          byte[] card_password);

        [DllImport("SRF32.dll")]
        private static extern ushort s_emid_Write8800(int m_hUSB, byte[] emid_number);

        [DllImport("SRF32.dll")]
        private static extern ushort s_em4305_readWord(
          int m_hUSB,
          ushort rf_freq,
          ushort block,
          byte[] card_data);

        [DllImport("SRF32.dll")]
        private static extern ushort s_emid_WriteEM4305(
          int m_hUSB,
          byte[] emid_number,
          ushort type,
          byte[] card_password);

        [DllImport("USER32.DLL")]
        public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);
        #endregion DllImport

        public RfidConnection(ILog log)
            :base(nameof(RfidConnection), log)
        {
        }

        public bool IsConnected
        {
            get
            {
                return Reader > 0;
            }
        }

        public bool Start()
        {
            WriteDebugLine("Starting service...");
            Thread thread = new Thread(ThreadProc);
            thread.Start();
            return true;
        }

        internal void Stop()
        {
            WriteDebugLine("Stopping service...");
            _stopped = true;
        }

        private void ThreadProc()
        {
            while (!_stopped)
            {
                try
                {
                    var init = s_init(0, 0, 0, 0);
                    if (init == -1005)
                        s_exit(0);

                    Reader = init;
                    if (Reader > 0)
                    {
                        Bell(Reader, 100);

                        string prevId = string.Empty;
                        byte[] buf = new byte[128];
                        while (!_stopped)
                        {
                            int readRes = s_emid_read(Reader, buf);
                            //Console.WriteLine($"readRes: {readRes}");
                            if (readRes == 5)
                            {
                                string id = $"{buf[0]:X2}{buf[1]:X2}{buf[2]:X2}{buf[3]:X2}{buf[4]:X2}\n";

                                if (id != prevId)
                                {
                                    prevId = id;
                                    OnRfidReceived(id);
                                    Bell(Reader, 10);
                                }
                            }
                            else if (readRes == 64424)
                            {
                                Reader = 0;
                                break;
                            }
                            else
                            {
                                prevId = string.Empty;
                            }

                            Thread.Sleep(200);
                        }
                    }
                    else
                    {
                        Thread.Sleep(2000);
                    }
                }
                catch (Exception ex)
                {
                    WriteDebugLine(ex);
                    Thread.Sleep(2000);
                }
                WriteDebugLine("Finished");
            }
        }

        void OnRfidReceived(string rfid)
        {
            RfidReceived?.Invoke(this, new RfidReceivedEventArgs(rfid));
        }

        void OnReaderStateChanged()
        {
            var isConnected = Reader > 0;
            ReaderStateChanged?.Invoke(this, new ReaderStateChangedEventArgs(isConnected));
        }

        readonly int bellIntervalMs = 500;
        DateTime lastBeepTime = DateTime.MinValue;
        /// <summary>
        /// Emit beep signal from reader, but no more than once per 500ms
        /// </summary>
        void TryBell(int u_hUsb, ushort belltime)
        {
            var delta = DateTime.UtcNow - lastBeepTime;
            if (delta.TotalMilliseconds > bellIntervalMs)
            {
                Bell(u_hUsb, belltime);
                lastBeepTime = DateTime.UtcNow;
            }
        }
        
        /// <summary>
        /// Emit beep signal from reader
        /// </summary>
        void Bell(int u_hUsb, ushort belltime)
        {
            s_bell(u_hUsb, belltime);
        }
    }
}
