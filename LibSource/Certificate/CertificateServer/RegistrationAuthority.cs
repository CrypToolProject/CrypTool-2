using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrypTool.Util.Cryptography;
using Org.BouncyCastle.Math;
using CrypTool.Util.Logging;
using CrypTool.CertificateLibrary.Network;
using CrypTool.CertificateLibrary.Util;
using System.Collections;
using System.Text.RegularExpressions;
using CrypTool.CertificateServer.Rules;

namespace CrypTool.CertificateServer
{
    /// <summary>
    /// The registration authority is responsible to process certificate registration requests.
    /// This includes the email verification and authorization of requests.
    /// Also password reset and reset verification are performed by the registration authority.
    /// </summary>
    public class RegistrationAuthority
    {

        #region Constants

        private const int VERIFICATION_CODE_LENGTH = 15;

        #endregion


        #region Constructor

        public RegistrationAuthority(CertificationAuthority ca, CertificateDatabase db, SMTPEmailClient smtpClient)
        {
            this.config = Configuration.GetConfiguration();
            this.policyConfig = PolicyConfig.GetPolicyConfig();

            // Uncomment to generate an example rule
            //this.policyConfig.GetPolicyRuleChain("default").GenerateExampleRule(1, "avatar", ".*", "accept");
            //this.policyConfig.Save();

            Log.Info(this.policyConfig.ToString());

            this.extensionConfig = ExtensionConfig.GetExtensionConfig();

            // Uncomment to generate an example rule
            //this.extensionConfig.GetExtensionRuleChain("default").GenerateExampleRule(1, "avatar", ".*", CrypTool.CertificateLibrary.Certificates.PAPObjectIdentifier.PeersAtPlay + ".123456789", "ENTER VALUE HERE");
            //this.extensionConfig.Save();

            Log.Info(this.extensionConfig.ToString());
            this.certificationAuthority = ca;
            this.certificateDatabase = db;
            this.smtpClient = smtpClient;
        }

        #endregion


        #region Certificate Registration

        public ProcessingResult ProcessRegistrationRequest(ClientInfo client, CertificateRegistration certRegistration)
        {
            // Check the registration data
            ValidateCertificateRegistration(certRegistration);

            // Check for duplicate
            this.CheckForDuplicate(certRegistration.Avatar, certRegistration.Email);

            // Check registration authorization status
            PolicyRuleChain ruleChain = this.policyConfig.GetPolicyRuleChain(certRegistration.ProgramName);
            bool isAuthorized = ApplyRegistrationFilter(ruleChain, certRegistration);

            // Check extension rule chain for applicable extensions
            Extension[] extensions = this.extensionConfig.GetExtensions(certRegistration);

            // Read the configuration
            Configuration.CertificateProgram programConfig = config.GetProgramConfiguration(certRegistration.ProgramName);
            string hashedPassword = SHA1.ComputeHashString(certRegistration.Password);
            string emailVerificationCode = null;
            RegistrationEntry regEntry = new RegistrationEntry(certRegistration.Avatar, certRegistration.Email, certRegistration.World, DateTime.UtcNow, emailVerificationCode, !programConfig.EmailVerificationRequired, hashedPassword, client.ProgramName, client.ProgramVersion, client.OptionalInfo, isAuthorized, extensions);

            if (programConfig.EmailVerificationRequired)
            {
                // Email verification required. Generate verification code
                regEntry.EmailCode = GenerateEmailVerificationCode(certRegistration.Email);

                // Store the request.
                this.StoreRegistrationRequest(regEntry);
                Log.Info(Logging.GetLogEnvelope("Registration request has been recorded. Client will be informed that the email address needs to be verified.", client, regEntry));

                // Send verification code
                bool emailSent = this.smtpClient.SendEmailVerificationCode(client, programConfig, regEntry);
                if (!emailSent)
                {
                    HandleUndeliveredEmail((byte)0, regEntry.Email);
                    throw new ProcessingException(
                        Logging.LogType.Warn, 
                        "Could not send email verification code as the SMTP server seems down.", 
                        new ProcessingError(ErrorType.SmtpServerDown));
                }

                // Send registration request information
                if (programConfig.RegistrationRequest.SendInformation)
                {
                    emailSent = this.smtpClient.SendRegistrationRequestInformation(client, programConfig, regEntry);
                    if (!emailSent)
                    {
                        HandleUndeliveredEmail((byte)2, regEntry.Email);
                    }
                }
                return new ProcessingResult(PacketType.EmailVerificationRequired);
            }
            else
            {
                // No email verification required

                if (!isAuthorized)
                {
                    // Certificate needs to be authorized
                    this.StoreRegistrationRequest(regEntry);
                    Log.Info(Logging.GetLogEnvelope("Registration request has been recorded. Client will be informed that the registration request needs to be authorized.", client, regEntry));

                    if (programConfig.RegistrationRequest.SendInformation)
                    {
                        this.smtpClient.SendRegistrationRequestInformation(client, programConfig, regEntry);
                    }
                    return new ProcessingResult(PacketType.CertificateAuthorizationRequired);
                }
                else
                {
                    // No authorization needed, issue and store and send the certificate
                    return this.certificationAuthority.IssuePeerCertificate(programConfig, client, regEntry, certRegistration.Password);
                }
            }
        }

