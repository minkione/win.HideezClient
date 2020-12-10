using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.Refactored.BLE;
using System.IO;
using System.Threading.Tasks;

namespace HideezMiddleware
{
    public sealed class BondManager: Logger
    {
        readonly string _folderPath;

        public BondManager(string folderPath, ILog _log): base(nameof(BondManager), _log)
        {
            _folderPath = folderPath;
        }

        public bool Exists(string deviceMac)
        {
            string filePath = Path.Combine(_folderPath, MacToFileName(deviceMac));
            if (File.Exists(filePath))
            {
                string content = File.ReadAllText(filePath);
                return !string.IsNullOrWhiteSpace(content);
            }
            else return false;
        }

        public bool Exists(ConnectionId connectionId)
        {
            var mac = BleUtils.ConnectionIdToMac(connectionId.Id);
            return Exists(mac);
        }
       
        public async Task<int> RemoveAll()
        {
            string[] files = Directory.GetFiles(_folderPath);
            int count = 0;
            int i = 0;

            while (i < 3)
            {
                foreach (var file in files)
                {
                    var isSuccess = Remove(file);
                    if (isSuccess)
                        count++;
                }

                i++;

                files = Directory.GetFiles(_folderPath);

                if (files.Length != 0)
                    await Task.Delay(2000);
                else break;
            }

            return count;
        }

        bool Remove(string filePath)
        {
            try
            {
                File.Delete(filePath);
                return true;
            }
            catch (IOException ex)
            {
                WriteLine(ex);
                return false;
            }
        }

        string MacToFileName(string mac)
        {
            var pairs = mac.Split(':');
            string result = pairs[pairs.Length-1];

            for(int i = pairs.Length-2; i >= 0; i--)
            {
                result = result + "-" + pairs[i];
            }
            return result +".hbf";
        }
    }
}
