using CrypTool.CertificateLibrary.Util;
using CrypTool.Util.Logging;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using System;
using System.IO;

namespace CrypTool.CertificateLibrary.Certificates
{
    public class PeerCertificate : Certificate
    {

        #region Static readonly

        public static readonly string DEFAULT_USER_CERTIFICATE_DIRECTORY = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CrypCloud" + Path.DirectorySeparatorChar + "Certificates" + Path.DirectorySeparatorChar);

        public static readonly int CERTIFICATE_VERSION = 1;

        #endregion


        #region Constructor

        /// <summary>
        /// Creates a stub PeerCertificate object.
        /// </summary>
        public PeerCertificate()
        {
            CaX509 = null;
            PeerX509 = null;
            Issuer = null;
            Subject = null;
            Avatar = string.Empty;
            World = string.Empty;
            HashedEmail = string.Empty;
            Password = string.Empty;
            KeyPair = null;
            IsLoaded = false;
        }

        /// <summary>
        /// Creates a PeerCertificate object. 
        /// The CommonName of peerX509 will be used to store the avatar name.
        /// </summary>
        /// <param name="caX509"></param>
        /// <param name="peerX509"></param>
        /// <param name="world"></param>
        /// <param name="hashedEmail"></param>
        /// <param name="password"></param>
        /// <param name="keyPair"></param>
        public PeerCertificate(X509Certificate caX509, X509Certificate peerX509, string world, string hashedEmail, string password, AsymmetricCipherKeyPair keyPair)
        {
            CaX509 = caX509;
            PeerX509 = peerX509;
            Issuer = new DNWrapper(peerX509.IssuerDN);
            Subject = new DNWrapper(peerX509.SubjectDN);
            Avatar = Subject.CommonName;
            World = world;
            HashedEmail = hashedEmail;
            Password = password;
            KeyPair = keyPair;
            IsLoaded = true;

            if (CertificateLoaded != null)
            {
                CertificateLoaded.Invoke(this, new EventArgs());
            }
        }

        #endregion


        #region Load a peer certificate

        /// <summary>
        /// Loads a peer certificate by extracting information from PKCS #12 personal information exchange (also known as PFX) formatted data
        /// </summary>
        /// <param name="stream">byte array containing the PKCS #12 store</param>
        /// <param name="password">Password to open the PKCS #12 store</param>
        /// <exception cref="ArgumentNullException">if the password or bytearray is null</exception>
        /// <exception cref="IOException">The PKCS #12 store could not be opened</exception>
        /// <exception cref="PKCS12FormatException">If PKCS #12 store is not genuine</exception>
        /// <exception cref="X509CertificateFormatException">Issuer distinguished name is empty or malformed</exception>
        public void Load(byte[] pkcs12, string password)
        {
            using (MemoryStream mstream = new MemoryStream(pkcs12))
            {
                Load(mstream, password);
                mstream.Close();
            }

        }

        /// <summary>
        /// Loads a peer certificate by extracting information from PKCS #12 personal information exchange (also known as PFX) formatted data
        /// </summary>
        /// <param name="stream">Stream containing the PKCS #12 store</param>
        /// <param name="password">Password to open the PKCS #12 store</param>
        /// <exception cref="ArgumentNullException">if the password or stream is null</exception>
        /// <exception cref="IOException">The PKCS #12 store could not be opened</exception>
        /// <exception cref="PKCS12FormatException">If PKCS #12 store is not genuine</exception>
        /// <exception cref="X509CertificateFormatException">Issuer distinguished name is empty or malformed</exception>
        public override void Load(Stream stream, string password)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream cannot be null!");
            }
            if (password == null)
            {
                throw new ArgumentNullException("password cannot be null!");
            }

            // Load peer certificate from pfx into temp store. Throws IOException if PKCS #12 could not be opened
            Pkcs12Store tempStore = new Pkcs12StoreBuilder().Build();
            tempStore.Load(stream, password.ToCharArray());

            int aliasCount = 0;
            string tempAlias = string.Empty;
            foreach (string alias in tempStore.Aliases)
            {
                tempAlias = alias;
                aliasCount++;
            }

            if (aliasCount != 1 || tempAlias == string.Empty)
            {
                string msg = "PKCS12 store contains a wrong certificate alias. Maybe no genuine PeersAtPlay certificate?";
                Log.Error(msg);
                throw new PKCS12FormatException(msg);
            }
            if (tempStore.GetCertificateChain(tempAlias).Length < 2)
            {
                string msg = "PKCS12 store does not contain two certificates. Maybe no genuine PeersAtPlay certificate?";
                Log.Error(msg);
                throw new PKCS12FormatException(msg);
            }

            X509Certificate tempPeerCert = null;
            X509Certificate tempCACert = null;
            DNWrapper tempIssuer = null;
            DNWrapper tempSubject = null;
            AsymmetricCipherKeyPair tempKeyPair = null;
            string tempAvatar = null;
            string tempWorld = null;
            string tempHashedEmail = null;
            int tempVersion = 0;

