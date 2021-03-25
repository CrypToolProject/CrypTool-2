using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Asn1;
using CrypTool.CertificateLibrary.Util;
using CrypTool.CertificateLibrary;
using CrypTool.CertificateLibrary.Certificates;
using CrypTool.Util.Logging;

namespace CrypTool.CertificateServer
{
    public delegate void CACertficiateEventHandler(object sender, CACertificateEventArgs e);

    public class CACertificate : Certificate
    {

        #region Private member

        private string caCertAlias = String.Empty;

        #endregion


        #region Constructors

        /// <summary>
        /// Creates stub CA certificate.
        /// </summary>
        public CACertificate()
        {
            this.CaX509 = null;
            this.Subject = null;
            this.Issuer = null;
            this.KeyPair = null;
            this.IsLoaded = false;
            this.Password = String.Empty;
            this.Version = 0;
        }

        /// <summary>
        /// Creates a CACertificate.
        /// </summary>
        /// <param name="caX509">The CA X509Certificate</param>
        /// <param name="password">The password, this certificate will be secured with</param>
        /// <param name="keypair">Public and private key of the CA certificate</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="X509CertificateFormatException"></exception>
        public CACertificate(X509Certificate caX509, string password, AsymmetricCipherKeyPair keypair)
        {
            this.CaX509 = caX509;
            this.Subject = new DNWrapper(caX509.SubjectDN);
            this.Issuer = new DNWrapper(caX509.IssuerDN);
            this.KeyPair = keypair;
            this.Password = password;
            this.IsLoaded = true;
            this.Version = 0;
            if(CertificateServices.IsCaCertificate(caX509))
            {
                this.Version = Int32.Parse(CertificateServices.GetExtensionValue(caX509, PAPObjectIdentifier.CertificateVersion));
            }
            else
            {
                this.Version = 0;
            }
        }

        #endregion


        #region Load a CA certificate

        /// <summary>
        /// Loads a CA certificate from the given stream. 
        /// The stream is expected to contain PKCS #12 Personal Information Exchange (.p12) data.
        /// 
        /// Do not call it directly, instead use CertificationAuthority.LoadCACertificate(...) to fire events.
        /// </summary>
        /// <param name="stream">Input stream containing the file</param>
        /// <param name="password">Password to open the pfx</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="IOException">The PKCS #12 store could not be opened</exception>
        /// <exception cref="PKCS12FormatException">The PKCS #12 store is not genuine</exception>
        /// <exception cref="X509CertificateFormatException">Issuer distinguished name is empty or malformed</exception>
        public override void Load(Stream stream, string password)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream can not be null!");
            }
            if (password == null)
            {
                throw new ArgumentNullException("password can not be null!");
            }

            // Load CA certificate from pfx into temp store. Throws IOException if PKCS #12 could not be opened
            Pkcs12Store tempStore = new Pkcs12StoreBuilder().Build();
            tempStore.Load(stream, password.ToCharArray());

            if (tempStore.Count == 0)
            {
                string msg = "PKCS #12 store contains no certificate, but should contain one. Maybe no genuine PeersAtPlay CA certificate?";
                Log.Error(msg);
                throw new PKCS12FormatException(msg);
            }
            if (tempStore.Count > 1)
            {
                string msg = String.Format("PKCS #12 store contains {0} certificates, but should contain only one. Maybe no genuine PeersAtPlay CA certificate?", tempStore.Count);
                Log.Error(msg);
                throw new PKCS12FormatException(msg);
            }

            int aliasCount = 0;
            string tempAlias = String.Empty;
            foreach (string alias in tempStore.Aliases)
            {
                tempAlias = alias;
                aliasCount++;
            }
            if (aliasCount != 1 || tempAlias == String.Empty)
            {
                string msg = "PKCS #12 store contains a wrong certificate alias. Maybe no genuine PeersAtPlay CA certificate?";
                Log.Error(msg);
                throw new PKCS12FormatException(msg);
            }

            X509Certificate tempCert = tempStore.GetCertificate(tempAlias).Certificate;
            if (tempCert == null)
            {
                string msg = String.Format("Could not get X509Certificate for alias: {0}", tempAlias);
                Log.Error(msg);
                throw new PKCS12FormatException(msg);
            }
            if (!CertificateServices.IsCaCertificate(tempCert))
            {
                string msg = "The X509Certificate is no valid Peers@Play CA certificate";
                Log.Error(msg);
                throw new PKCS12FormatException(msg);
            }

            // Certificate looks good!
            this.Subject = new DNWrapper(tempCert.SubjectDN);
            this.Issuer = new DNWrapper(tempCert.IssuerDN);
            this.KeyPair = new AsymmetricCipherKeyPair(tempCert.GetPublicKey(), tempStore.GetKey(tempAlias).Key);
            this.caCertAlias = tempAlias;
            this.CaX509 = tempCert;
            this.IsLoaded = true;
            this.Password = password;
            this.Version = Int32.Parse(CertificateServices.GetExtensionValue(tempCert, PAPObjectIdentifier.CertificateVersion));

            Log.Info("CA Certificate successfully imported: " + Issuer.GetDN());
        }

        #endregion


        #region Save a CA certificate

        /// <summary>
        /// Save the CA certificate inclusive private key in PKCS #12 Personal Information Exchange (.p12) format to the given stream.
        /// The PKCS #12 will be secured with the existing password.
        /// </summary>
        /// <param name="stream">Output stream, the pfx will be written to.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="X509CertificateFormatException">No CA certificate loaded</exception>
        public override void SaveAsPkcs12(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream", "stream can not be null");
            }
            if (!IsLoaded)
            {
                string msg = "You need to load a CA certificate first";
                Log.Error(msg);
                throw new X509CertificateFormatException(msg);
            }

            try
            {
                Pkcs12Store store = new Pkcs12StoreBuilder().Build();
                caCertAlias = BitConverter.ToString(BitConverter.GetBytes(new SecureRandom().NextLong()));
                store.SetKeyEntry(caCertAlias, new AsymmetricKeyEntry(KeyPair.Private), new X509CertificateEntry[] { new X509CertificateEntry(CaX509) });
                store.Save(stream, Password.ToCharArray(), new SecureRandom());
            }
            catch (Exception ex)
            {
                string msg = "Could not save CA PKCS #12 store ";
                Log.Error(msg, ex);
                throw new PKCS12FormatException(msg, ex);
            }
        }

        /// <summary>
        /// Save the CA certificate as crt file to the given stream.
        /// Just contains the certificate without private key.
        /// </summary>
        /// <param name="stream"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="X509CertificateFormatException">No CA certificate loaded</exception>
        /// <exception cref="IOException">Error while writing to stream</exception>
        public override void SaveAsCrt(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream", "stream can not be null");
            }
            if (!IsLoaded)
            {
                string msg = "You need to load a CA certificate first";
                Log.Error(msg);
                throw new X509CertificateFormatException(msg);
            }

            try
            {
                byte[] crtBytes = CaX509.GetEncoded();
                stream.Write(crtBytes, 0, crtBytes.Length);
            }
            catch (Exception ex)
            {
                string msg = "Could not save CA certificate!";
                Log.Error(msg, ex);
                throw new IOException(msg, ex);
            }
        }

        #endregion

    }
}
