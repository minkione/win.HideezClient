using System;
using System.Collections.Generic;
using System.Text;

namespace Hideez.SDK.Communication.Utils
{
    public static class DeviceLogParser
    {
        private const int EVENTLOG_BOOT = 0x0;
        private const int EVENTLOG_ASSERT = 0x1;
        private const int EVENTLOG_STACKOVF = 0x2;
        private const int EVENTLOG_USER = 0x7;
        private const int EVENTLOG_UIRQ = 0x8;
        private const int EVENTLOG_NMI = 0x9;
        private const int EVENTLOG_HFLT = 0xa;
        private const int EVENTLOG_MMFLT = 0xb;
        private const int EVENTLOG_BFLT = 0xc;
        private const int EVENTLOG_UFLT = 0xd;
        private const int EVENTLOG_DMON = 0xe;
        private const int EVENTLOG_SDFAULT = 0xf;
        private const int EVENTLOG_USER_CONNECT = 0x01;
        private const int EVENTLOG_USER_DISCONNECT = 0x02;

        public static UInt32 BinToUint32(byte[] bin, int index)
        {
            UInt32 dword = (UInt32)(bin[index] + (bin[index + 1] << 8) + (bin[index + 2] << 16) + (bin[index + 3] << 24));
            return dword;
        }

