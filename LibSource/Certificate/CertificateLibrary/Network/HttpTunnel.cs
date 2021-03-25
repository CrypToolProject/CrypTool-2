using System;
using System.Text;
using System.Net.Sockets;
using System.Net;
using CrypTool.Util.Logging;
using System.IO;

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

            this.DestinationAddress = destinationAddress;
            this.DestinationPort = destinationPort;
            this.Username = proxyUsername;
            this.Password = proxyPassword;
            this.IsConnected = false;

            try
            {
                if (proxyAddress == null && proxyPort == 0)
                {
                    // Get the system proxy
                    Uri destUri = new Uri("https://" + this.DestinationAddress + ":" + this.DestinationPort);
                    Uri proxyUri = WebRequest.GetSystemWebProxy().GetProxy(destUri);

                    this.ProxyAddress = proxyUri.Host;
                    this.ProxyPort = proxyUri.Port;
                }
                else
                {
                    this.ProxyAddress = proxyAddress;
                    this.ProxyPort = proxyPort;
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
            if (this.DestinationPort < 1 || this.DestinationPort > 65535)
            {
                throw new NetworkException("The server port must be a value between 1 and 65535!");
            }
            if (this.ProxyPort < 1 || this.ProxyPort > 65535)
            {
                throw new NetworkException("The proxy port must be a value between 1 and 65535!");
            }

            // If no proxy is configured, the proxy uri is the destination uri
            if (this.DestinationAddress.Equals(this.ProxyAddress) && this.DestinationPort == this.ProxyPort)
            {
                // return true to check whether direct communication works
                Log.Debug("The system has no proxy server configured.");
                return null;
            }

            this.TunneledSocket = null;
            try
            {
                Log.Debug(String.Format("Trying to create a HTTP tunnel to host {0}:{1} using proxy {2}:{3}", this.DestinationAddress, this.DestinationPort, this.ProxyAddress, this.ProxyPort));

                // Create a socket
                this.TunneledSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this.TunneledSocket.Connect(this.ProxyAddress, this.ProxyPort);

                this.TunneledNetworkStream = new NetworkStream(this.TunneledSocket);
                this.TunneledNetworkStream.ReadTimeout = timeout * 1000;

                // Build the HTTP CONNECT Request and send it to the proxy
                this.swriter = new StreamWriter(this.TunneledNetworkStream, new ASCIIEncoding());
                this.swriter.WriteLine("CONNECT " + this.DestinationAddress + ":" + this.DestinationPort + " HTTP/1.1");
                this.swriter.WriteLine("Host: " + this.DestinationAddress + ":" + this.DestinationPort);
                if (!String.IsNullOrWhiteSpace(this.Username) && !string.IsNullOrWhiteSpace(this.Password))
                {
                    this.swriter.WriteLine("Proxy-Authorization: basic " + EncodeCredentials(this.Username, this.Password));
                }
                this.swriter.WriteLine();
                this.swriter.Flush();

                // Parse response (first line example: HTTP/1.1 407 Proxy Authentication Required)
                string protocolVersion = null;
                string statusCodeString = null;
                string message = null;
                this.sreader = new StreamReader(this.TunneledNetworkStream, new ASCIIEncoding());
                // Parse first line
                string line = this.sreader.ReadLine();
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
                if (String.IsNullOrWhiteSpace(message))
                {
                    message = "Proxy provided no status message";
                }

                // rest of the message will be dropped
                do
                {
                    line = this.sreader.ReadLine();
                }
                while (line.Length > 0);

                HttpStatusCode resultCode = (HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), statusCodeString);
                if (resultCode == HttpStatusCode.OK)
                {
                    this.IsConnected = true;
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
            this.IsConnected = false;
            if (this.swriter != null)
            {
                this.swriter.Close();
            }
            if (this.sreader != null)
            {
                this.sreader.Close();
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