        /// <summary>
        /// Checks the certificate registration data. Throws a ProcessingException if format is incorrect.
        /// </summary>
        /// <param name="registration">The certificate registration data</param>
        /// <exception cref="ProcessingException">If the validation failed</exception>
        private void ValidateCertificateRegistration(CertificateRegistration registration)
        {
            if (!Verification.IsValidAvatar(registration.Avatar))
            {
                throw new ProcessingException(
                    Logging.LogType.Warn, 
                    "Rejected certificate registration, because avatar check failed.", 
                    new ProcessingError(ErrorType.AvatarFormatIncorrect));
            }
            if (!Verification.IsValidEmailAddress(registration.Email))
            {
                throw new ProcessingException(
                    Logging.LogType.Warn, 
                    "Rejected certificate registration, because email check failed.", 
                    new ProcessingError(ErrorType.EmailFormatIncorrect));
            }
            if (!Verification.IsValidWorld(registration.World))
            {
                throw new ProcessingException(
                    Logging.LogType.Warn, 
                    "Rejected certificate registration, because world check failed.", 
                    new ProcessingError(ErrorType.WorldFormatIncorrect));
            }
            if (!Verification.IsValidPassword(registration.Password))
            {
                throw new ProcessingException(
                    Logging.LogType.Warn, 
                    "Rejected certificate registration, because password check failed.", 
                    new ProcessingError(ErrorType.PasswordFormatIncorrect));
            }
        }

        /// <summary>
        /// Checks the registration data. Returns true if the registration data is correct and no manual authorization needs to be performed.
        /// Returns false if manual authorization is required.
        /// Throws a ProcessingException if the registration is denied.
        /// </summary>
        /// <param name="programConfig">The configuration relevant for the program</param>
        /// <param name="certRegistration">The certificate registration data</param>
        /// <returns>true if no manual authorization needs to be done</returns>
        private bool ApplyRegistrationFilter(PolicyRuleChain ruleChain, CertificateRegistration certRegistration)
        {
            string policy = ruleChain.GetPolicy(certRegistration);
            if (policy.Equals("deny"))
            {
                throw new ProcessingException(
                    Logging.LogType.Warn,
                    "A filtering rule denied a registration request.",
                    new ProcessingError(ErrorType.AvatarFormatIncorrect, "Your registration request was denied!"));
            }
            return (policy.Equals("accept")) ? true : false;
        }

