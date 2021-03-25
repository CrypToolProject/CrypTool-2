using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrypTool.CertificateLibrary.Network;

namespace CrypTool.CertificateServer
{
    public class ProcessingException : Exception
    {

        public ProcessingException(Logging.LogType logLevel, string logMessage, ProcessingError error, string clientInformation = null)
            :base(logMessage)
        {
            this.LogLevel = logLevel;
            this.Error = error;
            this.ClientInformation = clientInformation;
        }

        public Logging.LogType LogLevel { get; private set; }

        public ProcessingError Error { get; private set; }

        public string ClientInformation { get; private set; }
    }

    public class DatabaseException : Exception
    {
        /// <summary>
        /// Represents a database error
        /// </summary>
        public DatabaseException()
        {
        }

        /// <summary>
        /// Represents a database error
        /// </summary>
        public DatabaseException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Represents a database error
        /// </summary>
        public DatabaseException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class InvalidDataException : Exception
    {
        /// <summary>
        /// The received packet data is not valid.
        /// </summary>
        public InvalidDataException()
        {
        }

        /// <summary>
        /// The received packet data is not valid.
        /// </summary>
        public InvalidDataException(string message)
            : base(message)
        {
        }
    }
}
