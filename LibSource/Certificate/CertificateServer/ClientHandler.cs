using System;
using System.Text;
using System.Net.Sockets;
using System.ComponentModel;
using Org.BouncyCastle.Math;
using CrypTool.Util.Logging;
using CrypTool.CertificateLibrary.Network;
using CrypTool.CertificateLibrary.Util;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using CrypTool.CertificateLibrary.Certificates;
using CrypTool.Util.Cryptography;
using System.Net;
using System.Threading;
using System.IO;

namespace CrypTool.CertificateServer
{
    class ClientHandler
    {

        #region Constants (obsolete)

        private const int VERIFICATION_CODE_LENGTH = 15;

        #endregion


        #region Fallback values (if settings are invalid)

        private const int FALLBACK_CLIENT_TIMEOUT = 30;

        #endregion


        #region Constructor

        public ClientHandler(IProtocol protocol, CertificationAuthority ca, RegistrationAuthority ra, DirectoryServer ds, CertificateDatabase db)
        {
            this.config = Configuration.GetConfiguration();
            this.Timeout = global::CrypTool.CertificateServer.Properties.Settings.Default.DEFAULT_CLIENT_TIMEOUT;
            this.client = new ClientInfo(protocol);

            this.protocol = protocol;
            this.certificationAuthority = ca;
            this.registrationAuthority = ra;
            this.directoryServer = ds;
            this.certificateDatabase = db;

            ThreadPool.QueueUserWorkItem(ReceivePacket);
        }

        #endregion


        #region Disconnect

        /// <summary>
        /// Sends a disconnect message to the client and closes the socket
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
                this.protocol.SendPacket(disconnectPacket, this.timeout);
                Log.Debug(Logging.GetLogEnvelope("Disconnect signal sent to client", this.client));
            }
            catch (Exception ex)
            {
                Log.Warn(Logging.GetLogEnvelope("Could not disconnect from client.", this.client), ex);
            }
            finally
            {
                this.protocol.Close();
            }

