using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Net;
using CrypTool.Util.Logging;
using System.Collections;
using System.Collections.Concurrent;

namespace CrypTool.CertificateServer
{
    public class SMTPEmailClient
    {

        #region String Replacing

        public static readonly string REPLACE_STRING_AVATAR = "{AVATAR}";

        public static readonly string REPLACE_STRING_EMAIL = "{EMAIL}";

        public static readonly string REPLACE_STRING_WORLD = "{WORLD}";

        public static readonly string REPLACE_STRING_CODE = "{CODE}";

        public static readonly string REPLACE_STRING_REQUEST_DATE = "{REQUEST_DATE}";

        public static readonly string REPLACE_STRING_EXPIRATION_DATE = "{EXPIRATION_DATE}";

        public static readonly string REPLACE_STRING_DAYS_TILL_EXPIRATION = "{DAYS_TILL_EXPIRATION}";

        public static readonly string REPLACE_STRING_PROGRAM = "{PROGRAM_NAME}";

        public static readonly string REPLACE_STRING_PROGRAM_VER = "{PROGRAM_VERSION}";

        public static readonly string REPLACE_STRING_NEW_LINE = "\\n";

        #endregion


        #region Constructor

        /// <summary>
        /// Creates a new SMTPEmailClient for sending emails
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public SMTPEmailClient(string host, int port)
        {
            if (String.IsNullOrEmpty(host))
            {
                throw new ArgumentNullException("host can not be null or an empty string");
            }
            if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
            {
                throw new ArgumentException("port number can not be less " + IPEndPoint.MinPort + " or greater than " + IPEndPoint.MaxPort);
            }
            this.config = Configuration.GetConfiguration();
            this.SmtpClient = new SmtpClient(host, port);

            this.emailVerificationAntiSpamDict = new ConcurrentDictionary<string, DateTime>();
            this.passwordResetAntiSpamDict = new ConcurrentDictionary<string, DateTime>();
            this.AntiSpamTime = this.config.SmtpServer.AntiSpamTimeInMinutes;
        }

        #endregion


        #region Mail Sending

        /// <summary>
        /// Sends an email "from" to "to" with "subject" containing "body". Supports Carbon Copy.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="cc"></param>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        /// <exception cref="System.FormatException"></exception>
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.Net.Mail.SmtpException"></exception>
        /// <exception cref="System.Net.Mail.SmtpFailedRecepientsException"></exception>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <exception cref="System.ObjectDisposedException"></exception>
        public void SendEmail(string from, string to, string subject, string body, string[] cc = null)
        {
            MailMessage mail = new MailMessage(from, to,subject,body);
            if (cc != null && cc.Length > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(cc[0]);
                for (int i = 1; i < cc.Length; i++)
                {
                    sb.Append(",").Append(cc[i]);
                }
                mail.CC.Add(sb.ToString());
            }
            SmtpClient.Send(mail);
        }

        #endregion


        #region Email + Anti Spam

        /// <summary>
        /// Sends a registration request information email to the email addresses specified in the configuration.
        /// </summary>
        /// <param name="client">The client that was used to request the registration (can also be null)</param>
        /// <param name="programConfig">The configuration responsible for the registration request</param>
        /// <param name="entry">An Entry object that represents the registration or certificate entry</param>
        /// <returns>true if the email was sent correctly</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool SendRegistrationRequestInformation(ClientInfo client, Configuration.CertificateProgram programConfig, Entry entry)
        {
            if (programConfig == null)
            {
                throw new ArgumentNullException("programConfig", "CertificateProgram can not be null");
            }
            if (entry == null)
            {
                throw new ArgumentNullException("entry", "Entry can not be null");
            }

            try
            {
                if (programConfig.RegistrationRequest.Recipients.Length == 0)
                {
                    return true;
                }

                StringBuilder newSubject = new StringBuilder(programConfig.RegistrationRequest.Subject);
                newSubject.Replace(SMTPEmailClient.REPLACE_STRING_AVATAR, entry.Avatar);

                StringBuilder newBody = new StringBuilder(programConfig.RegistrationRequest.Body);
                newBody.Replace(SMTPEmailClient.REPLACE_STRING_AVATAR, entry.Avatar);
                newBody.Replace(SMTPEmailClient.REPLACE_STRING_EMAIL, entry.Email);
                newBody.Replace(SMTPEmailClient.REPLACE_STRING_WORLD, entry.World);
                newBody.Replace(SMTPEmailClient.REPLACE_STRING_PROGRAM, entry.ProgramName);
                newBody.Replace(SMTPEmailClient.REPLACE_STRING_PROGRAM_VER, entry.ProgramVersion);
                newBody.Replace(SMTPEmailClient.REPLACE_STRING_NEW_LINE, Environment.NewLine);
                string[] cc = null;
                if (programConfig.RegistrationRequest.Recipients.Length > 1)
                {
                    cc = new string[programConfig.RegistrationRequest.Recipients.Length - 1];
                    for (int i = 1; i < programConfig.RegistrationRequest.Recipients.Length; i++)
                    {
                        cc[i - 1] = programConfig.RegistrationRequest.Recipients[i];
                    }
                }

                this.SendEmail(programConfig.Sender, programConfig.RegistrationRequest.Recipients[0], newSubject.ToString(), newBody.ToString(), cc);
                this.emailVerificationAntiSpamDict.TryAdd(entry.Email.ToLower(), DateTime.Now);
                Log.Debug(Logging.GetLogEnvelope("Successfully sent a registration request information.", client, entry));
                return true;
            }
            catch (Exception ex)
            {
                Log.Warn(Logging.GetLogEnvelope("Could not send registration request information.", client, entry), ex);
                return false;
            }
        }

