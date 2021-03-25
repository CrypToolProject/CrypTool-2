using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using CrypTool.Util.Logging;
using System.Xml.Serialization;

namespace CrypTool.CertificateServer
{
    /// <summary>
    /// P@Porator Configuration
    /// </summary>
    [Serializable]
    public class Configuration
    {

        #region File name and path

        private static readonly string FILENAME = "paporator.xml";

        private static readonly string FILEPATH = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar;

        #endregion


        #region Singleton

        private static Configuration config = null;

        public static Configuration GetConfiguration()
        {
            if (config == null)
            {
                if (FileAvailable())
                {
                    try
                    {
                        config = Load();
                        Log.Debug("Configuration file successfully loaded");
                        // Save the config, to add missing values
                        config.Save();
                    }
                    catch (Exception ex)
                    {
                        config = new Configuration();
                        Log.Error("P@Porator XML configuration file is malformed! Using default values", ex);
                    }
                }
                else
                {
                    config = new Configuration();
                    config.Save();
                    Log.Info("No configuration file available. Created a new configuration file with default values.");
                }
                config.CreateDictionary();
            }
            return config;
        }

        #endregion


        #region Management methods

        /// <summary>
        /// Returns true if the configuration file is available.
        /// </summary>
        /// <returns>true if available</returns>
        public static bool FileAvailable()
        {
            if (Directory.Exists(FILEPATH) && File.Exists(FILENAME))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Saves the current configuration.
        /// </summary>
        public void Save()
        {
            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(Configuration));
            StreamWriter writer = File.CreateText(FILEPATH + FILENAME);
            serializer.Serialize(writer, config);
            writer.Flush();
            writer.Close();
        }

        /// <summary>
        /// Loads the configuration file.
        /// </summary>
        /// <returns>The configuration</returns>
        private static Configuration Load()
        {
            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(Configuration));
            StreamReader reader = File.OpenText(FILEPATH + FILENAME);
            Configuration tempConfig = (Configuration)serializer.Deserialize(reader);
            reader.Close();
            return tempConfig;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Certificate Server:").AppendLine();
            foreach (Configuration.CertificateServerConfig.ProtocolConfig protocol in config.CertificateServer.Protocols)
            {
                sb.Append("Protocol:                                ").Append(protocol.Name).AppendLine(); ;
                sb.Append("  Address:                               ").Append(protocol.Address).AppendLine();
                sb.Append("  Port:                                  ");
                sb.Append(protocol.Port[0]);
                for (int i = 1; i < protocol.Port.Length; i++)
                {
                    sb.Append(" | ").Append(protocol.Port[i]);
                }
                sb.AppendLine();
                sb.Append("  AllowedClients:                        ").Append(protocol.AllowedClients).AppendLine();
                sb.Append("  Active:                                ").Append(protocol.Active).AppendLine();
                sb.AppendLine();
            }

            sb.Append("Database:").AppendLine();
            sb.Append("  Host:                                  ").Append(this.Database.Host).AppendLine();
            sb.Append("  Port:                                  ").Append(this.Database.Port).AppendLine();
            sb.Append("  Database:                              ").Append(this.Database.DatabaseName).AppendLine();
            sb.Append("  User:                                  ").Append(this.Database.User).AppendLine();
            sb.Append("  Timeout:                               ").Append(this.Database.Timeout).AppendLine().AppendLine();

            sb.Append("Certification Authority:").AppendLine();
            sb.Append("  Registration removal time in days:     ").Append(this.CertificationAuthority.RegistrationDaysUntilDelete).AppendLine();
            sb.Append("  Password reset removal time in days:   ").Append(this.CertificationAuthority.PasswordResetDaysUntilDelete).AppendLine();
            sb.Append("  CA RSA Strength:                       ").Append(this.CertificationAuthority.CaRsaStrength).AppendLine();
            sb.Append("  Peer RSA Strength:                     ").Append(this.CertificationAuthority.PeerRsaStrength).AppendLine();
            sb.Append("  CA Month till expire:                  ").Append(this.CertificationAuthority.CaMonthTillExpire).AppendLine();
            sb.Append("  Peer Month till expire:                ").Append(this.CertificationAuthority.PeerMonthTillExpire).AppendLine();
            sb.Append("  CA certificate Serialnumber:           ").Append(this.CertificationAuthority.RecentCertificate.Serialnumber).AppendLine().AppendLine();

            sb.Append("SMTP Server Configuration:").AppendLine();
            sb.Append("  Address:                               ").Append(this.SmtpServer.Server).AppendLine();
            sb.Append("  Port:                                  ").Append(this.SmtpServer.Port.ToString()).AppendLine();
            sb.Append("  Anti Spam Protection Time (min):       ").Append(this.SmtpServer.AntiSpamTimeInMinutes.ToString()).AppendLine();

