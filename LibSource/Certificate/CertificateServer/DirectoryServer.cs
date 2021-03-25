using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CrypTool.CertificateLibrary.Network;
using CrypTool.CertificateLibrary.Util;
using CrypTool.CertificateLibrary.Certificates;
using CrypTool.Util.Logging;
using CrypTool.Util.Cryptography;

namespace CrypTool.CertificateServer
{

    /// <summary>
    /// The directory server is responsible to serve existing certificates to the server.
    /// Also password changes are performed here.
    /// </summary>
    public class DirectoryServer
    {

        #region Private member

        private CertificationAuthority certificationAuthority;

        private RegistrationAuthority registrationAuthority;

        private CertificateDatabase certificateDatabase;

        private SMTPEmailClient smtpClient;

        private Configuration config;

        #endregion


        #region Constructor

        public DirectoryServer(CertificationAuthority ca, RegistrationAuthority ra, CertificateDatabase db, SMTPEmailClient smptClient)
        {
            this.certificationAuthority = ca;
            this.registrationAuthority = ra;
            this.certificateDatabase = db;
            this.smtpClient = smptClient;

            this.config = Configuration.GetConfiguration();
        }

        #endregion


        #region Certificate Request

        public ProcessingResult ProcessCertificateRequest(ClientInfo client, CT2CertificateRequest certRequest)
        {
            // Check the request data
            ValidateCertificateRequest(certRequest);

            // Load the database entry
            CertificateEntry certEntry = (certRequest.Email != null)
                ? certificateDatabase.SelectPeerEntry(this.certificationAuthority.CaCertificate.CaX509.SerialNumber, certRequest.Email, true)
                : certificateDatabase.SelectPeerEntry(this.certificationAuthority.CaCertificate.CaX509.SerialNumber, certRequest.Avatar, false);

            if (certEntry != null)
            {
                // Certificate found, so load it
                byte[] pkcs12 = certificateDatabase.SelectPeerCertificate(true, this.certificationAuthority.CaCertificate.CaX509.SerialNumber, certEntry.Email, true);

                // Check the password
                if (!CertificateServices.CheckPassword(pkcs12, certRequest.Password))
                {
                    throw new ProcessingException(Logging.LogType.Warn, "Refused a certificate request, because the user entered a wrong password.", new ProcessingError(ErrorType.WrongPassword));
                }

                // Send the certificate
                Log.Info(Logging.GetLogEnvelope("Sent an existing certificate to the client.", client, certEntry));
                return new ProcessingResult(PacketType.CertificateResponse, pkcs12);
            }
            else
            {
                // No certificate found. Maybe there is an open registration...

                RegistrationEntry regEntry = (certRequest.Email != null)
                    ? certificateDatabase.SelectRegistrationEntry(this.certificationAuthority.CaCertificate.CaX509.SerialNumber, certRequest.Email, true)
                    : certificateDatabase.SelectRegistrationEntry(this.certificationAuthority.CaCertificate.CaX509.SerialNumber, certRequest.Avatar, false);
                if (regEntry == null)
                {
                    // No registration request found
                    throw new ProcessingException(
                        Logging.LogType.Warn, 
                        "Refused a certificate request, because neither a certificate nor a registration request was found in the database.", 
                        new ProcessingError(ErrorType.NoCertificateFound));
                }

                // Check the password
                if (regEntry.HashedPassword != SHA1.ComputeHashString(certRequest.Password))
                {
                    throw new ProcessingException(
                        Logging.LogType.Warn,
                        "Refused a certificate request, because the user entered a wrong password!",
                        new ProcessingError(ErrorType.WrongPassword));
                }

                if (regEntry.IsVerified)
                {
                    if (regEntry.IsAuthorized)
                    {
                        // Verified and authorized. So lets issue the certificate
                        Configuration.CertificateProgram programConfig = config.GetProgramConfiguration(regEntry.ProgramName);
                        ProcessingResult result = this.certificationAuthority.IssuePeerCertificate(programConfig, client, regEntry, certRequest.Password);
                        this.registrationAuthority.DeleteRegistrationRequest(regEntry);
                        return result;
                    }
                    else
                    {
                        // Manual authorization required
                        throw new ProcessingException(
                            Logging.LogType.Warn,
                            "Refused a certificate request, because the certificate registration has not been authorized by an admin yet.",
                            new ProcessingError(ErrorType.CertificateNotYetAuthorized));
                    }
                }
                else
                {
                    // Email has not been verified - resend verification code
                    Configuration.CertificateProgram programConfig = config.GetProgramConfiguration(regEntry.ProgramName);
                    bool emailSent = this.smtpClient.SendEmailVerificationCode(client, programConfig, regEntry);
                    if (!emailSent)
                    {
                        HandleUndeliveredEmail((byte)0, regEntry.Email);
                        throw new ProcessingException(
                            Logging.LogType.Warn,
                            "Could not send email verification code as the SMTP server seems down.",
                            new ProcessingError(ErrorType.SmtpServerDown));
                    }

                    throw new ProcessingException(
                        Logging.LogType.Info,
                        "Client tried to retrieve a certificate, but the email address is not verified yet. Resent the email verification code.",
                        new ProcessingError(ErrorType.EmailNotYetVerified));
                }
            }
        }

