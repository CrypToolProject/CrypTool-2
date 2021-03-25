using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PeersAtPlay.CertificateLibrary.Util
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
