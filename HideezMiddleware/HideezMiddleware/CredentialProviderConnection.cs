using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.NamedPipes;
using System;
using System.Text;
using System.Threading.Tasks;

namespace HideezMiddleware
{
    public enum CredentialProviderCommandCode
    {
        Logon = 1,
        Error = 2,
        UserList = 3,
        PasswordChange = 4,
        PasswordChangeCompleated = 5,
        Notification = 6,
        Status = 7,
    }

    public class CredentialProviderConnection : Logger
    {
        readonly PipeServer _pipeServer;

        public event EventHandler<EventArgs> OnProviderConnected;

        public CredentialProviderConnection(ILog log)
            : base(nameof(CredentialProviderConnection), log)
        {
            _pipeServer = new PipeServer("hideezsafe3", log);
            _pipeServer.MessageReceivedEvent += PipeServer_MessageReceivedEvent;
            _pipeServer.ClientConnectedEvent += PipeServer_ClientConnectedEvent;
        }

        public void Start()
        {
            _pipeServer.Start();
        }
        
        public void Stop()
        {
            _pipeServer.Stop();
        }

        void PipeServer_MessageReceivedEvent(object sender, MessageReceivedEventArgs e)
        {
            WriteDebugLine($"PipeServer_MessageReceivedEvent {e.Buffer.Length} bytes length");

            byte[] buf = e.Buffer;
            int len = e.ReadBytes;

            int code = BitConverter.ToInt32(buf, 0);

            if (code == 1)
            {
                string login = Encoding.Unicode.GetString(buf, 4, len - 4);
                OnLogonRequestByLoginName(login);
            }
            else if (code == 2)
            {
                string machineName = Encoding.Unicode.GetString(buf, 4, len - 4);
                //OnLogonRequestByMachineName(machineName.Trim());
            }
            else if (code == 3)
            {
                //OnUserListRequest();
            }
            else if (code == 4)
            {
                string login = Encoding.Unicode.GetString(buf, 4, len - 4);
                //OnPasswordChangeBegin(login);
            }
            else if (code == 5)
            {
                WriteDebugLine($"OnPasswordChangeEnd !!!!!!!!!!!!!!!!!!!!!!!!!!! ");
                string prms = Encoding.Unicode.GetString(buf, 4, len - 4);
                var strings = prms.Split('\n');
                if (strings.Length == 3)
                {
                    WriteDebugLine($"OnPasswordChangeEnd !!!!!!!!!!!!!!!!!!!!!!!!!!! 222 ");
                    //OnPasswordChangeEnd(strings[0], strings[1], strings[2]);
                }
                else
                {
                    WriteDebugLine($"OnPasswordChangeEnd Error ------------------------ {prms}");
                }
            }
        }

        void PipeServer_ClientConnectedEvent(object sender, ClientConnectedEventArgs e)
        {
            OnProviderConnected?.Invoke(this, EventArgs.Empty);
        }

        async void OnLogonRequestByLoginName(string login)
        {
            WriteDebugLine($"LogonWorkstationAsync ------------------------ {login}");
            //todo
            await SendMessageAsync(CredentialProviderCommandCode.Logon, true, $"{login}\n123");
        }

        public async Task SendLogonRequest(string login, string password, string prevPassword)
        {
            //WriteDebugLine($"SendLogonRequest ------------------------ {login}");
            await SendMessageAsync(CredentialProviderCommandCode.Logon, true, $"{login}\n{password}\n{prevPassword}");
        }

        public async Task SendError(string message)
        {
            WriteDebugLine($"SendError ------------------------ {message}");
            await SendMessageAsync(CredentialProviderCommandCode.Error, true, $"{DateTime.Now.ToShortTimeString()}: {message}");
        }

        public async Task SendNotification(string message)
        {
            WriteDebugLine($"SendNotification ------------------------ {message}");

            string formattedMessage = "";
            if (!string.IsNullOrEmpty(message))
                formattedMessage = $"{DateTime.Now.ToShortTimeString()}: {message}";
            await SendMessageAsync(CredentialProviderCommandCode.Notification, true, formattedMessage);
        }

        public async Task SendStatus(string statusMessage)
        {
            WriteDebugLine($"SendStatus ------------------------ {statusMessage}");
            await SendMessageAsync(CredentialProviderCommandCode.Status, true, statusMessage);
        }

        async Task SendMessageAsync(CredentialProviderCommandCode code, bool isSuccess, string message)
        {
            WriteDebugLine($"SendMessageAsync {isSuccess}, {code}, {message}");

            try
            {
                if (_pipeServer != null && _pipeServer.IsConnected)
                {
                    byte[] byteMessage = Encoding.Unicode.GetBytes(message);

                    byte[] buf = new byte[6 + byteMessage.Length];

                    //version
                    buf[0] = 0x20;

                    // is success flag
                    buf[1] = isSuccess ? (byte)1 : (byte)0;

                    // command code
                    byte[] bCode = BitConverter.GetBytes((int)code);
                    Buffer.BlockCopy(bCode, 0, buf, 2, bCode.Length);

                    // message
                    Buffer.BlockCopy(byteMessage, 0, buf, 6, byteMessage.Length);

                    await _pipeServer.WriteToAllAsync(buf);
                }
            }
            catch (Exception ex)
            {
                WriteLine(ex);
            }
        }
    }
}
