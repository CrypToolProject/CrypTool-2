using System;

namespace CrypTool.CertificateLibrary.Network
{
    public class NetworkException : Exception
    {
        /// <summary>
        /// Connection to the network opposite has failed
        /// </summary>
        public NetworkException()
        {
        }

        /// <summary>
        /// Connection to the network opposite has failed
        /// </summary>
        public NetworkException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Connection to the network opposite has failed
        /// </summary>
        public NetworkException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class NetworkStreamException : NetworkException
    {
        /// <summary>
        /// Error while reading the network stream
        /// </summary>
        public NetworkStreamException()
        {
        }

        /// <summary>
        /// Error while reading the network stream
        /// </summary>
        public NetworkStreamException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Error while reading the network stream
        /// </summary>
        public NetworkStreamException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class NetworkFormatException : NetworkException
    {
        /// <summary>
        /// Format of the network packet is invalid
        /// </summary>
        public NetworkFormatException()
        {
        }

        /// <summary>
        /// Format of the network packet is invalid
        /// </summary>
        public NetworkFormatException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Format of the network packet is invalid
        /// </summary>
        public NetworkFormatException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class UnexpectedEndOfStreamException : NetworkStreamException
    {
        /// <summary>
        /// The stream has less bytes then expected.
        /// </summary>
        public UnexpectedEndOfStreamException()
        {
        }

        /// <summary>
        /// The stream has less bytes then expected.
        /// </summary>
        public UnexpectedEndOfStreamException(string message)
            : base(message)
        {
        }
    }

    public class InvalidProtocolVersionException : NetworkFormatException
    {
        /// <summary>
        /// The packet has an unsupported protocol version.
        /// </summary>
        public InvalidProtocolVersionException()
        {
        }

        /// <summary>
        /// The packet has an unsupported protocol version.
        /// </summary>
        public InvalidProtocolVersionException(string message)
            : base(message)
        {
        }
    }

    public class InvalidPacketTypeException : NetworkFormatException
    {
        /// <summary>
        /// Received an invalid packet for the current system state
        /// </summary>
        public InvalidPacketTypeException()
        {
        }

        /// <summary>
        /// Received an invalid packet for the current system state
        /// </summary>
        public InvalidPacketTypeException(string message)
            : base(message)
        {
        }
    }

    public class InvalidPacketFormatException : NetworkFormatException
    {
        /// <summary>
        /// Received an invalid error type for the current system state
        /// </summary>
        public InvalidPacketFormatException()
        {
        }

        /// <summary>
        /// Received an invalid error type for the current system state
        /// </summary>
        public InvalidPacketFormatException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Received an invalid error type for the current system state
        /// </summary>
        public InvalidPacketFormatException(string message, Exception ex)
            : base(message, ex)
        {
        }
    }
}