            foreach (X509CertificateEntry certEntry in tempStore.GetCertificateChain(tempAlias))
            {
                // Check whether CertificateUsage extension is properly set
                string certificateUsage = CertificateServices.GetExtensionValue(certEntry.Certificate, PAPObjectIdentifier.CertificateUsage);
                if (certificateUsage == null)
                {
                    continue;
                }

                if (certificateUsage.Equals(CertificateUsageValue.CA) && CertificateServices.IsCaCertificate(certEntry.Certificate))
                {
                    tempCACert = certEntry.Certificate;
                }
                else if (certificateUsage.Equals(CertificateUsageValue.TLS) || certificateUsage.Equals(CertificateUsageValue.PEER))
                {
                    tempSubject = new DNWrapper(certEntry.Certificate.SubjectDN);
                    if (tempSubject.CommonName == string.Empty)
                    {
                        // No Avatar name, but we need one!
                        string msg = "Your certificate does not contain an Avatar name. Maybe not a genuine PeersAtPlay certificate?";
                        Log.Error(msg);
                        throw new X509CertificateFormatException(msg);
                    }

                    tempPeerCert = certEntry.Certificate;
                    tempIssuer = new DNWrapper(certEntry.Certificate.IssuerDN);
                    tempAvatar = tempSubject.CommonName;
                    tempKeyPair = new AsymmetricCipherKeyPair(certEntry.Certificate.GetPublicKey(), tempStore.GetKey(tempAlias).Key);

                    if (certificateUsage.Equals(CertificateUsageValue.PEER) && CertificateServices.IsPeerCertificate(certEntry.Certificate))
                    {
                        tempWorld = CertificateServices.GetExtensionValue(certEntry.Certificate, PAPObjectIdentifier.WorldName);
                        tempHashedEmail = CertificateServices.GetExtensionValue(certEntry.Certificate, PAPObjectIdentifier.HashedEmail);
                        tempVersion = int.Parse(CertificateServices.GetExtensionValue(certEntry.Certificate, PAPObjectIdentifier.CertificateVersion));
                    }
                    else if (certificateUsage.Equals(CertificateUsageValue.PEER) && CertificateServices.IsTlsCertificate(certEntry.Certificate))
                    {
                        tempWorld = string.Empty;
                        tempHashedEmail = string.Empty;
                        tempVersion = int.Parse(CertificateServices.GetExtensionValue(certEntry.Certificate, PAPObjectIdentifier.CertificateVersion));
                    }
                    else
                    {
                        tempWorld = string.Empty;
                        tempHashedEmail = string.Empty;
                        Version = 0;
                    }
                }
            }

            if (tempCACert == null || tempPeerCert == null)
            {
                string msg = "PKCS12 store does not contain peer and CA certificate. Maybe no genuine PeersAtPlay certificate?";
                Log.Error(msg);
                throw new PKCS12FormatException(msg);
            }

            // Everything seems ok
            PeerX509 = tempPeerCert;
            CaX509 = tempCACert;
            Issuer = tempIssuer;
            Subject = tempSubject;
            Avatar = tempAvatar;
            World = tempWorld;
            HashedEmail = tempHashedEmail;
            Version = tempVersion;
            Password = password;
            KeyPair = tempKeyPair;
            IsLoaded = true;
            Log.Debug("Certificate for user " + Avatar + " successfully loaded");