        /// <summary>
        /// Checks whether the given avatar or email address has been used before. Throws a ProcessingException in this case.
        /// </summary>
        /// <param name="avatar">The avatar name to look for duplicates</param>
        /// <param name="email">The email address to look for duplicates</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DatabaseException"></exception>
        /// <exception cref="ProcessingException">Avatar or email already exists</exception>
        private void CheckForDuplicate(string avatar, string email)
        {
            // Check whether avatar or email have already been registered
            ProcessingError processingError = certificateDatabase.SelectAvatarEmailExist(this.certificationAuthority.CaCertificate.CaX509.SerialNumber, avatar, email);
            if (processingError != null)
            {
                throw new ProcessingException(
                    Logging.LogType.Info, 
                    String.Format("A certificate registration was rejected, because of a duplicate entry ({0})", processingError.Type.ToString()), 
                    processingError);
            }
        }

        /// <summary>
        /// Generates a database unique email verification code
        /// </summary>
        /// <param name="s"></param>
        /// <exception cref="DatabaseException"></exception>
        private string GenerateEmailVerificationCode(string s)
        {
            string emailVerificationCode = null;
            do
            {
                Random random = new Random();
                // Stupid verification code generation, maybe changed later
                emailVerificationCode = (SHA1.ComputeHashString(s + random.Next().ToString())).Substring(0, VERIFICATION_CODE_LENGTH);
            }
            while (certificateDatabase.SelectRegistrationEntry(this.certificationAuthority.CaCertificate.CaX509.SerialNumber, emailVerificationCode) != null);
            return emailVerificationCode;
        }

        /// <summary>
        /// Stores the registration request in the database.
        /// </summary>
        /// <param name="regEntry">The registration data to store</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DatabaseException"></exception>
        private void StoreRegistrationRequest(RegistrationEntry regEntry)
        {
            // Store registration in the DB
            certificateDatabase.StoreRegistrationRequest(regEntry, this.certificationAuthority.CaCertificate.CaX509.SerialNumber);

            this.RegistrationCount++;
            this.LastRegistrationRequest = regEntry.DateOfRequest;

            if (RegistrationRequestReceived != null)
            {
                this.RegistrationRequestReceived.Invoke(this, new EventArgs());
            }
        }

        #endregion


        #region Email Verification

