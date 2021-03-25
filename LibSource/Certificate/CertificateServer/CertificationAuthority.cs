using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.X509.Extension;
using CrypTool.CertificateLibrary.Certificates;
using CrypTool.CertificateLibrary.Network;
using CrypTool.CertificateLibrary;
using CrypTool.CertificateLibrary.Util;
using CrypTool.Util.Logging;
using CrypTool.Util.Cryptography;
using CrypTool.CertificateServer.Rules;

namespace CrypTool.CertificateServer
{
    /// <summary>
    /// Responsible for generating and loading CA and peer certificates.
    /// New certificates are issued here.
    /// </summary>
    public class CertificationAuthority
    {

        #region Constant and static readonly

        public static readonly string DEFAULT_TLS_WORLD_REGEX = ".*";

        // Better keep your hands off these values, if you haven't checked BouncyCastles
        private static readonly string DEFAULT_CA_SIGNATURE_ALGORITHM = "SHA1withRSA";

        private static readonly string DEFAULT_TLS_SIGNATURE_ALGORITHM = "SHA1withRSA";

        private static readonly string DEFAULT_PEER_SIGNATURE_ALGORITHM = "SHA1withRSA";

        #endregion


        #region Constructors

        /// <summary>
        /// Creates a Certification Authority, which will use the specified CA certificate to sign certificate and store
        /// those signed certificates in the specified certificateDatabase.
        /// </summary>
        /// <param name="certDB"></param>
        /// <param name="smptclient"></param>
        public CertificationAuthority(CertificateDatabase certDB, SMTPEmailClient smtpClient)
        {
            this.config = Configuration.GetConfiguration();

            this.certificateDatabase = certDB;
            this.smtpClient = smtpClient;

            this.PeerMonthTillExpire = config.CertificationAuthority.PeerMonthTillExpire;
            this.PeerRsaStrength = config.CertificationAuthority.PeerRsaStrength;
            this.CaMonthTillExpire = config.CertificationAuthority.CaMonthTillExpire;
            this.CaRsaStrength = config.CertificationAuthority.CaRsaStrength;

            this.PeerCertificateCount = 0;
            this.DateOfLastRegister = DateTime.MinValue;

            this.CaCertificate = new CACertificate();
            this.TlsCertificate = new PeerCertificate();
        }

        #endregion


        #region Issue CA and TLS certificate

        /// <summary>
        /// Generates new CA and TLS certificates and automatically stores them in the database.
        /// <para>Returns true if the certificate was successfully generated.</para>
        /// <para>Triggers event CACertificateChanged</para>
        /// <para>Triggers event TlsCertificateChanged</para>
        /// </summary>
        /// <returns>false if a certificate for this issuer already exists in the database, true otherwise</returns>
        /// <param name="commonName"></param>
        /// <param name="organisation"></param>
        /// <param name="organisationalUnit"></param>
        /// <param name="country"></param>
        /// <param name="emailAddress"></param>
        /// <param name="password"></param>
        /// <exception cref="ArgumentNullException">One of the arguments is null</exception>
        /// <exception cref="CertificateException"></exception>
        /// <exception cref="DatabaseException">If CA or TLS certificate could not be stored in the database</exception>
        /// <exception cref="X509CertificateFormatException"></exception>
        public bool GenerateCaAndTlsCertificate(string commonName, string organisation, string organisationalUnit, string country, string emailAddress, string password)
        {
            string issuer = new DNWrapper(commonName, organisation, organisationalUnit, country, emailAddress).GetDN().ToString();

            CACertificate tempCaCert = GenerateCACertificate(commonName, organisation, organisationalUnit, country, emailAddress, password);
            PeerCertificate tempTlsCert = GenerateTlsCertificate(tempCaCert);
            StoreCaAndTlsCertificate(tempCaCert, tempTlsCert);

            // Everythings OK, reset statistics and configure this CA to use the new certificates
            this.PeerCertificateCount = 0;
            this.DateOfLastRegister = DateTime.MinValue;
            this.CaCertificate = tempCaCert;
            this.TlsCertificate = tempTlsCert;

            if (CACertificateChanged != null)
            {
                this.CACertificateChanged.Invoke(this, new CACertificateEventArgs(this.CaCertificate));
            }
            if (TlsCertificateChanged != null)
            {
                this.TlsCertificateChanged.Invoke(this, new PeerCertificateEventArgs(this.TlsCertificate));
            }
            return true;
        }

