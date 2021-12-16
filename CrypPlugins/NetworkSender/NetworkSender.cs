// Copyright 2014 Christopher Konze, University of Kassel
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#region

using CrypTool.PluginBase;
using CrypTool.PluginBase.IO;
using CrypTool.PluginBase.Miscellaneous;
using NetworkSender;
using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using Timer = System.Timers.Timer;

#endregion

namespace CrypTool.Plugins.NetworkSender
{
    [Author("Mirko Sartorius", "mirkosartorius@web.de", "University of Kassel", "")]
    [PluginInfo("NetworkSender.Properties.Resources", "PluginCaption", "PluginTooltip", "NetworkSender/userdoc.xml", new[] { "NetworkSender/Images/package.png" })]

    [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]
    public class NetworkInput : ICrypComponent
    {
        #region Member

        private const int UpdateSendingRate = 1;
        private readonly NetworkConnectionStore availableConnections = NetworkConnectionStore.Instance;
        private readonly NetworkSenderPresentation presentation;
        private readonly NetworkSenderSettings settings;
        private int sendDataSize;
        private Timer calculateSpeedrate;
        private int packageCount;
        private DateTime startTime;

        #endregion

        public NetworkInput()
        {
            presentation = new NetworkSenderPresentation(this);
            settings = new NetworkSenderSettings();
        }

        #region IPlugin Data Properties

        /// <summary>
        ///   Data to be send inside of a package over a network
        /// </summary>
        [PropertyInfo(Direction.InputData, "StreamInputCaption", "StreamInputTooltip")]
        public ICrypToolStream PackageStream { get; set; }

        /// <summary>
        ///   Socket ID
        /// </summary>
        [PropertyInfo(Direction.InputData, "ConnectionIDInputCaption", "ConnectionIDInputTooltip")]
        public int ConnectionIDInput { get; set; }

        /// <summary>
        ///   Socket ID
        /// </summary>
        [PropertyInfo(Direction.OutputData, "ConnectionIDOutputCaption", "ConnectionIDOutputTooltip")]
        public int ConnectionIDOutput { get; set; }

        /// <summary>
        ///   DestinationIp
        /// </summary>
        [PropertyInfo(Direction.InputData, "DeviceIPCaption", "DeviceIPTooltip")]
        public string DeviceIP { get; set; }

        #endregion

        #region IPlugin Members

        /// <summary>
        ///   Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings => settings;

        /// <summary>
        ///   Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation => presentation;

        #endregion

        #region IPlugin init/stop/dispose

        /// <summary>
        ///   Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize() { }

        /// <summary>
        ///   Called once when plugin is removed from editor workspace.
        /// </summary>
        public void Dispose() { }

        /// <summary>
        ///   Triggered time when user clicks stop button.
        ///   Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
            availableConnections.Stop();
            calculateSpeedrate.Stop();
        }

        #endregion

        #region IPlugin execution lifecycle

        /// <summary>
        ///   Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            ProgressChanged(0, 1);

            //init
            startTime = DateTime.Now;
            packageCount = 0;
            sendDataSize = 0;
            DeviceIP = "";
            ConnectionIDInput = 0;
            ConnectionIDOutput = 0;

            //resets the presentation
            presentation.ClearList();
            presentation.RefreshMetaData(0);
            presentation.SetStaticMetaData(startTime.ToLongTimeString(), settings.Port);

