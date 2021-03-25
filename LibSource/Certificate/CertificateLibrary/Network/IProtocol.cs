using System;
using System.Net.Security;

namespace CrypTool.CertificateLibrary.Network
{
    public interface IProtocol
    {

        /// <summary>
        /// Establishes a connection to the remote server. The server address can be either an IP address or a DNS name.
        /// The connection will be secured via SSL. Returns true if the connection is successfully established.
        /// </summary>
        /// <param name="remoteAddress">The address of the remote server</param>
        /// <param name="port">The TCP port used to connect</param>
        /// <param name="validationCallback">The method to validate the servers SSL certificate</param>
        /// <param name="httpTunnel">The httpTunnel object to use or null</param>
        /// <returns>true if the connection is established, false otherwise</returns>
        bool Connect(string remoteAddress, int port, RemoteCertificateValidationCallback validationCallback, HttpTunnel httpTunnel = null);

        /// <summary>
        /// Listens on the specified address and port for connecting clients. 
        /// The listen address is depending on the implemented protocol (eg. IP address or a DNS name)
        /// The method should block and handle clients in a loop until StopListen() is called.
        /// On new clients connecting, the ClientConnected event should be triggered.
        /// </summary>
        /// <param name="listenAddress"></param>
        /// <param name="port"></param>
        /// <param name="allowedClients"></param>
        void Listen(string listenAddress, int port, int allowedClients);

        void StopListen();

        /// <summary>
        /// Closes the network connection and all relevant resources.
        /// </summary>
        void Close();

        void SendPacket(Packet packet, int timeout);

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
        Packet ReceivePacket(int timeout);

        HttpTunnel Tunnel { get; }

        /// <summary>
        /// The version number of the protocol. Will be used in every packet that travels the network.
        /// </summary>
        byte Version { get; }

        /// <summary>
        /// An identification string of the remote host. This can be a an IP address or DNS name plus a port or anything else
        /// usable to identify the remote host. The identifier should be used in log messages.
        /// If no connection is established, should contain an empty string.
        /// </summary>
        string RemoteIdentifier { get; }

        /// <summary>
        /// An identification string of the local host. This can be a an IP address or DNS name plus a port or anything else
        /// usable to identify the remote host. The identifier should be used in log messages.
        /// If no connection is established, should contain an empty string.
        /// </summary>
        string LocalIdentifier { get; }

        /// <summary>
        /// Returns true if connected, false otherwise
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// This event should be triggered, when a new client connects to the server. A newly initiated protocol instance is included in the EventArgs.
        /// </summary>
        event EventHandler<ProtocolEventArgs> ClientConnected;

    }
}