        /// <summary>
        /// Generate a new TLS certificate with data from the CA certificate.
        /// <para>Triggers event TLSCertificateLoaded</para>
        /// </summary>
        /// <exception cref="CertificateException">No CA certificate loaded</exception>
        /// <exception cref="X509CertificateFormatException">The signature of the generated TLS certificate is invalid</exception>
        public void DeriveTlsCertificate()
        {
            if (this.CaCertificate == null || !this.CaCertificate.IsLoaded)
            {
                string msg = "CA certificate is null or not loaded";
                Log.Error(msg);
                throw new CertificateException(msg);
            }
            this.TlsCertificate = GenerateTlsCertificate(this.CaCertificate);

            if (TlsCertificateChanged != null)
            {
                this.TlsCertificateChanged.Invoke(this, new PeerCertificateEventArgs(this.TlsCertificate));
            }
        }

        /// <summary>
        /// Loads an existing CA certificate and automatically unloads TLS certificate
        /// <para>Triggers event CACertificateChanged</para>
        /// <para>Triggers event TlsCertificateChanged</para>
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="password"></param>
        /// <exception cref="ArgumentNullException">Stream or password is null</exception>
        /// <exception cref="IOException">The PKCS #12 store could not be opened</exception>
        /// <exception cref="PKCS12FormatException">The PKCS #12 store is not genuine</exception>
        /// <exception cref="X509CertificateFormatException"></exception>
        public void LoadCACertificate(Stream stream, string password)
        {
            CACertificate newCaCert = new CACertificate();
            newCaCert.Load(stream, password);
            if (!CertificateServices.VerifyCACertificate(newCaCert.CaX509))
            {
                throw new X509CertificateFormatException("Verification of CA certificate failed.");
            }

            this.CaCertificate = newCaCert;
            this.TlsCertificate = new PeerCertificate();
            ReadStatistics();

            if (CACertificateChanged != null)
            {
                CACertificateChanged(this, new CACertificateEventArgs(this.CaCertificate));
            }

            if (TlsCertificateChanged != null)
            {
                this.TlsCertificateChanged.Invoke(this, new PeerCertificateEventArgs(this.TlsCertificate));
            }
        }

        /// <summary>
        /// Load an existing TLS certificate.
        /// <para>Also checks whether the certificate is a genuine TLS certificate.</para>
        /// <para>Triggers event TLSCertificateLoaded</para>
        /// </summary>
        /// <param name="stream">Stream containing pfx data</param>
        /// <param name="password">Password to open the pfx</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="CertificateException">No CA certificate loaded</exception>
        /// <exception cref="CertificateExpiredException">If TLS certificate has been expired</exception>
        /// <exception cref="CertificateNotYetValidException">If TlS certificate is not yet valid</exception>
        /// <exception cref="IOException">The PKCS #12 store could not be opened</exception>
        /// <exception cref="PKCS12FormatException">If PKCS #12 store does not contain the right certificates</exception>
        /// <exception cref="X509CertificateFormatException">If X509 certificates signatures is not valid or the certificate does not contain the right values</exception>
        public void LoadTlsCertificate(Stream stream, string password)
        {
            if (CaCertificate == null || !CaCertificate.IsLoaded)
            {
                string msg = "Certification Authority has no CA certificate loaded";
                Log.Error(msg);
                throw new CertificateException(msg);
            }

            PeerCertificate tempCert = new PeerCertificate();
            tempCert.Load(stream, password);

            // Check if this is a real TLS certificate
            if (!CertificateServices.VerifyTlsCertificate(tempCert.PeerX509))
            {
                string msg = "The TLS certificate's signature is not valid for the loaded CA certificate";
                Log.Error(msg);
                throw new X509CertificateFormatException(msg);
            }

            // Check if the TLS certificate was signed by the loaded CA certificate. Throws Exceptions
            tempCert.PeerX509.Verify(CaCertificate.CaX509.GetPublicKey());

            this.TlsCertificate = tempCert;

            if (TlsCertificateChanged != null)
            {
                this.TlsCertificateChanged.Invoke(this, new PeerCertificateEventArgs(this.TlsCertificate));
            }
        }

