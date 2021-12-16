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
using CrypTool.Plugins.NetworkSender;
using NetworkReceiver;
using NetworkSender;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Timers;
using System.Windows.Controls;

#endregion

namespace CrypTool.Plugins.NetworkReceiver
{
    [Author("Christopher Konze", "Christopher.Konze@CrypTool.org", "University of Kassel", "http://www.uni-kassel.de/eecs/")]
    [PluginInfo("NetworkReceiver.Properties.Resources", "PluginCaption", "PluginTooltip", "NetworkReceiver/userdoc.xml", new[] { "NetworkReceiver/Images/package.png" })]
    [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]
    public class NetworkReceiver : ICrypComponent
    {
        #region Member

        private const int UpdateReceivingrate = 1;
        private readonly object _lockObject = new object();
        private readonly NetworkConnectionStore availableConnections = NetworkConnectionStore.Instance;
        private readonly NetworkReceiverPresentation presentation;
        private readonly NetworkReceiverSettings settings;

        private Timer calculateSpeedrate;
        private bool isRunning;
        private List<byte[]> lastPackages;
        private IPEndPoint localEndPoint;
        private int receivedPackagesCount;
        private bool returnLastPackage = true;
        private DateTime startTime;
        private TcpListener tcpServer;
        private HashSet<string> uniqueSrcIps;

        public int ReceivedDataSize { [MethodImpl(MethodImplOptions.Synchronized)] get; [MethodImpl(MethodImplOptions.Synchronized)] set; }

        #endregion

        public NetworkReceiver()
        {
            presentation = new NetworkReceiverPresentation(this);
            settings = new NetworkReceiverSettings(this);
        }

        #region IPlugin Data Properties

        [PropertyInfo(Direction.OutputData, "StreamOutputCaption", "StreamOutputTooltip")]
        public ICrypToolStream PackageStream { get; private set; }

        [PropertyInfo(Direction.OutputData, "SingleOutputCaption", "SingleOutputTooltip")]
        public byte[] SingleOutput { get; set; }

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

        #endregion

        #region IPlugin member

        /// <summary>
        ///   Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings => settings;

        /// <summary>
        ///   Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation => presentation;

        #endregion

        #region IPlugin init/dispose

        /// <summary>
        ///   Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
            presentation.ListView.MouseDoubleClick -= MouseDoubleClick;
            presentation.ListView.MouseDoubleClick += MouseDoubleClick;
        }

        /// <summary>
        ///   Called once when plugin is removed from editor workspace.
        /// </summary>
        public void Dispose()
        {
            presentation.ListView.MouseDoubleClick -= MouseDoubleClick;
        }

        #endregion

        #region IPlugin execution lifecycle

        /// <summary>
        ///   Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            ProgressChanged(0, 1);
            localEndPoint = new IPEndPoint(!settings.NetworkDevice ? IPAddress.Parse(settings.DeviceIp) : IPAddress.Any, settings.Port);

            //init or reuse socket
            if (!settings.ConnectionSwitch && settings.Protocol == NetworkSenderSettings.tcpProtocol)
            {
                StartTCPListener();
            }

            //init / reset 
            uniqueSrcIps = new HashSet<string>();
            isRunning = true;
            returnLastPackage = true;
            startTime = DateTime.Now;
            lastPackages = new List<byte[]>();
            receivedPackagesCount = 0;
            ConnectionIDInput = 0;
            ConnectionIDOutput = 0;

            // reset gui
            presentation.ClearPresentation();
            presentation.SetStaticMetaData(startTime.ToLongTimeString(), settings.Port.ToString(CultureInfo.InvariantCulture));

