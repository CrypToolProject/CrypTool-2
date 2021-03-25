using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrypTool.Util.Logging;
using CrypTool.CertificateLibrary.Network;

namespace CrypTool.CertificateServer
{
    public static class Logging
    {
        public enum LogType
        {
            Debug,
            Info,
            Warn,
            Error
        }

        /// <summary>
        /// Logs the message using the given log level.
        /// </summary>
        /// <param name="logLevel">The log level that should be used</param>
        /// <param name="message">The message that should be logged</param>
        public static void LogMessage(LogType logLevel, string message)
        {
                switch (logLevel)
                {
                    case Logging.LogType.Debug:
                        Log.Debug(message);
                        break;
                    case Logging.LogType.Info:
                        Log.Info(message);
                        break;
                    case Logging.LogType.Warn:
                        Log.Warn(message);
                        break;
                    default:
                        Log.Error(message);
                        break;
                }
        }

        /// <summary>
        /// Returns the log message encapsulated in a standardized format.
        /// </summary>
        /// <param name="msg">The log message</param>
        /// <param name="entry">The Entry object containing the data</param>
        /// <exception cref="ArgumentNullException">entry is null</exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static string GetLogEnvelope(string msg, ClientInfo client, Entry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException("entry", "AbstractEntry can not be null");
            }
            return GetLogEnvelope(msg, client, entry.Avatar, entry.Email, entry.World);
        }

        /// <summary>
        /// Returns the log message encapsulated in a standardized format.
        /// <para>avatar, email, world and clientProgram can be null</para>
        /// </summary>
        /// <param name="msg">The log message</param>
        /// <param name="avatar">The avatar name or null</param>
        /// <param name="email">The email address or null</param>
        /// <param name="world">The world or null</param>
        /// <exception cref="ArgumentNullException">msg is null</exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static string GetLogEnvelope(string msg, ClientInfo client, string avatar = null, string email = null, string world = null)
        {
            if (msg == null)
            {
                throw new ArgumentNullException("Log message and exception can not be both null");
            }
            StringBuilder sb = new StringBuilder();
            sb.Append(msg);
            sb.Append(" [ ");
            if (client != null && client.Protocol != null)
            {
                sb.Append(client.Protocol.GetType().Name);
                sb.Append(" | ");
                sb.Append(client.Protocol.LocalIdentifier);
                sb.Append(" | ");
                sb.Append(client.Protocol.RemoteIdentifier);
                sb.Append(" | ");
                sb.Append(client.RequestType.ToString());
            }
            if (!String.IsNullOrEmpty(avatar))
            {
                sb.Append(" | ");
                sb.Append(avatar);
            }
            if (!String.IsNullOrEmpty(email))
            {
                sb.Append(" | ");
                sb.Append(email);
            }
            if (!String.IsNullOrEmpty(world))
            {
                sb.Append(" | ");
                sb.Append(world);
            }
            if (client != null && !String.IsNullOrEmpty(client.ProgramName))
            {
                sb.Append(" | ");
                sb.Append(client.ProgramName);
            }
            if (client != null && !String.IsNullOrEmpty(client.ProgramVersion))
            {
                sb.Append(" | ");
                sb.Append(client.ProgramVersion);
            }
            sb.Append(" ]");
            return sb.ToString();
        }
    }
}
