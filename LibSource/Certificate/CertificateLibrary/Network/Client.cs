using CrypTool.CertificateLibrary.Certificates;
using CrypTool.CertificateLibrary.Util;
using CrypTool.Util.Logging;
using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace CrypTool.CertificateLibrary.Network
{
    public class CertificateClient
    {

        #region Available protocols

        public enum NetworkProtocol
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


        #region Client states

        private enum ClientState
        {
            /// <summary>
            /// Client has been initialized and is ready to send messages
            /// </summary>
            Initialized,

            /// <summary>
            /// Client has established a TLS secured connection to the server
            /// </summary>
            Connected,

            /// <summary>
            /// Client has sent a certificate request
            /// </summary>
            CertificateRegistrationSent,

            /// <summary>
            /// Client has sent the email address verification
            /// </summary>
            EmailVerificationSent,

            /// <summary>
            /// Client has requested an existing certificate 
            /// </summary>
            CertificateRequestSent,

            /// <summary>
            /// Client has sent a request to reset the password
            /// </summary>
            PasswordResetSent,

            /// <summary>
            /// Client has sent the verification to reset the password
            /// </summary>
            PasswordResetVerificationSent,

            /// <summary>
            /// Client has requested the additional fields required to register a new certificate
            /// </summary>
            RegistrationFormularRequestSent,

            /// <summary>
            /// Client has requested a password change
            /// </summary>
            PasswordChangeSent,

            /// <summary>
            /// Client has received the corresponding packet and stopped work
            /// </summary>
            Stopped
        }

        #endregion


        #region Fallback values (if settings are invalid)

        private const int FALLBACK_CONNECTION_TIMEOUT = 30;

        private const string FALLBACK_SERVER_HOST = "cloud.CrypTool.org";

        private const int FALLBACK_SERVER_PORT = 10443;

        #endregion


        #region Private members

        private ClientState state;

        private IProtocol protocol;

        private string serverAddress;

        private int serverPort;

        private int proxyPort;

        private int timeout;

        private readonly System.Security.Cryptography.X509Certificates.X509Certificate serverTlsCertificate;

        private string password;

        private HttpTunnel httpTunnel;

        #endregion


        #region Constructor

        /// <summary>
        /// Constructs a new certificate client with default values.
        /// </summary>
        public CertificateClient()
            : this(global::CrypTool.CertificateLibrary.Properties.Settings.Default.PCP_SERVER_ADDRESS,
                global::CrypTool.CertificateLibrary.Properties.Settings.Default.PCP_SERVER_PORT,
                global::CrypTool.CertificateLibrary.Properties.Settings.Default.CONNECTION_TIMEOUT,
                NetworkProtocol.PCP)
        {
        }

        /// <summary>
        /// Constructs a new certificate client with default values.
        /// </summary>
        public CertificateClient(NetworkProtocol protocol)
            : this(global::CrypTool.CertificateLibrary.Properties.Settings.Default.PCP_SERVER_ADDRESS,
                global::CrypTool.CertificateLibrary.Properties.Settings.Default.PCP_SERVER_PORT,
                global::CrypTool.CertificateLibrary.Properties.Settings.Default.CONNECTION_TIMEOUT,
                protocol)
        {
        }

        /// <summary>
        /// Constructs a new certificate client.
        /// </summary>
        /// <param name="serverHostOrIP">Hostname or IP address of the server</param>
        /// <param name="serverPort">TCP Port of the server</param>
        /// <param name="timeout">Connection timeout in seconds</param>
        public CertificateClient(string serverHostOrIP,
                      int serverPort,
                      int timeout, NetworkProtocol protocol)
        {
            ServerAddress = serverHostOrIP;
            ServerPort = serverPort;
            TimeOut = timeout;
            UsedProtocol = protocol;
            state = ClientState.Initialized;
            serverTlsCertificate = new System.Security.Cryptography.X509Certificates.X509Certificate(global::CrypTool.CertificateLibrary.Properties.Resources.paporator_ssl);
            UseProxy = false;
            UseSystemWideProxy = true;
        }

        #endregion


        #region Connection

        /// <summary>
        /// Connects to the certificate server.
        /// </summary>
        private bool Connect()
        {

            // Check whether to use a proxy
            if (UseProxy)
            {
                httpTunnel = TryCreateTunnel();

                if (!httpTunnel.IsConnected)
                {
                    return false;
                }
            }
            else
            {
                httpTunnel = null;
            }

            // Select and initialize the protocol
            switch (UsedProtocol)
            {
                case NetworkProtocol.Https:
                    Log.Error("HTTPS is not implemented yet");
                    return false;
                default:
                    protocol = new PCProtocol(serverTlsCertificate);
                    break;
            }

            // Connect 
            try
            {
                if (protocol.Connect(ServerAddress, ServerPort, new RemoteCertificateValidationCallback(ValidateServerCertificate), httpTunnel))
                {
                    state = ClientState.Connected;
                    return true;
                }
            }
            catch (AuthenticationException)
            {
                if (SslCertificateRefused != null)
                {
                    SslCertificateRefused.Invoke(this, new EventArgs());
                }
            }
            state = ClientState.Stopped;
            return false;
        }

        /// <summary>
        /// Tries to create a HTTP tunnel with the HTTP CONNECT method to pass the proxy. If no proxy is configured or the proxy denied tunneling, 
        /// the HttpTunnel IsConnected Property is set to false.
        /// <para>ProxyErrorOccured event is triggered when the proxy denied the HTTP tunnel.</para>
        /// <para>NoProxyConfigured event is triggered when no system proxy is configured.</para>
        /// </summary>
        /// <returns>The HttpTunnel object or null</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="NetworkException">Error while creating the HTTP tunnel</exception>
        private HttpTunnel TryCreateTunnel()
        {
            HttpTunnel tunnel = (UseSystemWideProxy)
                ? new HttpTunnel(ServerAddress, ServerPort, ProxyAuthName, ProxyAuthPassword)
                : new HttpTunnel(ServerAddress, ServerPort, ProxyAddress, ProxyPort, ProxyAuthName, ProxyAuthPassword);

            // Try to create the tunnel
            ProxyEventArgs eventArgs = tunnel.CreateTunnel(TimeOut);
            if (eventArgs == null)
            {
                if (NoProxyConfigured != null)
                {
                    NoProxyConfigured.Invoke(this, new EventArgs());
                }
                // No proxy configured
                return tunnel;
            }

            // Set some more meaningful messages for common error types.
            switch (eventArgs.StatusCode)
            {
                case HttpStatusCode.OK:
                    eventArgs.Message = string.Format("HTTP tunnel successfully established! Used proxy {0}:{1}", tunnel.ProxyAddress, tunnel.ProxyPort);
                    break;
                case HttpStatusCode.MethodNotAllowed:
                    eventArgs.Message = "The proxy server doesn't allow HTTP tunneling!";
                    break;
                case HttpStatusCode.NotImplemented:
                    eventArgs.Message = "The proxy server is not capable to establish a HTTP tunnel!";
                    break;
                case HttpStatusCode.ProxyAuthenticationRequired:
                    eventArgs.Message = "The proxy server rejected the username/password combination!";
                    break;
                case HttpStatusCode.Forbidden:
                    eventArgs.Message = "The proxy server forbids HTTP tunnel to the destination port!";
                    break;
                default:
                    eventArgs.Message = string.Format("Could not create HTTP tunnel! StatusCode: {0} | Message: {1}", eventArgs.StatusCode, eventArgs.Message);
                    break;
            }

            if (tunnel.IsConnected)
            {
                Log.Info(eventArgs.Message);
                if (HttpTunnelEstablished != null)
                {
                    HttpTunnelEstablished.Invoke(this, new ProxyEventArgs(eventArgs.StatusCode, eventArgs.Message));
                }
            }
            else
            {
                // Proxy denied tunneling
                Log.Error(eventArgs.Message);
                if (ProxyErrorOccured != null)
                {
                    ProxyErrorOccured.Invoke(this, eventArgs);
                }
            }
            return tunnel;
        }

        /// <summary>
        /// Forces a disconnect. This will send a disconnect message to the server and close the socket.
        /// </summary>
        public void ForceDisconnect()
        {
            if (!IsConnected)
            {
                return;
            }

            try
            {
                Packet disconnectPacket = new Packet(PacketType.Disconnect);
                Log.Debug(string.Format("Sending disconnect signal to server {0}", protocol.RemoteIdentifier));
                protocol.SendPacket(disconnectPacket, timeout);
            }
            catch (Exception ex)
            {
                Log.Warn(string.Format("Could not disconnect from server {0}", protocol.RemoteIdentifier), ex);
            }
            finally
            {
                protocol.Close();
            }
        }

        #endregion


        #region SSL/TLS validation

        /// <summary>
        /// Validates the received server TLS certificate.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="sslPolicyErrors"></param>
        /// <returns>returns true if the certificate is valid, false otherwise</returns>
        private bool ValidateServerCertificate(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (certificate == null)
            {
                Log.Error("Could not validate P@Porator TLS certificate, as it is null");
                return false;
            }

            if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateNotAvailable)
            {
                Log.Error("Could not validate P@Porator TLS certificate, as the server did not provide one");
                return false;
            }

            // Check equality of remote and local certificate
            if (!certificate.Equals(serverTlsCertificate))
            {
                Log.Error("Received TLS certificate is not a valid P@Porator certificate: Equality check failed");
                return false;
            }

            if (!CertificateServices.VerifyTlsCertificate(CertificateServices.ConvertToBC(certificate)))
            {
                // Reason is already logged
                return false;
            }

            Log.Debug("Received TLS certificate from P@Porator is valid and will be used for encrypted communication");
            return true;
        }

        #endregion


        #region Send Messages

        /// <summary>
        /// Sends a certificate registration request to the server.
        /// <para>Can trigger: CertificateReceived, CertificateAuthorizationRequired, EmailVerificationRequired, InvalidCertificateRegistration, ServerErrorOccurred, NewProtocolVersion, ServerDisconnected</para>
        /// </summary>
        /// <param name="avatar">The avatar to be registered</param>
        /// <param name="email">The email address to be registered</param>
        /// <param name="world">The world name to be registered</param>
        /// <param name="password">The password which will be used to secure the PKCS #12 store</param>
        /// <exception cref="ArgumentException">avatar, email, world or password are invalid</exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="InvalidPacketFormatException">The received error type is invalid or unknown</exception>
        /// <exception cref="InvalidOperationException">Could not serialize packet</exception>
        /// <exception cref="InvalidPacketTypeException">The received packet type is invalid for the current system state</exception>
        /// <exception cref="NetworkException">Could not connect to the server</exception>
        /// <exception cref="NetworkStreamException">Could not write packet to the network stream</exception>
        public void RegisterCertificate(CertificateRegistration certRegistration)
        {
            if (!Verification.IsValidAvatar(certRegistration.Avatar))
            {
                throw new ArgumentException("Avatar is not valid");
            }
            if (!Verification.IsValidEmailAddress(certRegistration.Email))
            {
                throw new ArgumentException("Email is not valid");
            }
            if (!Verification.IsValidWorld(certRegistration.World))
            {
                throw new ArgumentException("World is not valid");
            }
            if (!Verification.IsValidPassword(certRegistration.Password))
            {
                throw new ArgumentException("Password is not valid");
            }

            try
            {
                certRegistration.ProgramName = ProgramName;
                certRegistration.ProgramVersion = ProgramVersion;
                certRegistration.ProgramLocale = ProgramLocale;
                certRegistration.OptionalInfo = OptionalInfo;

                Packet registrationPacket = new Packet(PacketType.CertificateRegistration, certRegistration.Serialize());
                if (!Connect())
                {
                    throw new NetworkException(string.Format("Could not connect to {0}:{1}. Certificate registration aborted", ServerAddress, ServerPort));
                }

                protocol.SendPacket(registrationPacket, timeout);
                password = certRegistration.Password;
                state = ClientState.CertificateRegistrationSent;
                Log.Debug(string.Format("Certificate registration sent to the server {0}", protocol.RemoteIdentifier));
                ReceivePacket();
            }
            catch (Exception ex)
            {
                Log.Error("Error occured during certificate registration!", ex);
                throw ex;
            }
            finally
            {
                if (protocol != null)
                {
                    protocol.Close();
                }
            }
        }

        /// <summary>
        /// Sends the verification code to the server to verify the email address.
        /// <para>The code has been sent to the users email address.</para>
        /// <para>Can trigger: CertificateReceived, CertificateAuthorizationRequired, InvalidEmailVerification, ServerErrorOccurred, NewProtocolVersion, ServerDisconnected</para>
        /// </summary>
        /// <param name="emailVerification">The EmailVerification object</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="InvalidPacketFormatException">The received error type is invalid or unknown</exception>
        /// <exception cref="InvalidOperationException">Could not serialize packet</exception>
        /// <exception cref="InvalidPacketTypeException">The received packet type is invalid for the current system state</exception>
        /// <exception cref="NetworkException">Could not connect to the server</exception>
        /// <exception cref="NetworkStreamException">Could not write packet to the network stream</exception>
        public void VerifyEmail(EmailVerification emailVerification)
        {
            try
            {
                emailVerification.ProgramName = ProgramName;
                emailVerification.ProgramVersion = ProgramVersion;
                emailVerification.ProgramLocale = ProgramLocale;
                emailVerification.OptionalInfo = OptionalInfo;

                Packet verificationPacket = new Packet(PacketType.EmailVerification, emailVerification.Serialize());
                if (!Connect())
                {
                    throw new NetworkException(string.Format("Could not connect to {0}:{1}. Email verification aborted", ServerAddress, ServerPort));
                }

                protocol.SendPacket(verificationPacket, timeout);
                state = ClientState.EmailVerificationSent;
                Log.Debug(string.Format("Email verification code sent to the server {0}", protocol.RemoteIdentifier));
                ReceivePacket();
            }
            catch (Exception ex)
            {
                Log.Error("Error occured during email verification!", ex);
                throw ex;
            }
            finally
            {
                if (protocol != null)
                {
                    protocol.Close();
                }
            }
        }

        /// <summary>
        /// Requests a registered certificate. The request must have the avatar or email set. If both are set, email is used.
        /// <para>Can trigger: CertificateReceived, InvalidCertificateRequest, ServerErrorOccurred, NewProtocolVersion, ServerDisconnected</para>
        /// </summary>
        /// <param name="certRequest">the certificate request object</param>
        /// <exception cref="ArgumentException">email+avatar or password are invalid </exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="InvalidPacketFormatException">The received error type is invalid or unknown</exception>
        /// <exception cref="InvalidOperationException">Could not serialize packet</exception>
        /// <exception cref="InvalidPacketTypeException">The received packet type is invalid for the current system state</exception>
        /// <exception cref="NetworkException">Could not connect to the server</exception>
        /// <exception cref="NetworkStreamException">Could not write packet to the network stream</exception>
        public void RequestCertificate(CT2CertificateRequest certRequest)
        {
            if (!Verification.IsValidPassword(certRequest.Password))
            {
                throw new ArgumentException("Password is not valid");
            }

            if (!Verification.IsValidAvatar(certRequest.Avatar) && !Verification.IsValidEmailAddress(certRequest.Email))
            {
                throw new ArgumentException("Neither avatar nor email address are valid!");
            }

            try
            {
                certRequest.ProgramName = ProgramName;
                certRequest.ProgramVersion = ProgramVersion;
                certRequest.ProgramLocale = ProgramLocale;
                certRequest.OptionalInfo = OptionalInfo;

                Packet requestPacket = new Packet(PacketType.CertificateRequest, certRequest.Serialize());
                if (!Connect())
                {
                    throw new NetworkException(string.Format("Could not connect to {0}:{1}. Certificate request aborted", ServerAddress, ServerPort));
                }

                protocol.SendPacket(requestPacket, timeout);
                password = certRequest.Password;
                state = ClientState.CertificateRequestSent;
                Log.Debug(string.Format("Certificate request sent to the server {0}", protocol.RemoteIdentifier));
                ReceivePacket();
            }
            catch (Exception ex)
            {
                Log.Error("Error occured during certificate request", ex);
                throw ex;
            }
            finally
            {
                if (protocol != null)
                {
                    protocol.Close();
                }
            }
        }

        /// <summary>
        /// Sends a request to the server to reset the certificate password.  The request must have the avatar or email set. If both are set, email is used.
        /// <para>Can trigger: PasswordResetVerificationRequired, InvalidPasswordReset, ServerErrorOccurred, NewProtocolVersion, ServerDisconnected</para>
        /// </summary>
        /// <param name="passReset">A PasswordReset object </param>
        /// <exception cref="ArgumentException">One of the parameter is invalid</exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="InvalidPacketFormatException">The received error type is invalid or unknown</exception>
        /// <exception cref="InvalidOperationException">Could not serialize packet</exception>
        /// <exception cref="InvalidPacketTypeException">The received packet type is invalid for the current system state</exception>
        /// <exception cref="NetworkException">Could not connect to the server</exception>
        /// <exception cref="NetworkStreamException">Could not write packet to the network stream</exception>
        public void ResetPassword(PasswordReset passReset)
        {
            if (!Verification.IsValidAvatar(passReset.Avatar) && !Verification.IsValidEmailAddress(passReset.Email))
            {
                throw new ArgumentException("Neither avatar nor email address are valid!");
            }

            try
            {
                passReset.ProgramName = ProgramName;
                passReset.ProgramVersion = ProgramVersion;
                passReset.OptionalInfo = OptionalInfo;
                passReset.ProgramLocale = ProgramLocale;

                Packet resetPacket = new Packet(PacketType.PasswordReset, passReset.Serialize());
                if (!Connect())
                {
                    throw new NetworkException(string.Format("Could not connect to {0}:{1}. Password reset aborted", ServerAddress, ServerPort));
                }

                protocol.SendPacket(resetPacket, timeout);
                state = ClientState.PasswordResetSent;
                Log.Debug(string.Format("Password reset request sent to the server {0}", protocol.RemoteIdentifier));
                ReceivePacket();
            }
            catch (Exception ex)
            {
                Log.Error("Error occured during password reset!", ex);
                throw ex;
            }
            finally
            {
                if (protocol != null)
                {
                    protocol.Close();
                }
            }
        }

        /// <summary>
        /// Sends the verification code to the server to reset the certificate password.  The request must have the avatar or email set. If both are set, email is used.
        /// <para>Can trigger: CertificateReceived, InvalidPasswordResetVerification, ServerErrorOccurred, NewProtocolVersion, ServerDisconnected</para>
        /// </summary>
        /// <param name="passResetVerification">The PasswordResetVerification object</param>
        /// <exception cref="ArgumentException">password is invalid</exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="InvalidPacketFormatException">The received error type is invalid or unknown</exception>
        /// <exception cref="InvalidOperationException">Could not serialize packet</exception>
        /// <exception cref="InvalidPacketTypeException">The received packet type is invalid for the current system state</exception>
        /// <exception cref="NetworkException">Could not connect to the server</exception>
        /// <exception cref="NetworkStreamException">Could not write packet to the network stream</exception>
        public void VerifyPasswordReset(PasswordResetVerification passResetVerification)
        {
            if (!Verification.IsValidPassword(passResetVerification.NewPassword))
            {
                throw new ArgumentException("NewPassword is not valid");
            }

            try
            {
                passResetVerification.ProgramName = ProgramName;
                passResetVerification.ProgramVersion = ProgramVersion;
                passResetVerification.OptionalInfo = OptionalInfo;
                passResetVerification.ProgramLocale = ProgramLocale;

                Packet verificationPacket = new Packet(PacketType.PasswordResetVerification, passResetVerification.Serialize());
                if (!Connect())
                {
                    throw new NetworkException(string.Format("Could not connect to {0}:{1}. Password reset verification aborted", ServerAddress, ServerPort));
                }

                protocol.SendPacket(verificationPacket, timeout);
                password = passResetVerification.NewPassword;
                state = ClientState.PasswordResetVerificationSent;
                Log.Debug(string.Format("Password reset verification sent to the server {0}", protocol.RemoteIdentifier));
                ReceivePacket();
            }
            catch (Exception ex)
            {
                Log.Error("Error occured during password reset verification!", ex);
                throw ex;
            }
            finally
            {
                if (protocol != null)
                {
                    protocol.Close();
                }
            }
        }

        /// <summary>
        /// Sends a request to the server to change the certificate password.  The request must have the avatar or email set. If both are set, email is used.
        /// <para>Can trigger: CertificateResponse, InvalidPasswordChange, ServerErrorOccurred, NewProtocolVersion, ServerDisconnected</para>
        /// </summary>
        /// <param name="passwordChange">A PasswordChange object </param>
        /// <exception cref="ArgumentException">One of the parameter is invalid</exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="InvalidPacketFormatException">The received error type is invalid or unknown</exception>
        /// <exception cref="InvalidOperationException">Could not serialize packet</exception>
        /// <exception cref="InvalidPacketTypeException">The received packet type is invalid for the current system state</exception>
        /// <exception cref="NetworkException">Could not connect to the server</exception>
        /// <exception cref="NetworkStreamException">Could not write packet to the network stream</exception>
        public void ChangePassword(PasswordChange passwordChange)
        {
            if (!Verification.IsValidAvatar(passwordChange.Avatar) && !Verification.IsValidEmailAddress(passwordChange.Email))
            {
                throw new ArgumentException("Neither avatar nor email address are valid!");
            }
            if (!Verification.IsValidPassword(passwordChange.OldPassword))
            {
                throw new ArgumentException("Old password is invalid!");
            }
            if (!Verification.IsValidPassword(passwordChange.NewPassword))
            {
                throw new ArgumentException("New password is invalid!");
            }

            try
            {
                passwordChange.ProgramName = ProgramName;
                passwordChange.ProgramVersion = ProgramVersion;
                passwordChange.ProgramLocale = ProgramLocale;
                passwordChange.OptionalInfo = OptionalInfo;
                Packet resetPacket = new Packet(PacketType.PasswordChange, passwordChange.Serialize());
                if (!Connect())
                {
                    throw new NetworkException(string.Format("Could not connect to {0}:{1}. Password change aborted", ServerAddress, ServerPort));
                }

                protocol.SendPacket(resetPacket, timeout);
                password = passwordChange.NewPassword;
                state = ClientState.PasswordChangeSent;
                Log.Debug(string.Format("Password change request sent to the server {0}", protocol.RemoteIdentifier));
                ReceivePacket();
            }
            catch (Exception ex)
            {
                Log.Error("Error occured during password change!", ex);
                throw ex;
            }
            finally
            {
                if (protocol != null)
                {
                    protocol.Close();
                }
            }
        }

        #endregion


        #region Packet receiving state machine

        /// <summary>
        /// Receives packets from the network (state machine)
        /// </summary>
        /// <exception cref="ArgumentNullException">stream is null</exception>
        /// <exception cref="InvalidPacketFormatException">The received error type is invalid or unknown</exception>
        /// <exception cref="InvalidPacketTypeException">The received packet type is invalid for the current system state</exception>
        /// <exception cref="NetworkStreamException">Error while reading the network stream</exception>
        /// <exception cref="UnexpectedEndOfStreamException">The stream did not contain the expected number of bytes</exception>
        private void ReceivePacket()
        {
            Packet receivedPacket = null;
            try
            {
                receivedPacket = protocol.ReceivePacket(timeout);
            }
            catch (InvalidProtocolVersionException ex)
            {
                Log.Error(ex);
                if (NewProtocolVersion != null)
                {
                    NewProtocolVersion.Invoke(this, new EventArgs());
                }
                return;
            }

            switch (state)
            {

                case ClientState.CertificateRegistrationSent:
                    switch (receivedPacket.Type)
                    {
                        case PacketType.CertificateResponse:
                            HandleCertificateResponse(receivedPacket.Data);
                            break;
                        case PacketType.CertificateAuthorizationRequired:
                            HandleCertificateAuthorizationRequired();
                            break;
                        case PacketType.Disconnect:
                            HandleServerDisconnect();
                            break;
                        case PacketType.EmailVerificationRequired:
                            HandleEmailVerificationRequired();
                            break;
                        case PacketType.ProcessingError:
                            HandleProcessingError(receivedPacket.Data);
                            break;
                        default:
                            throw new InvalidPacketTypeException(string.Format("Received packet type {0} is invalid for client state {1}", receivedPacket.Type, state));
                    }
                    break;

                case ClientState.EmailVerificationSent:
                    switch (receivedPacket.Type)
                    {
                        case PacketType.CertificateAuthorizationRequired:
                            HandleCertificateAuthorizationRequired();
                            break;
                        case PacketType.Disconnect:
                            HandleServerDisconnect();
                            break;
                        case PacketType.EmailVerified:
                            HandleEmailVerified();
                            break;
                        case PacketType.ProcessingError:
                            HandleProcessingError(receivedPacket.Data);
                            break;
                        case PacketType.RegistrationDeleted:
                            HandleRegistrationDeleted();
                            break;
                        default:
                            throw new InvalidPacketTypeException(string.Format("Received packet type {0} is invalid for client state {1}", receivedPacket.Type, state));
                    }
                    break;

                case ClientState.CertificateRequestSent:
                    switch (receivedPacket.Type)
                    {
                        case PacketType.CertificateResponse:
                            HandleCertificateResponse(receivedPacket.Data);
                            break;
                        case PacketType.Disconnect:
                            HandleServerDisconnect();
                            break;
                        case PacketType.ProcessingError:
                            HandleProcessingError(receivedPacket.Data);
                            break;
                        default:
                            throw new InvalidPacketTypeException(string.Format("Received packet type {0} is invalid for client state {1}", receivedPacket.Type, state));
                    }
                    break;

                case ClientState.PasswordResetSent:
                    switch (receivedPacket.Type)
                    {
                        case PacketType.Disconnect:
                            HandleServerDisconnect();
                            break;
                        case PacketType.PasswordResetVerificationRequired:
                            HandlePasswordResetVerificationRequired();
                            break;
                        case PacketType.ProcessingError:
                            HandleProcessingError(receivedPacket.Data);
                            break;
                        default:
                            throw new InvalidPacketTypeException(string.Format("Received packet type {0} is invalid for client state {1}", receivedPacket.Type, state));
                    }
                    break;

                case ClientState.PasswordResetVerificationSent:
                    switch (receivedPacket.Type)
                    {
                        case PacketType.CertificateResponse:
                            HandleCertificateResponse(receivedPacket.Data);
                            break;
                        case PacketType.Disconnect:
                            HandleServerDisconnect();
                            break;
                        case PacketType.ProcessingError:
                            HandleProcessingError(receivedPacket.Data);
                            break;
                        default:
                            throw new InvalidPacketTypeException(string.Format("Received packet type {0} is invalid for client state {1}", receivedPacket.Type, state));
                    }
                    break;

                case ClientState.PasswordChangeSent:
                    switch (receivedPacket.Type)
                    {
                        case PacketType.CertificateResponse:
                            HandleCertificateResponse(receivedPacket.Data);
                            break;
                        case PacketType.Disconnect:
                            HandleServerDisconnect();
                            break;
                        case PacketType.ProcessingError:
                            HandleProcessingError(receivedPacket.Data);
                            break;
                        default:
                            throw new InvalidPacketTypeException(string.Format("Received packet type {0} is invalid for client state {1}", receivedPacket.Type, state));
                    }
                    break;

                default:
                    throw new InvalidPacketTypeException(string.Format("Client received a packet but is in an invalid system state. Received packet type: {0} | Client state: {1}", receivedPacket.Type, state));
            }
        }

        #endregion


        #region Handle received packets

        private void HandleRegistrationFormular(byte[] data)
        {
            if (data.Length == 0)
            {
                Log.Error("Received registration formular has a size of 0 byte!");
                return;
            }

            Log.Debug("Registration formular has been received");
            try
            {
                // do some deserialization stuff...
                if (RegistrationFormularReceived != null)
                {
                    RegistrationFormularReceived(this, new EventArgs());
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        private void HandleCertificateResponse(byte[] data)
        {
            if (data.Length == 0)
            {
                Log.Error("Received certificate has a size of 0 byte!");
                return;
            }

            Log.Debug("Certificate response has been received");
            PeerCertificate peerCert = new PeerCertificate();
            try
            {
                using (MemoryStream mstream = new MemoryStream(data))
                {
                    peerCert.Load(mstream, password);
                    mstream.Close();
                }
                if (CertificateReceived != null && peerCert != null)
                {
                    CertificateReceived(this, new CertificateReceivedEventArgs(peerCert, password));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        private void HandleEmailVerified()
        {
            Log.Debug("Server signals that the email address has been validated.");
            if (EmailVerified != null)
            {
                EmailVerified.Invoke(this, new EventArgs());
            }
        }

        private void HandlePasswordResetVerificationRequired()
        {
            Log.Debug("Server signals that the password reset needs to be validated");
            if (PasswordResetVerificationRequired != null)
            {
                PasswordResetVerificationRequired.Invoke(this, new EventArgs());
            }
        }

        private void HandleEmailVerificationRequired()
        {
            Log.Debug("Server signals that the email address needs to be validated");
            if (EmailVerificationRequired != null)
            {
                EmailVerificationRequired.Invoke(this, new EventArgs());
            }
        }

        private void HandleCertificateAuthorizationRequired()
        {
            Log.Debug("Server signals that the certificate needs to be authorized by an admin.");
            if (CertificateAuthorizationRequired != null)
            {
                CertificateAuthorizationRequired.Invoke(this, new EventArgs());
            }
        }

        private void HandleRegistrationDeleted()
        {
            Log.Debug("Server signals that the request was proceeded successfully. System state: " + state);
            if (RegistrationDeleted != null)
            {
                RegistrationDeleted.Invoke(this, new EventArgs());
            }
        }

        private void HandleServerDisconnect()
        {
            Log.Debug("Connection closed by the server");
            if (ServerDisconnected != null)
            {
                ServerDisconnected.Invoke(this, new EventArgs());
            }
        }

        /// <summary>
        /// Handles a network packet of type ProcessingError. Checks the error code against the current system state.
        /// <para>Can trigger: InvalidCertificateRegistration, InvalidEmailVerification, InvalidCertificateRequest, InvalidPasswordReset, InvalidPasswordResetVerification, ServerErrorOccurred</para>
        /// </summary>
        /// <param name="data">Must be a one byte array presenting the error type</param>
        /// <exception cref="InvalidPacketFormatException">Error type is unknown or invalid for the system state</exception>
        private void HandleProcessingError(byte[] data)
        {
            ProcessingError requestError = new ProcessingError();
            bool deserializationSucceeded = requestError.Deserialize(data);
            if (!deserializationSucceeded || requestError.Type == ErrorType.Invalid)
            {
                throw new InvalidPacketFormatException("The server signals problems processing the request, but the error type could not be read");
            }

            // Checks the error type against the current system state
            switch (state)
            {
                case ClientState.RegistrationFormularRequestSent:
                    switch (requestError.Type)
                    {
                        // No known valid error types for now
                        case ErrorType.Unknown:
                            if (ServerErrorOccurred != null)
                            {
                                ServerErrorOccurred.Invoke(this, new ProcessingErrorEventArgs(requestError.Type, requestError.Message));
                            }
                            break;
                        // invalid error types
                        default:
                            throw new InvalidPacketFormatException(string.Format("Received the invalid error type '{0}' for system state '{1}'", requestError.Type.ToString(), state.ToString()));
                    }
                    break;

                case ClientState.CertificateRegistrationSent:
                    switch (requestError.Type)
                    {
                        // valid error types for this state
                        case ErrorType.AdditionalFieldsIncorrect:
                        case ErrorType.AvatarAlreadyExists:
                        case ErrorType.AvatarFormatIncorrect:
                        case ErrorType.DeserializationFailed:
                        case ErrorType.EmailAlreadyExists:
                        case ErrorType.EmailFormatIncorrect:
                        case ErrorType.PasswordFormatIncorrect:
                        case ErrorType.SmtpServerDown:
                        case ErrorType.WorldFormatIncorrect:
                            Log.Debug(string.Format("Server signals problems processing the certificate registration. Error type: [ {0}{1} ]", requestError.Type.ToString(), (requestError.Message != null) ? " | " + requestError.Message : ""));
                            if (InvalidCertificateRegistration != null)
                            {
                                InvalidCertificateRegistration.Invoke(this, new ProcessingErrorEventArgs(requestError.Type, requestError.Message));
                            }
                            break;
                        case ErrorType.Unknown:
                            if (ServerErrorOccurred != null)
                            {
                                ServerErrorOccurred.Invoke(this, new ProcessingErrorEventArgs(requestError.Type, requestError.Message));
                            }
                            break;
                        // invalid error types
                        default:
                            throw new InvalidPacketFormatException(string.Format("Received the invalid error type '{0}' for system state '{1}'", requestError.Type.ToString(), state.ToString()));
                    }
                    break;

                case ClientState.EmailVerificationSent:
                    switch (requestError.Type)
                    {
                        case ErrorType.AlreadyVerified:
                        case ErrorType.DeserializationFailed:
                        case ErrorType.NoCertificateFound:
                            Log.Debug(string.Format("Server signals problems processing the email verification. Error type: [ {0}{1} ]", requestError.Type.ToString(), (requestError.Message != null) ? " | " + requestError.Message : ""));
                            if (InvalidEmailVerification != null)
                            {
                                InvalidEmailVerification.Invoke(this, new ProcessingErrorEventArgs(requestError.Type, requestError.Message));
                            }
                            break;
                        case ErrorType.Unknown:
                            if (ServerErrorOccurred != null)
                            {
                                ServerErrorOccurred.Invoke(this, new ProcessingErrorEventArgs(requestError.Type, requestError.Message));
                            }
                            break;
                        default:
                            throw new InvalidPacketFormatException(string.Format("Received the invalid error type '{0}' for system state '{1}'", requestError.Type.ToString(), state.ToString()));
                    }
                    break;

                case ClientState.CertificateRequestSent:
                    switch (requestError.Type)
                    {
                        case ErrorType.CertificateNotYetAuthorized:
                        case ErrorType.CertificateRevoked:
                        case ErrorType.DeserializationFailed:
                        case ErrorType.EmailNotYetVerified:
                        case ErrorType.NoCertificateFound:
                        case ErrorType.SmtpServerDown:
                        case ErrorType.WrongPassword:
                            Log.Debug(string.Format("Server signals problems processing the certificate request. Error type: [ {0}{1} ]", requestError.Type.ToString(), (requestError.Message != null) ? " | " + requestError.Message : ""));
                            if (InvalidCertificateRequest != null)
                            {
                                InvalidCertificateRequest.Invoke(this, new ProcessingErrorEventArgs(requestError.Type, requestError.Message));
                            }
                            break;
                        case ErrorType.Unknown:
                            if (ServerErrorOccurred != null)
                            {
                                ServerErrorOccurred.Invoke(this, new ProcessingErrorEventArgs(requestError.Type, requestError.Message));
                            }
                            break;
                        default:
                            throw new InvalidPacketFormatException(string.Format("Received the invalid error type '{0}' for system state '{1}'", requestError.Type.ToString(), state.ToString()));
                    }
                    break;

                case ClientState.PasswordResetSent:
                    switch (requestError.Type)
                    {
                        case ErrorType.CertificateRevoked:
                        case ErrorType.DeserializationFailed:
                        case ErrorType.NoCertificateFound:
                        case ErrorType.SmtpServerDown:
                            Log.Debug(string.Format("Server signals problems processing the password reset request. Error type: [ {0}{1} ]", requestError.Type.ToString(), (requestError.Message != null) ? " | " + requestError.Message : ""));
                            if (InvalidPasswordReset != null)
                            {
                                InvalidPasswordReset.Invoke(this, new ProcessingErrorEventArgs(requestError.Type, requestError.Message));
                            }
                            break;
                        case ErrorType.Unknown:
                            if (ServerErrorOccurred != null)
                            {
                                ServerErrorOccurred.Invoke(this, new ProcessingErrorEventArgs(requestError.Type, requestError.Message));
                            }
                            break;
                        default:
                            throw new InvalidPacketFormatException(string.Format("Received the invalid error type '{0}' for system state '{1}'", requestError.Type.ToString(), state.ToString()));
                    }
                    break;

                case ClientState.PasswordResetVerificationSent:
                    switch (requestError.Type)
                    {
                        case ErrorType.AlreadyVerified:
                        case ErrorType.CertificateRevoked:
                        case ErrorType.DeserializationFailed:
                        case ErrorType.NoCertificateFound:
                        case ErrorType.PasswordFormatIncorrect:
                        case ErrorType.WrongCode:
                            Log.Debug(string.Format("Server signals problems processing the password reset verification. Error type: [ {0}{1} ]", requestError.Type.ToString(), (requestError.Message != null) ? " | " + requestError.Message : ""));
                            if (InvalidPasswordResetVerification != null)
                            {
                                InvalidPasswordResetVerification.Invoke(this, new ProcessingErrorEventArgs(requestError.Type, requestError.Message));
                            }
                            break;
                        case ErrorType.Unknown:
                            if (ServerErrorOccurred != null)
                            {
                                ServerErrorOccurred.Invoke(this, new ProcessingErrorEventArgs(requestError.Type, requestError.Message));
                            }
                            break;
                        default:
                            throw new InvalidPacketFormatException(string.Format("Received the invalid error type '{0}' for system state '{1}'", requestError.Type.ToString(), state.ToString()));
                    }
                    break;

                case ClientState.PasswordChangeSent:
                    switch (requestError.Type)
                    {
                        case ErrorType.CertificateRevoked:
                        case ErrorType.DeserializationFailed:
                        case ErrorType.NoCertificateFound:
                        case ErrorType.PasswordFormatIncorrect:
                        case ErrorType.WrongPassword:
                            Log.Debug(string.Format("Server signals problems processing the password change. Error type: [ {0}{1} ]", requestError.Type.ToString(), (requestError.Message != null) ? " | " + requestError.Message : ""));
                            if (InvalidPasswordChange != null)
                            {
                                InvalidPasswordChange.Invoke(this, new ProcessingErrorEventArgs(requestError.Type, requestError.Message));
                            }
                            break;
                        case ErrorType.Unknown:
                            if (ServerErrorOccurred != null)
                            {
                                ServerErrorOccurred.Invoke(this, new ProcessingErrorEventArgs(requestError.Type, requestError.Message));
                            }
                            break;
                        default:
                            throw new InvalidPacketFormatException(string.Format("Received the invalid error type '{0}' for system state '{1}'", requestError.Type.ToString(), state.ToString()));
                    }
                    break;

                default:
                    throw new InvalidPacketFormatException(string.Format("Client is in an invalid state to receive packets '{0}'", state.ToString()));
            }
        }

        #endregion


        #region Helper

        /// <summary>
        /// Returns the systems configured proxy address. Returns String.Empty if no proxy is configured.
        /// </summary>
        /// <returns>The proxy address or String.Empty</returns>
        public string GetSystemProxyAddress()
        {
            Uri uri = WebRequest.GetSystemWebProxy().GetProxy(new Uri("https://" + ServerAddress + ":" + ServerPort));
            string host = uri.Host;
            int port = uri.Port;
            return (host.ToLower().Equals(ServerAddress.ToLower()) && port == ServerPort) ? string.Empty : host;
        }

        /// <summary>
        /// Returns the systems configured proxy port. Returns -1 if no proxy is configured.
        /// </summary>
        /// <returns>The port number or -1</returns>
        public int GetSystemProxyPort()
        {
            Uri uri = WebRequest.GetSystemWebProxy().GetProxy(new Uri("https://" + ServerAddress + ":" + ServerPort));
            string host = uri.Host;
            int port = uri.Port;
            return (host.ToLower().Equals(ServerAddress.ToLower()) && port == ServerPort) ? -1 : port;
        }

        #endregion


        #region Properties

        public NetworkProtocol UsedProtocol { get; set; }

        public bool IsConnected
        {
            get
            {
                if (state != ClientState.Initialized && state != ClientState.Stopped)
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Host or IP Address of the server.
        /// </summary>
        public string ServerAddress
        {
            get => serverAddress;
            set
            {
                if (value == null)
                {
                    Log.Warn(string.Format("Server host was not set. Using fallback value '{0}' instead", FALLBACK_SERVER_HOST));
                    value = FALLBACK_SERVER_HOST;
                }
                serverAddress = value;
            }
        }

        /// <summary>
        /// Server port that will be connected to.
        /// </summary>
        public int ServerPort
        {
            get => serverPort;
            set
            {
                if (value < 1 || value > 65535)
                {
                    Log.Warn(string.Format("Server port was not set to a valid value. Using fallback value '{0}' instead", FALLBACK_SERVER_PORT));
                    value = FALLBACK_SERVER_PORT;
                }
                serverPort = value;
            }
        }

        /// <summary>
        /// Socket times out after TimeOut seconds. (Default: 30)
        /// </summary>
        public int TimeOut
        {
            get => timeout;
            set
            {
                if (value < 1)
                {
                    Log.Warn(string.Format("Connection timeout was not set to a valid value. Using fallback value '{0}' instead", FALLBACK_CONNECTION_TIMEOUT));
                    value = FALLBACK_CONNECTION_TIMEOUT;
                }
                timeout = value;
            }
        }

        /// <summary>
        /// Use HTTP Connect to create a HTTP tunnel through the proxy server. Default is false.
        /// </summary>
        public bool UseProxy { get; set; }

        /// <summary>
        /// Uses the proxy settings configured in the system (i.e Internet Explorer settings). Default is true.
        /// </summary>
        public bool UseSystemWideProxy { get; set; }

        /// <summary>
        /// Hostname or IP address of the proxy used (if CreateHttpTunnel is set to true).
        /// </summary>
        public string ProxyAddress { get; set; }

        /// <summary>
        /// TCP port of the proxy used (if CreateHttpTunnel is set to true).
        /// </summary>
        public int ProxyPort
        {
            get => proxyPort;
            set
            {
                if (value < 1 || value > 65535)
                {
                }
                proxyPort = value;
            }
        }

        /// <summary>
        /// The username used to authenticate at the proxy server (if neccessary).
        /// </summary>
        public string ProxyAuthName { get; set; }

        /// <summary>
        /// The password used to authenticate at the proxy server (if neccessary).
        /// </summary>
        public string ProxyAuthPassword { get; set; }

        /// <summary>
        /// Gets or sets the name of the program used to communicate with the certificate server
        /// </summary>
        public string ProgramName { get; set; }

        /// <summary>
        /// Gets or sets the version of the program used to communicate with the certificate server
        /// </summary>
        public string ProgramVersion { get; set; }

        /// <summary>
        /// Gets or sets the locale of the program used to communicate with the certificate server
        /// </summary>
        public string ProgramLocale { get; set; }

        /// <summary>
        /// Gets or sets optional information to be transmitted to the server
        /// </summary>
        public string OptionalInfo { get; set; }

        #endregion


        #region Events

        /// <summary>
        /// The requested registration formular has been received
        /// </summary>
        public event EventHandler<EventArgs> RegistrationFormularReceived;

        /// <summary>
        /// The requested certificate has been received
        /// </summary>
        public event EventHandler<CertificateReceivedEventArgs> CertificateReceived;

        /// <summary>
        /// The email address needs to be verified (Verification code has been sent to the given email address)
        /// </summary>
        public event EventHandler<EventArgs> EmailVerificationRequired;

        /// <summary>
        /// The email address has been successfully verified and the client can request the certificate now.
        /// </summary>
        public event EventHandler<EventArgs> EmailVerified;

        /// <summary>
        /// The peer certificate needs to be authorized by an admin
        /// </summary>
        public event EventHandler<EventArgs> CertificateAuthorizationRequired;

        /// <summary>
        /// The password reset request has been received and needs to be validated (Verification code has been sent to the given email address)
        /// </summary>
        public event EventHandler<EventArgs> PasswordResetVerificationRequired;

        /// <summary>
        /// The certificate registration request is invalid
        /// </summary>
        public event EventHandler<ProcessingErrorEventArgs> InvalidCertificateRegistration;

        /// <summary>
        /// The entered email verification code is invalid
        /// </summary>
        public event EventHandler<ProcessingErrorEventArgs> InvalidEmailVerification;

        /// <summary>
        /// The server denied to deliver the existing certificate
        /// </summary>
        public event EventHandler<ProcessingErrorEventArgs> InvalidCertificateRequest;

        /// <summary>
        /// The server denied to reset the password
        /// </summary>
        public event EventHandler<ProcessingErrorEventArgs> InvalidPasswordReset;

        /// <summary>
        /// The entered password reset verification code is invalid
        /// </summary>
        public event EventHandler<ProcessingErrorEventArgs> InvalidPasswordResetVerification;

        /// <summary>
        /// The password change request was invalid
        /// </summary>
        public event EventHandler<ProcessingErrorEventArgs> InvalidPasswordChange;

        /// <summary>
        /// The server reports an internal error and closed the connection
        /// </summary>
        public event EventHandler<ProcessingErrorEventArgs> ServerErrorOccurred;

        /// <summary>
        /// The certificate registration was successfully deleted.
        /// </summary>
        public event EventHandler<EventArgs> RegistrationDeleted;

        /// <summary>
        /// Received a packet with a higher protocol version. Packet handling has stopped, user should be informed that a new client version is available.
        /// </summary>
        public event EventHandler<EventArgs> NewProtocolVersion;

        /// <summary>
        /// The server send a disconnect message. (i.e server shutdown)
        /// </summary>
        public event EventHandler<EventArgs> ServerDisconnected;

        /// <summary>
        /// The proxy server signals that the HTTP Connect request was denied.
        /// </summary>
        public event EventHandler<ProxyEventArgs> ProxyErrorOccured;

        /// <summary>
        /// No proxy server has been configured.
        /// </summary>
        public event EventHandler<EventArgs> NoProxyConfigured;

        /// <summary>
        /// The HTTP tunnel through the proxy has been established.
        /// </summary>
        public event EventHandler<ProxyEventArgs> HttpTunnelEstablished;

        /// <summary>
        /// Client refused the received server SSL/TLS certificate.
        /// </summary>
        public event EventHandler<EventArgs> SslCertificateRefused;

        #endregion

    }

}