            foreach (CertificateProgram program in this.CertificatePrograms)
            {
                sb.AppendLine();
                sb.Append("Client Program Configuration:            (");
                for (int i = 0; i < program.Name.Length; i++)
                {
                    sb.Append(" ").Append(program.Name[i]).Append(" ");
                    if (i < program.Name.Length - 1)
                    {
                        sb.Append("|");
                    }
                }
                sb.Append(")").AppendLine();
                sb.Append("  Email verification required:           ").Append(program.EmailVerificationRequired).AppendLine();
                sb.Append("  Sender Address:                        ").Append(program.Sender).AppendLine();
                foreach (string regInfo in program.RegistrationPerformed.Recipients)
                {
                    sb.Append("  Send registration information to:      ").Append(regInfo).AppendLine();
                }
                if (program.EmailVerificationRequired)
                {
                    sb.Append("  Send email on requested registration:  ").Append(program.RegistrationRequest.SendInformation).AppendLine();
                }
                sb.Append("  Send email on succeeded registration:  ").Append(program.RegistrationPerformed.SendInformation).AppendLine();
            }
            return sb.ToString();
        }

        /// <summary>
        /// Generates a dictionary containing the configurations for each certificate program. 
        /// This dictionary provides fast access to the responsible configuration.
        /// </summary>
        private void CreateDictionary()
        {
            foreach (CertificateProgram program in CertificatePrograms)
            {
                foreach (string name in program.Name)
                {
                    if (this.programDictionary.ContainsKey(name))
                    {
                        if (name == "default")
                        {
                            Log.Warn("The default program configuration was overwritten. Check your configuration for duplicate/missing program name entries!");
                        }
                        else
                        {
                            Log.WarnFormat("More than one program configuration for program {0} found! Check your program configuration!", name);
                        }
                    }
                    this.programDictionary.Add(name.ToLower(), program);
                }
            }
        }

        private Dictionary<string, CertificateProgram> programDictionary;

        #endregion


        #region General Configuration

        private Configuration()
        {
            this.CertificateServer = new CertificateServerConfig();
            this.CertificationAuthority = new CertificationAuthorityConfig();
            this.Database = new CertificateDatabaseConfig();
            this.SmtpServer = new SmtpServerConfig();
            this.CertificatePrograms = new CertificateProgram[] { new CertificateProgram() };
            this.programDictionary = new Dictionary<string, CertificateProgram>();
        }

        public CertificateServerConfig CertificateServer { get; set; }

        public CertificateDatabaseConfig Database { get; set; }

        public CertificationAuthorityConfig CertificationAuthority { get; set; }

        public SmtpServerConfig SmtpServer { get; set; }

        [XmlArray("CertificatePrograms")]
        [XmlArrayItem("Program")]
        public CertificateProgram[] CertificatePrograms { get; set; }

        #endregion


        #region Certificate Server

        /// <summary>
        /// General server configuration
        /// </summary>
        public class CertificateServerConfig
        {
            public CertificateServerConfig()
            {
                // Read default values here, so they can be overwritten when loading an existing config
                Protocols = new ProtocolConfig[]
                    {
                        new ProtocolConfig() 
                        {
                            Name = "PCP",
                            Address = global::CrypTool.CertificateServer.Properties.Settings.Default.DEFAULT_PCP_LISTEN_ADDRESS,
                            Port = new int[] { 
                                global::CrypTool.CertificateServer.Properties.Settings.Default.DEFAULT_PCP_LISTEN_PORT_1,
                                global::CrypTool.CertificateServer.Properties.Settings.Default.DEFAULT_PCP_LISTEN_PORT_2 },
                            AllowedClients = global::CrypTool.CertificateServer.Properties.Settings.Default.DEFAULT_ALLOWED_CLIENTS,
                            Active = true
                        },
                    };
            }

            public ProtocolConfig[] Protocols { get; set; }

            /// <summary>
            /// Protocol configuration
            /// </summary>
            public class ProtocolConfig
            {
                [XmlAttribute]
                public string Name { get; set; }

                [XmlAttribute]
                public string Address { get; set; }

                [XmlAttribute]
                public uint AllowedClients { get; set; }

                [XmlAttribute]
                public bool Active { get; set; }

                public int[] Port { get; set; }
            }

