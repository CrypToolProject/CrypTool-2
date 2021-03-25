using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrypTool.Util.Logging;

namespace CrypTool.CertificateServer.Rules
{

    [Serializable]
    public class Extension
    {
        #region Escape chars

        private static char EXTENSION_SEPARATOR = '#';
        private static char OID_VALUE_SEPARATOR = '|';
        private static string EXTENSION_ENTRY_BEGIN = "{";
        private static string EXTENSION_ENTRY_END = "}";

        #endregion


        public Extension()
        {
        }

        public Extension(string oid, string value)
        {
            this.ObjectIdentifier = oid;
            this.Value = value;
        }

        public override string ToString()
        {
            return String.Format("{0} = {1}", this.ObjectIdentifier, this.Value);
        }

        public bool IsValidExtension()
        {
            if (String.IsNullOrWhiteSpace(this.ObjectIdentifier))
            {
                Log.Warn("One of your object identifier is empty! Rule will be ignored.");
                return false;
            }
            try
            {
                // Test OID and value
                new Org.BouncyCastle.Asn1.DerObjectIdentifier(this.ObjectIdentifier);
                new Org.BouncyCastle.Asn1.DerUtf8String(this.Value);
                return true;
            }
            catch (Exception ex)
            {
                Log.Warn(String.Format("You have entered an invalid extension '{0}' = '{1}'! Rule will be ignored.", this.ObjectIdentifier, this.Value), ex);
                return false;
            }
        }

        public static Extension[] Parse(string s)
        {
            string[] sarray = s.Split(EXTENSION_SEPARATOR);
            List<Extension> extList = new List<Extension>();
            foreach (string ext in sarray)
            {
                if (ext.StartsWith(EXTENSION_ENTRY_BEGIN) && ext.EndsWith(EXTENSION_ENTRY_END))
                {
                    string tempstr = ext.Substring(EXTENSION_ENTRY_BEGIN.Length, ext.Length - 1 - EXTENSION_ENTRY_END.Length);
                    string[] oidValue = tempstr.Split(OID_VALUE_SEPARATOR);
                    if (oidValue.Length == 2)
                    {
                        extList.Add(new Extension(oidValue[0], oidValue[1]));
                    }
                }
            }
            return extList.ToArray();
        }

        public static string Convert(Extension[] extArray)
        {
            if(extArray.Length == 0)
            {
                return null;
            }
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < extArray.Length; i++)
            {
                if (i > 0)
                {
                    sb.Append(EXTENSION_SEPARATOR);
                }
                sb.Append(ExtensionToString(extArray[i]));
            }
            return sb.ToString();
        }

        private static string ExtensionToString(Extension e)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(EXTENSION_ENTRY_BEGIN);
            sb.Append(e.ObjectIdentifier);
            sb.Append(OID_VALUE_SEPARATOR);
            sb.Append(e.Value);
            sb.Append(EXTENSION_ENTRY_END);
            return sb.ToString();
        }

        #region Properties

        public string ObjectIdentifier { get; set; }

        public string Value { get; set; }

        #endregion

    }

    public class ExtensionEqualityComparer : IEqualityComparer<Extension>
    {
        public bool Equals(Extension x, Extension y)
        {
            return x.ObjectIdentifier.Equals(y.ObjectIdentifier);
        }

        public int GetHashCode(Extension obj)
        {
            return obj.ObjectIdentifier.GetHashCode();
        }
    }
}
