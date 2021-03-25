using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Utilities.Encoders;
using CrypTool.CertificateLibrary.Network;
using System.Security.Cryptography.X509Certificates;
using CrypTool.CertificateLibrary;
using CrypTool.CertificateLibrary.Certificates;
using System.Collections;
using CrypTool.Util.Logging;

namespace CrypTool.CertificateServer
{
    public class CertificateServer
    {

        #region Supported protocols

        public enum Protocol
        {
            /// <summary>
            /// Paporator Communication Protocol (default)
            /// </summary>
            PCP,

            /// <summary>
            /// SSL secured HyperText Transport Protocol 
            /// </summary>
            Https
        }

        #endregion


        #region Fallback values (if settings are invalid)

        private const int FALLBACK_LISTEN_PORT = 10443;

        private const int FALLBACK_LISTEN_HTTP_PORT = 8080;

        private const int FALLBACK_ALLOWED_CLIENTS = 10;

        #endregion


        #region Constructors

        public CertificateServer(CertificateDatabase db, CertificationAuthority ca, RegistrationAuthority ra, DirectoryServer ds, SMTPEmailClient smtpClient)
        {
            this.IsRunning = false;
            this.shutdownRequested = false;
            this.certificationAuthority = ca;
            this.registrationAuthority = ra;
            this.directoryServer = ds;
            this.certificateDatabase = db;
            this.certificateDatabase.ConnectionSuccesfullyEstablished += new EventHandler<EventArgs>(OnDatabaseConnected);
            this.certificateDatabase.ConnectionBroken += new EventHandler<DatabaseErrorEventArgs>(OnDatabaseBroken);
            this.smtpClient = smtpClient;

            this.config = Configuration.GetConfiguration();
            this.pcpConfig = config.CertificateServer.GetProtocol("PCP");
            this.httpsConfig = config.CertificateServer.GetProtocol("HTTPS");

            this.activeProtocols = new List<IProtocol>();
            this.clientList = new List<ClientHandler>();

            // Perform regular work every 3 hours
            timer = new System.Timers.Timer((double)3 * 60 * 60 * 1000);
            timer.Elapsed += new System.Timers.ElapsedEventHandler(PerformRegularWork);
            timer.AutoReset = true;
            timer.Enabled = true;
        }

        #endregion


        #region Start, stop and shutdown the server

        /// <summary>
        /// Starts the server
        /// </summary>
        /// <exception cref="CertificateException">Certificates not properly loaded or not valid</exception>
        /// <exception cref="DatabaseException">No connection to the database</exception>
        public void Start()
        {
            if (certificationAuthority.CaCertificate == null || !certificationAuthority.CaCertificate.IsLoaded)
            {
                string msg = "The certification authority has no CA certificate loaded.";
                Log.Error(msg);
                throw new CertificateException(msg);
            }
            if (certificationAuthority.TlsCertificate == null || !certificationAuthority.TlsCertificate.IsLoaded)
            {
                string msg = "The server has no TLS certificate loaded.\nPlease load or generate one under File -> TLS Certificate -> Import/Derive";
                Log.Error(msg);
                throw new CertificateException(msg);
            }
            if(!certificationAuthority.CaCertificate.CaX509.IsValidNow || !certificationAuthority.TlsCertificate.PeerX509.IsValidNow)
            {
                string msg = String.Format("The CA and TLS certificate combination is not valid. Please check the validity timespans:\nCA Certificate: {0} - {1}\nTLS Certificate: {2} - {3}",
                    certificationAuthority.CaCertificate.CaX509.NotBefore,
                    certificationAuthority.CaCertificate.CaX509.NotAfter,
                    certificationAuthority.TlsCertificate.PeerX509.NotBefore,
                    certificationAuthority.TlsCertificate.PeerX509.NotAfter);
                Log.Error(msg);
                throw new CertificateException(msg);
            }

            bool started = false;
            if (pcpConfig != null && pcpConfig.Active)
            {
                foreach(int port in pcpConfig.Port)
                {
                    Thread thread = new Thread(new ParameterizedThreadStart(StartListenPcp));
                    thread.IsBackground = true;
                    thread.Start(new string[] { pcpConfig.Address, port.ToString(), pcpConfig.AllowedClients.ToString() });
                }
                started = true;
            }

            if (started)
            {
                this.IsRunning = true;
                timer.Start();                
            }
            else
            {
                Log.Error("Server is not running, because no protocol has been activated.");
            }
        }

        /// <summary>
        /// Same as Stop() but doesnt not send StatusChanged event
        /// </summary>
        public void Shutdown()
        {
            this.shutdownRequested = true;
            Stop();
        }