        /// <summary>
        /// Sends a registration information email to the email addresses specified in the configuration.
        /// </summary>
        /// <param name="client">The client that was used to perform the registration (can also be null)</param>
        /// <param name="programConfig">The configuration responsible for the registration</param>
        /// <param name="entry">An Entry object that represents the registration or certificate entry</param>
        /// <returns>true if the email was sent correctly</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool SendRegistrationPerformedInformation(ClientInfo client, Configuration.CertificateProgram programConfig, Entry entry)
        {
            if (programConfig == null)
            {
                throw new ArgumentNullException("programConfig", "CertificateProgram can not be null");
            }
            if (entry == null)
            {
                throw new ArgumentNullException("entry", "Entry can not be null");
            }

            try
            {
                if (programConfig.RegistrationPerformed.Recipients.Length == 0)
                {
                    return true;
                }

                StringBuilder newSubject = new StringBuilder(programConfig.RegistrationPerformed.Subject);
                newSubject.Replace(SMTPEmailClient.REPLACE_STRING_AVATAR, entry.Avatar);

                StringBuilder newBody = new StringBuilder(programConfig.RegistrationPerformed.Body);
                newBody.Replace(SMTPEmailClient.REPLACE_STRING_AVATAR, entry.Avatar);
                newBody.Replace(SMTPEmailClient.REPLACE_STRING_EMAIL, entry.Email);
                newBody.Replace(SMTPEmailClient.REPLACE_STRING_WORLD, entry.World);
                newBody.Replace(SMTPEmailClient.REPLACE_STRING_PROGRAM, entry.ProgramName);
                newBody.Replace(SMTPEmailClient.REPLACE_STRING_PROGRAM_VER, entry.ProgramVersion);
                newBody.Replace(SMTPEmailClient.REPLACE_STRING_NEW_LINE, Environment.NewLine);
                string[] cc = null;
                if (programConfig.RegistrationPerformed.Recipients.Length > 1)
                {
                    cc = new string[programConfig.RegistrationPerformed.Recipients.Length - 1];
                    for (int i = 1; i < programConfig.RegistrationPerformed.Recipients.Length; i++)
                    {
                        cc[i - 1] = programConfig.RegistrationPerformed.Recipients[i];
                    }
                }

                this.SendEmail(programConfig.Sender, programConfig.RegistrationPerformed.Recipients[0], newSubject.ToString(), newBody.ToString(), cc);
                Log.Debug(Logging.GetLogEnvelope("Successfully sent a certificate registration information.", client, entry));
                return true;
            }
            catch (Exception ex)
            {
                Log.Warn(Logging.GetLogEnvelope("Could not send certificate registration information.", client, entry), ex);
                return false;
            }
        }