            /// <summary>
            /// Returns the protocol specific configuration for the given protocol name.
            /// </summary>
            /// <param name="name">The name of the protocol</param>
            /// <returns>The protocol specific configuration</returns>
            public ProtocolConfig GetProtocol(string name)
            {
                foreach (ProtocolConfig protocol in Protocols)
                {
                    if (protocol.Name.Equals(name))
                    {
                        return protocol;
                    }
                }
                return null;
            }
        }

        #endregion


        #region Certificate Database

        public class CertificateDatabaseConfig
        {
            public CertificateDatabaseConfig()
            {
                // Default values
                Host = global::CrypTool.CertificateServer.Properties.Settings.Default.DEFAULT_DB_HOST;
                Port = global::CrypTool.CertificateServer.Properties.Settings.Default.DEFAULT_DB_PORT;
                DatabaseName = global::CrypTool.CertificateServer.Properties.Settings.Default.DEFAULT_DB_NAME;
                User = global::CrypTool.CertificateServer.Properties.Settings.Default.DEFAULT_DB_USER;
                Password = global::CrypTool.CertificateServer.Properties.Settings.Default.DEFAULT_DB_PASSWORD;
                Timeout = global::CrypTool.CertificateServer.Properties.Settings.Default.DEFAULT_DB_TIMEOUT;
            }

            public string Host { get; set; }

            public uint Port { get; set; }

            public string DatabaseName { get; set; }

            public string User { get; set; }

            public string Password { get; set; }

            public uint Timeout  { get; set; }
        }

        #endregion


        #region Certification Authority

        public class CertificationAuthorityConfig
        {
            public CertificationAuthorityConfig()
            {
                // Default values
                CaRsaStrength = global::CrypTool.CertificateServer.Properties.Settings.Default.DEFAULT_CA_RSA_STRENGTH;
                CaMonthTillExpire = global::CrypTool.CertificateServer.Properties.Settings.Default.DEFAULT_CA_MONTH_EXPIRE;
                PeerRsaStrength = global::CrypTool.CertificateServer.Properties.Settings.Default.DEFAULT_PEER_RSA_STRENGTH;
                PeerMonthTillExpire = global::CrypTool.CertificateServer.Properties.Settings.Default.DEFAULT_PEER_MONTH_EXPIRE;
                RegistrationDaysUntilDelete = global::CrypTool.CertificateServer.Properties.Settings.Default.DEFAULT_REGISTRATION_EXPIRE_IN_DAYS;
                PasswordResetDaysUntilDelete = global::CrypTool.CertificateServer.Properties.Settings.Default.DEFAULT_PASSWORD_RESET_EXPIRE_IN_DAYS;

                RecentCertificate = new RecentCertificateConfig();
            }

            public int CaRsaStrength { get; set; }

            public int CaMonthTillExpire { get; set; }

            public int PeerRsaStrength { get; set; }

            public int PeerMonthTillExpire { get; set; }

            public uint RegistrationDaysUntilDelete { get; set; }

            public uint PasswordResetDaysUntilDelete { get; set; }

            public RecentCertificateConfig RecentCertificate { get; set; }


            public class RecentCertificateConfig
            {
                public RecentCertificateConfig()
                {
                    // Default values
                    Serialnumber = String.Empty;
                    Password = String.Empty;
                }

                public string Serialnumber { get; set; }

                public string Password { get; set; }
            }
        }

        #endregion


        #region SMTP Server

        public class SmtpServerConfig
        {
            public SmtpServerConfig()
            {
                // Default values
                Server = global::CrypTool.CertificateServer.Properties.Settings.Default.DEFAULT_SMTP_SERVER_ADDRESS;
                Port = global::CrypTool.CertificateServer.Properties.Settings.Default.DEFAULT_SMTP_SERVER_PORT;
                AntiSpamTimeInMinutes = global::CrypTool.CertificateServer.Properties.Settings.Default.DEFAULT_SMTP_ANTI_SPAM_TIME_IN_MIN;
            }

            public string Server { get; set; }

            public int Port { get; set; }

            public uint AntiSpamTimeInMinutes { get; set; }
        }

        #endregion


        #region Get specific program configuration

        /// <summary>
        /// Gives back the relevant configuration for the given program name. If there exists no configuration, 
        /// the default configuration will be returned.
        /// </summary>
        /// <param name="programName">The name of the program which was used</param>
        /// <returns>The relevant configuration</returns>
        public CertificateProgram GetProgramConfiguration(string programName)
        {
            programName = programName ?? "default";
            programName = programName.ToLower();
            if (!this.programDictionary.ContainsKey(programName))
            {
                if (!this.programDictionary.ContainsKey("default"))
                {
                    return new CertificateProgram();
                }
                return this.programDictionary["default"];
            }
            return this.programDictionary[programName];
        }