        /// <summary>
        /// Checks the certificate request data. Throws a ProcessingException if data format is incorrect.
        /// </summary>
        /// <param name="request">The certificate request data</param>
        /// <exception cref="ProcessingException"></exception>
        private void ValidateCertificateRequest(CT2CertificateRequest request)
        {
            if (!Verification.IsValidPassword(request.Password))
            {
                //Log.Warn(GetLogEnvelope("Rejected certificate request, because password check failed.", this.client, request.Avatar, request.Email));
                throw new ProcessingException(Logging.LogType.Warn, "Rejected certificate request, because password check failed.", new ProcessingError(ErrorType.WrongPassword));
            }
            if (!Verification.IsValidAvatar(request.Avatar) && !Verification.IsValidEmailAddress(request.Email))
            {
                //Log.Warn(GetLogEnvelope("Rejected certificate request, because both avatar and email address were invalid.", this.client, request.Avatar, request.Email));
                throw new ProcessingException(Logging.LogType.Warn, "Rejected certificate request, because both avatar and email address were invalid.", new ProcessingError(ErrorType.NoCertificateFound));
            }
        }

        #endregion


        #region Password Change

        public ProcessingResult ProcessPasswordChange(ClientInfo client, PasswordChange passwordChange)
        {
            // Check the password reset data
            ValidatePasswordChangeData(passwordChange);

            // Load the database entry
            CertificateEntry certEntry = (passwordChange.Email != null)
                ? certificateDatabase.SelectPeerEntry(this.certificationAuthority.CaCertificate.CaX509.SerialNumber, passwordChange.Email, true)
                : certificateDatabase.SelectPeerEntry(this.certificationAuthority.CaCertificate.CaX509.SerialNumber, passwordChange.Avatar, false);
            if (certEntry == null)
            {
                // No certificate with this avatar/email found in the database
                throw new ProcessingException(
                    Logging.LogType.Warn, 
                    "Refused a password change, because the avatar/email does not exist.", 
                    new ProcessingError(ErrorType.NoCertificateFound));
            }

            // Get the certificate
            byte[] pkcs12 = certificateDatabase.SelectPeerCertificate(true, this.certificationAuthority.CaCertificate.CaX509.SerialNumber, certEntry.Email, true);

            // Check the password
            PeerCertificate peerCert = new PeerCertificate();
            try
            {
                // try to open the PKCS #12 with the given password
                using (MemoryStream mstream = new MemoryStream(pkcs12))
                {
                    peerCert.Load(mstream, passwordChange.OldPassword);
                    mstream.Close();
                }
                if (!peerCert.IsLoaded)
                {
                    throw new Exception();
                }
            }
            catch
            {
                throw new ProcessingException(
                    Logging.LogType.Warn,
                    "Refused a password change, because the user entered a wrong password.",
                    new ProcessingError(ErrorType.WrongPassword));
            }
            Log.Debug(Logging.GetLogEnvelope("Successfully opened PKCS #12 store to change password.", client, certEntry));

            // Everything is OK. Change the password
            peerCert.Password = passwordChange.NewPassword;
            pkcs12 = peerCert.GetPkcs12();
            certificateDatabase.UpdatePeerPkcs12(certificationAuthority.CaCertificate.CaX509.SerialNumber, certEntry.Email, pkcs12);

            // Send the certificate
            Log.Info(Logging.GetLogEnvelope("Sent modified peer certificate to the client (password changed).", client, certEntry));
            return new ProcessingResult(PacketType.CertificateResponse, pkcs12);
        }

        /// <summary>
        /// Checks the password change data. Returns an ProcessingError object if data format is incorrect.
        /// </summary>
        /// <param name="passwordChange">The password change data</param>
        /// <exception cref="ProcessingException"></exception>
        private void ValidatePasswordChangeData(PasswordChange passwordChange)
        {
            if (!Verification.IsValidAvatar(passwordChange.Avatar) && !Verification.IsValidEmailAddress(passwordChange.Email))
            {
                throw new ProcessingException(Logging.LogType.Warn, "Rejected invalid password change, because both avatar and email address were invalid.", new ProcessingError(ErrorType.NoCertificateFound));
            }
            if (!Verification.IsValidPassword(passwordChange.NewPassword))
            {
                throw new ProcessingException(Logging.LogType.Warn, "Rejected password change, because the new password was invalid.", new ProcessingError(ErrorType.PasswordFormatIncorrect));
            }
        }

        #endregion


        #region Helper

        /// <summary>
        /// Stores an entry in the database, that the email sending failed.
        /// Type: EmailVerificationCode = 0, PasswordResetCode = 1, RegistrationRequest = 2, RegistrationPerformed = 3, AuthorizationGranted = 4
        /// </summary>
        /// <param name="emailType"></param>
        /// <param name="email"></param>
        private void HandleUndeliveredEmail(byte emailType, string email)
        {
            try
            {
                this.certificateDatabase.StoreUndeliveredEmailEntry(this.certificationAuthority.CaCertificate.CaX509.SerialNumber, emailType, email, DateTime.Now);
            }
            catch (Exception ex)
            {
                // This is the worst case, smtp server and database down...
                Log.Error("WARNING! Could not store undelivered email entry!", ex);
            }
        }

        #endregion

    }
}