        /// <summary>
        /// Stop the server, close open sockets and close database connection
        /// </summary>
        public void Stop()
        {
            if (!IsRunning)
            {
                return;
            }
            timer.Stop();

            // Shutdown the protocols
            foreach (IProtocol protocol in this.activeProtocols)
            {
                protocol.StopListen();
            }
            this.activeProtocols.Clear();

            // Disconnect from clients
            foreach (ClientHandler handler in clientList)
            {
                handler.ForceDisconnect();
            }
            this.clientList.Clear();
            this.IsRunning = false;
         }

        #endregion


        #region Methods to handle Paporator Communication Protocol (PCP)

        private void StartListenPcp(object o)
        {
            try
            {
                string[] objArray = (string[])o;
                string address = objArray[0];
                int port = Int32.Parse(objArray[1]);
                int allowedClients = Int32.Parse(objArray[2]);

                X509Certificate2 tlsCert = new X509Certificate2(this.certificationAuthority.TlsCertificate.GetPkcs12(), this.certificationAuthority.TlsCertificate.Password);
                PCProtocol protocol = new PCProtocol(tlsCert);
                protocol.ClientConnected += new EventHandler<ProtocolEventArgs>(OnClientConnected);

                if (StatusChanged != null)
                {
                    this.StatusChanged.Invoke(this, new ServerStatusChangeEventArgs(true));
                }

                Log.Info(String.Format("P@Porator starts handling PCP clients on socket {0}:{1}", address, port));
                this.activeProtocols.Add(protocol);
                protocol.Listen(address, port, allowedClients);

            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
            finally
            {
                if (StatusChanged != null && !shutdownRequested)
                {
                    this.StatusChanged.Invoke(this, new ServerStatusChangeEventArgs(false));
                }
                Log.Info("P@Porator stops handling PCP clients");
            }
        }

        #endregion


        #region Performing regular work

        private void PerformRegularWork(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                this.ResendUndeliveredEmails();
            }
            catch (Exception ex)
            {
                Log.Error("Error while resending undelivered emails!", ex);
            }

            try
            {
                this.smtpClient.CleanEmailVerificationAntiList();
                this.smtpClient.CleanPasswordResetAntiSpamList();
            }
            catch (Exception ex)
            {
                Log.Error("Could not clean the anti spam memory", ex);
            }

            try
            {
                // Delete expired registration and password reset requests. 
                int registrationsDeleted = this.certificateDatabase.DeleteOldRegistrationEntries(this.certificationAuthority.CaCertificate.CaX509.SerialNumber, (int)config.CertificationAuthority.RegistrationDaysUntilDelete);
                int passwordResetsDeleted = this.certificateDatabase.DeleteOldPasswordReset(this.certificationAuthority.CaCertificate.CaX509.SerialNumber, (int)config.CertificationAuthority.PasswordResetDaysUntilDelete);
                if (registrationsDeleted + passwordResetsDeleted > 0)
                {
                    Log.Debug(String.Format("Successfully cleaned {0} old registration and {1} password reset entries out of the datebase!", registrationsDeleted, passwordResetsDeleted));
                }
            }
            catch (Exception ex)
            {
                Log.Warn("Could not clean expired registration and password reset entries!", ex);
            }
        }