            //start speedrate calculator
            calculateSpeedrate = new Timer { Interval = UpdateSendingRate * 1000 }; // seconds
            calculateSpeedrate.Elapsed += CalculateSpeedrateTick;
            calculateSpeedrate.Start();
        }

        /// <summary>
        ///   Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(1, 100);

            //get or create Connection
            if (ConnectionIDInput == 0)
            {
                NetworkConnection con = TryCreateNewConnection();
                if (con == null)
                {
                    GuiLogMessage("Could not create a network connection", NotificationLevel.Error);
                    return;
                }

                ConnectionIDInput = availableConnections.AddConnection(con);
            }
            NetworkConnection connection = availableConnections.GetConnection(ConnectionIDInput);

            //chop and send data
            using (CStreamReader streamReader = PackageStream.CreateReader())
            {
                byte[] streamBuffer = new byte[65507];
                int bytesRead;
                while ((bytesRead = streamReader.Read(streamBuffer)) > 0)
                {
                    byte[] packetData = new byte[bytesRead];
                    Array.Copy(streamBuffer, packetData, bytesRead);

                    if (TrySendData(connection, packetData, bytesRead))
                    {
                        UpdateOutputs(connection.ID);
                        UpdatePresentation(packetData, connection.RemoteEndPoint);
                    }
                }
            }
            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Tries the send data and returns true if succeeded
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="packetData">The packet data.</param>
        /// <param name="bytesRead">The bytes read.</param>
        /// <returns></returns>
        private bool TrySendData(NetworkConnection connection, byte[] packetData, int bytesRead)
        {
            if (connection is TCPConnection)
            {
                TCPConnection tcpConnection = (connection as TCPConnection);
                TcpClient tcpClient = tcpConnection.TCPClient;

                int retryCount = 0;
                while (!tcpClient.Connected && retryCount < 5)
                {
                    try
                    {
                        tcpClient.Connect(tcpConnection.RemoteEndPoint);
                    }
                    catch (Exception) { }
                    retryCount++;
                    Thread.Sleep(100);
                }

                if (!tcpClient.Connected)
                {
                    GuiLogMessage("Not connected", NotificationLevel.Error);
                    return false;
                }

                tcpClient.GetStream().Write(packetData, 0, bytesRead);

            }
            else
            {
                ((UDPConnection)connection).UDPClient.Send(packetData, packetData.Length, connection.RemoteEndPoint);
            }
            return true;
        }

        /// <summary>
        /// Creates a new connection. Returns null if no connections could been created
        /// </summary> 
        /// <returns></returns>
        private NetworkConnection TryCreateNewConnection()
        {
            string destinationIp = ("".Equals(DeviceIP)) ? settings.DeviceIP : DeviceIP;
            IPEndPoint remoteEndPoint = null;
            if (ValidateIP(destinationIp))
            {
                remoteEndPoint = new IPEndPoint(IPAddress.Parse(destinationIp), settings.Port);
            }
            else
            {
                try
                {
                    IPAddress[] remoteAddress = Dns.GetHostAddresses(destinationIp);
                    if (remoteAddress.Length == 0)
                    {
                        GuiLogMessage(string.Format("Could not get an IP address to given DNS name ({0}).", destinationIp), NotificationLevel.Error);
                        return null;
                    }
                    remoteEndPoint = new IPEndPoint(remoteAddress[0], settings.Port);
                }
                catch (Exception ex)
                {
                    GuiLogMessage(string.Format("Error while creation of Endpoint to IP addresse or DNS name ({0}) : {1}", destinationIp, ex.Message), NotificationLevel.Error);
                    return null;
                }
            }

            NetworkConnection newConnection;
            if (settings.Protocol.Equals(NetworkSenderSettings.udpProtocol))
            {
                newConnection = new UDPConnection
                {
                    RemoteEndPoint = remoteEndPoint,
                    UDPClient = new UdpClient()
                };
            }
            else
            {
                TcpClient client = new TcpClient();
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        client.Connect(remoteEndPoint);
                    }
                    catch (Exception e)
                    {
                        GuiLogMessage("Connection Failed: " + e.Message, NotificationLevel.Error);
                    }
                });
                newConnection = new TCPConnection
                {
                    RemoteEndPoint = remoteEndPoint,
                    TCPClient = client
                };
                Thread.Sleep(100);
            }
            return newConnection;
        }

        /// <summary>
        ///   Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution() { }

        #endregion

        #region IPlugin Updates

        /// <summary>
        /// Updates the presentation.
        /// </summary>
        /// <param name="packetData">The packet data.</param>
        /// <param name="remoteEndPoint"></param>
        private void UpdatePresentation(byte[] packetData, IPEndPoint remoteEndPoint)
        {
            presentation.RefreshMetaData(++packageCount);
            sendDataSize += packetData.Length;
            int length = packetData.Length % 100;
            presentation.AddPresentationPackage(new PresentationPackage
            {
                IPFrom = remoteEndPoint.Address.ToString(),
                Payload = (settings.ByteAsciiSwitch
                    ? Encoding.ASCII.GetString(packetData, 0, length)
                    : BitConverter.ToString(packetData, 0, length)),
                PackageSize = GenerateSizeString(packetData.Length) + "yte"
            });
        }

        /// <summary>
        /// Updates the outputs.
        /// </summary>
        /// <param name="id"></param>
        private void UpdateOutputs(int id)
        {
            if (ConnectionIDOutput != id)
            {
                ConnectionIDOutput = id;
                OnPropertyChanged("ConnectionIDOutput");
            }
        }

        #endregion

        #region Helper Function

        /// <summary>
        ///   tickmethod for the CalculateSpeedrateTick timer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CalculateSpeedrateTick(object sender, EventArgs e)
        {
            int speedrate = sendDataSize / UpdateSendingRate;
            presentation.UpdateSpeedrate(GenerateSizeString(speedrate) + "/s"); // 42kb +"/s"
            sendDataSize = 0;
        }

        /// <summary>
        ///   creates a size string corespornsing to the size of the given amount of bytes with a B or kB ending
        /// </summary>
        /// <returns></returns>
        private static string GenerateSizeString(int size)
        {
            if (size < 1024)
            {
                return size + " B";
            }

            return Math.Round(size / 1024.0, 2) + " kB";
        }

        /// <summary>
        ///   Validates the ipaddress.
        /// </summary>
        /// <param name="ip">The ip.</param>
        /// <returns></returns>
        public static bool ValidateIP(string ip)
        {
            if (string.IsNullOrEmpty(ip))
            {
                return false;
            }

            return IPAddress.TryParse(ip, out IPAddress ipOut);
        }
        #endregion

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        public void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }


        #endregion
    }
}