            //start speedrate calculator
            calculateSpeedrate = new Timer { Interval = UpdateReceivingrate * 1000 }; // seconds
            calculateSpeedrate.Elapsed += CalculateSpeedrateTick;
            calculateSpeedrate.Start();
        }

        /// <summary>
        ///   Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            if (!settings.ConnectionSwitch && settings.Protocol == NetworkSenderSettings.udpProtocol)
            {
                UdpClient udpClient = new UdpClient(localEndPoint);
                ConnectionIDInput = availableConnections.AddConnection(new UDPConnection
                {
                    UDPClient = udpClient
                });
            }

            if (ConnectionIDInput == 0) //is still 0?
            {
                return;
            }

            NetworkConnection connection = availableConnections.GetConnection(ConnectionIDInput);
            try
            {
                if (connection is UDPConnection)
                {
                    ReceiveFromUDPClient(connection as UDPConnection);
                }
                else
                {
                    TCPConnection tcpConnection = (TCPConnection)connection;
                    BeginReceivingFromTCPClient(tcpConnection);
                }
                ProgressChanged(0.5, 1);
            }
            catch (SocketException e)
            {
                if (e.ErrorCode != 10004 || isRunning) // if we stop during the socket waits,
                {
                    throw; // we won't show an error message
                }
            }
        }

        /// <summary>
        ///   Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution() { }

        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
            isRunning = false;
            availableConnections.Stop();
            calculateSpeedrate.Stop();
            if (settings.Protocol == NetworkReceiverSettings.tcpProtocol
                && tcpServer != null) // null chk needed for the "use given connection"-mode
            {
                tcpServer.Stop();
            }
        }

        #endregion

        #region IPlugin updates

        private void UpdatePresentation(byte[] data, string ipFrom)
        {
            if (data == null || ipFrom == null)
            {
                return;
            }

            receivedPackagesCount++;
            // package recieved. fill local storage
            if (lastPackages.Count > NetworkReceiverPresentation.MaxStoredPackage)
            {
                lastPackages.RemoveAt(lastPackages.Count - 1);
            }
            else
            {
                lastPackages.Add(data);
            }

            uniqueSrcIps.Add(ipFrom);
            ReceivedDataSize += data.Length;

            // create Package
            int length = data.Length % 100;
            PresentationPackage packet = new PresentationPackage
            {
                PackageSize = generateSizeString(data.Length) + "yte", // 42B + "yte"
                IPFrom = ipFrom,
                Payload = (settings.ByteAsciiSwitch
                    ? Encoding.ASCII.GetString(data, 0, length)
                    : BitConverter.ToString(data, 0, length))
            };

            // update Presentation
            int clientCount = settings.Protocol == 0 ? uniqueSrcIps.Count : 0;
            presentation.UpdatePresentation(packet, receivedPackagesCount, clientCount);
        }

        private void UpdateOutputs(byte[] data, int connectionID)
        {
            using (CStreamWriter streamWriter = new CStreamWriter())
            {
                PackageStream = streamWriter;
                streamWriter.Write(data);
                streamWriter.Close();
            }
            OnPropertyChanged("PackageStream");

            if (returnLastPackage) //change single output if no item is selected
            {
                SingleOutput = data;
                OnPropertyChanged("SingleOutput");
            }

            ConnectionIDOutput = connectionID;
            OnPropertyChanged("ConnectionIDOutput");
        }

        #endregion

        #region networking

        private void StartTCPListener()
        {
            tcpServer = new TcpListener(localEndPoint);
            tcpServer.Start(20);
            tcpServer.BeginAcceptTcpClient(OnClientConnect, tcpServer);
        }

        private void OnClientConnect(IAsyncResult ar)
        {
            try
            {
                TcpListener listener = (TcpListener)ar.AsyncState;
                listener.BeginAcceptTcpClient(OnClientConnect, listener);

                TcpClient client = listener.EndAcceptTcpClient(ar);
                TCPConnection tcpConnection = new TCPConnection
                {
                    RemoteEndPoint = (IPEndPoint)client.Client.RemoteEndPoint,
                    TCPClient = client
                };
                availableConnections.AddConnection(tcpConnection);
                BeginReceivingFromTCPClient(tcpConnection);
            }
            catch (Exception) { }
        }

        private void BeginReceivingFromTCPClient(TCPConnection connection)
        {
            Socket socket = connection.TCPClient.Client;
            StateObject state = new StateObject { Connection = connection };
            socket.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, OnReceivingFromClient, state);
        }

        private void OnReceivingFromClient(IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject)ar.AsyncState;
                Socket socket = state.Connection.TCPClient.Client;

                int bytesRead = socket.EndReceive(ar);
                socket.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, OnReceivingFromClient, state);

                if (bytesRead > 0)
                {
                    lock (_lockObject)
                    {
                        byte[] data = new byte[bytesRead];
                        Array.Copy(state.Buffer, data, bytesRead);
                        UpdatePresentation(data, state.Connection.RemoteEndPoint.Address.ToString());
                        UpdateOutputs(data, state.Connection.ID);
                    }
                }
            }
            catch (Exception) { }
        }

        private void ReceiveFromUDPClient(UDPConnection connection)
        {
            UdpClient udpClient = connection.UDPClient;
            while (isRunning)
            {
                ProgressChanged(1, 1);

                IPEndPoint remotEndPoint = null;
                byte[] data = udpClient.Receive(ref remotEndPoint);
                connection.RemoteEndPoint = remotEndPoint;

                ProgressChanged(0.5, 1);
                lock (_lockObject)
                {
                    UpdatePresentation(data, remotEndPoint.Address.ToString());
                    UpdateOutputs(data, connection.ID);
                }
            }
        }

        #endregion

        #region Helper


        /// <summary>
        ///   creates a size string corespornsing to the size of the given amount of bytes with a B or kB ending
        /// </summary>
        /// <returns></returns>
        private string generateSizeString(int size)
        {
            if (size < 1024)
            {
                return size + " B";
            }

            return Math.Round(size / 1024.0, 2) + " kB";
        }

        /// <summary>
        ///   tickmethod for the CalculateSpeedrateTick timer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CalculateSpeedrateTick(object sender, EventArgs e)
        {
            int speedrate = ReceivedDataSize / UpdateReceivingrate;
            presentation.UpdateSpeedrate(generateSizeString(speedrate) + "/s"); // 42kb +"/s"
            ReceivedDataSize = 0;
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

        private void MouseDoubleClick(object sender, EventArgs e)
        {
            if (-1 < presentation.ListView.SelectedIndex
                && presentation.ListView.SelectedIndex < NetworkReceiverPresentation.MaxStoredPackage)
            {
                returnLastPackage = false;
                SingleOutput = lastPackages[presentation.ListView.SelectedIndex];
                OnPropertyChanged("SingleOutput");
            }
            else
            {
                returnLastPackage = true;
            }
        }

        #endregion
    }
}