        #endregion


        #region Program Configuration

        /// <summary>
        /// This class represents the individual configuration for each program.
        /// It can be generally be used to define different email messages for each used program, but also enables
        /// to switch email verification on/off for the different programs.
        /// A configuration can be applied to more than one program by defining a list of names.
        /// </summary>
        public class CertificateProgram
        {
            public CertificateProgram()
            {
                Name = new string[] { "default" };
                EmailVerificationRequired = global::CrypTool.CertificateServer.Properties.Settings.Default.DEFAULT_EMAIL_VERIFICATION_ENABLED;

                Sender = global::CrypTool.CertificateServer.Properties.Settings.Default.DEFAULT_EMAIL_SENDER;

                RegistrationRequest = new RequestInfo();
                RegistrationPerformed = new RegistInfo();
                AuthorizationGranted = new AuthorizationGrantedInfo();
                EmailVerificationCode = new EmailCode();
                PasswordResetCode = new PassCode();
            }

            [XmlAttribute]
            public string[] Name { get; set; }

            [XmlAttribute]
            public bool EmailVerificationRequired { get; set; }

            public string Sender { get; set; }

            public RequestInfo RegistrationRequest { get; set; }

            public RegistInfo RegistrationPerformed { get; set; }

            public AuthorizationGrantedInfo AuthorizationGranted { get; set; }

            public EmailCode EmailVerificationCode { get; set; }

            public PassCode PasswordResetCode { get; set; }


            public class RequestInfo : AbstractEmail
            {
                public RequestInfo()
                {
                    // Default values
                    SendInformation = global::CrypTool.CertificateServer.Properties.Settings.Default.DEFAULT_EMAIL_REQUEST_SEND_INFO;
                    Recipients = new string[] { global::CrypTool.CertificateServer.Properties.Settings.Default.DEFAULT_EMAIL_REQUEST_INFO_ADDRESS};
                    Subject = global::CrypTool.CertificateServer.Properties.Settings.Default.DEFAULT_EMAIL_REQUEST_INFO_SUBJECT;
                    Body = global::CrypTool.CertificateServer.Properties.Settings.Default.DEFAULT_EMAIL_REQUEST_INFO_BODY;
                }

                [XmlAttribute]
                public bool SendInformation { get; set; }

                public string[] Recipients { get; set; }
            }

            public class RegistInfo : AbstractEmail
            {
                public RegistInfo()
                {
                    // Default values
                    SendInformation = global::CrypTool.CertificateServer.Properties.Settings.Default.DEFAULT_EMAIL_REGISTER_SEND_INFO;
                    Recipients = new string[] { global::CrypTool.CertificateServer.Properties.Settings.Default.DEFAULT_EMAIL_REGISTER_INFO_ADDRESS };
                    Subject = global::CrypTool.CertificateServer.Properties.Settings.Default.DEFAULT_EMAIL_REGISTRATION_INFO_SUBJECT;
                    Body = global::CrypTool.CertificateServer.Properties.Settings.Default.DEFAULT_EMAIL_REGISTRATION_INFO_BODY;
                }

                [XmlAttribute]
                public bool SendInformation { get; set; }

                public string[] Recipients { get; set; }
            }

            public class AuthorizationGrantedInfo : AbstractEmail
            {
                public AuthorizationGrantedInfo()
                {
                    // Default values
                    Subject = global::CrypTool.CertificateServer.Properties.Settings.Default.DEFAULT_EMAIL_AUTHORIZATION_SUBJECT;
                    Body = global::CrypTool.CertificateServer.Properties.Settings.Default.DEFAULT_EMAIL_AUTHORIZATION_BODY;
                }
            }

            public class EmailCode : AbstractEmail
            {
                public EmailCode()
                {
                    // Default values
                    Subject = global::CrypTool.CertificateServer.Properties.Settings.Default.DEFAULT_EMAIL_REGISTRATION_SUBJECT;
                    Body = global::CrypTool.CertificateServer.Properties.Settings.Default.DEFAULT_EMAIL_REGISTRATION_BODY;
                }
            }

            public class PassCode : AbstractEmail
            {
                public PassCode()
                {
                    // Default values
                    Subject = global::CrypTool.CertificateServer.Properties.Settings.Default.DEFAULT_EMAIL_PASSWORD_RESET_SUBJECT;
                    Body = global::CrypTool.CertificateServer.Properties.Settings.Default.DEFAULT_EMAIL_PASSWORD_RESET_BODY;
                }
            }

            public abstract class AbstractEmail
            {
                public string Subject { get; set; }

                public string Body { get; set; }
            }
        }

        #endregion

    }
}
