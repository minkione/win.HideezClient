using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.NamedPipes;
using Hideez.SDK.Communication.Utils;

namespace HideezMiddleware
{
    // Commands to the Credential Provider
    public enum CredentialProviderCommandCode
    {
        Unknown = 0,
        Logon = 1,
        Error = 2,
        UserList = 3,
        PasswordChange = 4,
        PasswordChangeCompleated = 5,
        Notification = 6,
        Status = 7,
    }

    // Events from the Credential Provider
    public enum CredentialProviderEventCode
    {
        Unknown = 0,
        LogonResolution = 101,
        UserListRequest = 102,
        BeginPasswordChangeRequest = 103,
        EndPasswordChangeRequest = 104,
        CheckPin = 105,
        LogonResult = 106,
    }

    public class CredentialProviderConnection : Logger
    {
        readonly PipeServer _pipeServer;

        public event EventHandler<EventArgs> OnProviderConnected;
        public event EventHandler<string> OnPinEntered;

        public bool IsConnected => _pipeServer.IsConnected;

        readonly ConcurrentDictionary<string, TaskCompletionSource<bool>> _pendingLogonRequests 
            = new ConcurrentDictionary<string, TaskCompletionSource<bool>>();

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
            CredentialProviderEventCode code = CredentialProviderEventCode.Unknown;

            try
            {
                byte[] buf = e.Buffer;
                int readBytes = e.ReadBytes;

                code = (CredentialProviderEventCode)BitConverter.ToInt32(buf, 0);

                if (code == CredentialProviderEventCode.LogonResolution)
                {
                    var strings = GetParams(buf, readBytes, expectedParamCount: 1);
                    OnLogonRequestByLoginName(strings[0]);
                }
                else if (code == CredentialProviderEventCode.BeginPasswordChangeRequest)
                {
                    var strings = GetParams(buf, readBytes, expectedParamCount: 1);
                    OnPasswordChangeBegin(strings[0]);
                }
                else if (code == CredentialProviderEventCode.EndPasswordChangeRequest)
                {
                    var strings = GetParams(buf, readBytes, expectedParamCount: 3);
                    OnPasswordChangeEnd(strings[0], strings[1], strings[2]);
                }
                else if (code == CredentialProviderEventCode.LogonResult)
                {
                    var strings = GetParams(buf, readBytes, expectedParamCount: 3);
                    OnLogonResult(Convert.ToInt32(strings[0]), strings[1], strings[2]);
                }
                else if (code == CredentialProviderEventCode.CheckPin)
                {
                    var strings = GetParams(buf, readBytes, expectedParamCount: 1);
                    OnCheckPin(strings[0]);
                }
            }
            catch(Exception ex)
            {
                WriteLine(ex);
            }
        }

        // bufLen can be less, than the size of the buffer
        string[] GetParams(byte[] buf, int bufLen, int expectedParamCount)
        {
            const int HEADER_LEN = 4;
            string prms = Encoding.Unicode.GetString(buf, HEADER_LEN, bufLen - HEADER_LEN);
            var strings = prms.Split('\n');
            if (strings.Length != expectedParamCount)
                throw new Exception();
            return strings;
        }

        void OnPasswordChangeEnd(string v1, string v2, string v3)
        {
            throw new NotImplementedException();
        }

        void OnPasswordChangeBegin(string v)
        {
            throw new NotImplementedException();
        }

        // ntstatus - NTSTATUS type from the credential provider (see wincred.h for details)
        // error - is a string representation of the ntstatus code
        void OnLogonResult(int ntstatus, string userName, string error)
        {
            WriteLine($"OnLogonResult: {userName}, {error}");
            if (_pendingLogonRequests.TryGetValue(userName, out TaskCompletionSource<bool> tcs))
                tcs.TrySetResult(ntstatus == 0);
        }

        void PipeServer_ClientConnectedEvent(object sender, ClientConnectedEventArgs e)
        {
            SafeInvoke(OnProviderConnected, EventArgs.Empty);
        }

        async void OnLogonRequestByLoginName(string login)
        {
            WriteLine($"LogonWorkstationAsync: {login}");
            await SendMessageAsync(CredentialProviderCommandCode.Logon, true, $"{login}");
        }

        void OnCheckPin(string pin)
        {
            WriteLine($"OnCheckPin: {pin}");
            SafeInvoke(OnPinEntered, pin);
        }

        public async Task<bool> SendLogonRequest(string login, string password, string prevPassword)
        {
            WriteLine($"SendLogonRequest: {login}");
            await SendMessageAsync(CredentialProviderCommandCode.Logon, true, $"{login}\n{password}\n{prevPassword}");

            var tcs = _pendingLogonRequests.GetOrAdd(login, (x) =>
            {
                return new TaskCompletionSource<bool>();
            });

            try
            {
                return await tcs.Task.TimeoutAfter(AppConfig.CredentialProviderLogonTimeout);
            }
            catch(TimeoutException)
            {
                return false;
            }
            finally
            {
                _pendingLogonRequests.TryRemove(login, out TaskCompletionSource<bool> removed);
            }
        }

        public async Task SendError(string message)
        {
            WriteLine($"SendError: {message}");
            await SendMessageAsync(CredentialProviderCommandCode.Error, true, $"{DateTime.Now.ToShortTimeString()}: {message}");
        }

        public async Task SendNotification(string message)
        {
            WriteLine($"SendNotification: {message}");

            string formattedMessage = "";
            if (!string.IsNullOrEmpty(message))
                formattedMessage = $"{DateTime.Now.ToLongTimeString()}: {message}";
            await SendMessageAsync(CredentialProviderCommandCode.Notification, true, formattedMessage);
        }

        public async Task SendStatus(string statusMessage)
        {
            WriteLine($"SendStatus: {statusMessage}");
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
                    buf[1] = (byte)(isSuccess ? 1 : 0);

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