        private void ResendUndeliveredEmails()
        {
            List<UndeliveredEmailEntry> undeliveredEmails = this.certificateDatabase.SelectUndeliveredEmailEntries(this.certificationAuthority.CaCertificate.CaX509.SerialNumber);
            if (undeliveredEmails.Count == 0)
            {
                return;
            }

            List<uint> deliveredEmails = new List<uint>();
            List<uint> removedEntries = new List<uint>();
            foreach (UndeliveredEmailEntry email in undeliveredEmails)
            {
                try
                {
                    Entry entry = null;
                    switch (email.Type)
                    {
                        // Email Verification Code
                        case 0:
                             entry  = this.certificateDatabase.SelectRegistrationEntry(this.certificationAuthority.CaCertificate.CaX509.SerialNumber, email.Email, true);
                            if (entry != null)
                            {
                                // Read the relevant configuration
                                Configuration.CertificateProgram programConfig = config.GetProgramConfiguration(entry.ProgramName);
                                bool emailSent = this.smtpClient.SendEmailVerificationCode(null, programConfig, (RegistrationEntry)entry);
                                if (!emailSent)
                                {
                                    throw new Exception("Stopped sending undelivered emails, as the SMTP server still seems to be down!");
                                }
                                deliveredEmails.Add(email.Index);
                            }
                            else
                            {
                                removedEntries.Add(email.Index);
                            }
                            break;

                        // Password Reset Code
                        case 1:
                            entry = this.certificateDatabase.SelectPeerEntry(this.certificationAuthority.CaCertificate.CaX509.SerialNumber, email.Email, true);
                            if (entry != null)
                            {
                                // Read the relevant configuration
                                Configuration.CertificateProgram programConfig = config.GetProgramConfiguration(entry.ProgramName);
                                bool emailSent = this.smtpClient.SendPasswordResetCode(null, programConfig, (CertificateEntry)entry);
                                if (!emailSent)
                                {
                                    throw new Exception("Stopped sending undelivered emails, as the SMTP server still seems to be down!");
                                }
                                deliveredEmails.Add(email.Index);
                            }
                            else
                            {
                                removedEntries.Add(email.Index);
                            }
                            break;

                        // Registration Request
                        case 2:
                            entry = this.certificateDatabase.SelectRegistrationEntry(this.certificationAuthority.CaCertificate.CaX509.SerialNumber, email.Email, true);
                            if (entry != null)
                            {
                                // Read the relevant configuration
                                Configuration.CertificateProgram programConfig = config.GetProgramConfiguration(entry.ProgramName);
                                bool emailSent = this.smtpClient.SendRegistrationRequestInformation(null, programConfig, entry);
                                if (!emailSent)
                                {
                                    throw new Exception("Stopped sending undelivered emails, as the SMTP server still seems to be down!");
                                }
                                deliveredEmails.Add(email.Index);
                            }
                            else
                            {
                                removedEntries.Add(email.Index);
                            }
                            break;

                        // Registration Performed
                        case 3:
                            entry = this.certificateDatabase.SelectPeerEntry(this.certificationAuthority.CaCertificate.CaX509.SerialNumber, email.Email, true);
                            if (entry != null)
                            {
                                // Read the relevant configuration
                                Configuration.CertificateProgram programConfig = config.GetProgramConfiguration(entry.ProgramName);
                                bool emailSent = this.smtpClient.SendRegistrationPerformedInformation(null, programConfig, entry);
                                if (!emailSent)
                                {
                                    throw new Exception("Stopped sending undelivered emails, as the SMTP server still seems to be down!");
                                }
                                deliveredEmails.Add(email.Index);
                            }
                            else
                            {
                                removedEntries.Add(email.Index);
                            }
                            break;

                        // Authorization Granted
                        case 4:
                            entry = this.certificateDatabase.SelectRegistrationEntry(this.certificationAuthority.CaCertificate.CaX509.SerialNumber, email.Email, true);
                            if (entry != null)
                            {
                                // Read the relevant configuration
                                Configuration.CertificateProgram programConfig = config.GetProgramConfiguration(entry.ProgramName);
                                bool emailSent = this.smtpClient.SendAuthorizationGrantedInformation(programConfig, entry);
                                if (!emailSent)
                                {
                                    throw new Exception("Stopped sending undelivered emails, as the SMTP server still seems to be down!");
                                }
                                deliveredEmails.Add(email.Index);
                            }
                            else
                            {
                                removedEntries.Add(email.Index);
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    // SMTP server still down, so stop
                    Log.Warn(ex.Message);
                    break;
                }
            }

            if (deliveredEmails.Count > 0)
            {
                this.certificateDatabase.DeleteUndeliveredEmailEntries(deliveredEmails);
                Log.Debug(String.Format("Successfully resend {0} undelivered emails!", deliveredEmails.Count));
            }
            if (removedEntries.Count > 0)
            {
                // Remove emails, that have been performed or are outdated
                this.certificateDatabase.DeleteUndeliveredEmailEntries(removedEntries);
                Log.Debug(String.Format("Successfully removed {0} outdated emails!", removedEntries.Count));
            }
        }

        #endregion


        #region HTTPS methods (mostly obsolete)

        //private void StartListenHttps()
        //{
        //    try
        //    {
        //        string prefix = "http://" + pcpConfig.Address + ":" + this.pcpConfig.Port + "/";
        //        this.httpListener = new HttpListener();
        //        httpListener.Prefixes.Add(prefix);
        //        Log.Debug("Server (HTTP) is configured to listen on " + prefix);

        //        if (HttpStatusChanged != null && !shutdownRequested)
        //        {
        //            this.HttpStatusChanged.Invoke(this, new ServerStatusChangeEventArgs(false));
        //        }

        //        httpListener.Start();
        //        Log.Info("P@Porator (HTTP) successfully started");

        //        while (!shutdownRequested)
        //        {
        //            // Waiting for clients to connect                    
        //            HandleClientHttps(httpListener.GetContext());
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error(ex);
        //    }
        //    finally
        //    {
        //        if (HttpStatusChanged != null && !shutdownRequested)
        //        {
        //            this.HttpStatusChanged.Invoke(this, new ServerStatusChangeEventArgs(false));
        //        }
        //        Log.Info("P@Porator (HTTP) stopped");
        //    }
        //}

        //private void HandleClientHttps(HttpListenerContext context)
        //{
        //    try
        //    {

        //        X509Certificate2 tlsCert = new X509Certificate2(this.certificationAuthority.TlsCertificate.GetPkcs12(), this.certificationAuthority.TlsCertificate.Password);

        //        ClientHandler newClient = new ClientHandler(context, certificationAuthority, certificateDatabase, tlsCert);
        //        //newClient.ConnectionClosed += new EventHandler<EventArgs>(OnConnectionClosed);
        //        //clientList.Add(newClient);
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error("Could not handle new client", ex);
        //    }
        //}

        //private void StartListenHttps()
        //{
        //    try
        //    {
        //        X509Certificate2 tlsCert = new X509Certificate2(this.certificationAuthority.TlsCertificate.GetPkcs12(), this.certificationAuthority.TlsCertificate.Password);
        //        this.https = new PCProtocol(true, tlsCert);
        //        this.https.ClientConnected += new EventHandler<ProtocolEventArgs>(OnPcpClientConnected);

        //        if (StatusChanged != null && !shutdownRequested)
        //        {
        //            this.StatusChanged.Invoke(this, new ServerStatusChangeEventArgs(true));
        //        }
        //        Log.Info("P@Porator starts handling PCP clients");

        //        //while (!shutdownRequested)
        //        //{
        //        //    // Waiting for clients to connect
        //        //    this.pcp.Listen(httpsConfig.Address, httpsConfig.Port, (int)httpsConfig.AllowedClients);
        //        //    //HandleClientPcp(listenSocket.Accept());
        //        //}
        //    }
        //    catch (SocketException ex)
        //    {
        //        if (ex.SocketErrorCode != SocketError.Interrupted)
        //        {
        //            Log.Error(ex);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error(ex);
        //    }
        //    finally
        //    {
        //        if (StatusChanged != null && !shutdownRequested)
        //        {
        //            this.StatusChanged.Invoke(this, new ServerStatusChangeEventArgs(false));
        //        }
        //        Log.Info("P@Porator stops handling PCP clients");
        //    }
        //}

        #endregion


        #region Event Handler

        private void OnDatabaseConnected(object sender, EventArgs e)
        {
            if (this.DatabaseConnected != null)
            {
                this.DatabaseConnected.Invoke(sender, e);
            }
        }

        private void OnDatabaseBroken(object sender, DatabaseErrorEventArgs e)
        {
            if (this.DatabaseBroken != null)
            {
                this.DatabaseBroken.Invoke(sender, e);
            }
        }

        private void OnClientConnected(object sender, ProtocolEventArgs e)
        {
            ClientHandler newClient = new ClientHandler(e.Protocol, this.certificationAuthority, this.registrationAuthority, this.directoryServer, this.certificateDatabase);
            newClient.ConnectionClosed += new EventHandler<EventArgs>(OnConnectionClosed);
            clientList.Add(newClient);
        }

        private void OnConnectionClosed(object sender, EventArgs e)
        {
            clientList.Remove((ClientHandler)sender);
        }

        #endregion


        #region Private members

        private Configuration config;

        private CertificateDatabase certificateDatabase;

        private CertificationAuthority certificationAuthority;

        private RegistrationAuthority registrationAuthority;

        private DirectoryServer directoryServer;

        private SMTPEmailClient smtpClient;

        private bool shutdownRequested;

        private uint allowedClients;

        private List<ClientHandler> clientList;

        private List<IProtocol> activeProtocols;

        private Configuration.CertificateServerConfig.ProtocolConfig pcpConfig;

        private Configuration.CertificateServerConfig.ProtocolConfig httpsConfig;

        private static System.Timers.Timer timer;

        #endregion


        #region Properties

        public bool IsRunning { get; private set; }

        /// <summary>
        /// Gives the number of currently connected clients.
        /// </summary>
        public int ConnectedClients
        {
            get { return this.clientList.Count; }
        }

        public uint AllowedClients
        {
            get { return allowedClients; }
            set
            {
                // As MaxConnections can not be determined by .NET framework, don't allow more then 100 connections
                if (value < 1 || value > 100)
                {
                    allowedClients = FALLBACK_ALLOWED_CLIENTS;
                }
                else
                {
                    allowedClients = value;
                }
            }
        }

        #endregion


        #region Events

        public event EventHandler<ServerStatusChangeEventArgs> StatusChanged;

        public event EventHandler<DatabaseErrorEventArgs> DatabaseBroken;

        public event EventHandler<EventArgs> DatabaseConnected;

        #endregion

    }
}