            if (ConnectionClosed != null)
            {
                this.ConnectionClosed.Invoke(this, new EventArgs());
            }
        }

        #endregion


        #region Receive and handle packets

        /// <summary>
        /// Receives a packet
        /// </summary>
        /// <param name="o">The object is forced by ThreadPool.QueueUserWorkItem</param>
        private void ReceivePacket(object o)
        {
            try
            {
                Packet responsePacket = null;
                try
                {
                    Packet receivedPacket = this.protocol.ReceivePacket(this.timeout);
                    responsePacket = HandleReceivedPacket(receivedPacket);
                }
                catch (InvalidProtocolVersionException ex)
                {
                    Log.Warn(Logging.GetLogEnvelope("Received a packet with an unsupported protocol version. Client was informed.", client), ex);

                    // Send a message to inform the client about the unsupported protocol version. (PacketType is irrelevant)
                    responsePacket = new Packet(PacketType.Disconnect);
                }
                catch (NetworkFormatException ex)
                {
                    Log.Warn(Logging.GetLogEnvelope("Received a malformed packet with an unsupported protocol version. Client was informed.", client), ex);

                    // Send a message to inform the client that the packet format is invalid.
                    ProcessingError processingError = new ProcessingError(ErrorType.DeserializationFailed);
                    responsePacket = new Packet(PacketType.ProcessingError, processingError.Serialize());
                }

                this.protocol.SendPacket(responsePacket, this.timeout);
            }
            catch (Exception ex)
            {
                // This will happen on network errors, so just log.
                Log.Error(Logging.GetLogEnvelope(ex.Message, client), ex);
            }
            finally
            {
                this.protocol.Close();

                if (ConnectionClosed != null)
                {
                    this.ConnectionClosed.Invoke(this, new EventArgs());
                }
            }
        }

        private Packet HandleReceivedPacket(Packet receivedPacket)
        {
            this.client.RequestType = receivedPacket.Type;
            Packet responsePacket = null;

            switch (receivedPacket.Type)
            {
                case PacketType.CertificateRegistration:
                    responsePacket = HandleCertificateRegistration(receivedPacket.Data);
                    break;
                case PacketType.EmailVerification:
                    responsePacket = HandleEmailVerification(receivedPacket.Data);
                    break;
                case PacketType.CertificateRequest:
                    responsePacket = HandleCertificateRequest(receivedPacket.Data);
                    break;
                case PacketType.PasswordReset:
                    responsePacket = HandlePasswordReset(receivedPacket.Data);
                    break;
                case PacketType.PasswordResetVerification:
                    responsePacket = HandlePasswordResetVerification(receivedPacket.Data);
                    break;
                case PacketType.PasswordChange:
                    responsePacket = HandlePasswordChange(receivedPacket.Data);
                    break;
                case PacketType.Disconnect:
                    HandleClientDisconnect();
                    return null;
                default:
                    throw new InvalidPacketTypeException(String.Format("Received packet type {0} is invalid", receivedPacket.Type));
            }
            return responsePacket;
        }

        /// <summary>
        /// Handles an incoming certificate registration packet.
        /// </summary>
        /// <param name="data">the packet payload</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="CertificateException"></exception>
        /// <exception cref="DatabaseException">Database connection error</exception>
        /// <exception cref="InvalidOperationException">Could not serialize response into xml</exception>
        /// <exception cref="NetworkStreamException">Could not write to the network stream</exception>
        /// <exception cref="X509CertificateFormatException"></exception>
        private Packet HandleCertificateRegistration(byte[] data)
        {
            // Deserialize XML request
            CertificateRegistration certRegistration = new CertificateRegistration();
            bool deserializationSucceeded = certRegistration.Deserialize(data);
            if (!deserializationSucceeded)
            {
                Log.Warn(Logging.GetLogEnvelope("Received a CertificateRegistration message that could not be deserialized. Processing was aborted.", client));
                ProcessingError error = new ProcessingError(ErrorType.DeserializationFailed); 
                return new Packet(PacketType.ProcessingError, error.Serialize());
            }

            // Read the client program information
            this.client.ProgramName = certRegistration.ProgramName;
            this.client.ProgramVersion = certRegistration.ProgramVersion;
            this.client.ProgramLocale = certRegistration.ProgramLocale;
            this.client.OptionalInfo = certRegistration.OptionalInfo;

            Packet responsePacket = null;
            try
            {
                ProcessingResult result = this.registrationAuthority.ProcessRegistrationRequest(this.client, certRegistration);
                switch (result.Type)
                {
                    case PacketType.CertificateAuthorizationRequired:
                        responsePacket = new Packet(PacketType.CertificateAuthorizationRequired);
                        break;
                    case PacketType.CertificateResponse:
                        responsePacket = new Packet(PacketType.CertificateResponse, result.Pkcs12);
                        break;
                    case PacketType.EmailVerificationRequired:
                        responsePacket = new Packet(PacketType.EmailVerificationRequired);
                        break;
                    default:
                        Log.Error(Logging.GetLogEnvelope(String.Format("Invalid ClientHandlerResult {0} while processing CertificateRegistration", result.Type), this.client));
                        responsePacket = new Packet(PacketType.ProcessingError, (new ProcessingError(ErrorType.Unknown)).Serialize());
                        break;
                }
            }
            catch (ProcessingException ex)
            {
                string msg = Logging.GetLogEnvelope(ex.Message, this.client, certRegistration.Avatar, certRegistration.Email, certRegistration.World);
                Logging.LogMessage(ex.LogLevel, msg);
                responsePacket = new Packet(PacketType.ProcessingError, ex.Error.Serialize());
            }
            return responsePacket;
        }

        /// <summary>
        /// Handles an incoming email verification packet.
        /// </summary>
        /// <param name="data">the packet payload</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="DatabaseException">Database connection error</exception>
        /// <exception cref="InvalidOperationException">Could not serialize response into xml</exception>
        /// <exception cref="NetworkStreamException">Could not write to the network stream</exception>
        private Packet HandleEmailVerification(byte[] data)
        {
            // Deserialize XML request
            EmailVerification emailVerification = new EmailVerification();
            bool deserializationSucceeded = emailVerification.Deserialize(data);
            if (!deserializationSucceeded)
            {
                Log.Warn(Logging.GetLogEnvelope("Received an EmailVerification message that could not be deserialized. Processing was aborted.", this.client));
                ProcessingError error = new ProcessingError(ErrorType.DeserializationFailed);
                return new Packet(PacketType.ProcessingError, error.Serialize());
            }

            // Read the client program information
            this.client.ProgramName = emailVerification.ProgramName;
            this.client.ProgramVersion = emailVerification.ProgramVersion;
            this.client.ProgramLocale = emailVerification.ProgramLocale;
            this.client.OptionalInfo = emailVerification.OptionalInfo;

            Packet responsePacket = null;
            try
            {
                ProcessingResult result = this.registrationAuthority.ProcessEmailVerification(this.client, emailVerification);
                switch (result.Type)
                {
                    case PacketType.CertificateAuthorizationRequired:
                        responsePacket = new Packet(PacketType.CertificateAuthorizationRequired);
                        break;
                    case PacketType.CertificateResponse:
                        responsePacket = new Packet(PacketType.CertificateResponse, result.Pkcs12);
                        break;
                    case PacketType.EmailVerified:
                        responsePacket = new Packet(PacketType.EmailVerified);
                        break;
                    case PacketType.RegistrationDeleted:
                        responsePacket = new Packet(PacketType.RegistrationDeleted);
                        break;
                    default:
                        Log.Error(Logging.GetLogEnvelope(String.Format("Invalid ClientHandlerResult {0} while processing EmailVerification", result.Type), this.client));
                        responsePacket = new Packet(PacketType.ProcessingError, (new ProcessingError(ErrorType.Unknown)).Serialize());
                        break;
                }
            }
            catch (ProcessingException ex)
            {
                Logging.LogMessage(ex.LogLevel, Logging.GetLogEnvelope(ex.Message, this.client));
                responsePacket = new Packet(PacketType.ProcessingError, ex.Error.Serialize());
            }
            return responsePacket;
        }

        /// <summary>
        /// Handles an incoming certificate request packet.
        /// </summary>
        /// <param name="data">the packet payload</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="DatabaseException">Database connection error</exception>
        /// <exception cref="InvalidOperationException">Could not serialize response into xml</exception>
        /// <exception cref="NetworkStreamException">Could not write to the network stream</exception>
        private Packet HandleCertificateRequest(byte[] data)
        {
            // Deserialize XML request
            CT2CertificateRequest certRequest = new CT2CertificateRequest();
            bool deserializationSucceeded = certRequest.Deserialize(data);
            if (!deserializationSucceeded)
            {
                Log.Warn(Logging.GetLogEnvelope("Received a certificate request message that could not be deserialized. Processing was aborted.", this.client));
                ProcessingError error = new ProcessingError(ErrorType.DeserializationFailed);
                return new Packet(PacketType.ProcessingError, error.Serialize());
            }

            // Read the client program information
            this.client.ProgramName = certRequest.ProgramName;
            this.client.ProgramVersion = certRequest.ProgramVersion;
            this.client.ProgramLocale = certRequest.ProgramLocale;
            this.client.OptionalInfo = certRequest.OptionalInfo;

            Packet responsePacket = null;
            try
            {
                ProcessingResult result = this.directoryServer.ProcessCertificateRequest(this.client, certRequest);
                switch (result.Type)
                {
                    case PacketType.CertificateResponse:
                        responsePacket = new Packet(PacketType.CertificateResponse, result.Pkcs12);
                        break;
                    default:
                        Log.Error(Logging.GetLogEnvelope(String.Format("Invalid ClientHandlerResult {0} while processing CertificateRequest", result.Type), this.client));
                        responsePacket = new Packet(PacketType.ProcessingError, (new ProcessingError(ErrorType.Unknown)).Serialize());
                        break;
                }
            }
            catch (ProcessingException ex)
            {
                string msg = Logging.GetLogEnvelope(ex.Message, this.client, certRequest.Avatar, certRequest.Email);
                Logging.LogMessage(ex.LogLevel, msg);
                responsePacket = new Packet(PacketType.ProcessingError, ex.Error.Serialize());
            }
            return responsePacket;
        }

        /// <summary>
        /// Handles an incoming password reset packet.
        /// </summary>
        /// <param name="data">the packet payload</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="DatabaseException">Database connection error</exception>
        /// <exception cref="InvalidOperationException">Could not serialize response into xml</exception>
        /// <exception cref="NetworkStreamException">Could not write to the network stream</exception>
        private Packet HandlePasswordReset(byte[] data)
        {
            // Deserialize XML request
            PasswordReset passwordReset = new PasswordReset();
            bool deserializationSucceeded = passwordReset.Deserialize(data);
            if (!deserializationSucceeded)
            {
                Log.Warn(Logging.GetLogEnvelope("Received a PasswordReset message that could not be deserialized. Processing was aborted.", this.client));
                ProcessingError error = new ProcessingError(ErrorType.DeserializationFailed);
                return new Packet(PacketType.ProcessingError, error.Serialize());
            }

            // Read the client program information
            this.client.ProgramName = passwordReset.ProgramName;
            this.client.ProgramVersion = passwordReset.ProgramVersion;
            this.client.ProgramLocale = passwordReset.ProgramLocale;
            this.client.OptionalInfo = passwordReset.OptionalInfo;

            Packet responsePacket = null;
            try
            {
                ProcessingResult result = this.registrationAuthority.ProcessPasswordReset(client, passwordReset);
                switch (result.Type)
                {
                    case PacketType.PasswordResetVerificationRequired:
                        responsePacket = new Packet(PacketType.PasswordResetVerificationRequired);
                        break;
                    default:
                        Log.Error(Logging.GetLogEnvelope(String.Format("Invalid ClientHandlerResult {0} while processing PasswordReset", result.Type), this.client));
                        responsePacket = new Packet(PacketType.ProcessingError, (new ProcessingError(ErrorType.Unknown)).Serialize());
                        break;
                }
            }
            catch (ProcessingException ex)
            {
                string msg = Logging.GetLogEnvelope(ex.Message, this.client, passwordReset.Avatar, passwordReset.Email);
                Logging.LogMessage(ex.LogLevel, msg);
                responsePacket = new Packet(PacketType.ProcessingError, ex.Error.Serialize());
            }
            return responsePacket;
        }

        private Packet HandlePasswordResetVerification(byte[] data)
        {
            // Deserialize XML request
            PasswordResetVerification passwordResetVerification = new PasswordResetVerification();
            bool deserializationSucceeded = passwordResetVerification.Deserialize(data);
            if (!deserializationSucceeded)
            {
                Log.Warn(Logging.GetLogEnvelope("Received a password reset verification message that could not be deserialized. Processing was aborted.", this.client));
                ProcessingError error = new ProcessingError(ErrorType.DeserializationFailed);
                return new Packet(PacketType.ProcessingError, error.Serialize());
            }

            // Read the client program information
            this.client.ProgramName = passwordResetVerification.ProgramName;
            this.client.ProgramVersion = passwordResetVerification.ProgramVersion;
            this.client.ProgramLocale = passwordResetVerification.ProgramLocale;
            this.client.OptionalInfo = passwordResetVerification.OptionalInfo;

            Packet responsePacket = null;
            try
            {
                ProcessingResult result = this.registrationAuthority.ProcessPasswordResetVerification(client, passwordResetVerification);
                switch (result.Type)
                {
                    case PacketType.CertificateResponse:
                        responsePacket = new Packet(PacketType.CertificateResponse, result.Pkcs12);
                        break;
                    default:
                        Log.Error(Logging.GetLogEnvelope(String.Format("Invalid ClientHandlerResult {0} while processing PasswordResetVerification", result.Type), this.client));
                        responsePacket = new Packet(PacketType.ProcessingError, (new ProcessingError(ErrorType.Unknown)).Serialize());
                        break;
                }
            }
            catch (ProcessingException ex)
            {
                string msg = Logging.GetLogEnvelope(ex.Message, this.client);
                Logging.LogMessage(ex.LogLevel, msg);
                responsePacket = new Packet(PacketType.ProcessingError, ex.Error.Serialize());
            }
            return responsePacket;
        }

        private Packet HandlePasswordChange(byte[] data)
        {
            // Deserialize XML request
            PasswordChange passwordChange = new PasswordChange();
            bool deserializationSucceeded = passwordChange.Deserialize(data);
            if (!deserializationSucceeded)
            {
                Log.Warn(Logging.GetLogEnvelope("Received a password change message that could not be deserialized. Processing was aborted.", this.client));
                ProcessingError error = new ProcessingError(ErrorType.DeserializationFailed);
                return new Packet(PacketType.ProcessingError, error.Serialize());
            }

            // Read the client program information
            this.client.ProgramName = passwordChange.ProgramName;
            this.client.ProgramVersion = passwordChange.ProgramVersion;
            this.client.ProgramLocale = passwordChange.ProgramLocale;
            this.client.OptionalInfo = passwordChange.OptionalInfo;

            Packet responsePacket = null;
            try
            {
                ProcessingResult result = this.directoryServer.ProcessPasswordChange(this.client, passwordChange);
                switch (result.Type)
                {
                    case PacketType.CertificateResponse:
                        responsePacket = new Packet(PacketType.CertificateResponse, result.Pkcs12);
                        break;
                    default:
                        Log.Error(Logging.GetLogEnvelope(String.Format("Invalid ClientHandlerResult {0} while processing PasswordChange", result.Type), this.client));
                        responsePacket = new Packet(PacketType.ProcessingError, (new ProcessingError(ErrorType.Unknown)).Serialize());
                        break;
                }
            }
            catch (ProcessingException ex)
            {
                string msg = Logging.GetLogEnvelope(ex.Message, this.client, passwordChange.Avatar, passwordChange.Email);
                Logging.LogMessage(ex.LogLevel, msg);
                responsePacket = new Packet(PacketType.ProcessingError, ex.Error.Serialize());
            }
            return responsePacket;
        }

        private void HandleClientDisconnect()
        {
            // No more to do at the moment
            Log.Debug(Logging.GetLogEnvelope("Connection closed by client.", this.client));
        }

        #endregion


        #region Handle received packets (HTTP) - mostly obsolete 

        //private void ReceiveHttpPacket(Object o)
        //{
        //    HttpListenerContext context = (HttpListenerContext)o;
        //    HttpListenerRequest request = context.Request;
        //    HttpListenerResponse response = context.Response;
        //    WebPacket webPacket = null;
        //    try
        //    {
        //        // Check HTTP Method
        //        if (!request.HttpMethod.Equals("POST"))
        //        {
        //            Log.Warn("Received a HTTP packet with the invalid method: " + context.Request.HttpMethod);
        //            ProcessingError processingError = new ProcessingError(ErrorType.DeserializationFailed);
        //            webPacket = new WebPacket(context.Response, PacketType.ProcessingError, "application/xml", processingError.Serialize());
        //            webPacket.Send(this.TimeOut);
        //            return;
        //        }

        //        // Check PacketType
        //        string requestType = request.Headers.Get(WebPacket.HEADER_PACKET_TYPE);
        //        if (requestType == null)
        //        {
        //            Log.Warn("The received packet has no request type set!");
        //            ProcessingError processingError = new ProcessingError(ErrorType.DeserializationFailed);
        //            webPacket = new WebPacket(context.Response, PacketType.ProcessingError, "application/xml", processingError.Serialize());
        //            webPacket.Send(this.TimeOut);
        //            return;
        //        }

        //        PacketType packetType = PacketType.Invalid;
        //        int version = -1;
        //        try
        //        {
        //            packetType = PacketTypeCheck.Parse(Byte.Parse(requestType));
        //            string requestVersion = request.Headers.Get(WebPacket.PROTOCOL_VERSION);
        //            version = Int32.Parse(requestVersion);
        //        }
        //        catch (Exception ex)
        //        {
        //            Log.Warn("Error while parsing packet type", ex);
        //            ProcessingError processingError = new ProcessingError(ErrorType.DeserializationFailed);
        //            webPacket = new WebPacket(context.Response, PacketType.ProcessingError, "application/xml", processingError.Serialize());
        //            webPacket.Send(this.TimeOut);
        //            return;
        //        }

        //        // Check protocol version
        //        if (version > WebPacket.PROTOCOL_VERSION)
        //        {
        //            Log.Warn(Logging.GetLogEnvelope("Received a packet with an outdated protocol version. Client was informed to update."));
        //            ProcessingError processingError = new ProcessingError(ErrorType.DeserializationFailed, "Server stopped processing because your client seems to have a newer version. Please report this error to us.");
        //            webPacket = new WebPacket(context.Response, PacketType.ProcessingError, "application/xml", processingError.Serialize());
        //            webPacket.Send(this.TimeOut);
        //            return;
        //        }

        //        if (request.ContentLength64 < 0)
        //        {
        //            Log.Warn(Logging.GetLogEnvelope("Received a packet asserting a negative content length. Processing aborted."));
        //            ProcessingError processingError = new ProcessingError(ErrorType.DeserializationFailed);
        //            webPacket = new WebPacket(context.Response, PacketType.ProcessingError, "application/xml", processingError.Serialize());
        //            webPacket.Send(this.TimeOut);
        //            return;
        //        }

        //        // Check the content
        //        if (!request.HasEntityBody || request.InputStream == null)
        //        {
        //            Log.Warn("Received a HTTP POST packet without data");
        //            ProcessingError processingError = new ProcessingError(ErrorType.DeserializationFailed);
        //            webPacket = new WebPacket(context.Response, PacketType.ProcessingError, "application/xml", processingError.Serialize());
        //            webPacket.Send(this.TimeOut);
        //            return;
        //        }

        //        byte[] requestData = new byte[request.ContentLength64];
        //        int bytesRead = 0;
        //        Log.Debug("ContentLength: " + request.ContentLength64);
        //        Log.Debug("ContentType: " + request.ContentType);
        //        Log.Debug("PacketType: " + request.Headers.Get(WebPacket.HEADER_PACKET_TYPE));
        //        using (Stream stream = request.InputStream)
        //        {
        //            bytesRead = stream.Read(requestData, 0, (int)request.ContentLength64);
        //            stream.Close();
        //        }
        //        if (bytesRead != (int)request.ContentLength64)
        //        {
        //            Log.Warn(Logging.GetLogEnvelope("Received a packet asserting an invalid content length. Processing aborted."));
        //            ProcessingError processingError = new ProcessingError(ErrorType.DeserializationFailed);
        //            webPacket = new WebPacket(context.Response, PacketType.ProcessingError, "application/xml", processingError.Serialize());
        //            webPacket.Send(this.TimeOut);
        //            return;
        //        }

        //        switch (packetType)
        //        {
        //            case PacketType.CertificateRegistration:
        //                webPacket = HandleHttpCertificateRegistration(response, requestData);
        //                break;
        //            case PacketType.EmailVerification:
        //                webPacket = HandleHttpEmailVerification(response, requestData);
        //                break;
        //            case PacketType.CertificateRequest:
        //                webPacket = HandleHttpCertificateRequest(response, requestData);
        //                break;
        //            case PacketType.PasswordReset:
        //                webPacket = HandleHttpPasswordReset(response, requestData);
        //                break;
        //            case PacketType.PasswordResetVerification:
        //                webPacket = HandleHttpPasswordResetVerification(response, requestData);
        //                break;
        //            case PacketType.PasswordChange:
        //                webPacket = HandleHttpPasswordChange(response, requestData);
        //                break;
        //            case PacketType.Disconnect:
        //                HandleClientDisconnect();
        //                return;
        //            default:
        //                throw new InvalidPacketTypeException(String.Format("Received packet type {0} is invalid.", packetType));
        //        }
        //        webPacket.Send(this.TimeOut);
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error(Logging.GetLogEnvelope(ex.Message), ex);
        //    }
        //}

        //private WebPacket HandleHttpCertificateRegistration(HttpListenerResponse response, byte[] data)
        //{
        //    // Deserialize XML request
        //    CertificateRegistration certRegistration = new CertificateRegistration();
        //    bool deserializationSucceeded = certRegistration.Deserialize(data);
        //    if (!deserializationSucceeded)
        //    {
        //        Log.Warn(Logging.GetLogEnvelope("Received a CertificateRegistration message that could not be deserialized. Processing was aborted."));
        //        ProcessingError processingError = new ProcessingError(ErrorType.DeserializationFailed);
        //        return new WebPacket(response, PacketType.ProcessingError, "application/xml", processingError.Serialize());
        //    }

        //    WebPacket responsePacket = null;
        //    ClientHandlerResult result = PerformCertificateRegistration(certRegistration);
        //    switch (result.Type)
        //    {
        //        case PacketType.CertificateAuthorizationRequired:
        //            responsePacket = new WebPacket(response, PacketType.CertificateAuthorizationRequired);
        //            break;
        //        case PacketType.CertificateResponse:
        //            responsePacket = new WebPacket(response, PacketType.CertificateResponse, "application/x-pkcs12", result.Pkcs12);
        //            break;
        //        case PacketType.EmailVerificationRequired:
        //            responsePacket = new WebPacket(response, PacketType.EmailVerificationRequired);
        //            break;
        //        case PacketType.ProcessingError:
        //            responsePacket = new WebPacket(response, PacketType.ProcessingError, "application/xml", result.Error.Serialize());
        //            break;
        //        default:
        //            Log.Error(Logging.GetLogEnvelope(String.Format("Invalid ClientHandlerResult {0} while processing CertificateRegistration", result.Type)));
        //            responsePacket = new WebPacket(response, PacketType.ProcessingError, "application/xml", (new ProcessingError(ErrorType.Unknown)).Serialize());
        //            break;
        //    }
        //    return responsePacket;
        //}

        //private WebPacket HandleHttpEmailVerification(HttpListenerResponse response, byte[] data)
        //{
        //    // Deserialize XML request
        //    EmailVerification emailVerification = new EmailVerification();
        //    bool deserializationSucceeded = emailVerification.Deserialize(data);
        //    if (!deserializationSucceeded)
        //    {
        //        Log.Warn(Logging.GetLogEnvelope("Received an EmailVerification message that could not be deserialized. Processing was aborted."));
        //        ProcessingError processingError = new ProcessingError(ErrorType.DeserializationFailed);
        //        return new WebPacket(response, PacketType.ProcessingError, "application/xml", processingError.Serialize());
        //    }

        //    WebPacket responsePacket = null;
        //    ClientHandlerResult result = PerformEmailVerification(emailVerification);
        //    switch (result.Type)
        //    {
        //        case PacketType.CertificateAuthorizationRequired:
        //            responsePacket = new WebPacket(response, PacketType.CertificateAuthorizationRequired);
        //            break;
        //        case PacketType.CertificateResponse:
        //            responsePacket = new WebPacket(response, PacketType.CertificateResponse, "application/x-pkcs12", result.Pkcs12);
        //            break;
        //        case PacketType.ProcessingError:
        //            responsePacket = new WebPacket(response, PacketType.ProcessingError, "application/xml", result.Error.Serialize());
        //            break;
        //        default:
        //            Log.Error(Logging.GetLogEnvelope(String.Format("Invalid ClientHandlerResult {0} while processing EmailVerification", result.Type)));
        //            responsePacket = new WebPacket(response, PacketType.ProcessingError, "application/xml", (new ProcessingError(ErrorType.Unknown)).Serialize());
        //            break;
        //    }
        //    return responsePacket;
        //}

        //private WebPacket HandleHttpCertificateRequest(HttpListenerResponse response, byte[] data)
        //{
        //    // Deserialize XML request
        //    CT2CertificateRequest certRequest = new CertificateRequest();
        //    bool deserializationSucceeded = certRequest.Deserialize(data);
        //    if (!deserializationSucceeded)
        //    {
        //        Log.Warn(Logging.GetLogEnvelope("Received a certificate request message that could not be deserialized. Processing was aborted."));
        //        ProcessingError error = new ProcessingError(ErrorType.DeserializationFailed);
        //        return new WebPacket(response, PacketType.ProcessingError, "application/xml", error.Serialize());
        //    }

        //    WebPacket responsePacket = null;
        //    ClientHandlerResult result = PerformCertificateRequest(certRequest);
        //    switch (result.Type)
        //    {
        //        case PacketType.CertificateResponse:
        //            responsePacket = new WebPacket(response, PacketType.CertificateResponse, "application/x-pkcs12", result.Pkcs12);
        //            break;
        //        case PacketType.ProcessingError:
        //            responsePacket = new WebPacket(response, PacketType.ProcessingError, "application/xml", result.Error.Serialize());
        //            break;
        //        default:
        //            Log.Error(Logging.GetLogEnvelope(String.Format("Invalid ClientHandlerResult {0} while processing CertificateRequest", result.Type)));
        //            responsePacket = new WebPacket(response, PacketType.ProcessingError, "application/xml", (new ProcessingError(ErrorType.Unknown)).Serialize());
        //            break;
        //    }
        //    return responsePacket;
        //}

        //private WebPacket HandleHttpPasswordReset(HttpListenerResponse response, byte[] data)
        //{
        //    // Deserialize XML request
        //    PasswordReset passwordReset = new PasswordReset();
        //    bool deserializationSucceeded = passwordReset.Deserialize(data);
        //    if (!deserializationSucceeded)
        //    {
        //        Log.Warn(Logging.GetLogEnvelope("Received a PasswordReset message that could not be deserialized. Processing was aborted."));
        //        ProcessingError error = new ProcessingError(ErrorType.DeserializationFailed);
        //        return new WebPacket(response, PacketType.ProcessingError, "application/xml", error.Serialize());
        //    }

        //    WebPacket responsePacket = null;
        //    ClientHandlerResult result = PerformPasswordReset(passwordReset);
        //    switch (result.Type)
        //    {
        //        case PacketType.PasswordResetVerificationRequired:
        //            responsePacket = new WebPacket(response, PacketType.PasswordResetVerificationRequired);
        //            break;
        //        case PacketType.ProcessingError:
        //            responsePacket = new WebPacket(response, PacketType.ProcessingError, "application/xml", result.Error.Serialize());
        //            break;
        //        default:
        //            Log.Error(Logging.GetLogEnvelope(String.Format("Invalid ClientHandlerResult {0} while processing PasswordReset", result.Type)));
        //            responsePacket = new WebPacket(response, PacketType.ProcessingError, "application/xml", (new ProcessingError(ErrorType.Unknown)).Serialize());
        //            break;
        //    }
        //    return responsePacket;
        //}

        //private WebPacket HandleHttpPasswordResetVerification(HttpListenerResponse response, byte[] data)
        //{
        //    // Deserialize XML request
        //    PasswordResetVerification passwordResetVerification = new PasswordResetVerification();
        //    bool deserializationSucceeded = passwordResetVerification.Deserialize(data);
        //    if (!deserializationSucceeded)
        //    {
        //        Log.Warn(Logging.GetLogEnvelope("Received a password reset verification message that could not be deserialized. Processing was aborted."));
        //        ProcessingError error = new ProcessingError(ErrorType.DeserializationFailed);
        //        return new WebPacket(response, PacketType.ProcessingError, "application/xml", error.Serialize());
        //    }

        //    WebPacket responsePacket = null;
        //    ClientHandlerResult result = PerformPasswordResetVerification(passwordResetVerification);
        //    switch (result.Type)
        //    {
        //        case PacketType.CertificateResponse:
        //            responsePacket = new WebPacket(response, PacketType.CertificateResponse, "application/x-pkcs12", result.Pkcs12);
        //            break;
        //        case PacketType.ProcessingError:
        //            responsePacket = new WebPacket(response, PacketType.ProcessingError, "application/xml", result.Error.Serialize());
        //            break;
        //        default:
        //            Log.Error(Logging.GetLogEnvelope(String.Format("Invalid ClientHandlerResult {0} while processing PasswordResetVerification", result.Type)));
        //            responsePacket = new WebPacket(response, PacketType.ProcessingError, "application/xml", (new ProcessingError(ErrorType.Unknown)).Serialize());
        //            break;
        //    }
        //    return responsePacket;
        //}

        //private WebPacket HandleHttpPasswordChange(HttpListenerResponse response, byte[] data)
        //{
        //    // Deserialize XML request
        //    PasswordChange passwordChange = new PasswordChange();
        //    bool deserializationSucceeded = passwordChange.Deserialize(data);
        //    if (!deserializationSucceeded)
        //    {
        //        Log.Warn(Logging.GetLogEnvelope("Received a password change message that could not be deserialized. Processing was aborted."));
        //        ProcessingError error = new ProcessingError(ErrorType.DeserializationFailed);
        //        return new WebPacket(response, PacketType.ProcessingError, "application/xml", error.Serialize());
        //    }

        //    WebPacket responsePacket = null;
        //    ClientHandlerResult result = PerformPasswordChange(passwordChange);
        //    switch (result.Type)
        //    {
        //        case PacketType.CertificateResponse:
        //            responsePacket = new WebPacket(response, PacketType.CertificateResponse, "application/x-pkcs12", result.Pkcs12);
        //            break;
        //        case PacketType.ProcessingError:
        //            responsePacket = new WebPacket(response, PacketType.ProcessingError, "application/xml", result.Error.Serialize());
        //            break;
        //        default:
        //            Log.Error(Logging.GetLogEnvelope(String.Format("Invalid ClientHandlerResult {0} while processing PasswordChange", result.Type)));
        //            responsePacket = new WebPacket(response, PacketType.ProcessingError, "application/xml", (new ProcessingError(ErrorType.Unknown)).Serialize());
        //            break;
        //    }
        //    return responsePacket;
        //}

        #endregion


        #region Private member

        private Configuration config;

        private Configuration.CertificateProgram ProgramConfig;

        private CertificationAuthority certificationAuthority;

        private RegistrationAuthority registrationAuthority;

        private DirectoryServer directoryServer;

        private CertificateDatabase certificateDatabase;

        private ClientInfo client;

        private IProtocol protocol;

        private int timeout;

        private string clientProgramName;

        private string clientProgramVersion;

        private string clientProgramLocale;

        private string clientOptionalInfo;

        #endregion


        #region Properties

        public bool IsConnected { get { return this.protocol.IsConnected; } }

        /// <summary>
        /// Socket times out after TimeOut seconds.
        /// </summary>
        public int Timeout
        {
            get { return timeout; }
            set
            {
                if (value < 1)
                {
                    Log.Warn(String.Format("Time must be greater zero. Using fallback value '{0}' instead", FALLBACK_CLIENT_TIMEOUT));
                    value = FALLBACK_CLIENT_TIMEOUT;
                }
                this.timeout = value;
            }
        }

        #endregion


        #region Events

        public event EventHandler<EventArgs> ConnectionClosed;

        #endregion

    }

    public class ClientInfo
    {
        public ClientInfo(IProtocol protocol)
        {
            this.Protocol = protocol;
            this.RequestType = PacketType.Invalid;
        }

        public ClientInfo(IProtocol protocol, string programName, string programVersion, string programLocale, string optionalInfo)
            :this(protocol)
        {
            this.ProgramName = programName;
            this.ProgramVersion = programVersion;
            this.ProgramLocale = programLocale;
            this.OptionalInfo = optionalInfo;
        }

        public IProtocol Protocol { get; set; }
        public PacketType RequestType { get; set; }
        public string ProgramName { get; set; }
        public string ProgramVersion { get; set; }
        public string ProgramLocale { get; set; }
        public string OptionalInfo { get; set; }
    }

    public class ProcessingResult
    {
        public ProcessingResult(PacketType type)
        {
            this.Type = type;
        }

        public ProcessingResult(PacketType type, byte[] pkcs12)
        {
            this.Type = type;
            this.Pkcs12 = pkcs12;
        }

        public PacketType Type { get; private set; }

        public byte[] Pkcs12 { get; private set; }
    }
}