        /// <summary>
        /// Sends an information email to the client that the registration has been authorized.
        /// </summary>
        /// <param name="programConfig">The configuration responsible for the registration</param>
        /// <param name="entry">An Entry object that represents the registration entry</param>
        /// <returns>true if the email was sent correctly</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool SendAuthorizationGrantedInformation(Configuration.CertificateProgram programConfig, Entry entry)
        {
            if (programConfig == null)
            {
                throw new ArgumentNullException("programConfig", "CertificateProgram can not be null");
            }
            if (entry == null)
            {
                throw new ArgumentNullException("entry", "Entry can not be null");
            }

            try
            {
                StringBuilder newSubject = new StringBuilder(programConfig.AuthorizationGranted.Subject);
                newSubject.Replace(SMTPEmailClient.REPLACE_STRING_AVATAR, entry.Avatar);

                StringBuilder newBody = new StringBuilder(programConfig.AuthorizationGranted.Body);
                newBody.Replace(SMTPEmailClient.REPLACE_STRING_AVATAR, entry.Avatar);
                newBody.Replace(SMTPEmailClient.REPLACE_STRING_EMAIL, entry.Email);
                newBody.Replace(SMTPEmailClient.REPLACE_STRING_NEW_LINE, Environment.NewLine);

                this.SendEmail(programConfig.Sender, entry.Email, newSubject.ToString(), newBody.ToString());
                Log.Debug(Logging.GetLogEnvelope("Successfully sent an authorization granted information.", null, entry));
                return true;
            }
            catch (Exception ex)
            {
                Log.Warn(Logging.GetLogEnvelope("Could not send an authorization granted information.", null, entry), ex);
                return false;
            }
        }

        /// <summary>
        /// Sends the email verification code per email.
        /// </summary>
        /// <param name="regEntry">A RegistrationEntry object</param>
        /// <returns>true if the email was sent correctly</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool SendEmailVerificationCode(ClientInfo client, Configuration.CertificateProgram programConfig, RegistrationEntry regEntry)
        {
            if (programConfig == null)
            {
                throw new ArgumentNullException("programConfig", "CertificateProgram can not be null");
            }
            if (regEntry == null)
            {
                throw new ArgumentNullException("regEntry", "RegistrationEntry can not be null");
            }

            // Send verifiation code - Anti spam detection
            DateTime dateOfLastSend;
            if (this.emailVerificationAntiSpamDict.TryGetValue(regEntry.Email.ToLower(), out dateOfLastSend))
            {
                if (DateTime.Compare(dateOfLastSend.AddMinutes((double)this.AntiSpamTime), DateTime.Now) > 0)
                {
                    Log.Info(Logging.GetLogEnvelope("Canceled sending an email verification code, to avoid spaming.", client, regEntry));
                    return true;
                }
            }

            try
            {
                StringBuilder newSubject = new StringBuilder(programConfig.EmailVerificationCode.Subject);
                newSubject.Replace(SMTPEmailClient.REPLACE_STRING_AVATAR, regEntry.Avatar);

                StringBuilder newBody = new StringBuilder(programConfig.EmailVerificationCode.Body);
                newBody.Replace(SMTPEmailClient.REPLACE_STRING_AVATAR, regEntry.Avatar);
                newBody.Replace(SMTPEmailClient.REPLACE_STRING_EMAIL, regEntry.Email);
                newBody.Replace(SMTPEmailClient.REPLACE_STRING_CODE, regEntry.EmailCode.ToString());
                newBody.Replace(SMTPEmailClient.REPLACE_STRING_REQUEST_DATE, regEntry.DateOfRequest.ToString());
                newBody.Replace(SMTPEmailClient.REPLACE_STRING_EXPIRATION_DATE, regEntry.DateOfRequest.AddDays((double)config.CertificationAuthority.RegistrationDaysUntilDelete).ToString());
                newBody.Replace(SMTPEmailClient.REPLACE_STRING_DAYS_TILL_EXPIRATION, config.CertificationAuthority.RegistrationDaysUntilDelete.ToString());
                newBody.Replace(SMTPEmailClient.REPLACE_STRING_NEW_LINE, Environment.NewLine);

                this.SendEmail(programConfig.Sender, regEntry.Email, newSubject.ToString(), newBody.ToString());
                this.emailVerificationAntiSpamDict.TryAdd(regEntry.Email.ToLower(), DateTime.Now);
                Log.Debug(Logging.GetLogEnvelope("Successfully send an email verification code.", client, regEntry));
                return true;
            }
            catch (Exception ex)
            {
                Log.Warn(Logging.GetLogEnvelope("Could not send email verification code.", client, regEntry), ex);
                return false;
            }
        }