        #endregion


        #region Issue peer certificate

        /// <summary>
        /// Issues a new certificate.
        /// </summary>
        /// <param name="certEntry"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public ProcessingResult IssuePeerCertificate(Configuration.CertificateProgram programConfig, ClientInfo client, RegistrationEntry regEntry, string password)
        {
            // Get a serialnumber
            ulong serialnumber = GetSerialnumber(this.CaCertificate.CaX509.SerialNumber);
            DateTime dateOfIssue = DateTime.UtcNow;
            CertificateEntry certEntry = new CertificateEntry(serialnumber, regEntry.Avatar, regEntry.Email, regEntry.World, dateOfIssue, dateOfIssue.AddMonths(config.CertificationAuthority.PeerMonthTillExpire), null, DateTime.MinValue, regEntry.ProgramName, regEntry.ProgramVersion, regEntry.OptionalInfo, regEntry.Extensions);

            PeerCertificate peerCert = GeneratePeerCertificate(certEntry, password);
            Log.Debug(Logging.GetLogEnvelope("Successfully generated peer certificate.", client, certEntry));

            // Store in DB
            this.StorePeerCertificate(peerCert, certEntry);
            Log.Debug(Logging.GetLogEnvelope("Successfully stored peer certificate in the database.", client, certEntry));

            // Send registration information email
            if (programConfig.RegistrationPerformed.SendInformation)
            {
                bool emailSent = this.smtpClient.SendRegistrationPerformedInformation(client, programConfig, certEntry);
                if (!emailSent)
                {
                    HandleUndeliveredEmail((byte)3, certEntry.Email);
                }
            }

            // Send the certificate
            Log.Info(Logging.GetLogEnvelope("Signed peer certificate sent to client.", client, certEntry));
            return new ProcessingResult(PacketType.CertificateResponse, peerCert.GetPkcs12());
        }

        public ProcessingResult ReIssuePeerCertificate(Configuration.CertificateProgram programConfig, ClientInfo client, CertificateEntry certEntry, string password)
        {
            PeerCertificate peerCert = GeneratePeerCertificate(certEntry, password);
            Log.Debug(Logging.GetLogEnvelope("Successfully regenerated peer certificate.", client, certEntry));

            // Update the CRT and PKCS #12 in the database
            certificateDatabase.UpdatePasswordResetData(this.CaCertificate.CaX509.SerialNumber, certEntry.Email, true, null, DateTime.MinValue, peerCert.PeerX509.GetEncoded(), peerCert.GetPkcs12());
            Log.Debug(Logging.GetLogEnvelope("Successfully stored regenerated peer certificate in the database.", client, certEntry));

            // Send the certificate
            Log.Info(Logging.GetLogEnvelope("Sent regenerated peer certificate to the client", client, certEntry));
            return new ProcessingResult(PacketType.CertificateResponse, peerCert.GetPkcs12());
        }

        #endregion


        #region Store certificates in the database (CA + TLS, Peer)

        /// <summary>
        /// Stores the CA and TLS certificate in the database.
        /// </summary>
        /// <param name="caCert">The CA certificate used to sign new certificates</param>
        /// <param name="tlsCert">The TLS certificate used to decrypt the client messages</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="CertificateException">One of the certificates is not loaded</exception>
        /// <exception cref="DatabaseException">If CA or TLS certificate could not be stored in the database</exception>
        public void StoreCaAndTlsCertificate(CACertificate caCert, PeerCertificate tlsCert)
        {
            if (caCert == null)
            {
                throw new ArgumentException("caCert", "CA certificate can not be null");
            }
            if (tlsCert == null)
            {
                throw new ArgumentException("tlsCert", "TLS certificate can not be null");
            }
            if (!caCert.IsLoaded)
            {
                throw new CertificateException("Could not store the CA certificate, because no CA certificate is loaded");
            }
            if (!tlsCert.IsLoaded)
            {
                throw new CertificateException("Could not store the TLS certificate, because no TLS certificate is loaded");
            }

            this.certificateDatabase.StoreCACertificate(
                caCert.CaX509.SerialNumber,
                caCert.CaX509.SubjectDN.ToString(),
                caCert.CaX509.NotBefore,
                caCert.CaX509.NotAfter,
                caCert.GetPkcs12(),
                tlsCert.GetPkcs12());
            Log.Debug(String.Format("CA certificate {0} stored in database", caCert.CaX509.SubjectDN.ToString()));
        }

