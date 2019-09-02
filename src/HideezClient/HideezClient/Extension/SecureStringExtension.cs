using System;
using System.Linq;
using System.Text;
using System.Security;
using System.Runtime.InteropServices;

namespace HideezClient.Extension
{
    static class SecureStringExtension
    {
        public static bool IsEqualTo(this SecureString s1, SecureString s2)
        {
            if (s1 != null && s2 != null)
            {
                if(s1.Length != s2.Length)
                {
                    return false;
                }

                byte[] s1Bytes = null;
                byte[] s2Bytes = null;
                try
                {
                    s1Bytes = s1.ToUtf8Bytes();
                    s2Bytes = s2.ToUtf8Bytes();
                    return Enumerable.SequenceEqual(s1Bytes, s2Bytes);
                }
                finally
                {
                    if (s1Bytes != null)
                    {
                        for (int i = 0; i < s1Bytes.Length; i++)
                        {
                            s1Bytes[i] = 0;
                        }
                    }

                    if (s2Bytes != null)
                    {
                        for (int i = 0; i < s2Bytes.Length; i++)
                        {
                            s2Bytes[i] = 0;
                        }
                    }
                }
            }
            else
            {
                return false;
            }
        }

        public static string GetAsString(this SecureString value)
        {
            IntPtr bstr = Marshal.SecureStringToBSTR(value);

            try
            {
                return Marshal.PtrToStringBSTR(bstr);
            }
            finally
            {
                Marshal.FreeBSTR(bstr);
            }
        }

        public static byte[] ToUtf8Bytes(this SecureString secureString)
        {
            IntPtr bstr = Marshal.SecureStringToBSTR(secureString);
            int length = Marshal.ReadInt32(bstr, -4);
            var utf16Bytes = new byte[length];
            GCHandle utf16BytesPin = GCHandle.Alloc(utf16Bytes, GCHandleType.Pinned);
            byte[] utf8Bytes = null;

            try
            {
                Marshal.Copy(bstr, utf16Bytes, 0, length);
                Marshal.ZeroFreeBSTR(bstr);
                // At this point I have the UTF-16 byte[] perfectly.
                // The next line works at converting the encoding, but it does nothing
                // to protect the data from being spread throughout memory.
                utf8Bytes = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, utf16Bytes);
                return utf8Bytes;
            }
            finally
            {
                for (var i = 0; i < utf16Bytes.Length; i++)
                {
                    utf16Bytes[i] = 0;
                }
                utf16BytesPin.Free();
            }
        }
    }
}