        public static string Parser(byte[] log)
        {
            List<string> items = new List<string>();
            string s0;
            UInt32 hword, d0, d1,d2,d3,d4,d5;
            string timestr;
            int p = 4;
            do
            {
                hword = BinToUint32(log, p);
                p += 4;
                if (hword == 0xffffffffUL) break;
                int type = (int)(hword >> 28);
                int count = (type == 0) ? 0 : (int)((hword >> 24) & 0xf);
                if ((hword & 0xffffff) != 0xffffff)
                {
                    DateTimeOffset offset = DateTimeOffset.Now;
                    UInt32 currenttime =(UInt32)offset.ToUnixTimeSeconds();
                    UInt32 devtime;
                    devtime = (currenttime & ~0xffffffU) | ((UInt32)(hword & 0xffffffU));
                    while (devtime > currenttime) devtime -= 0x1000000;
                    //DateTimeOffset value  =DateTimeOffset.FromUnixTimeSeconds(devtime).DateTime;
                    DateTime DT = ConvertUtils.UnixTimeToDateTime((uint)devtime);
                    timestr = DT.ToString();
                }
                else
                {                    
                    timestr = "  -- . -- . ----    -- : -- : --";
                }
                s0 = null;
                switch (type)
                {
                    case EVENTLOG_BOOT:
                        count = 0;                        
                        items.Add(timestr + " Device boot [ reset by " + ((int)((hword >> 24) & 0xf)).ToString() + " ] ");
                        break;
                    case EVENTLOG_ASSERT:                        
                        d0= BinToUint32(log, p);
                        d1 = BinToUint32(log, p+4); 
                        items.Add(timestr + "  Assertion failed 0x" +  d1.ToString("X") + " line " + d0.ToString("X"));
                        break;
                    case EVENTLOG_STACKOVF:                        
                        items.Add(timestr + " Stack overflow");
                        break;
                    case EVENTLOG_USER:

                        d0 = BinToUint32(log, p);                        
                        int usertype = (int)(d0 & 0xff);
                        switch (usertype)
                        {
                            case EVENTLOG_USER_CONNECT:
                                {
                                    items.Add(timestr + " Connect: 0x" + (d0>>8).ToString("X"));
                                    break;
                                }
                            case EVENTLOG_USER_DISCONNECT:
                                {
                                    UInt32 disconnect = (d0 >> 8);
                                    string disconn_Err;
                                    switch (disconnect)
                                    {
                                        case 0x5:
                                            disconn_Err = "BLE_HCI_AUTHENTICATION_FAILURE";
                                            break;
                                        case 0x6:
                                            disconn_Err = "BLE_HCI_STATUS_CODE_PIN_OR_KEY_MISSING";
                                            break;
                                        case 0x8:
                                            disconn_Err = "BLE_HCI_CONNECTION_TIMEOUT";
                                            break;
                                        case 0x13:
                                            disconn_Err = "BLE_HCI_REMOTE_USER_TERMINATED_CONNECTION";
                                            break;
                                        case 0x14:
                                            disconn_Err = "BLE_HCI_REMOTE_DEV_TERMINATION_DUE_TO_LOW_RESOURCES";
                                            break;
                                        case 0x15:
                                            disconn_Err = "BLE_HCI_REMOTE_DEV_TERMINATION_DUE_TO_POWER_OFF";
                                            break;
                                        case 0x16:
                                            disconn_Err = "BLE_HCI_LOCAL_HOST_TERMINATED_CONNECTION";
                                            break;
                                        case 0x1e:
                                            disconn_Err = "BLE_HCI_STATUS_CODE_INVALID_LMP_PARAMETERS";
                                            break;
                                        case 0x1f:
                                            disconn_Err = "BLE_HCI_STATUS_CODE_UNSPECIFIED_ERROR";
                                            break;                                
                                        case 0x22:
                                            disconn_Err = "BLE_HCI_STATUS_CODE_LMP_RESPONSE_TIMEOUT";
                                            break;
                                        case 0x23:
                                            disconn_Err = "BLE_HCI_STATUS_CODE_LMP_ERROR_TRANSACTION_COLLISION";
                                            break;
                                        case 0x24:
                                            disconn_Err = "BLE_HCI_STATUS_CODE_LMP_PDU_NOT_ALLOWED";
                                            break;
                                        default:
                                            disconn_Err = "No Name";
                                            break;
                                    }
                                    items.Add(timestr + " Disconnect: 0x" + disconnect.ToString("X") + " ["+ disconn_Err+"]");
                                    break;
                                }
                            default:
                                {
                                    items.Add(timestr + " User event: " + usertype.ToString() + " Var:" + (d0 >> 8).ToString("X"));
                                    break;
                                }
                        }                                
                        break;
                    
                    case EVENTLOG_UIRQ:
                        s0 = " Unhandled IRQ "; 
                        break;
                    case EVENTLOG_NMI:
                        s0 = " NMI "; 
                        break;
                    case EVENTLOG_HFLT:
                        s0 = " Hard Fault "; 
                        break;
                    case EVENTLOG_MMFLT:
                        s0 = " MM Fault "; 
                        break;
                    case EVENTLOG_BFLT:
                        s0 = " Bus Fault "; 
                        break;
                    case EVENTLOG_UFLT:
                        s0 = " Usage Fault "; 
                        break;
                    case EVENTLOG_DMON:
                        s0 = " Debug event "; 
                        break;                        
                    case EVENTLOG_SDFAULT:                        
                        d0 = BinToUint32(log, p);
                        d1 = BinToUint32(log, p + 4);
                        d2 = BinToUint32(log, p + 8);
                        items.Add(timestr + "  SD fault ID: 0x" + d0.ToString("X") + " PC: 0x" + d1.ToString("X")+ " info: 0x" + d2.ToString("X"));
                        break;
                    default:                        
                        items.Add(timestr + "  unknown event:" + type);
                        break;
                }

                if(s0!=null)
                {
                    d0 = BinToUint32(log, p);
                    d1 = BinToUint32(log, p + 4);
                    d2 = BinToUint32(log, p + 8);
                    d3 = BinToUint32(log, p + 12);
                    d4 = BinToUint32(log, p + 16);
                    d5 = BinToUint32(log, p + 20);                    
                    items.Add(timestr + s0+ " PC:" + d0.ToString("X")+  " LR:" + d1.ToString("X") + " SP:" + d2.ToString("X") + " PSR:" + d3.ToString("X") + " CFSR:" + d4.ToString("X") + " ADDR:" + d5.ToString("X"));
                }

                p += count*4;
            } while (p < log.Length);

            StringBuilder builder = new StringBuilder();
            foreach (string safePrime in items)
            {                
                builder.Append(safePrime).Append(Environment.NewLine);
            }
            string result = builder.ToString();
            return result;

        }



    }
}