        public ProcessingResult ProcessEmailVerification(ClientInfo client, EmailVerification emailVerification)
        {
            // Check the email verification data
            this.ValidateEmailVerificationData(emailVerification);

            // Load the database entry
            RegistrationEntry regEntry = certificateDatabase.SelectRegistrationEntry(this.certificationAuthority.CaCertificate.CaX509.SerialNumber, emailVerification.Code);
            if (regEntry == null)
            {
                // No registration request found
                throw new ProcessingException(
                    Logging.LogType.Warn, 
                    "Refused an email verification, because no registration was found in the database.", 
                    new ProcessingError(ErrorType.NoCertificateFound));
            }

            if (emailVerification.Delete)
            {
                // Delete the certificate registration
                Log.Info(Logging.GetLogEnvelope("User canceled a certificate registration.", client, regEntry));
                this.DeleteRegistrationRequest(regEntry);
                return new ProcessingResult(PacketType.RegistrationDeleted);
            }

            // These lines ensure backwards compatibility with CT2 Beta 3 and may be removed in the future.
            // Old versions had the password to be transmitted (so it won't be null) and if so, the server will directly issue a certificate and send it.
            // START BACKWARD COMPATIBLITY CODE
            if (emailVerification.Password != null)
            {
                // Check the password
                if (regEntry.HashedPassword != SHA1.ComputeHashString(emailVerification.Password))
                {
                    throw new ProcessingException(
                        Logging.LogType.Warn,
                        "Refused an email verification, because the user entered a wrong password!",
                        new ProcessingError(ErrorType.WrongPassword));
                }
                if (regEntry.IsVerified)
                {
                    // Email address has been verified, but authorization is required
                    throw new ProcessingException(
                        Logging.LogType.Warn,
                        "Client tried to verify an email address that was already verified, but not authorized!",
                        new ProcessingError(ErrorType.AlreadyVerified));
                }
                else
                {
                    // Email address has not been authorized yet

                    if (regEntry.IsAuthorized)
                    {
                        // Issue the certificate
                        Configuration.CertificateProgram programConfig = config.GetProgramConfiguration(regEntry.ProgramName);
                        ProcessingResult result = certificationAuthority.IssuePeerCertificate(programConfig, client, regEntry, emailVerification.Password);
                        this.DeleteRegistrationRequest(regEntry);
                        return result;
                    }
                    else
                    {
                        // Not authorized, verify the email and inform the client
                        this.certificateDatabase.UpdateEmailVerificationStatus(certificationAuthority.CaCertificate.CaX509.SerialNumber, regEntry.Email, true, true);
                        Log.Info(Logging.GetLogEnvelope("Email address has been verified, but the registration needs to be authorized. Client was informed.", client, regEntry));
                        return new ProcessingResult(PacketType.CertificateAuthorizationRequired);
                    }
                }
            }
            // END BACKWARD COMPATIBLITY CODE
            else
            {
                if (regEntry.IsVerified)
                {
                    // Email address has been verified, but authorization is required
                    throw new ProcessingException(
                        Logging.LogType.Warn,
                        "Client tried to verify an email address that was already verified, but not authorized!",
                        new ProcessingError(ErrorType.AlreadyVerified));
                }
                else
                {
                    // Email address has not been authorized yet

                    // Verify email address
                    this.certificateDatabase.UpdateEmailVerificationStatus(certificationAuthority.CaCertificate.CaX509.SerialNumber, regEntry.Email, true, true);

                    if (!regEntry.IsAuthorized)
                    {
                        // Not authorized
                        Log.Info(Logging.GetLogEnvelope("Email address has been verified, but the registration needs to be authorized. Client was informed.", client, regEntry));
                        return new ProcessingResult(PacketType.CertificateAuthorizationRequired);
                    }
                    else
                    {
                        Log.Info(Logging.GetLogEnvelope("Email address has been verified.", client, regEntry));
                        return new ProcessingResult(PacketType.EmailVerified);
                    }
                }
            }
        }

        /// <summary>
        /// Delete a registration request from the database.
        /// </summary>
        /// <param name="regEntry"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public void DeleteRegistrationRequest(RegistrationEntry regEntry)
        {
            this.certificateDatabase.DeleteRegistrationEntry(this.certificationAuthority.CaCertificate.CaX509.SerialNumber, regEntry.EmailCode);

            this.RegistrationCount--;

            if (this.RegistrationRequestDeleted != null)
            {
                this.RegistrationRequestDeleted.Invoke(this, new EventArgs());
            }
        }

        /// <summary>
        /// Checks the email verification data. Throws a ProcessingException if format is incorrect.
        /// </summary>
        /// <param name="verification">The email verification data</param>
        /// <exception cref="ProcessingException">If the validation failed</exception>
        private void ValidateEmailVerificationData(EmailVerification verification)
        {
            if (!Verification.IsValidCode(verification.Code))
            {
                throw new ProcessingException(Logging.LogType.Warn, "Rejected email verification, because code check failed.", new ProcessingError(ErrorType.NoCertificateFound));
            }
        }

        #endregion


        #region Password Reset

