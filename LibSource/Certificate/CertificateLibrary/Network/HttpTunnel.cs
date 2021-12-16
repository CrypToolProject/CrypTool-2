using CrypTool.Util.Logging;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CrypTool.CertificateLibrary.Network
{
    public class HttpTunnel
    {

        #region Constructor

        /// <summary>
        /// Creates a HTTP tunnel using the proxy server configured by the system (Internet Explorer options).
        /// </summary>
        /// <param name="destinationAddress">The destination address</param>
        /// <param name="destinationPort">The destination port</param>
        /// <param name="proxyUsername">The username used to authenticate at the proxy</param>
        /// <param name="proxyPassword">The password used to authenticate at the proxy</param>
        /// <exception cref="ArgumentNullException">destinationAddress is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">destinationPort is out of port range</exception>
        public HttpTunnel(string destinationAddress, int destinationPort, string proxyUsername = null, string proxyPassword = null)
            : this(destinationAddress, destinationPort, null, 0, proxyUsername, proxyPassword)
        {
        }

        /// <summary>
        /// Creates a HTTP tunnel using the proxy server configured by the system (Internet Explorer options).
        /// </summary>
        /// <param name="destinationAddress">The destination address</param>
        /// <param name="destinationPort">The destination port</param>
        /// <param name="proxyAddress">Hostname or IP of the proxy</param>
        /// <param name="proxyPort">Port of the proxy</param>
        /// <param name="proxyUsername">The username used to authenticate at the proxy</param>
        /// <param name="proxyPassword">The password used to authenticate at the proxy</param>
        /// <exception cref="ArgumentNullException">destinationAddress is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">destinationPort is out of port range</exception>
        /// <exception cref="NetworkException">Could not determine system proxy, as the destination address is invalid</exception>
        public HttpTunnel(string destinationAddress, int destinationPort, string proxyAddress, int proxyPort, string proxyUsername, string proxyPassword)
        {
            if (destinationAddress == null)
            {
                throw new ArgumentNullException("destinationAddress", "The destination address cannot be null");
            }
            if (destinationPort < 1 || destinationPort > 65535)
            {
                throw new ArgumentOutOfRangeException("You must specify a correct destination port.");
            }

            DestinationAddress = destinationAddress;
            DestinationPort = destinationPort;
            Username = proxyUsername;
            Password = proxyPassword;
            IsConnected = false;

            try
            {
                if (proxyAddress == null && proxyPort == 0)
                {
                    // Get the system proxy
                    Uri destUri = new Uri("https://" + DestinationAddress + ":" + DestinationPort);
                    Uri proxyUri = WebRequest.GetSystemWebProxy().GetProxy(destUri);

                    ProxyAddress = proxyUri.Host;
                    ProxyPort = proxyUri.Port;
                }
                else
                {
                    ProxyAddress = proxyAddress;
                    ProxyPort = proxyPort;
                }
            }
            catch (Exception ex)
            {
                throw new NetworkException("Could not determine system proxy", ex);
            }
        }

        #endregion


        #region Tunnel Management

        /// <summary>
        /// Creates a HTTP tunnel using the proxy server given proxy server (Hostname or IP).
        /// Returns null if no proxy is configured.
        /// <para>In general, proxies will deny connections to destination ports unequal to 443</para>
        /// </summary>
        /// <param name="timeout">The timeout in seconds</param>
        /// <returns>The HTTP response received from the proxy</returns>
        /// <exception cref="NetworkException">Error while creating the HTTP tunnel</exception>
        public ProxyEventArgs CreateTunnel(int timeout)
        {
            if (DestinationPort < 1 || DestinationPort > 65535)
            {
                throw new NetworkException("The server port must be a value between 1 and 65535!");
            }
            if (ProxyPort < 1 || ProxyPort > 65535)
            {
                throw new NetworkException("The proxy port must be a value between 1 and 65535!");
            }

            // If no proxy is configured, the proxy uri is the destination uri
            if (DestinationAddress.Equals(ProxyAddress) && DestinationPort == ProxyPort)
            {
                // return true to check whether direct communication works
                Log.Debug("The system has no proxy server configured.");
                return null;
            }

            TunneledSocket = null;
            try
            {
                Log.Debug(string.Format("Trying to create a HTTP tunnel to host {0}:{1} using proxy {2}:{3}", DestinationAddress, DestinationPort, ProxyAddress, ProxyPort));

                // Create a socket
                TunneledSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                TunneledSocket.Connect(ProxyAddress, ProxyPort);

                TunneledNetworkStream = new NetworkStream(TunneledSocket)
                {
                    ReadTimeout = timeout * 1000
                };

                // Build the HTTP CONNECT Request and send it to the proxy
                swriter = new StreamWriter(TunneledNetworkStream, new ASCIIEncoding());
                swriter.WriteLine("CONNECT " + DestinationAddress + ":" + DestinationPort + " HTTP/1.1");
                swriter.WriteLine("Host: " + DestinationAddress + ":" + DestinationPort);
                if (!string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password))
                {
                    swriter.WriteLine("Proxy-Authorization: basic " + EncodeCredentials(Username, Password));
                }
                swriter.WriteLine();
                swriter.Flush();

                // Parse response (first line example: HTTP/1.1 407 Proxy Authentication Required)
                string protocolVersion = null;
                string statusCodeString = null;
                string message = null;
                sreader = new StreamReader(TunneledNetworkStream, new ASCIIEncoding());
                // Parse first line
                string line = sreader.ReadLine();
                int index1 = line.IndexOf(" ");
                if (index1 == -1)
                {
                    throw new InvalidPacketFormatException("Received an invalid HTTP Response.");
                }
                protocolVersion = line.Substring(0, index1);

                // Status code is 3 digit (plus one whitespace)
                if (line.Length < index1 + 4)
                {
                    throw new InvalidPacketFormatException("Received an invalid HTTP Response.");
                }
                statusCodeString = line.Substring(index1 + 1, 3);

                // Message is everything behind
                message = line.Substring(index1 + 4);
                if (string.IsNullOrWhiteSpace(message))
                {
                    message = "Proxy provided no status message";
                }

                // rest of the message will be dropped
                do
                {
                    line = sreader.ReadLine();
                }
                while (line.Length > 0);

                HttpStatusCode resultCode = (HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), statusCodeString);
                if (resultCode == HttpStatusCode.OK)
                {
                    IsConnected = true;
                }
                return new ProxyEventArgs(resultCode, message);
            }
            catch (Exception ex)
            {
                throw new NetworkException("Could not create HTTP tunnel!", ex);
            }
        }

        /// <summary>
        /// Closes the StreamReader and StreamWriter used to initialize the tunnel
        /// </summary>
        public void Close()
        {
            IsConnected = false;
            if (swriter != null)
            {
                swriter.Close();
            }
            if (sreader != null)
            {
                sreader.Close();
            }
        }

        #endregion


        #region Encoding

        /// <summary>
        /// Encodes the string username:password in Base64. This format is demanded by proxies.
        /// </summary>
        /// <param name="proxyUser">The username to authenticate at the proxy</param>
        /// <param name="proxyPwd">The password to authenticate at the proxy</param>
        /// <returns>The encoded string</returns>
        private static string EncodeCredentials(string proxyUser, string proxyPwd)
        {
            if (proxyUser == null)
            {
                throw new ArgumentNullException("proxyUser", "The proxy username cannot be null");
            }
            if (proxyPwd == null)
            {
                throw new ArgumentNullException("proxyPwd", "The proxy password cannot be null");
            }

            string credential = proxyUser + ":" + proxyPwd;
            byte[] data = System.Text.UnicodeEncoding.UTF8.GetBytes(credential);
            return Convert.ToBase64String(data);
        }

        #endregion


        #region Private member

        private StreamWriter swriter;

        private StreamReader sreader;

        #endregion


        #region Properties

        public bool IsConnected { get; private set; }

        public string DestinationAddress { get; set; }

        public int DestinationPort { get; set; }

        public string ProxyAddress { get; set; }

        public int ProxyPort { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public Socket TunneledSocket { get; private set; }

        public NetworkStream TunneledNetworkStream { get; private set; }

        #endregion

    }
}
