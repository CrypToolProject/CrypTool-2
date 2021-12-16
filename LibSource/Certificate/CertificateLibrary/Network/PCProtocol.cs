using CrypTool.CertificateLibrary.Util;
using CrypTool.Util.Logging;
using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace CrypTool.CertificateLibrary.Network
{
    /// <summary>
    /// The Paporator Communication Protocol
    /// </summary>
    public class PCProtocol : IProtocol
    {

        #region Constants

        public const int DEFAULT_TIMEOUT_SEND = 30;

        public const int DEFAULT_TIMEOUT_RECEIVE = 30;

        private const int HEADER_BYTE_SIZE = 6;

        public const byte PROTOCOL_VERSION = 1;

        #endregion


        #region Constructor 

        public PCProtocol(X509Certificate cert)
        {
            if (cert == null)
            {
                throw new ArgumentNullException("cert", "X509Certificate cannot be null");
            }
            Certificate = cert;
            Version = PROTOCOL_VERSION;
        }

        #endregion


        #region Connection Management

        /// <summary>
        /// Establishes a connection to the remote server. The server address can be either an IP address or a DNS name.
        /// The connection will be secured via SSL. Returns true if the connection is successfully established.
        /// </summary>
        /// <param name="remoteAddress">The address of the remote server</param>
        /// <param name="port">The TCP port used to connect</param>
        /// <param name="validationCallback">The method to validate the servers SSL certificate</param>
        /// <returns>true if the connection is established, false otherwise</returns>
        public bool Connect(string remoteAddress, int port, RemoteCertificateValidationCallback validationCallback, HttpTunnel httpTunnel = null)
        {
            try
            {
                if (httpTunnel != null && httpTunnel.TunneledSocket != null && httpTunnel.IsConnected)
                {
                    NetSocket = httpTunnel.TunneledSocket;
                    RemoteEndPoint = NetSocket.RemoteEndPoint;
                    NetStream = httpTunnel.TunneledNetworkStream;
                    Log.Debug("Using existing HTTP tunnel " + NetSocket.RemoteEndPoint.ToString());
                }
                else
                {
                    if (IPAddress.TryParse(remoteAddress, out IPAddress address))
                    {
                        RemoteEndPoint = new IPEndPoint(address, port);
                        NetSocket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                        NetSocket.Connect(RemoteEndPoint);
                    }
                    else
                    {
                        NetSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        NetSocket.Connect(remoteAddress, port);
                        RemoteEndPoint = NetSocket.RemoteEndPoint;
                    }
                    NetStream = new NetworkStream(NetSocket);
                }

                LocalEndPoint = NetSocket.LocalEndPoint;

                SSLstream = new SslStream(NetStream, false, validationCallback, null);
                SSLstream.AuthenticateAsClient(new DNWrapper(Certificate.Subject).CommonName);

                Log.Debug(string.Format("Encrypted connection to server {0} established. Remote TLS subject distinguished name is {1}", RemoteIdentifier, Certificate.Subject));
                return true;
            }
            catch (System.Security.Authentication.AuthenticationException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("PCProtocol: Could not connect to {0}:{1}", remoteAddress, port), ex);
                return false;
            }
        }

        public bool Connect(Socket socket, RemoteCertificateValidationCallback validationCallback)
        {
            if (socket == null)
            {
                throw new ArgumentNullException("socket", "The network socket cannot be null");
            }
            try
            {
                NetSocket = socket;
                RemoteEndPoint = NetSocket.RemoteEndPoint;
                LocalEndPoint = NetSocket.LocalEndPoint;
                NetStream = new NetworkStream(NetSocket);

                SSLstream = new SslStream(NetStream, false, validationCallback, null);
                SSLstream.AuthenticateAsClient(new DNWrapper(Certificate.Subject).CommonName);

                Log.Debug(string.Format("Encrypted connection to server {0} established. Remote TLS subject distinguished name is {1}", RemoteIdentifier, Certificate.Subject));
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("PCProtocol: Could not connect to {0}", RemoteEndPoint), ex);
                return false;
            }
        }

        public bool Connect(NetworkStream stream, RemoteCertificateValidationCallback validationCallback)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream", "The stream cannot be null");
            }
            try
            {
                RemoteEndPoint = NetSocket.RemoteEndPoint;
                LocalEndPoint = NetSocket.LocalEndPoint;
                NetStream = new NetworkStream(NetSocket);

                SSLstream = new SslStream(NetStream, false, validationCallback, null);
                SSLstream.AuthenticateAsClient(new DNWrapper(Certificate.Subject).CommonName);

                Log.Debug(string.Format("Encrypted connection to server {0} established. Remote TLS subject distinguished name is {1}", RemoteIdentifier, Certificate.Subject));
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("PCProtocol: Could not connect to {0}", RemoteEndPoint), ex);
                return false;
            }
        }

        public void Listen(string listenAddress, int port, int backlog)
        {
            if (backlog < 1)
            {
                throw new ArgumentOutOfRangeException("backlog cannot be smaller then one!");
            }

            if (listenAddress.ToLower().Equals("all"))
            {
                LocalEndPoint = new IPEndPoint(IPAddress.Any, port);
            }
            else if (listenAddress.ToLower().Equals("localhost"))
            {
                LocalEndPoint = new IPEndPoint(IPAddress.Loopback, port);
            }
            else
            {
                if (!IPAddress.TryParse(listenAddress, out IPAddress address))
                {
                    throw new ArgumentException("listenAddress is no valid IP address!");
                }

                LocalEndPoint = new IPEndPoint(address, port);
            }

            Listen(LocalEndPoint, backlog);
        }

        public void Listen(EndPoint localEndPoint, int backlog)
        {
            if (backlog < 1)
            {
                throw new ArgumentOutOfRangeException("backlog cannot be smaller then one!");
            }

            NetSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            NetSocket.Bind(localEndPoint);
            NetSocket.Listen(backlog);
            Log.Debug(string.Format("Listening on socket {0}", NetSocket.LocalEndPoint.ToString()));

            bool running = true;
            while (running)
            {
                try
                {
                    Socket clientSocket = NetSocket.Accept();
                    if (ClientConnected != null)
                    {
                        PCProtocol clientProtocol = new PCProtocol(Certificate);
                        clientProtocol.HandleListen(clientSocket);
                        ClientConnected.Invoke(this, new ProtocolEventArgs(clientProtocol));
                    }
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.Interrupted)
                    {
                        running = false;
                    }
                    else
                    {
                        Log.Warn(string.Format("Error while listening on socket {0}", LocalIdentifier), ex);
                    }
                }
                catch (Exception ex)
                {
                    Log.Warn(string.Format("Error while listening on socket {0}", LocalIdentifier), ex);
                }
            }
        }

        private void HandleListen(Socket socket)
        {
            NetSocket = socket;
            RemoteEndPoint = socket.RemoteEndPoint;
            LocalEndPoint = socket.LocalEndPoint;
            NetStream = new NetworkStream(NetSocket);
            SSLstream = new SslStream(NetStream);
            SSLstream.AuthenticateAsServer(Certificate, false, System.Security.Authentication.SslProtocols.Tls, false);
        }

        public void StopListen()
        {
            if (NetSocket == null)
            {
                return;
            }
            try
            {
                NetSocket.Shutdown(SocketShutdown.Both);
            }
            catch
            {
            }
            NetSocket.Close();
        }

        public void Close()
        {
            if (SSLstream != null)
            {
                SSLstream.Close();
            }
            if (Tunnel != null)
            {
                Tunnel.Close();
            }
            if (NetStream != null)
            {
                NetStream.Close();
            }
            if (NetSocket != null)
            {
                try
                {
                    if (NetSocket.Connected)
                    {
                        NetSocket.Shutdown(SocketShutdown.Both);
                    }
                }
                catch
                {
                    // Socket already closed, so we don't mind
                }
                finally
                {
                    NetSocket.Close();
                }
            }
        }

        #endregion


        #region Packet sending and receiving

        /// <summary>
        /// Builds a new Packet and sends it over the stream
        /// <para>| Version | PacketType | Payload | Data         |</para>
        /// <para>| 1 Byte  |   1 Byte   | 4 Byte  | Payload Byte |</para>
        /// <para>Returns false if one of the header fields were incorrect or incompatible.</para>
        /// </summary>
        /// <param name="stream">The stream, the packet will be written to</param>
        /// <returns>true if packet was send</returns>
        public void SendPacket(Packet packet, int timeout = DEFAULT_TIMEOUT_SEND)
        {
            if (packet == null)
            {
                throw new ArgumentNullException("packet", "The packet cannot be null.");
            }
            if (packet.Type == PacketType.Invalid)
            {
                throw new ArgumentException("Can not send an invalid packet.");
            }
            if (packet.Data == null)
            {
                throw new ArgumentException("Can not send packet, because Data was null!");
            }
            if (!SSLstream.CanWrite)
            {
                throw new NetworkStreamException("The OutputStream is not writable!");
            }
            packet.Version = Version;
            SSLstream.WriteTimeout = timeout * 1000;

            try
            {
                int packetSize = HEADER_BYTE_SIZE + packet.Data.Length;
                byte[] buffer = new byte[packetSize];

                // Version
                Array.Copy(new byte[] { Version }, 0, buffer, 0, 1);

                // PacketType
                Array.Copy(new byte[] { (byte)packet.Type }, 0, buffer, 1, 1);

                // Payload
                Array.Copy(BitConverter.GetBytes(packet.Data.Length), 0, buffer, 2, 4);

                // Data
                Array.Copy(packet.Data, 0, buffer, HEADER_BYTE_SIZE, packet.Data.Length);

                int index = 0;
                int bytesLeft = packetSize;
                SSLstream.Write(buffer, index, bytesLeft);
            }
            catch (Exception ex)
            {
                string msg = "Could not send packet!";
                throw new NetworkStreamException(msg, ex);
            }
        }

        /// <summary>
        /// Receives a packet from the network. Will timeout after the specified time in seconds.
        /// This method converts the data from the the underlying protocol (e.g PCP or HTTP) into a Packet.
        /// </summary>
        /// <param name="timeout">Seconds until timeout</param>
        /// <returns>The received packet</returns>
        /// <exception cref="InvalidProtocolVersionException">The protocol version is not supported</exception>
        /// <exception cref="InvalidPacketTypeException">The packet type is invalid</exception>
        /// <exception cref="InvalidPacketFormatException">Packet payload state a negative length</exception>
        /// <exception cref="NetworkStreamException">Error while reading the stream</exception>
        /// <exception cref="UnexpectedEndOfStreamException">Unexpected end of the network stream</exception>
        public Packet ReceivePacket(int timeout = DEFAULT_TIMEOUT_RECEIVE)
        {
            try
            {
                SSLstream.ReadTimeout = timeout * 1000;

                Packet packet = new Packet();
                byte[] header = new byte[HEADER_BYTE_SIZE];

                // Version
                int bytes = SSLstream.Read(header, 0, 1);
                if (bytes != 1)
                {
                    throw new UnexpectedEndOfStreamException("Unexpected end of stream while reading Version!");
                }
                packet.Version = header[0];
                // Enter code for backward compatibility here!
                if (packet.Version != PROTOCOL_VERSION)
                {
                    throw new InvalidProtocolVersionException("Received a network packet with a new network protocol version. Packet deserialization aborted.");
                }

                // PacketType
                bytes = SSLstream.Read(header, 1, 1);
                if (bytes != 1)
                {
                    throw new UnexpectedEndOfStreamException("Unexpected end of stream while reading PacketType!");
                }
                packet.Type = PacketTypeCheck.Parse(header[1]);
                if (packet.Type == PacketType.Invalid)
                {
                    throw new InvalidPacketTypeException("Received an invalid network packet. Packet deserialization aborted.");
                }

                // Payload
                bytes = SSLstream.Read(header, 2, 4);
                if (bytes != 4)
                {
                    throw new UnexpectedEndOfStreamException("Unexpected end of stream while reading Payload!");
                }
                int payload = BitConverter.ToInt32(header, 2);
                if (payload < 0)
                {
                    throw new InvalidPacketFormatException("Received a network packet with a wrong payload value. Packet deserialization aborted.");
                }

                // Data
                packet.Data = new byte[payload];

                if (payload > 0)
                {
                    SSLstream.Read(packet.Data, 0, payload);
                }

                return packet;
            }
            catch (NetworkException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new NetworkStreamException("Could not deserialize packet!", ex);
            }
        }

        #endregion


        #region Properties

        public HttpTunnel Tunnel { get; private set; }

        public EndPoint RemoteEndPoint { get; private set; }

        public EndPoint LocalEndPoint { get; private set; }

        public Socket NetSocket { get; private set; }

        public NetworkStream NetStream { get; private set; }

        public SslStream SSLstream { get; private set; }

        public X509Certificate Certificate { get; private set; }

        public byte Version { get; private set; }

        public string RemoteIdentifier => (RemoteEndPoint != null) ? RemoteEndPoint.ToString() : "";

        public string LocalIdentifier => (LocalEndPoint != null) ? LocalEndPoint.ToString() : "";

        public bool IsConnected
        {
            get
            {
                if (NetSocket != null)
                {
                    return NetSocket.Connected;
                }
                return false;
            }
        }

        #endregion


        #region Events

        public event EventHandler<ProtocolEventArgs> ClientConnected;

        #endregion

    }

}