        public ProcessingResult ProcessPasswordReset(ClientInfo client, PasswordReset passwordReset)
        {
            // Check the password reset data
            this.ValidatePasswordResetData(passwordReset);

            // Load the database entry
            CertificateEntry certEntry = (passwordReset.Email != null)
                ? certificateDatabase.SelectPeerEntry(this.certificationAuthority.CaCertificate.CaX509.SerialNumber, passwordReset.Email, true)
                : certificateDatabase.SelectPeerEntry(this.certificationAuthority.CaCertificate.CaX509.SerialNumber, passwordReset.Avatar, false);
            if (certEntry == null)
            {
                // No certificate with this avatar/email found in the database
                throw new ProcessingException(
                    Logging.LogType.Warn,
                    "Refused a password reset, because no certificate for the avatar/email exist.",
                    new ProcessingError(ErrorType.NoCertificateFound));
            }

            // Generate a new password reset code if no code exist
            if (certEntry.PasswordCode == null)
            {
                certEntry.PasswordCode = this.GeneratePasswordResetCode(certEntry.Email);
                certificateDatabase.UpdatePasswordResetData(certificationAuthority.CaCertificate.CaX509.SerialNumber, certEntry.Email, true, certEntry.PasswordCode, DateTime.UtcNow);
            }

            // Read the relevant configuration
            Configuration.CertificateProgram programConfig = config.GetProgramConfiguration(passwordReset.ProgramName);

            // Send the code per email
            bool emailSent = this.smtpClient.SendPasswordResetCode(client, programConfig, certEntry);
            if (!emailSent)
            {
                HandleUndeliveredEmail((byte)1, certEntry.Email);
                throw new ProcessingException(
                    Logging.LogType.Warn, 
                    "Could not send password reset code as the SMTP server seems down.", 
                    new ProcessingError(ErrorType.SmtpServerDown));
            }
            return new ProcessingResult(PacketType.PasswordResetVerificationRequired);
        }

        /// <summary>
        /// Checks the password reset data. Throws a ProcessingException if format is incorrect.
        /// </summary>
        /// <param name="passwordReset">The password reset data</param>
        /// <exception cref="ProcessingException">If the validation failed</exception>
        private void ValidatePasswordResetData(PasswordReset passwordReset)
        {
            if (!Verification.IsValidAvatar(passwordReset.Avatar) && !Verification.IsValidEmailAddress(passwordReset.Email))
            {
                throw new ProcessingException(
                    Logging.LogType.Warn,
                    "Rejected password reset, because both avatar and email address were invalid.",
                    new ProcessingError(ErrorType.NoCertificateFound));
            }
        }

        /// <summary>
        /// Generates a database unique password reset code.
        /// </summary>
        /// <param name="s"></param>
        /// <returns>The reset code</returns>
        /// <exception cref="DatabaseException"></exception>
        private string GeneratePasswordResetCode(string s)
        {
            string passwordResetCode = null;
            do
            {
                Random random = new Random();
                // Stupid verification code, may be changed later
                passwordResetCode = (SHA1.ComputeHashString(s + random.Next().ToString())).Substring(0, VERIFICATION_CODE_LENGTH);
            }
            while (certificateDatabase.SelectPasswordResetCodeExist(this.certificationAuthority.CaCertificate.CaX509.SerialNumber, passwordResetCode));
            return passwordResetCode;
        }

        #endregion


        #region Password Reset Verification

        public ProcessingResult ProcessPasswordResetVerification(ClientInfo client, PasswordResetVerification passwordResetVerification)
        {
            // Check the password reset data
            ValidatePasswordResetVerificationData(passwordResetVerification);

            // Load the database entry
            CertificateEntry certEntry = certificateDatabase.SelectPeerEntry(this.certificationAuthority.CaCertificate.CaX509.SerialNumber, passwordResetVerification.Code);
            if (certEntry == null)
            {
                // No certificate with this avatar/email found in the database
                throw new ProcessingException(
                    Logging.LogType.Warn, 
                    "Refused a password reset verification, because the code does not exist.", 
                    new ProcessingError(ErrorType.NoCertificateFound));
            }

            // Everything is OK, generate a new certificate with the existing data
            Configuration.CertificateProgram programConfig = config.GetProgramConfiguration(certEntry.ProgramName);
            return certificationAuthority.ReIssuePeerCertificate(programConfig, client, certEntry, passwordResetVerification.NewPassword);
        }

