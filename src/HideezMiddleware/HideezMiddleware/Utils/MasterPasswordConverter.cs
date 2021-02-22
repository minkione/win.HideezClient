using System;
using System.Linq;
using System.Text;

namespace HideezMiddleware.Utils
{
    public sealed class MasterPasswordConverter
    {
        /// <summary>
        /// Returns KDF hash of master password
        /// </summary>
        public static byte[] GetMasterKey(byte[] masterPassword, string serialNo)
        {
            byte[] salt = Encoding.ASCII.GetBytes(serialNo);
            var masterKey = KdfKeyProvider.CreateKDFKey(masterPassword, 32, salt);
            while (masterKey.Contains((byte)0))
                masterKey = KdfKeyProvider.CreateKDFKey(masterKey, 32, salt);

            return masterKey;
        }

        /// <summary>
        /// Returns KDF hash of master password 
        /// </summary>
        public static byte[] GetMasterKey(string masterPassword, string serialNo)
        {
            var byteArray = Encoding.UTF8.GetBytes(masterPassword);
            return GetMasterKey(byteArray, serialNo);
        }
    }
}
