using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PeersAtPlay.CertificateLibrary.Util
{
    public class X509CertificateFormatException : Exception
    {
        /// <summary>
        /// Represents an error while reading a certificate.
        /// </summary>
        public X509CertificateFormatException()
        {
        }

        /// <summary>
        /// Represents an error while reading a certificate.
        /// </summary>
        public X509CertificateFormatException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Represents an error while reading a certificate.
        /// </summary>
        public X509CertificateFormatException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
