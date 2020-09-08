using System;
using System.IO;
using System.Linq;
using System.Text;

namespace HideezMiddleware
{
    public class BondManager
    {
        readonly string _folderPath;

        public BondManager(string folderPath)
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
