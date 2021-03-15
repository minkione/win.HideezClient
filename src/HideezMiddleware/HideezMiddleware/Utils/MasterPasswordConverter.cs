using Hideez.SDK.Communication.Security;
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
            var masterKey = AesCryptoHelper.GetPbkdf2Bytes(masterPassword, salt);

            while (masterKey.Contains((byte)0))
                masterKey = AesCryptoHelper.GetPbkdf2Bytes(masterKey, salt);

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