        /// <summary>
        /// Checks the password reset verification data. Throws a ProcessingException if format is incorrect.
        /// </summary>
        /// <param name="passwordResetVerification">The password reset verificatino data</param>
        /// <exception cref="ProcessingException">If the validation failed</exception>
        private void ValidatePasswordResetVerificationData(PasswordResetVerification passwordResetVerification)
        {
            if (!Verification.IsValidCode(passwordResetVerification.Code))
            {
                throw new ProcessingException(Logging.LogType.Warn, "Rejected password reset verification, because the code was invalid.", new ProcessingError(ErrorType.NoCertificateFound));
            }
            if (!Verification.IsValidPassword(passwordResetVerification.NewPassword))
            {
                throw new ProcessingException(Logging.LogType.Warn, "Rejected password reset verification, because the new password was invalid.", new ProcessingError(ErrorType.PasswordFormatIncorrect));
            }
        }

        #endregion


        #region Administer registration requests

        /// <summary>
        /// Gets all registration requests.
        /// </summary>
        /// <returns></returns>
        public List<RegistrationEntry> GetRegistrationEntries()
        {
            if (!this.certificationAuthority.CaCertificate.IsLoaded)
            {
                return new List<RegistrationEntry>();
            }
            return this.certificateDatabase.SelectRegistrationEntries(this.certificationAuthority.CaCertificate.CaX509.SerialNumber);
        }

        /// <summary>
        /// Authorizes the given list of registration requests
        /// </summary>
        /// <param name="regEntries"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public void AuthorizeRegistrationEntries(List<RegistrationEntry> regEntries)
        {
            if (!this.certificationAuthority.CaCertificate.IsLoaded)
            {
                return;
            }
            this.certificateDatabase.UpdateAuthorizationStatus(this.certificationAuthority.CaCertificate.CaX509.SerialNumber, regEntries, true);
            Log.Debug(String.Format("Manually authorized {0} registration requests.", regEntries.Count));

            foreach (RegistrationEntry entry in regEntries)
            {
                if (entry.IsVerified)
                {
                    Configuration.CertificateProgram programConfig = this.config.GetProgramConfiguration(entry.ProgramName);
                    bool emailSent = this.smtpClient.SendAuthorizationGrantedInformation(programConfig, entry);
                    if (!emailSent)
                    {
                        HandleUndeliveredEmail((byte)4, entry.Email);
                    }
                }
            }
        }

        /// <summary>
        /// Rejects the given list of registration requests
        /// </summary>
        /// <param name="avatarNames"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public void RejectRegistrationEntries(List<RegistrationEntry> regEntries)
        {
            if (!this.certificationAuthority.CaCertificate.IsLoaded)
            {
                return;
            }
            this.certificateDatabase.UpdateAuthorizationStatus(this.certificationAuthority.CaCertificate.CaX509.SerialNumber, regEntries, false);
            Log.Debug(String.Format("Manually rejected {0} registration requests.", regEntries.Count));
        }

        /// <summary>
        /// Deletes the given list of registration requests
        /// </summary>
        /// <param name="avatarNames"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DatabaseException"></exception>
        public void DeleteRegistrationEntries(List<RegistrationEntry> regEntries)
        {
            if (!this.certificationAuthority.CaCertificate.IsLoaded)
            {
                return;
            }
            this.certificateDatabase.DeleteRegistrationEntries(this.certificationAuthority.CaCertificate.CaX509.SerialNumber, regEntries);
            Log.Debug(String.Format("Manually deleted {0} registration requests.", regEntries.Count));
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


        #region Private members

        private Configuration config;

        private PolicyConfig policyConfig;

        private ExtensionConfig extensionConfig;

        private CertificationAuthority certificationAuthority;

        private CertificateDatabase certificateDatabase;

        private SMTPEmailClient smtpClient;

        #endregion


        #region Properties

        public bool ManualAuthorization { get; set; }

        public long RegistrationCount { get; private set; }

        public DateTime LastRegistrationRequest { get; private set; }

        #endregion


        #region Events

        public event EventHandler<EventArgs> RegistrationRequestReceived;

        public event EventHandler<EventArgs> RegistrationRequestDeleted;

        #endregion

    }
}