        /// <summary>
        /// Sends the password reset code per email.
        /// </summary>
        /// <param name="certEntry">A PeerEntry object</param>
        /// <returns>true if the email was sent correctly</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool SendPasswordResetCode(ClientInfo client, Configuration.CertificateProgram programConfig, CertificateEntry certEntry)
        {
            if (programConfig == null)
            {
                throw new ArgumentNullException("programConfig", "CertificataProgram can not be null");
            }
            if (certEntry == null)
            {
                throw new ArgumentNullException("peerEntry", "PeerEntry can not be null");
            }

            // Anti spam protection
            DateTime dateOfLastSend;
            if (passwordResetAntiSpamDict.TryGetValue(certEntry.Email.ToLower(), out dateOfLastSend))
            {
                if (DateTime.Compare(dateOfLastSend.AddMinutes((double)AntiSpamTime), DateTime.Now) > 0)
                {
                    Log.Warn(Logging.GetLogEnvelope("Canceled sending a password reset verification code, to avoid spaming.", client, certEntry));
                    return true;
                }
            }

            try
            {
                StringBuilder newSubject = new StringBuilder(programConfig.PasswordResetCode.Subject);
                newSubject.Replace(SMTPEmailClient.REPLACE_STRING_AVATAR, certEntry.Avatar);

                StringBuilder newBody = new StringBuilder(programConfig.PasswordResetCode.Body);
                newBody.Replace(SMTPEmailClient.REPLACE_STRING_AVATAR, certEntry.Avatar);
                newBody.Replace(SMTPEmailClient.REPLACE_STRING_EMAIL, certEntry.Email);
                newBody.Replace(SMTPEmailClient.REPLACE_STRING_CODE, certEntry.PasswordCode.ToString());
                newBody.Replace(SMTPEmailClient.REPLACE_STRING_REQUEST_DATE, certEntry.DateOfPasswordReset.ToString());
                newBody.Replace(SMTPEmailClient.REPLACE_STRING_EXPIRATION_DATE, certEntry.DateOfPasswordReset.AddDays((double)config.CertificationAuthority.PasswordResetDaysUntilDelete).ToString());
                newBody.Replace(SMTPEmailClient.REPLACE_STRING_DAYS_TILL_EXPIRATION, config.CertificationAuthority.PasswordResetDaysUntilDelete.ToString());
                newBody.Replace(SMTPEmailClient.REPLACE_STRING_NEW_LINE, Environment.NewLine);

                this.SendEmail(programConfig.Sender, certEntry.Email, newSubject.ToString(), newBody.ToString());
                this.passwordResetAntiSpamDict.TryAdd(certEntry.Email.ToLower(), DateTime.Now);
                Log.Debug(Logging.GetLogEnvelope("Successfully sent password reset code.", client, certEntry));
                return true;
            }
            catch (Exception ex)
            {
                Log.Warn(Logging.GetLogEnvelope("Could not send password reset code.", client, certEntry), ex);
                return false;
            }
        }

        public void CleanEmailVerificationAntiList()
        {
            this.CleanAntiSpamDictionary(this.emailVerificationAntiSpamDict);
        }

        public void CleanPasswordResetAntiSpamList()
        {
            this.CleanAntiSpamDictionary(this.passwordResetAntiSpamDict);
        }

        private void CleanAntiSpamDictionary(ConcurrentDictionary<string, DateTime> dictionary)
        {
            List<string> removalList = new List<string>();
            IEnumerator enumerator = dictionary.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<string, DateTime> pair = (KeyValuePair<string, DateTime>)enumerator.Current;
                if (DateTime.Compare(pair.Value.AddMinutes((double)this.AntiSpamTime), DateTime.Now) < 0)
                {
                    removalList.Add(pair.Key);
                }
            }
            foreach (string key in removalList)
            {
                DateTime value;
                dictionary.TryRemove(key, out value);
            }
            removalList.Clear();
        }

        #endregion


        #region Private Members

        private SmtpClient SmtpClient { get; set; }

        private Configuration config;

        private ConcurrentDictionary<string, DateTime> emailVerificationAntiSpamDict;

        private ConcurrentDictionary<string, DateTime> passwordResetAntiSpamDict;

        #endregion


        #region Properties

        /// <summary>
        /// Timespan in minutes while verification codes won't be send again.
        /// </summary>
        public uint AntiSpamTime { get; set; }

        #endregion

    }
}
