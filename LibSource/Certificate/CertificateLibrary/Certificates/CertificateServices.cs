using CrypTool.CertificateLibrary.Util;
using CrypTool.Util.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CrypTool.CertificateLibrary.Certificates
{
    public static class CertificateServices
    {

        #region CA certificate singleton

        private static System.Security.Cryptography.X509Certificates.X509Certificate caCertificate = null;

        public static System.Security.Cryptography.X509Certificates.X509Certificate CaCertificate
        {
            get => caCertificate;
            set
            {
                if (caCertificate == null)
                {
                    caCertificate = value;
                }
            }
        }

        #endregion


        #region Information about all certificates

        /// <summary>
        /// Returns the number of peer certificates. A peer certificate is the combination of the crt and pfx file.
        /// </summary>
        /// <param name="filepath">The filepath to search for certificates</param>
        /// <returns>Number of peer certificate files</returns>
        public static int GetCertificateCount(string filepath)
        {
            DirectoryInfo di = new DirectoryInfo(filepath);
            FileInfo[] rgFiles = di.GetFiles("*.crt");
            int certificateCount = 0;
            for (int i = 0; i < rgFiles.Length; i++)
            {
                if (File.Exists(string.Format("{0}{1}.p12", filepath, Path.GetFileNameWithoutExtension(rgFiles[i].Name))))
                {
                    // Check for old certificates (will be removed later)
                    FileStream stream = null;
                    try
                    {
                        stream = rgFiles[i].Open(FileMode.Open, FileAccess.Read, FileShare.Read);
                        Org.BouncyCastle.X509.X509CertificateParser x509Parser = new Org.BouncyCastle.X509.X509CertificateParser();
                        Org.BouncyCastle.X509.X509Certificate cert = x509Parser.ReadCertificate(stream);

                        // Skip old certificates
                        if (IsPeerCertificate(cert, true))
                        {
                            certificateCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Could not read certificate " + rgFiles[i].FullName, ex);
                    }
                    finally
                    {
                        if (stream != null)
                        {
                            stream.Close();
                        }
                    }
                }
            }
            return certificateCount;
        }

        /// <summary>
        /// Returns a list of BouncyCastle X509Certificate objects for all certificates available in the given filepath.
        /// The list only contains a specific certificate, if both files (crt and p12) are available.
        /// </summary>
        /// <param name="filepath">Filepath to search for the certificates</param>
        /// <returns>List of Org.BouncyCastle.X509.X509Certificate</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException">filepath is null</exception>
        /// <exception cref="DirectoryNotFoundException">the specified directory does not exist</exception>
        /// <exception cref="PathTooLongException">filepath is invalid</exception>
        /// <exception cref="SecurityException"></exception>
        /// <exception cref="FormatException">Filepath/-name format is invalid</exception>
        public static List<Org.BouncyCastle.X509.X509Certificate> GetX509Certificates(string filepath)
        {
            DirectoryInfo di = new DirectoryInfo(filepath);
            FileInfo[] rgFiles = di.GetFiles("*.crt");
            List<Org.BouncyCastle.X509.X509Certificate> certificates = new List<Org.BouncyCastle.X509.X509Certificate>();
            for (int i = 0; i < rgFiles.Length; i++)
            {
                if (!File.Exists(string.Format("{0}{1}.p12", filepath, rgFiles[i].Name.Substring(0, rgFiles[i].Name.Length - 4))))
                {
                    continue;
                }

                FileStream stream = null;
                try
                {
                    stream = rgFiles[i].Open(FileMode.Open, FileAccess.Read, FileShare.Read);
                    Org.BouncyCastle.X509.X509CertificateParser x509Parser = new Org.BouncyCastle.X509.X509CertificateParser();
                    Org.BouncyCastle.X509.X509Certificate cert = x509Parser.ReadCertificate(stream);

                    // Skip old certificates (will be removed later)
                    if (IsPeerCertificate(cert, true))
                    {
                        certificates.Add(cert);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("Could not read certificate " + rgFiles[i].FullName, ex);
                }
                finally
                {
                    if (stream != null)
                    {
                        stream.Close();
                    }
                }
            }
            return certificates;
        }

        #endregion


        #region Specific BouncyCastle X509Certificate

        /// <summary>
        /// Returns a BouncyCastle X509 certificate with the given avatar.
        /// Throws exception if no such certificate exists.
        /// </summary>
        /// <param name="filepath">The path where to search for the certificate</param>
        /// <param name="avatar">The certificate's avatar name</param>
        /// <returns>Org.BouncyCastle.X509.X509Certificate</returns>
        /// <exception cref="ArgumentNullException">one of the parameter is null</exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="DirectoryNotFoundException">the specified directory does not exist</exception>
        /// <exception cref="PathTooLongException">filepath is invalid</exception>
        /// <exception cref="SecurityException"></exception>
        /// <exception cref="FormatException">Filepath/-name format is invalid</exception>
        /// <exception cref="X509CertificateFormatException">The crt File is corrupted</exception>
        /// <exception cref="NoCertificateFoundException">The crt File doesnt exist</exception>
        public static Org.BouncyCastle.X509.X509Certificate GetX509CertificateByAvatar(string filepath, string avatar)
        {
            if (avatar == null)
            {
                throw new ArgumentNullException("avatar", "Avatar cannot be null");
            }

            foreach (Org.BouncyCastle.X509.X509Certificate cert in GetX509Certificates(filepath))
            {
                if (avatar.ToLower().Equals(GetAvatarName(cert).ToLower()))
                {
                    return cert;
                }
            }
            throw new NoCertificateFoundException(string.Format("No Certificate found for avatar '{0}' in '{1}'", avatar, filepath));
        }

        /// <summary>
        /// Searches for a BouncyCastle X509 certificate with the email in the given filepath
        /// Throws exception, if no certificate was found for the specified email
        /// </summary>
        /// <param name="filepath">The path where to search for the certificate</param>
        /// <param name="email">The certificate's email</param>
        /// <returns>Org.BouncyCastle.X509.X509Certificate</returns>
        /// <exception cref="ArgumentNullException">one of the parameter is null</exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="DirectoryNotFoundException">the specified directory does not exist</exception>
        /// <exception cref="PathTooLongException">filepath is invalid</exception>
        /// <exception cref="SecurityException"></exception>
        /// <exception cref="FormatException">Filepath/-name format is invalid</exception>
        /// <exception cref="X509CertificateFormatException">The crt File is corrupted</exception>
        /// <exception cref="NoCertificateFoundException">The crt File doesnt exist</exception>
        public static Org.BouncyCastle.X509.X509Certificate GetX509CertificateByEmail(string filepath, string email)
        {
            if (email == null)
            {
                throw new ArgumentNullException("email", "Email cannot be null");
            }

            foreach (Org.BouncyCastle.X509.X509Certificate cert in GetX509Certificates(filepath))
            {
                if (CrypTool.Util.Cryptography.SHA1.ComputeHashString(email.ToLower()).Equals(GetExtensionValue(cert, PAPObjectIdentifier.HashedEmail)))
                {
                    return cert;
                }
            }
            throw new NoCertificateFoundException(string.Format("No Certificate found for email '{0}' in '{1}'", email, filepath));
        }

        #endregion


        #region Specific PeerCertificate

        /// <summary>
        /// Checks if the PKCS #12 store can be opened with the given password.
        /// </summary>
        /// <param name="pkcs12">PKCS #12 store to open</param>
        /// <param name="password">Password used to open the store</param>
        /// <returns>true if the password is correct</returns>
        public static bool CheckPassword(byte[] pkcs12, string password)
        {
            try
            {
                // try to open the PKCS #12 with the given password
                PeerCertificate peerCert = new PeerCertificate();
                using (MemoryStream mstream = new MemoryStream(pkcs12))
                {
                    peerCert.Load(mstream, password);
                    mstream.Close();
                }
                if (!peerCert.IsLoaded)
                {
                    throw new Exception();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Loads a PeerCertificate from users AppData directory.
        /// </summary>
        /// <param name="filepath">The path where to search for the certificate</param>
        /// <param name="avatar">The certificate's avatar</param>
        /// <param name="password">The password to open the PKCS #12 store</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="IOException">The PKCS #12 store could not be opened</exception>
        /// <exception cref="PKCS12FormatException">If PKCS #12 store does not contain the right certificates</exception>
        /// <exception cref="X509CertificateFormatException">If X509 certificate's signature is not valid or the certificate does not contain the right values</exception>
        public static PeerCertificate GetPeerCertificateByAvatar(string filepath, string avatar, string password)
        {
            PeerCertificate peerCert = new PeerCertificate();
            MemoryStream stream = null;
            try
            {
                stream = new MemoryStream(GetPkcs12ByAvatar(filepath, avatar));
                peerCert.Load(stream, password);

                if (!VerifyPeerCertificate(peerCert.PeerX509, peerCert.CaX509))
                {
                    throw new X509CertificateFormatException("Peer certificate check has failed");
                }
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }
            return peerCert;
        }

        /// <summary>
        /// Loads a PeerCertificate from users AppData directory.
        /// </summary>
        /// <param name="filepath">The path where to search for the certificate</param>
        /// <param name="avatar">The certificate's email</param>
        /// <param name="password">The password to open the PKCS #12 store</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="IOException">The PKCS #12 store could not be opened</exception>
        /// <exception cref="PKCS12FormatException">If PKCS #12 store does not contain the right certificates</exception>
        /// <exception cref="X509CertificateFormatException">If X509 certificate's signature is not valid or the certificate does not contain the right values</exception>
        /// <exception cref="CertificateExpiredException">if the certificate is expired by given date</exception>
        /// <exception cref="CertificateNotYetValidException">if the certificate is not yet valid on given date</exception>
        public static PeerCertificate GetPeerCertificateByEmail(string filepath, string email, string password)
        {
            PeerCertificate peerCert = new PeerCertificate();
            MemoryStream stream = null;
            try
            {
                stream = new MemoryStream(GetPkcs12ByEmail(filepath, email));
                peerCert.Load(stream, password);

                if (!VerifyPeerCertificate(peerCert.PeerX509, peerCert.CaX509))
                {
                    throw new X509CertificateFormatException("Peer certificate check has failed");
                }
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }
            return peerCert;
        }

        #endregion


        #region Specific byte array

        /// <summary>
        /// Returns a PKCS #12 store byte array for the specified avatar in the given filepath
        /// Returns null, if no certificate was found for the specified avatar
        /// </summary>
        /// <param name="filepath">The path where to search for the certificate</param>
        /// <param name="avatar">The avatar name of the certificate</param>
        /// <returns>byte[] of the PKCS #12 store</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="IOException">Error while reading PKCS #12 file</exception>
        public static byte[] GetPkcs12ByAvatar(string filepath, string avatar)
        {
            if (avatar == null)
            {
                throw new ArgumentNullException("email", "Email cannot be null");
            }

            Org.BouncyCastle.X509.X509Certificate cert = GetX509CertificateByAvatar(filepath, avatar);
            if (cert == null)
            {
                return null;
            }
            string filename = cert.SerialNumber.ToString() + ".p12";

            FileStream stream = null;
            byte[] pkcs12;
            try
            {
                stream = new FileStream(filepath + filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                pkcs12 = new byte[stream.Length];

                int offset = 0;
                int remaining = pkcs12.Length;
                while (remaining > 0)
                {
                    int read = stream.Read(pkcs12, offset, remaining);
                    if (read <= 0)
                    {
                        throw new EndOfStreamException(string.Format("End of stream reached with {0} bytes left to read", remaining));
                    }

                    remaining -= read;
                    offset += read;
                }
            }
            catch (Exception ex)
            {
                string msg = "Could not read PKCS #12 certificate file";
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
            return pkcs12;
        }

        /// <summary>
        /// Returns a PKCS #12 store byte array for the specified email in the given filepath
        /// Returns null, if no certificate was found for the specified email
        /// </summary>
        /// <param name="filepath">The path where to search for the certificate</param>
        /// <param name="email">The email address of the certificate</param>
        /// <returns>byte[] of the PKCS #12 store</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="IOException">Error while reading PKCS #12 file</exception>
        public static byte[] GetPkcs12ByEmail(string filepath, string email)
        {
            if (email == null)
            {
                throw new ArgumentNullException("email", "Email cannot be null");
            }

            Org.BouncyCastle.X509.X509Certificate cert = GetX509CertificateByEmail(filepath, email);
            if (cert == null)
            {
                return null;
            }
            string filename = cert.SerialNumber.ToString() + ".p12";

            FileStream stream = null;
            byte[] pkcs12;
            try
            {
                stream = new FileStream(filepath + filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                pkcs12 = new byte[stream.Length];

                int offset = 0;
                int remaining = pkcs12.Length;
                while (remaining > 0)
                {
                    int read = stream.Read(pkcs12, offset, remaining);
                    if (read <= 0)
                    {
                        throw new EndOfStreamException(string.Format("End of stream reached with {0} bytes left to read", remaining));
                    }

                    remaining -= read;
                    offset += read;
                }
            }
            catch (Exception ex)
            {
                string msg = "Could not read PKCS #12 certificate file";
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
            return pkcs12;
        }

        #endregion


        #region Extract information out of a certificate

        /// <summary>
        /// Returns the avatar name of the given certificate. (Avatar name is stored as CommonName)
        /// </summary>
        /// <param name="certificate"></param>
        /// <returns>String - avatar name</returns>
        /// <exception cref="ArgumentNullException">Certificate SubjectDN is null</exception>
        /// <exception cref="X509CertificateFormatException">Certificate is in a very ugly format</exception>
        public static string GetAvatarName(Org.BouncyCastle.X509.X509Certificate certificate)
        {
            return new DNWrapper(certificate.SubjectDN).CommonName;
        }

        /// <summary>
        /// Returns the extension value of BouncyCastle X509Certificate for the given Object Identifier. (Only works if extension is saved as DerUtf8String!)
        /// <para>Returns null if the certificate does not have the extension set.</para>
        /// <para>You may be interested in PAPObjectIdentifier static values to get the currently supported ObjectIdentifier.</para>
        /// </summary>
        /// <param name="certificate">The Bouncy Castle X509Certificate to look the value up</param>
        /// <param name="oid">The Object Identifier that will be searched for</param>
        /// <returns>String - specified Object Identifier or null</returns>
        /// <exception cref="ArgumentNullException">If one parameter is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">StringBuilder could not clean the dirty string</exception>
        public static string GetExtensionValue(Org.BouncyCastle.X509.X509Certificate certificate, string oid)
        {
            if (certificate == null)
            {
                throw new ArgumentNullException("certificate", "Certificate not set");
            }
            if (oid == null)
            {
                throw new ArgumentNullException("oid", "Object identifier not set");
            }
            string value = UTF8Encoding.UTF8.GetString(certificate.GetExtensionValue(new Org.BouncyCastle.Asn1.DerObjectIdentifier(oid)).GetOctets());
            if (value == null)
            {
                return null;
            }
            return new StringBuilder(value.ToString(), 2, value.ToString().Length - 2, value.ToString().Length - 2).ToString();
        }

        /// <summary>
        /// Get the bouncy castle public key of a given microsoft certificate
        /// </summary>
        /// <param name="certificate"></param>
        /// <returns></returns>
        public static Org.BouncyCastle.Crypto.AsymmetricKeyParameter GetPublicKey(System.Security.Cryptography.X509Certificates.X509Certificate certificate)
        {
            Org.BouncyCastle.X509.X509CertificateParser x509Parser = new Org.BouncyCastle.X509.X509CertificateParser();
            Org.BouncyCastle.X509.X509Certificate bcert = x509Parser.ReadCertificate(certificate.GetRawCertData());
            return bcert.GetPublicKey();
        }

        /// <summary>
        /// Returns true if the Certificate KeyUsage has DigitalSignature Flag set.
        /// </summary>
        /// <param name="certificate">BouncyCastle X509Certificate to check</param>
        /// <returns>true if the certificate has the flag is set.</returns>
        /// <exception cref="X509CertificateFormatException">Certificate does not have the KeyUsage extension set.</exception>
        public static bool KeyUsageIsDigitalSignature(Org.BouncyCastle.X509.X509Certificate certificate)
        {
            if (certificate == null)
            {
                throw new ArgumentNullException("certificate", "Certificate not set");
            }

            bool[] keyUsage = certificate.GetKeyUsage();
            if (keyUsage == null)
            {
                throw new X509CertificateFormatException("Certificate does not have the KeyUsage extension.");
            }
            return keyUsage[0];
        }

        /// <summary>
        /// Returns true if the Certificate KeyUsage has KeyEncipherment Flag set.
        /// </summary>
        /// <param name="certificate">BouncyCastle X509Certificate to check</param>
        /// <returns>true if the certificate has the flag is set.</returns>
        /// <exception cref="X509CertificateFormatException">Certificate does not have the KeyUsage extension set.</exception>
        public static bool KeyUsageIsKeyEncipherment(Org.BouncyCastle.X509.X509Certificate certificate)
        {
            if (certificate == null)
            {
                throw new ArgumentNullException("certificate", "Certificate not set");
            }

            bool[] keyUsage = certificate.GetKeyUsage();
            if (keyUsage == null)
            {
                throw new X509CertificateFormatException("Certificate does not have the KeyUsage extension.");
            }
            return keyUsage[2];
        }

        /// <summary>
        /// Returns true if the Certificate KeyUsage has DataEncipherment Flag set.
        /// </summary>
        /// <param name="certificate">BouncyCastle X509Certificate to check</param>
        /// <returns>true if the certificate has the flag is set.</returns>
        /// <exception cref="X509CertificateFormatException">Certificate does not have the KeyUsage extension set.</exception>
        public static bool KeyUsageIsDataEncipherment(Org.BouncyCastle.X509.X509Certificate certificate)
        {
            if (certificate == null)
            {
                throw new ArgumentNullException("certificate", "Certificate not set");
            }
            bool[] keyUsage = certificate.GetKeyUsage();
            if (keyUsage == null)
            {
                throw new X509CertificateFormatException("Certificate does not have the KeyUsage extension.");
            }
            return keyUsage[3];
        }

        /// <summary>
        /// Returns true if the Certificate KeyUsage has keyCertSign Flag set. (Relevant for CA certificates)
        /// </summary>
        /// <param name="certificate">BouncyCastle X509Certificate to check</param>
        /// <returns>true if the certificate has the flag is set.</returns>
        /// <exception cref="X509CertificateFormatException">Certificate does not have the KeyUsage extension set.</exception>
        public static bool KeyUsageIsKeyCertSign(Org.BouncyCastle.X509.X509Certificate certificate)
        {
            if (certificate == null)
            {
                throw new ArgumentNullException("certificate", "Certificate not set");
            }

            bool[] keyUsage = certificate.GetKeyUsage();
            if (keyUsage == null)
            {
                throw new X509CertificateFormatException("Certificate does not have the KeyUsage extension.");
            }
            return keyUsage[5];
        }

        /// <summary>
        /// Returns true if the Certificate KeyUsage has cRLSign Flag set. (Relevant for CA certificates)
        /// </summary>
        /// <param name="certificate">BouncyCastle X509Certificate to check</param>
        /// <returns>true if the certificate has the flag is set.</returns>
        /// <exception cref="X509CertificateFormatException">Certificate does not have the KeyUsage extension set.</exception>
        public static bool KeyUsageIscRLSign(Org.BouncyCastle.X509.X509Certificate certificate)
        {
            if (certificate == null)
            {
                throw new ArgumentNullException("certificate", "Certificate not set");
            }

            bool[] keyUsage = certificate.GetKeyUsage();
            if (keyUsage == null)
            {
                throw new X509CertificateFormatException("Certificate does not have the KeyUsage extension.");
            }
            return keyUsage[6];
        }

        #endregion


        #region Converter

        /// <summary>
        /// Converts a Microsoft .Net X509Certificate or X509Certificate2 into a BouncyCastle X509Certificate
        /// </summary>
        /// <param name="certificate"></param>
        /// <returns></returns>
        public static Org.BouncyCastle.X509.X509Certificate ConvertToBC(System.Security.Cryptography.X509Certificates.X509Certificate certificate)
        {
            Org.BouncyCastle.X509.X509CertificateParser x509Parser = new Org.BouncyCastle.X509.X509CertificateParser();
            return x509Parser.ReadCertificate(certificate.GetRawCertData());
        }

        /// <summary>
        /// Converts a BouncyCastle X509Certificate into a Microsoft .Net X509Certificate2 
        /// </summary>
        /// <param name="certificate"></param>
        /// <returns></returns>
        public static System.Security.Cryptography.X509Certificates.X509Certificate ConvertToDotNet(Org.BouncyCastle.X509.X509Certificate certificate)
        {
            return new System.Security.Cryptography.X509Certificates.X509Certificate(certificate.GetEncoded());
        }

        #endregion


        #region Verify certificates

        public static bool HasUnknownCriticalExtension(Org.BouncyCastle.X509.X509Certificate certificate)
        {
            IEnumerator enumerator = certificate.GetCriticalExtensionOids().GetEnumerator();
            for (int i = 0; i < certificate.GetCriticalExtensionOids().Count; i++)
            {
                enumerator.MoveNext();
                string oid = (string)enumerator.Current;
                if (oid.Equals(Org.BouncyCastle.Asn1.X509.X509Extensions.KeyUsage.ToString()))
                {
                    continue;
                }
                Log.Error("Certificate was refused because of the unknown critical extension: " + oid.ToString());
                return true;
            }
            return false;
        }

        /// <summary>
        /// Verify the BouncyCastle X509Certificate using the nominated CA certificate.
        /// <para>Checks the certificate's signature as well as the validity.</para>
        /// </summary>
        /// <param name="certificate">Org.BouncyCastle.X509.X509Certificate to check</param>
        /// <param name="caPublicKey">CA certificate</param>
        /// <returns>True if the certificate is valid.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool VerifyPeerCertificate(Org.BouncyCastle.X509.X509Certificate peerCertificate, Org.BouncyCastle.X509.X509Certificate caCertificate)
        {
            if (peerCertificate == null)
            {
                throw new ArgumentNullException("certificate", "Certificate cannot be null");
            }
            if (caCertificate == null)
            {
                throw new ArgumentNullException("caPublicKey", "CA Public key cannot be null");
            }

            try
            {
                // Check CA certificate
                if (!VerifyCACertificate(caCertificate))
                {
                    // already logged error
                    return false;
                }

                // Check whether peer certificate's signature is ok
                peerCertificate.Verify(caCertificate.GetPublicKey());

                // Check whether peer certificate has the Peers@Play relevant extensions set.
                if (!IsPeerCertificate(peerCertificate))
                {
                    return false;
                }

                if (HasUnknownCriticalExtension(peerCertificate))
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error("Peer certificate's signature is invalid.", ex);
                return false;
            }


            // Check whether the peer certificate's validity period is ok
            if (peerCertificate.NotBefore > DateTime.UtcNow)
            {
                Log.Error(string.Format("Peer certificate is not valid yet. NotBefore value (local time): {0}", peerCertificate.NotBefore.ToLocalTime().ToString()));
                return false;
            }
            if (peerCertificate.NotAfter < DateTime.UtcNow)
            {
                Log.Error(string.Format("Peer certificate has been expired. NotAfter value (local time): {0}", peerCertificate.NotAfter.ToLocalTime().ToString()));
                return false;
            }

            return true;
        }

        public static bool IsPeerCertificate(Org.BouncyCastle.X509.X509Certificate peerCertificate)
        {
            return IsPeerCertificate(peerCertificate, false);
        }

        /// <summary>
        /// Check whether certificate has the Peers@Play relevant Peer extensions set.
        /// </summary>
        /// <param name="certificate">Org.BouncyCastle.X509.X509Certificate to check</param>
        /// <returns>True if the certificate is a peers@play certificate.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool IsPeerCertificate(Org.BouncyCastle.X509.X509Certificate peerCertificate, bool silent)
        {
            if (peerCertificate == null)
            {
                throw new ArgumentNullException("certificate", "Certificate cannot be null");
            }

            try
            {
                // Check whether CertificateVersion extension is properly set
                if (CertificateServices.GetExtensionValue(peerCertificate, PAPObjectIdentifier.CertificateVersion) == null)
                {
                    if (!silent)
                    {
                        Log.Error("Certificate is not a valid Peers@Play peer certificate: CertificateVersion not set");
                    }
                    return false;
                }

                // Check whether CertificateUsage extension is properly set
                if (GetExtensionValue(peerCertificate, PAPObjectIdentifier.CertificateUsage) == null)
                {
                    if (!silent)
                    {
                        Log.Error("Certificate is not a valid Peers@Play peer certificate: CertificateUsage is not set");
                    }
                    return false;
                }

                if (!GetExtensionValue(peerCertificate, PAPObjectIdentifier.CertificateUsage).Equals(CertificateUsageValue.PEER))
                {
                    if (!silent)
                    {
                        Log.Error("Certificate is not a valid Peers@Play peer certificate: CertificateUsage is invalid");
                    }
                    return false;
                }

                // Check whether KeyUsage extension is set to data encipherment on peer certificate
                if (!CertificateServices.KeyUsageIsDataEncipherment(peerCertificate))
                {
                    if (!silent)
                    {
                        Log.Error("Certificate is not a valid Peers@Play peer certificate: KeyUsage DataEncipherment is not set");
                    }
                    return false;
                }
                if (!CertificateServices.KeyUsageIsKeyEncipherment(peerCertificate))
                {
                    if (!silent)
                    {
                        Log.Error("Certificate is not a valid Peers@Play peer certificate: KeyUsage KeyEncipherment is not set");
                    }
                    return false;
                }
                if (!CertificateServices.KeyUsageIsDigitalSignature(peerCertificate))
                {
                    if (!silent)
                    {
                        Log.Error("Certificate is not a valid Peers@Play peer certificate: KeyUsage DigitaleSignature is not set");
                    }
                    return false;
                }

                // Check the CertificateVersion
                int certVersion = int.Parse(GetExtensionValue(peerCertificate, PAPObjectIdentifier.CertificateVersion));
                if (certVersion < PeerCertificate.CERTIFICATE_VERSION)
                {
                    if (!silent)
                    {
                        Log.Error("Certificate is not a valid Peers@Play peer certificate: Certificate version is outdated");
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                if (!silent)
                {
                    Log.Error("Certificate is not a valid Peers@Play peer certificate", ex);
                }
                return false;
            }

            return true;
        }

        /// <summary>
        /// Verify if the BouncyCastle X509Certificate is a valid TLS certificate
        /// <para>Checks if KeyUsage is set to KeyEncipherment and the certificate is acutally valid</para>
        /// </summary>
        /// <param name="tlsCertificate">Org.BouncyCastle.X509.X509Certificate to check</param>
        /// <returns>True if the certificate is valid.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool VerifyTlsCertificate(Org.BouncyCastle.X509.X509Certificate tlsCertificate)
        {
            if (tlsCertificate == null)
            {
                throw new ArgumentNullException("tlsCertificate", "TLS certificate cannot be null");
            }

            try
            {
                // Check whether TLS certificate has the Peers@Play relevant extensions set.
                if (!IsTlsCertificate(tlsCertificate))
                {
                    return false;
                }

                if (HasUnknownCriticalExtension(tlsCertificate))
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error("Certificate is not a valid Peers@Play TLS certificate", ex);
                return false;
            }

            // Check whether the TLS certificate's validity period is ok
            if (tlsCertificate.NotBefore > DateTime.UtcNow)
            {
                Log.Error(string.Format("TLS certificate for P@Porator is not valid yet. NotBefore value (local time): {0}", tlsCertificate.NotBefore.ToLocalTime().ToString()));
                return false;
            }
            if (tlsCertificate.NotAfter < DateTime.UtcNow)
            {
                Log.Error(string.Format("TLS certificate for P@Porator has been expired. NotAfter value (local time): {0}", tlsCertificate.NotAfter.ToLocalTime().ToString()));
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check whether certificate has the Peers@Play relevant SSL/TLS extensions set.
        /// </summary>
        /// <param name="certificate">Org.BouncyCastle.X509.X509Certificate to check</param>
        /// <returns>True if the certificate is a peers@play TLS certificate.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool IsTlsCertificate(Org.BouncyCastle.X509.X509Certificate tlsCertificate)
        {
            if (tlsCertificate == null)
            {
                throw new ArgumentNullException("tlsCertificate", "Certificate cannot be null");
            }

            try
            {
                // Check whether CertificateUsage extension is properly set
                if (GetExtensionValue(tlsCertificate, PAPObjectIdentifier.CertificateUsage) == null)
                {
                    Log.Error("Certificate is not a valid Peers@Play TLS certificate: CertificateUsage is not set");
                    return false;
                }

                if (!GetExtensionValue(tlsCertificate, PAPObjectIdentifier.CertificateUsage).Equals(CertificateUsageValue.TLS))
                {
                    Log.Error("Certificate is not a valid Peers@Play TLS certificate: CertificateUsage is invalid");
                    return false;
                }

                // Check whether KeyUsage extension is set to data encipherment on peer certificate
                if (!CertificateServices.KeyUsageIsDataEncipherment(tlsCertificate))
                {
                    Log.Error("Certificate is not a valid Peers@Play TLS certificate: KeyUsage DataEncipherment is not set");
                    return false;
                }
                if (!CertificateServices.KeyUsageIsKeyEncipherment(tlsCertificate))
                {
                    Log.Error("Certificate is not a valid Peers@Play TLS certificate: KeyUsage KeyEncipherment is not set");
                    return false;
                }
                if (!CertificateServices.KeyUsageIsDigitalSignature(tlsCertificate))
                {
                    Log.Error("Certificate is not a valid Peers@Play TLS certificate: KeyUsage DigitaleSignature is not set");
                    return false;
                }

                // Check if CertificateVersion is a number, throws extensions if not
                int.Parse(GetExtensionValue(tlsCertificate, PAPObjectIdentifier.CertificateVersion));
            }
            catch (Exception ex)
            {
                Log.Error("Certificate is not a valid Peers@Play TLS certificate", ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Verify if the BouncyCastle X509Certificate is a valid CA certificate
        /// <para>Checks if KeyUsage is set to KeyCertSign and the certificate is acutally valid</para>
        /// </summary>
        /// <param name="caCertificate">Org.BouncyCastle.X509.X509Certificate to check</param>
        /// <returns>True if the certificate is valid.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool VerifyCACertificate(Org.BouncyCastle.X509.X509Certificate caCertificate)
        {
            if (caCertificate == null)
            {
                throw new ArgumentNullException("caCertificate", "CA certificate cannot be null");
            }

            try
            {
                // Check whether CA certificate has the Peers@Play relevant extensions set.
                if (!IsCaCertificate(caCertificate))
                {
                    return false;
                }

                if (HasUnknownCriticalExtension(caCertificate))
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error("Certificate is not a valid Peers@Play CA certificate", ex);
                return false;
            }

            // Check whether the CA certificate's validity period is ok
            if (caCertificate.NotBefore > DateTime.UtcNow)
            {
                Log.Error(string.Format("CA certificate is not valid yet. NotBefore value (local time): {0}", caCertificate.NotBefore.ToLocalTime().ToString()));
                return false;
            }
            if (caCertificate.NotAfter < DateTime.UtcNow)
            {
                Log.Error(string.Format("CA certificate has been expired. NotAfter value (local time): {0}", caCertificate.NotAfter.ToLocalTime().ToString()));
                return false;
            }
            return true;
        }

        /// <summary>
        /// Check whether certificate has the Peers@Play relevant CA extensions set.
        /// </summary>
        /// <param name="certificate">Org.BouncyCastle.X509.X509Certificate to check</param>
        /// <returns>True if the certificate is a peers@play TLS certificate.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool IsCaCertificate(Org.BouncyCastle.X509.X509Certificate caCertificate)
        {
            if (caCertificate == null)
            {
                throw new ArgumentNullException("certificate", "Certificate cannot be null");
            }

            try
            {
                // Check whether CertificateUsage extension is properly set
                if (GetExtensionValue(caCertificate, PAPObjectIdentifier.CertificateUsage) == null)
                {
                    Log.Error("Certificate is not a valid Peers@Play CA certificate: CertificateUsage is not set");
                    return false;
                }

                if (!GetExtensionValue(caCertificate, PAPObjectIdentifier.CertificateUsage).Equals(CertificateUsageValue.CA))
                {
                    Log.Error("Certificate is not a valid Peers@Play CA certificate: CertificateUsage is invalid");
                    return false;
                }

                // Check whether KeyUsage extension is set to the right values
                if (!KeyUsageIscRLSign(caCertificate))
                {
                    Log.Error("Certificate is not a valid Peers@Play CA certificate: KeyUsage cRLSign is not set");
                    return false;
                }
                if (!KeyUsageIsKeyCertSign(caCertificate))
                {
                    Log.Error("Certificate is not a valid Peers@Play CA certificate: KeyUsage keyCertSign is not set");
                    return false;
                }

                Org.BouncyCastle.Asn1.X509.BasicConstraints basicConst = new Org.BouncyCastle.Asn1.X509.BasicConstraints(caCertificate.GetBasicConstraints());
                if (caCertificate.GetBasicConstraints() == -1 || !basicConst.IsCA())
                {
                    Log.Error("Certificate is not a valid Peers@Play CA certificate: KeyUsage keyCertSign is not set");
                    return false;
                }

                // Check if CertificateVersion is a number, throws extensions if not
                int.Parse(GetExtensionValue(caCertificate, PAPObjectIdentifier.CertificateVersion));
            }
            catch (Exception ex)
            {
                Log.Error("Certificate is not a valid Peers@Play CA certificate", ex);
                return false;
            }

            return true;
        }

        #endregion

    }

    public class NoCertificateFoundException : Exception
    {
        public NoCertificateFoundException(string msg)
            : base(msg)
        {
        }
    }
}