            if (CertificateLoaded != null)
            {
                CertificateLoaded.Invoke(this, new EventArgs());
            }
        }

        /// <summary>
        /// Loads a PKCS #12 certificate from users AppData directory.
        /// </summary>
        /// <param name="serial">Serialnumber of the certificate, that should be opened</param>
        /// <param name="password">Password to open the PKCS #12 store</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="IOException">The PKCS #12 store could not be opened</exception>
        /// <exception cref="PKCS12FormatException">If PKCS #12 store does not contain the right certificates</exception>
        /// <exception cref="X509CertificateFormatException">If X509 certificate does not contain the right values</exception>
        public void LoadPkcs12FromAppData(string serial, string password)
        {
            string appDataCertificate = DEFAULT_USER_CERTIFICATE_DIRECTORY + serial + ".p12";
            FileStream stream = null;

            try
            {
                stream = new FileStream(appDataCertificate, FileMode.Open, FileAccess.Read);
            }
            catch (Exception ex)
            {
                if (stream != null)
                {
                    stream.Close();
                }
                throw new IOException("Could not load PKCS #12 file from AppData", ex);
            }

            try
            {
                Load(stream, password);
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }
        }

        #endregion


        #region Save a peer certificate

        /// <summary>
        /// Saves the peer certificate in PKCS #12 personal information exchange (pfx) format to the given stream.
        /// Certificate will be secured with the existing password.
        /// </summary>
        /// <param name="stream">Stream used to save the PKCS #12 store</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="CertificateException">No peer certificate loaded</exception>
        /// <exception cref="PKCS12FormatException">Could not write PKCS #12 store to stream</exception>
        public override void SaveAsPkcs12(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream", "stream cannot be null");
            }
            if (!IsLoaded)
            {
                string msg = "Nothing to export. You need to load a peer certificate first!";
                Log.Error(msg);
                throw new CertificateException(msg);
            }

            X509CertificateEntry[] certChain = new X509CertificateEntry[2];
            certChain[0] = new X509CertificateEntry(PeerX509);
            certChain[1] = new X509CertificateEntry(CaX509);
            Pkcs12Store store = new Pkcs12StoreBuilder().Build();

            try
            {
                store.SetKeyEntry(Avatar, new AsymmetricKeyEntry(KeyPair.Private), certChain);
                store.Save(stream, Password.ToCharArray(), new SecureRandom());
            }
            catch (Exception ex)
            {
                string msg = "Could not save peer PKCS #12 store!";
                Log.Error(msg, ex);
                throw new PKCS12FormatException(msg, ex);
            }
        }

        /// <summary>
        /// Saves the peer certificate in CRT format to the given stream.
        /// CRT just contains the certificate without private key.
        /// </summary>
        /// <param name="stream"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="CertificateException">No peer certificate loaded</exception>
        /// <exception cref="IOException">Error while writing to stream</exception>
        public override void SaveAsCrt(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream", "stream cannot be null");
            }
            if (!IsLoaded)
            {
                string msg = "Nothing to export. You need to load a peer certificate first!";
                Log.Error(msg);
                throw new CertificateException(msg);
            }

            byte[] crtBytes = PeerX509.GetEncoded();
            try
            {
                stream.Write(crtBytes, 0, crtBytes.Length);
            }
            catch (Exception ex)
            {
                string msg = "Could not save peer certificate!";
                Log.Error(msg, ex);
                throw new IOException(msg, ex);
            }
        }

        /// <summary>
        /// Saves the certificate in PKCS #12 Format to users AppData directory.
        /// </summary>
        /// <param name="password"></param>
        /// <exception cref="CertificateException">No peer certificate loaded</exception>
        /// <exception cref="IOException">Error writing to AppData</exception>
        public void SavePkcs12ToAppData(string password)
        {
            if (!IsLoaded)
            {
                throw new CertificateException("No peer certificate loaded");
            }

            string appDataCertificate = DEFAULT_USER_CERTIFICATE_DIRECTORY + Avatar + ".p12";
            FileStream stream = null;
            try
            {
                if (!Directory.Exists(DEFAULT_USER_CERTIFICATE_DIRECTORY))
                {
                    Directory.CreateDirectory(DEFAULT_USER_CERTIFICATE_DIRECTORY);
                }
                stream = new FileStream(appDataCertificate, FileMode.Create, FileAccess.Write);
                SaveAsPkcs12(stream);
            }
            catch (CertificateException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                string msg = "Could not save peer certificate in AppData!";
                Log.Error(msg, ex);
                throw new IOException(msg, ex);
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }
        }

        /// <summary>
        /// Saves the certificate as crt file to users AppData directory.
        /// </summary>
        /// <exception cref="CertificateException">No peer certificate loaded</exception>
        /// <exception cref="IOException">Could not write to AppData</exception>
        public void SaveCrtToAppData()
        {
            if (!IsLoaded)
            {
                throw new CertificateException("No peer certificate loaded");
            }

            FileStream stream = null;
            try
            {
                if (!Directory.Exists(DEFAULT_USER_CERTIFICATE_DIRECTORY))
                {
                    Directory.CreateDirectory(DEFAULT_USER_CERTIFICATE_DIRECTORY);
                }

                string appDataCertificate = DEFAULT_USER_CERTIFICATE_DIRECTORY + PeerX509.SerialNumber.ToString() + ".crt";
                stream = new FileStream(appDataCertificate, FileMode.Create, FileAccess.Write);
                SaveAsCrt(stream);
            }
            catch (CertificateException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                string msg = "Could not save peer certificate in AppData!";
                Log.Error(msg, ex);
                throw new IOException(msg, ex);
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }
        }

        #endregion


        #region Accessor

        /// <summary>
        /// Returns a .Net System.Security.Cryptography.X509Certificates.X509Certificate2 object of the peer certificate.
        /// </summary>
        /// <returns>System.Security.Cryptography.X509Certificates.X509Certificate2</returns>
        /// <exception cref="CryptographicException">If the certificate could not be transformed</exception>
        public System.Security.Cryptography.X509Certificates.X509Certificate2 GetX509Certificate2()
        {
            return new System.Security.Cryptography.X509Certificates.X509Certificate2(GetPkcs12(), Password);
        }

        #endregion


        #region Properties and Events

        public X509Certificate PeerX509 { get; private set; }

        public string Avatar { get; private set; }

        public string HashedEmail { get; private set; }

        public string World { get; private set; }

        public event EventHandler<EventArgs> CertificateLoaded;

        #endregion

    }
}