        /// <summary>
        /// Stores a newly generated peer certificate in the database.
        /// </summary>
        /// <param name="peerCert">The PeerCertificate object</param>
        /// <param name="certEntry">The CertificateEntry object</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public void StorePeerCertificate(PeerCertificate peerCert, CertificateEntry certEntry)
        {
            this.certificateDatabase.StorePeerCertificate(
                peerCert.PeerX509.SerialNumber,
                certEntry.Email,
                peerCert.Avatar,
                peerCert.World,
                peerCert.PeerX509.NotBefore,
                peerCert.PeerX509.NotAfter,
                CaCertificate.CaX509.SerialNumber,
                peerCert.PeerX509.GetEncoded(),
                peerCert.GetPkcs12(),
                certEntry.PasswordCode,
                certEntry.DateOfPasswordReset,
                certEntry.ProgramName,
                certEntry.ProgramVersion,
                certEntry.OptionalInfo,
                certEntry.Extensions);

            this.PeerCertificateCount++;
            this.DateOfLastRegister = peerCert.PeerX509.NotBefore;

            if (PeerCertificateStored != null)
            {
                this.PeerCertificateStored.Invoke(this, new PeerCertificateEventArgs(peerCert));
            }
        }

        #endregion


        #region Private methods to generate certificates (CA, TLS, Peer)

        /// <summary>
        /// Generates a new CA certificate.
        /// </summary>
        /// <returns>A new CACertificate object</returns>
        /// <param name="commonName">Common Name of the peerCertificate owner</param>
        /// <param name="organisation">Organisation of the peerCertificate owner</param>
        /// <param name="organisationalUnit">Organisational Unit Name of the peerCertificate owner</param>
        /// <param name="country">Country of the peerCertificate owner</param>
        /// <param name="emailAddress">Email address of the peerCertificate owner</param>
        /// <param name="password">Password to secure this peerCertificate</param>
        /// <param name="expireDateInMonth">Number of months, this peerCertificate will be valid</param>
        /// <param name="rsaStrength">Number of Bits</param>
        /// <exception cref="X509CertificateFormatException">Certificate's signature is invalid</exception>
        private CACertificate GenerateCACertificate(string commonName, string organisation, string organisationalUnit, string country, string emailAddress, string password)
        {
            DNWrapper tempIssuer = new DNWrapper(commonName, organisation, organisationalUnit, country, emailAddress.ToLower());
            DateTime dateOfIssue = DateTime.Today.ToUniversalTime();

            // Initialize peerCert generator
            X509V3CertificateGenerator certGen = new X509V3CertificateGenerator();
            certGen.SetSerialNumber(new BigInteger(1, BitConverter.GetBytes(DateTime.Now.Ticks)));
            certGen.SetNotBefore(dateOfIssue);
            certGen.SetNotAfter(dateOfIssue.AddMonths(this.CaMonthTillExpire));

            // This is a self-issued CA certificate, so the subject name and issuer name are the same.
            certGen.SetIssuerDN(tempIssuer.GetDN());
            certGen.SetSubjectDN(tempIssuer.GetDN());

            // Initialize RSA
            SecureRandom random = new SecureRandom();
            RsaKeyPairGenerator generator = new RsaKeyPairGenerator();
            KeyGenerationParameters genParam = new KeyGenerationParameters(random, this.CaRsaStrength);
            generator.Init(genParam);
            AsymmetricCipherKeyPair tempKeyPair = generator.GenerateKeyPair();
            certGen.SetPublicKey(tempKeyPair.Public);
            certGen.SetSignatureAlgorithm(DEFAULT_CA_SIGNATURE_ALGORITHM);

            // Add extensions
            certGen.AddExtension(X509Extensions.BasicConstraints, false, new BasicConstraints(true));
            certGen.AddExtension(X509Extensions.KeyUsage, true, new KeyUsage(KeyUsage.KeyCertSign + KeyUsage.CrlSign));
            certGen.AddExtension(PAPObjectIdentifier.CertificateVersion, false, new DerUtf8String(PeerCertificate.CERTIFICATE_VERSION.ToString()));
            certGen.AddExtension(PAPObjectIdentifier.CertificateUsage, false, new DerUtf8String(CertificateUsageValue.CA));

            // Generate X509 certificate
            X509Certificate tempCertificate = certGen.Generate(tempKeyPair.Private);

            // We want to be sure the certificate is OK.
            try
            {
                tempCertificate.Verify(tempKeyPair.Public);
            }
            catch (Exception ex)
            {
                string msg = "CA certificate's signature is invalid. Certificate generation aborted.";
                Log.Error(msg, ex);
                throw new X509CertificateFormatException(msg, ex);
            }

            Log.Info("CA Certificate successfully generated: " + tempCertificate.SubjectDN.ToString());
            return new CACertificate(tempCertificate, password, tempKeyPair);
        }

