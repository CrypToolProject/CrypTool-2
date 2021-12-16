using CrypTool.CertificateLibrary.Util;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.X509;
using System;
using System.IO;

namespace CrypTool.CertificateLibrary.Certificates
{
    public abstract class Certificate
    {
        /// <summary>
        /// Loads a certificate from the given stream. Stream should contain a PKCS #12 store.
        /// </summary>
        /// <param name="stream">The stream containing the PKCS #12 store</param>
        /// <param name="password">The password, which was used to secure the PKCS #12 store</param>
        public abstract void Load(Stream stream, string password);

        /// <summary>
        /// Saves the certificate to the given stream. Result is a PKCS #12 store.
        /// The PKCS #12 store will be secured with the existing Password.
        /// </summary>
        /// <param name="stream">The stream used to save the PKCS #12 store</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="X509CertificateFormatException">No certificate loaded</exception>
        /// <exception cref="IOException">Error while writing to stream</exception>
        public abstract void SaveAsPkcs12(Stream stream);

        /// <summary>
        /// Saves the X509 certificate (the public part) to the given stream.
        /// The result is a DER encoded certificate file (usually ending with .crt)
        /// </summary>
        /// <param name="stream">The stream used to save the DER encoded crt</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="X509CertificateFormatException">No certificate loaded</exception>
        /// <exception cref="IOException">Error while writing to stream</exception>
        public abstract void SaveAsCrt(Stream stream);

        /// <summary>
        /// Get a byte array containing this certificate as PKCS #12 store
        /// </summary>
        /// <returns>Byte array containing the PKCS #12 store</returns>
        /// <exception cref="UnauthorizedAccessException"></exception>
        public byte[] GetPkcs12()
        {
            MemoryStream mstream = new MemoryStream();
            SaveAsPkcs12(mstream);
            byte[] bytes = mstream.GetBuffer();
            mstream.Close();
            return bytes;
        }

        /// <summary>
        /// Returns true if a certificate has been loaded.
        /// </summary>
        public bool IsLoaded { get; set; }

        /// <summary>
        /// The issuer of this certificate as DNWrapper object
        /// </summary>
        public DNWrapper Issuer { get; protected set; }

        /// <summary>
        /// The subject of this certificate as DNWrapper object
        /// </summary>
        public DNWrapper Subject { get; protected set; }

        /// <summary>
        /// The X509 CA certificate of this certificate
        /// </summary>
        public X509Certificate CaX509 { get; protected set; }

        /// <summary>
        /// Public and private keys for this certificate
        /// </summary>
        public AsymmetricCipherKeyPair KeyPair { get; protected set; }

        /// <summary>
        /// Password this certificate is stored in the PKCS #12 file
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// The version of the certificate used.
        /// </summary>
        public int Version { get; protected set; }
    }

    public class CertificateException : Exception
    {
        public CertificateException()
            : base()
        {
        }

        public CertificateException(string message)
            : base(message)
        {
        }

        public CertificateException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

}
