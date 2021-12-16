using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;

namespace CrypCloud.Manager.ViewModels.Helper
{
    public static class SecureStringExtensions
    {
        //https://stackoverflow.com/questions/4502676/c-sharp-compare-two-securestrings-for-equality
        // equal without memoryLeak and unsafe blocks
        public static bool IsEqualTo(this SecureString ss1, SecureString ss2)
        {
            IntPtr bstr1 = IntPtr.Zero;
            IntPtr bstr2 = IntPtr.Zero;
            try
            {
                bstr1 = Marshal.SecureStringToBSTR(ss1);
                bstr2 = Marshal.SecureStringToBSTR(ss2);
                int length1 = Marshal.ReadInt32(bstr1, -4);
                int length2 = Marshal.ReadInt32(bstr2, -4);
                if (length1 == length2)
                {
                    for (int x = 0; x < length1; ++x)
                    {
                        byte b1 = Marshal.ReadByte(bstr1, x);
                        byte b2 = Marshal.ReadByte(bstr2, x);
                        if (b1 != b2)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }

                return true;
            }
            finally
            {
                if (bstr2 != IntPtr.Zero)
                {
                    Marshal.ZeroFreeBSTR(bstr2);
                }

                if (bstr1 != IntPtr.Zero)
                {
                    Marshal.ZeroFreeBSTR(bstr1);
                }
            }
        }

        public static string ToUnsecuredString(this SecureString secString)
        {
            return new System.Net.NetworkCredential(string.Empty, secString).Password;
        }
        public static SecureString FromString(this SecureString secString, string from)
        {
            secString.Clear();
            Array.ForEach(from.ToArray(), secString.AppendChar);
            secString.MakeReadOnly();
            return secString;
        }

    }
}