        /// <summary>
        /// Generates a new TLS certificate. This will be treated as normal peer certificate, but will have some other values
        /// </summary>
        /// <returns>A PeerCertificate object for TLS usage</returns>
        /// <exception cref="CertificateException">No CA certificate loaded</exception>
        /// <exception cref="X509CertificateFormatException">Certificate's signature is invalid</exception>
        private PeerCertificate GenerateTlsCertificate(CACertificate caCert)
        {
            if (caCert == null || !caCert.IsLoaded)
            {
                string msg = "CA certificate is null or not loaded";
                Log.Error(msg);
                throw new CertificateException(msg);
            }

            DNWrapper tlsDN = new DNWrapper(caCert.CaX509.SubjectDN);
            tlsDN.CommonName = tlsDN.CommonName + " (TLS)";
            DateTime dateOfIssue = DateTime.UtcNow;

            // Initialize certificate generator
            X509V3CertificateGenerator certGen = new X509V3CertificateGenerator();
            certGen.SetSerialNumber(new BigInteger(1, BitConverter.GetBytes(DateTime.Now.Ticks)));
            certGen.SetNotBefore(dateOfIssue);
            certGen.SetNotAfter(dateOfIssue.AddMonths(CaMonthTillExpire));
            certGen.SetSubjectDN(tlsDN.GetDN());
            certGen.SetIssuerDN(caCert.CaX509.SubjectDN);

            // Initialize RSA
            RsaKeyPairGenerator generator = new RsaKeyPairGenerator();
            KeyGenerationParameters genParam = new KeyGenerationParameters(new SecureRandom(), PeerRsaStrength);
            generator.Init(genParam);
            AsymmetricCipherKeyPair tempKeyPair = generator.GenerateKeyPair();
            certGen.SetPublicKey(tempKeyPair.Public);
            certGen.SetSignatureAlgorithm(DEFAULT_TLS_SIGNATURE_ALGORITHM);

            // Add extensions
            certGen.AddExtension(X509Extensions.KeyUsage, true, new KeyUsage(KeyUsage.DataEncipherment + KeyUsage.DigitalSignature + KeyUsage.KeyEncipherment));
            certGen.AddExtension(PAPObjectIdentifier.CertificateUsage, false, new DerUtf8String(CertificateUsageValue.TLS));
            certGen.AddExtension(PAPObjectIdentifier.CertificateVersion, false, new DerUtf8String(PeerCertificate.CERTIFICATE_VERSION.ToString()));

            // Generates TLS certificate signed with CA private key
            X509Certificate tlsX509 = certGen.Generate(caCert.KeyPair.Private);

            // We want to be sure the certificate is OK
            try
            {
                tlsX509.Verify(caCert.CaX509.GetPublicKey());
            }
            catch (Exception ex)
            {
                string msg = "The TLS certificate's signature is not valid for this CA certificate";
                Log.Error(msg, ex);
                throw new X509CertificateFormatException(msg, ex);
            }

            return new PeerCertificate(caCert.CaX509, tlsX509, null, null, caCert.Password, tempKeyPair);
        }

