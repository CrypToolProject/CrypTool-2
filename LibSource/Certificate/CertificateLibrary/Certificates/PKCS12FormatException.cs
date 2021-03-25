using System;

namespace CrypTool.CertificateLibrary.Certificates
{
    public class PKCS12FormatException : Exception
    {
        /// <summary>
        /// Represents an error in the PKCS12 file format.
        /// </summary>
        public PKCS12FormatException()
        {
        }

        /// <summary>
        /// Represents an error in the PKCS12 file format.
        /// </summary>
        public PKCS12FormatException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Represents an error in the PKCS12 file format.
        /// </summary>
        public PKCS12FormatException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