        /// <summary>
        /// Generates a PeerCertificate object.
        /// </summary>
        /// <param name="certEntry">The certificate entry object used to generate the certificate</param>
        /// <param name="password">The password, the PKCS #12 store will be secured with</param>
        /// <returns>The generated PeerCertificate object</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="CertificateException">No CA certificate loaded</exception>
        /// <exception cref="X509CertificateFormatException"></exception>
        private PeerCertificate GeneratePeerCertificate(CertificateEntry certEntry, string password)
        {
            if (certEntry == null)
            {
                throw new ArgumentNullException("certEntry", "Certificate entry can not be null");
            }
            if (CaCertificate == null || !CaCertificate.IsLoaded)
            {
                string msg = "Certification Authority has no CA certificate loaded";
                Log.Error(msg);
                throw new CertificateException(msg);
            }

            // Initialize certificate generator
            X509V3CertificateGenerator certGen = new X509V3CertificateGenerator();
            certGen.SetSerialNumber(new BigInteger(certEntry.Serialnumber.ToString()));
            certGen.SetNotBefore(certEntry.DateOfIssue);
            certGen.SetNotAfter(certEntry.DateOfExpire);
            certGen.SetSubjectDN(new DNWrapper(certEntry.Avatar).GetDN());
            certGen.SetIssuerDN(CaCertificate.CaX509.SubjectDN);

            // Initialize RSA
            RsaKeyPairGenerator generator = new RsaKeyPairGenerator();
            KeyGenerationParameters genParam = new KeyGenerationParameters(new SecureRandom(), PeerRsaStrength);
            generator.Init(genParam);
            AsymmetricCipherKeyPair tempKeyPair = generator.GenerateKeyPair();
            certGen.SetPublicKey(tempKeyPair.Public);
            certGen.SetSignatureAlgorithm(DEFAULT_PEER_SIGNATURE_ALGORITHM);

            // Add extensions (No BasicConstraints means no CA)
            string tempHashedEmail = SHA1.ComputeHashString(certEntry.Email.ToLower());
            // remember to also update validation methods when adding critical flags
            certGen.AddExtension(X509Extensions.KeyUsage, true, new KeyUsage(KeyUsage.DataEncipherment + KeyUsage.DigitalSignature + KeyUsage.KeyEncipherment));
            certGen.AddExtension(PAPObjectIdentifier.CertificateUsage, false, new DerUtf8String(CertificateUsageValue.PEER));
            certGen.AddExtension(PAPObjectIdentifier.CertificateVersion, false, new DerUtf8String(PeerCertificate.CERTIFICATE_VERSION.ToString()));
            certGen.AddExtension(PAPObjectIdentifier.WorldName, false, new DerUtf8String(certEntry.World));
            certGen.AddExtension(PAPObjectIdentifier.HashedEmail, false, new DerUtf8String(tempHashedEmail));

            if (certEntry.Extensions != null && certEntry.Extensions.Length > 0)
            {
                foreach (Extension extension in certEntry.Extensions)
                {
                    try
                    {
                        certGen.AddExtension(new DerObjectIdentifier(extension.ObjectIdentifier), false, new DerUtf8String(extension.Value));
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Could not add rule based extension!", ex);
                    }                     
                }
            }

            // Generates peer certificate signed with CA private key
            X509Certificate peerX509 = certGen.Generate(CaCertificate.KeyPair.Private);

            // We want to be sure the certificate is OK, so check with CA public key
            if (!CertificateServices.VerifyPeerCertificate(peerX509, CaCertificate.CaX509))
            {
                string msg = "Peer certificate's signature is invalid. Certificate generation aborted.";
                Log.Error(msg);
                throw new X509CertificateFormatException(msg);
            }

            return new PeerCertificate(CaCertificate.CaX509, peerX509, certEntry.World, tempHashedEmail, password, tempKeyPair);
        }

        #endregion


        #region Helper

        /// <summary>
        /// Generates a serialnumber and checks whether is is already used for the given CA certificate
        /// </summary>
        /// <param name="caSerialnumber"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="DatabaseException"></exception>
        private ulong GetSerialnumber(BigInteger caSerialnumber)
        {
            ulong serialnumber = 0;
            do
            {
                byte[] bytes = new byte[sizeof(UInt64)];
                Random random = new Random();
                random.NextBytes(bytes);
                serialnumber = BitConverter.ToUInt64(bytes, 0);
            }
            while (certificateDatabase.SelectSerialnumberExist(caSerialnumber, serialnumber));
            return serialnumber;
        }

        /// <summary>
        /// Stores an entry in the database, that the email sending failed.
        /// Type: 0 = EmailVerificationCode, 1 = PasswordResetCode, 2 = RegistrationRequest, 3 = RegistrationPerformed
        /// </summary>
        /// <param name="emailType"></param>
        /// <param name="email"></param>
        private void HandleUndeliveredEmail(byte emailType, string email)
        {
            try
            {
                this.certificateDatabase.StoreUndeliveredEmailEntry(this.CaCertificate.CaX509.SerialNumber, emailType, email, DateTime.Now);
            }
            catch (Exception ex)
            {
                // This is the worst case, smtp server and database down...
                Log.Error("WARNING! Could not store undelivered email entry!", ex);
            }
        }

        #endregion


        #region Statistics

        private void ReadStatistics()
        {
            if (!CaCertificate.IsLoaded)
            {
                this.PeerCertificateCount = 0;
                this.DateOfLastRegister = DateTime.MinValue;
                return;
            }

            try
            {
                this.PeerCertificateCount = this.certificateDatabase.SelectPeerCertificateCount(this.CaCertificate.CaX509.SerialNumber);
                this.DateOfLastRegister = this.certificateDatabase.SelectDateOfLastRegister(this.CaCertificate.CaX509.SerialNumber);
            }
            catch
            {
                // Not critical, message already logged, do nothing
            }
        }

        #endregion


        #region Properties

        /// <summary>
        /// CA certificate this certification authority use 
        /// </summary>
        public CACertificate CaCertificate { get; private set; }

        /// <summary>
        /// TLS certificate this certification authority use 
        /// </summary>
        public PeerCertificate TlsCertificate { get; private set; }

        /// <summary>
        /// Get or set the RSA strength for the CA and SSL certificate.
        /// </summary>
        public int CaRsaStrength;

        /// <summary>
        /// Get or set the timespan in month, the newly generated CA and SSL certificates will be valid.
        /// </summary>
        public int CaMonthTillExpire;

        /// <summary>
        /// Get or set the RSA strength.
        /// </summary>
        public int PeerRsaStrength;

        /// <summary>
        /// Get or set the timespan in month, a newly generated peer certificate will be valid.
        /// </summary>
        public int PeerMonthTillExpire;

        /// <summary>
        /// Returns the number of registered peers.
        /// Returns -1 if no CA certificate is loaded.
        /// </summary>
        public long PeerCertificateCount { get; private set; }

        /// <summary>
        /// The number of registration requests that have not been verified yet.
        /// </summary>
        public int PeerRegistrationRequestCount { get; private set; }

        /// <summary>
        /// Returns the DateTime of last peer certificate register.
        /// Returns DateTime.MinValue if no CA certificate is loaded or no peer certificate has been registered with the CA certificate.
        /// </summary>
        public DateTime DateOfLastRegister { get; private set; }

        /// <summary>
        /// The Datetime of the last received certificate registration request.
        /// </summary>
        public DateTime DateofLastRegistrationRequest { get; private set; }

        /// <summary>
        /// Timespan in minutes while verification codes won't be send again.
        /// </summary>
        public uint AntiSpamTime { get; set; }

        #endregion


        #region Events

        /// <summary>
        /// A new CA certificate has been loaded or generated
        /// </summary>
        public event EventHandler<CACertificateEventArgs> CACertificateChanged;

        /// <summary>
        /// A new TLS certificate has been loaded or generated
        /// </summary>
        public event EventHandler<PeerCertificateEventArgs> TlsCertificateChanged;

        /// <summary>
        /// A new peer certificate has been generated
        /// </summary>
        public event EventHandler<PeerCertificateEventArgs> PeerCertificateStored;

        /// <summary>
        /// A new peer certificate registration request has been received
        /// </summary>
        public event EventHandler<EventArgs> RegistrationRequestReceived;

        #endregion


        #region Private member

        private Configuration config;

        private CertificateDatabase certificateDatabase;

        private SMTPEmailClient smtpClient;

        #endregion

    }
}
