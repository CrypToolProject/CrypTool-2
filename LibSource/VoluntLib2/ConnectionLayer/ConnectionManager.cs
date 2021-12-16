/*
   Copyright 2018 Nils Kopal <Nils.Kopal<AT>Uni-Kassel.de>

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using VoluntLib2.ConnectionLayer.Messages;
using VoluntLib2.ConnectionLayer.Operations;
using VoluntLib2.Tools;

namespace VoluntLib2.ConnectionLayer
{
    /// <summary>
    /// The ConnectionManager is responsible for the "low level" connections between our nodes.
    /// It takes care that a node has a minimum number of "udp connections" to different peers.
    /// Furthermore, it forwards data between our peers.
    /// It implements a "simple hole punching protocol".
    /// </summary>
    internal class ConnectionManager
    {
        private readonly Logger Logger = Logger.GetLogger();

        //all known contacts of this peer
        internal ConcurrentDictionary<IPEndPoint, Contact> Contacts = new ConcurrentDictionary<IPEndPoint, Contact>();

        //all contacts received by neighbors
        internal ConcurrentDictionary<IPEndPoint, Contact> ReceivedContacts = new ConcurrentDictionary<IPEndPoint, Contact>();

        //a queue containing all operations
        internal ConcurrentQueue<Operation> Operations = new ConcurrentQueue<Operation>();

        //a queue containing all received DataMessages
        internal ConcurrentQueue<DataMessage> DataMessagesIngoing = new ConcurrentQueue<DataMessage>();

        //a queue containing all to be sended DataMessages
        internal ConcurrentQueue<DataMessage> DataMessagesOutgoing = new ConcurrentQueue<DataMessage>();

        //a dictionary containing all external ip addresse
        internal ConcurrentDictionary<IPAddress, DateTime> ExternalIpAddresses = new ConcurrentDictionary<IPAddress, DateTime>();

        //Port where this ConnectionManager listens on
        internal ushort Port = 0;

        //unique randomly chosen number identifying this peer
        private readonly byte[] PeerId = Guid.NewGuid().ToByteArray();

        private bool Running = false;
        private UdpClient Client;                   // udp client for sending/receiving
        private Thread ReceivingThread;             // responsible thread for receiving UDP packets
        private Thread WorkerThread;                // responsible thread for execution of operations

        //a list containing our bootstrap peers
        private readonly List<Contact> WellKnownPeers = new List<Contact>();

        internal VoluntLib VoluntLib { get; set; }

        /// <summary>
        /// Number of connection changed
        /// </summary>
        public event EventHandler<ConnectionsNumberChangedEventArgs> ConnectionsNumberChanged;

        /// <summary>
        /// Observable list of contacts for UI
        /// </summary>
        internal ObservableCollection<Contact> ObservableContactList;

        /// <summary>
        /// Creates a new ConnectionManager listening on the given UDP port
        /// </summary>
        /// <param name="listenport"></param>
        public ConnectionManager(VoluntLib voluntLib, ushort listenport)
        {
            VoluntLib = voluntLib;
            Port = listenport;
            //if we are in a WPF application, we create the ObservableCollection in UI thread
            if (Application.Current != null)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    ObservableContactList = new ObservableItemsCollection<Contact>();
                }));
            }
            else
            {
                ObservableContactList = new ObservableItemsCollection<Contact>();
            }

        }

        /// <summary>
        /// Starts the Connection Manager
        /// It will listen to the listen port and try to establish P2P connections
        /// to other hosts
        /// </summary>
        public void Start()
        {
            if (Running)
            {
                throw new InvalidOperationException("The ConnectionManager is already running!");
            }
            Logger.LogText("Starting the ConnectionManager", this, Logtype.Info);
            //Clear complete data of this ConnectionManager; thus, it can be restarted
            Contacts.Clear();
            ReceivedContacts.Clear();
            Operations = new ConcurrentQueue<Operation>();
            DataMessagesIngoing = new ConcurrentQueue<DataMessage>();
            DataMessagesOutgoing = new ConcurrentQueue<DataMessage>();

            //Create a new udp client for sending and receiving data            
            Client = new UdpClient(new IPEndPoint(IPAddress.Any, Port));
            Client.Client.ReceiveTimeout = Constants.CONNECTIONMANAGER_RECEIVE_TIMEOUT;
            Client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            Client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.IpTimeToLive, 255);
            Client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            Client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontRoute, 1);

            //Set Running to true; thus, threads know we are alive
            Running = true;
            //Create a thread for receving data
            ReceivingThread = new Thread(HandleIncomingPackets)
            {
                Name = "ConnectionManagerReceivingThread",
                IsBackground = true
            };
            ReceivingThread.Start();
            //Create a thread for the operations
            WorkerThread = new Thread(ConnectionManagerWork)
            {
                Name = "ConnectionManagerWorkerThread",
                IsBackground = true
            };
            WorkerThread.Start();

            //This operation is responsible for answering HelloMessages:
            Operations.Enqueue(new HelloResponseOperation() { ConnectionManager = this });
            //This operation is responsilbe for answering neighbor list requests
            Operations.Enqueue(new ResponseNeighborListOperation() { ConnectionManager = this });
            //this operation requests every 5 minutes all neighbors from our contacts
            Operations.Enqueue(new AskForNeighborListsOperation() { ConnectionManager = this });
            //This operation is responsible for checking our contacts. It automatically creates HelloOperations for the contacts
            Operations.Enqueue(new CheckContactsOperation() { ConnectionManager = this });
            //this operation tries to help peers to connect to others
            Operations.Enqueue(new HelpWithConnectionOperation() { ConnectionManager = this });
            //this operation waits for WantsConnectionMessages and initiates HelloOperations
            Operations.Enqueue(new WantsConnectionOperation() { ConnectionManager = this });
            //add a CheckMyConnectionsNumber which takes care that we have a minimum number of connections and don't go over a maximum number of connections
            Operations.Enqueue(new CheckMyConnectionsNumberOperation() { ConnectionManager = this });
            //add a SendDataOperation which sends data messages to all peers
            Operations.Enqueue(new SendDataOperation() { ConnectionManager = this });
            //add a ReceiveDataOperation which handles received DataMessages
            Operations.Enqueue(new ReceiveDataOperation() { ConnectionManager = this });
            //add a BootstrapOperation which is responsible for initial connections; it will always bootstrap if we have 0 connections
            Operations.Enqueue(new BootstrapOperation(WellKnownPeers) { ConnectionManager = this });
            //add a GoingOfflineOperation which is responsible to remove peers that send GoingOfflineMessages
            Operations.Enqueue(new GoingOfflineOperation() { ConnectionManager = this });
            //add a HousekeepReceivedNeighborsOperation which is responsible to remove peers received by neighbors whose KnownBys are all offline
            Operations.Enqueue(new HousekeepReceivedNeighborsOperation() { ConnectionManager = this });
            //add a HouseKeepExternalIPAddresses which is responsible to remove external ip addresses that we did not see for 5 minutes
            Operations.Enqueue(new HouseKeepExternalIPAddressesOperation() { ConnectionManager = this });

            //shows the state of this peer every 5 seconds (displays the current number of connections)
            //state is only shown, when number of peers changed
            Operations.Enqueue(new MyStatusOperation() { ConnectionManager = this });

            Logger.LogText(string.Format("ConnectionManager started and listening to UDP port {0} now.", Port), this, Logtype.Info);
        }

        /// <summary>
        /// Ad a well known peer with open nat for bootstrapping
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public void AddWellknownPeer(IPAddress ip, ushort port)
        {
            WellKnownPeers.Add(new Contact() { IPAddress = ip, Port = port });
        }

        /// <summary>
        /// Main method of the thread of the ConnectionManager
        /// </summary>
        private void HandleIncomingPackets()
        {
            Logger.LogText("ReceivingThread started", this, Logtype.Info);
            while (Running)
            {
                try
                {
                    IPEndPoint remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);
                    byte[] data = Client.Receive(ref remoteEndpoint);
                    bool ignore = false;
                    foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
                    {
                        foreach (UnicastIPAddressInformation unicastIPAddressInformation in networkInterface.GetIPProperties().UnicastAddresses)
                        {
                            if (IPAddress.Equals(remoteEndpoint.Address, unicastIPAddressInformation.Address))
                            {
                                //we ignore messages, that we receive from ourselves (= our own ips), i.e. broadcast messages
                                ignore = true;
                                goto next; // hack to leave the foreach-loops
                            }
                        }
                    }
                next:
                    if (ignore)
                    {
                        continue;
                    }
                    Logger.LogText(string.Format("Data from {0}:{1} : {2} bytes", remoteEndpoint.Address, remoteEndpoint.Port, data.Length), this, Logtype.Debug);

                    Message message = null;
                    try
                    {
                        message = MessageHelper.Deserialize(data);
                        message.MessageHeader.SenderIPAddress = remoteEndpoint.Address.GetAddressBytes();
                        message.MessageHeader.SenderExternalPort = (ushort)remoteEndpoint.Port;
                        Logger.LogText(string.Format("Received a {0} from {1}.", message.MessageHeader.MessageType.ToString(), remoteEndpoint.Address + ":" + remoteEndpoint.Port), this, Logtype.Debug);

                        try
                        {
                            //we memorize the "receiver ip address" since these are our external addresses
                            UpdateExternalIPAddresses(message.MessageHeader.ReceiverIPAddress);
                            //we received a message from this peer, thus, we can update our contact
                            UpdateContact(new IPAddress(message.MessageHeader.SenderIPAddress), message.MessageHeader.SenderExternalPort, message.MessageHeader.SenderPeerId);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogText(string.Format("Exception occured during update of contact: {0}", ex.Message), this, Logtype.Error);
                            Logger.LogException(ex, this, Logtype.Error);
                        }

                    }
                    catch (VoluntLibSerializationException vl2mdex)
                    {
                        Logger.LogText(string.Format("Message could not be deserialized: {0}", vl2mdex.Message), this, Logtype.Warning);
                        Logger.LogException(vl2mdex, this, Logtype.Warning);
                        continue;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogText(string.Format("Exception during deserialization: {0}", ex.Message), this, Logtype.Error);
                        Logger.LogException(ex, this, Logtype.Error);
                        continue;
                    }

                    try
                    {
                        Task.Factory.StartNew(() =>
                        {
                            try
                            {
                                HandleMessage(message);
                            }
                            catch (Exception ex)
                            {
                                Logger.LogText(string.Format("Exception during message handling: {0}", ex.Message), this, Logtype.Error);
                                Logger.LogException(ex, this, Logtype.Error);
                            }
                        }
                        );

                    }
                    catch (Exception ex)
                    {
                        Logger.LogText(string.Format("Exception creating a message handling thread: {0}", ex.Message), this, Logtype.Error);
                        Logger.LogException(ex, this, Logtype.Error);
                        continue;
                    }

                }
                catch (SocketException)
                {
                    //do nothing
                }
                catch (Exception ex)
                {
                    Logger.LogText(string.Format("Uncaught exception in HandleIncomingPackets(). Terminate now! {0}", ex.Message), this, Logtype.Error);
                    Logger.LogException(ex, this, Logtype.Error);
                    Running = false;
                }
            }
            Logger.LogText("ReceivingThread terminated", this, Logtype.Info);
        }

        /// <summary>
        /// Memorizes our external ip addresses
        /// </summary>
        /// <param name="ipbytes"></param>
        private void UpdateExternalIPAddresses(byte[] ipbytes)
        {
            try
            {
                IPAddress ip = new IPAddress(ipbytes);
                if (!ExternalIpAddresses.ContainsKey(ip))
                {
                    ExternalIpAddresses.TryAdd(ip, DateTime.Now);
                }
                else
                {
                    ExternalIpAddresses[ip] = DateTime.Now;
                }
            }
            catch (Exception)
            {
                //wtf?
            }
        }

        /// <summary>
        /// This method runs in a seperate thread.
        /// It iterates over all operations and calls their execute-methods
        /// It also removes operations that are finished
        /// </summary>
        private void ConnectionManagerWork()
        {
            Logger.LogText("WorkerThread started", this, Logtype.Info);
            while (Running)
            {
                try
                {
                    if (Operations.TryDequeue(out Operation operation) == true)
                    {
                        // before we execute an operation, we check if it is finished
                        if (operation.IsFinished == false)
                        {
                            //operations that are not finished are enqueued again
                            Operations.Enqueue(operation);
                        }
                        else
                        {
                            Logger.LogText(string.Format("Operation {0}-{1} has finished. Removed it.", operation.GetType().FullName, operation.GetHashCode()), this, Logtype.Debug);
                            //we dont execute this operation since it is finished
                            continue;
                        }

                        try
                        {
                            operation.Execute();
                        }
                        catch (Exception ex)
                        {
                            Logger.LogText(string.Format("Exception during execution of operation {0}-{1}: {2}", operation.GetType().FullName, operation.GetHashCode(), ex.Message), this, Logtype.Error);
                            Logger.LogException(ex, this, Logtype.Error);
                        }

                    }
                }
                catch (Exception ex)
                {
                    Logger.LogText(string.Format("Exception during handling of operation: {0}", ex.Message), this, Logtype.Error);
                    Logger.LogException(ex, this, Logtype.Error);
                }
                try
                {
                    Thread.Sleep(Constants.CONNECTIONMANAGER_WORKER_THREAD_SLEEPTIME);
                }
                catch (Exception ex)
                {
                    Logger.LogText(string.Format("Exception during sleep of thread: {0}", ex.Message), this, Logtype.Error);
                    Logger.LogException(ex, this, Logtype.Error);
                }
            }
            Logger.LogText("WorkerThread terminated", this, Logtype.Info);
        }

        /// <summary>
        /// Handles a given message by calling the apropriate methods for each message type
        /// </summary>
        /// <param name="message"></param>
        private void HandleMessage(Message message)
        {
            foreach (Operation operation in Operations)
            {
                try
                {
                    operation.HandleMessage(message);
                }
                catch (Exception ex)
                {
                    Logger.LogText(string.Format("Exception during execution of HandleMessage of operation: {0}", ex.Message), this, Logtype.Error);
                    Logger.LogException(ex, this, Logtype.Error);
                }
            }
        }

        /// <summary>
        /// Send a given message to the ip and port in a single UDP datagram
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="data"></param>
        internal void SendData(IPAddress ip, int port, byte[] data)
        {
            if (data.Length > Constants.CONNECTIONMANAGER_MAX_UDP_MESSAGE_PAYLOAD_SIZE)
            {
                throw new Exception(string.Format("Given message size is too long. Got {0} byte, but max size is {1} bytes!", data.Length, Constants.CONNECTIONMANAGER_MAX_UDP_MESSAGE_PAYLOAD_SIZE));
            }
            lock (this)
            {
                if (!IPAddress.Equals(ip, IPAddress.Broadcast))
                {
                    Client.Send(data, data.Length, new IPEndPoint(ip, port));
                }
                else
                {
                    //send broadcast to all interfaces
                    foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
                    {
                        if (networkInterface.OperationalStatus == OperationalStatus.Up && networkInterface.SupportsMulticast && networkInterface.GetIPProperties().GetIPv4Properties() != null)
                        {
                            int index = networkInterface.GetIPProperties().GetIPv4Properties().Index;
                            if (NetworkInterface.LoopbackInterfaceIndex != index)
                            {
                                foreach (UnicastIPAddressInformation uip in networkInterface.GetIPProperties().UnicastAddresses)
                                {
                                    if (uip.Address.AddressFamily == AddressFamily.InterNetwork)
                                    {
                                        IPEndPoint target = new IPEndPoint(IPAddress.Broadcast, Port);
                                        Client.Send(data, data.Length, target);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Send a HelloMessage to ip/port with the specific nonce
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="nonce"></param>
        internal void SendHello(IPAddress ip, ushort port, byte[] nonce)
        {
            HelloMessage helloMessage = new HelloMessage();
            helloMessage.MessageHeader.SenderPeerId = PeerId;
            helloMessage.MessageHeader.ReceiverIPAddress = ip.GetAddressBytes();
            helloMessage.MessageHeader.ReceiverExternalPort = port;
            helloMessage.HelloNonce = nonce;
            SendData(ip, port, helloMessage.Serialize());
        }

        /// <summary>
        /// Send a HelloResponseMessage to ip/port with the specific nonce
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="nonce"></param>
        internal void SendHelloResponse(IPAddress ip, ushort port, byte[] nonce)
        {
            HelloResponseMessage helloResponseMessage = new HelloResponseMessage();
            helloResponseMessage.MessageHeader.SenderPeerId = PeerId;
            helloResponseMessage.MessageHeader.ReceiverIPAddress = ip.GetAddressBytes();
            helloResponseMessage.MessageHeader.ReceiverExternalPort = port;
            helloResponseMessage.HelloResponseNonce = nonce;
            SendData(ip, port, helloResponseMessage.Serialize());
        }

        /// <summary>
        /// Send a RequestNeighborListMessage to ip/port with the specific nonce
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="nonce"></param>
        internal void SendRequestNeighborListMessage(IPAddress ip, ushort port, byte[] nonce)
        {
            RequestNeighborListMessage requestNeighborListMessage = new RequestNeighborListMessage();
            requestNeighborListMessage.MessageHeader.SenderPeerId = PeerId;
            requestNeighborListMessage.MessageHeader.ReceiverIPAddress = ip.GetAddressBytes();
            requestNeighborListMessage.MessageHeader.ReceiverExternalPort = port;
            requestNeighborListMessage.RequestNeighborListNonce = nonce;
            SendData(ip, port, requestNeighborListMessage.Serialize());
        }

        /// <summary>
        /// Send a ResponseNeighborList to ip/port with the specific nonce and the given contact list
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="nonce"></param>
        internal void SendResponseNeighborList(IPAddress ip, ushort port, byte[] nonce, List<Contact> contacts)
        {
            ResponseNeighborListMessage responseNeighborListMessage = new ResponseNeighborListMessage();
            responseNeighborListMessage.Neighbors.AddRange(contacts);
            responseNeighborListMessage.MessageHeader.SenderPeerId = PeerId;
            responseNeighborListMessage.MessageHeader.ReceiverIPAddress = ip.GetAddressBytes();
            responseNeighborListMessage.MessageHeader.ReceiverExternalPort = port;
            responseNeighborListMessage.ResponseNeighborListNonce = nonce;
            SendData(ip, port, responseNeighborListMessage.Serialize());
        }

        /// <summary>
        /// Send a HelpMeConnectMessage to ip/port with specific topip/toport
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="toip"></param>
        /// <param name="toport"></param>
        internal void SendHelpMeConnectMessage(IPAddress ip, ushort port, IPAddress toip, ushort toport)
        {
            HelpMeConnectMessage helpMeConnectMessage = new HelpMeConnectMessage();
            helpMeConnectMessage.MessageHeader.SenderPeerId = PeerId;
            helpMeConnectMessage.MessageHeader.ReceiverIPAddress = ip.GetAddressBytes();
            helpMeConnectMessage.MessageHeader.ReceiverExternalPort = port;
            helpMeConnectMessage.IPAddress = toip;
            helpMeConnectMessage.Port = toport;
            SendData(ip, port, helpMeConnectMessage.Serialize());
        }

        /// <summary>
        /// Send a WantsConnectionMessage to ip/port with specific topip/toport
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="toip"></param>
        /// <param name="toport"></param>
        internal void SendWantsConnectionMessage(IPAddress ip, ushort port, IPAddress toip, ushort toport)
        {
            WantsConnectionMessage wantsConnectionMessage = new WantsConnectionMessage();
            wantsConnectionMessage.MessageHeader.SenderPeerId = PeerId;
            wantsConnectionMessage.MessageHeader.ReceiverIPAddress = ip.GetAddressBytes();
            wantsConnectionMessage.MessageHeader.ReceiverExternalPort = port;
            wantsConnectionMessage.IPAddress = toip;
            wantsConnectionMessage.Port = toport;
            SendData(ip, port, wantsConnectionMessage.Serialize());
        }

        /// <summary>
        /// Send a DataMessage to ip/port
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="toip"></param>
        /// <param name="toport"></param>
        internal void SendDataMessage(IPAddress ip, ushort port, DataMessage dataMessage)
        {
            dataMessage.MessageHeader.SenderPeerId = PeerId;
            dataMessage.MessageHeader.ReceiverIPAddress = ip.GetAddressBytes();
            dataMessage.MessageHeader.ReceiverExternalPort = port;
            SendData(ip, port, dataMessage.Serialize());
        }

        /// <summary>
        /// Send a GoingOfflineMessage to ip/port with specific topip/toport
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        internal void SendGoingOfflineMessage(IPAddress ip, ushort port)
        {
            GoingOfflineMessage goingOfflineMessage = new GoingOfflineMessage();
            goingOfflineMessage.MessageHeader.SenderPeerId = PeerId;
            goingOfflineMessage.MessageHeader.ReceiverIPAddress = ip.GetAddressBytes();
            goingOfflineMessage.MessageHeader.ReceiverExternalPort = port;
            SendData(ip, port, goingOfflineMessage.Serialize());
        }

        /// <summary>
        /// Stops the ConnectionManager by setting the Running flag to false
        /// </summary>
        public void Stop()
        {
            if (!Running)
            {
                return;
            }
            Logger.LogText("Stop method was called...", this, Logtype.Info);
            Running = false;
            DateTime start = DateTime.Now;
            while ((ReceivingThread.IsAlive || WorkerThread.IsAlive) && DateTime.Now < start.AddMilliseconds(Constants.CONNECTIONMANAGER_MAX_TERMINATION_WAIT_TIME))
            {
                Thread.Sleep(100);
            }
            if (ReceivingThread.IsAlive)
            {
                Logger.LogText("ReceivingThread did not end within 5 seconds", this, Logtype.Info);
                try
                {
                    ReceivingThread.Abort();
                    Logger.LogText("Aborted ReceivingThread", this, Logtype.Info);
                }
                catch (Exception ex)
                {
                    Logger.LogText(string.Format("Exception during abortion of ReceivingThread: {0}", ex.Message), this, Logtype.Error);
                    Logger.LogException(ex, this, Logtype.Error);
                }
            }
            if (WorkerThread.IsAlive)
            {
                Logger.LogText("WorkerThread did not end within 5 seconds", this, Logtype.Info);
                try
                {
                    WorkerThread.Abort();
                    Logger.LogText("Aborted WorkerThread", this, Logtype.Info);
                }
                catch (Exception ex)
                {
                    Logger.LogText(string.Format("Exception during abortion of WorkerThread: {0}", ex.Message), this, Logtype.Error);
                    Logger.LogException(ex, this, Logtype.Error);
                }
            }
            //Send a GoingOfflineMessage to each of our neighbors; thus, they can set us to offline
            try
            {
                foreach (Contact contact in Contacts.Values)
                {
                    if (contact.IsOffline == false)
                    {
                        SendGoingOfflineMessage(contact.IPAddress, contact.Port);
                        Logger.LogText(string.Format("Sent GoingOfflineMessage to {0}:{1}", contact.IPAddress, contact.Port), this, Logtype.Info);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogText(string.Format("Exception during sending of GoingOfflineMessages: {0}", ex.Message), this, Logtype.Error);
                Logger.LogException(ex, this, Logtype.Error);
            }
            try
            {
                //Close the udp client to release the ip endpoint; thus, it can be reallocated again by starting a ConnectionManager again
                Client.Close();
            }
            catch (Exception ex)
            {
                Logger.LogText(string.Format("Could not close UDP client: {0}", ex.Message), this, Logtype.Error);
                Logger.LogException(ex, this, Logtype.Error);
            }
            try
            {
                //Dispose the udp client to release the ip endpoint; thus, it can be reallocated again by starting a ConnectionManager again
                Client.Dispose();
            }
            catch (Exception ex)
            {
                Logger.LogText(string.Format("Could not dispose UDP client: {0}", ex.Message), this, Logtype.Error);
                Logger.LogException(ex, this, Logtype.Error);
            }
            Logger.LogText("Terminated", this, Logtype.Info);
        }

        /// <summary>
        /// Puts the given contact to the list of contacts or updates the contact, if already in list.
        /// Returns the corresponding contact object
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="peerID"></param>
        internal void UpdateContact(IPAddress ip, ushort port, byte[] peerID)
        {
            if (ExternalIpAddresses.ContainsKey(ip))
            {
                //we dont add peers to our contacts having the same external ip address than we have
                //this can be the case, when a peer is behind the same router than we are, e.g. inside the same network
                return;
            }
            IPEndPoint endpoint = new IPEndPoint(ip, port);
            if (ReceivedContacts.ContainsKey(endpoint))
            {
                //if the contact is in our received contacts, we remove it
                Contact contact = new Contact() { IPAddress = ip, Port = port, LastSeen = DateTime.Now, LastHelloSent = DateTime.Now, PeerId = peerID };
                ReceivedContacts.TryRemove(endpoint, out contact);
            }
            if (!Contacts.ContainsKey(endpoint))
            {
                Contact contact = new Contact() { IPAddress = ip, Port = port, LastSeen = DateTime.Now, LastHelloSent = DateTime.Now, PeerId = peerID };
                Contacts[endpoint] = contact;
                Logger.LogText(string.Format("Insert new contact: {0}", endpoint), this, Logtype.Debug);
                //Create a RequestNeighborList operation for this new neighbor to receive his neighbors
                RequestNeighborListOperation requestNeighborListOperation = new RequestNeighborListOperation(ip, port) { ConnectionManager = this };
                Operations.Enqueue(requestNeighborListOperation);
                return;
            }
            else
            {
                Contacts[endpoint].LastSeen = DateTime.Now;
                Contacts[endpoint].LastHelloSent = DateTime.Now;
                Contacts[endpoint].IsOffline = false;
                Logger.LogText(string.Format("Updated contact: {0}", endpoint), this, Logtype.Debug);
                return;
            }
        }

        /// <summary>
        /// Returns the number of peers of the contact list that are not flagged as offline
        /// </summary>
        /// <returns></returns>
        public ushort GetConnectionCount()
        {
            ushort counter = 0;
            foreach (KeyValuePair<IPEndPoint, Contact> entry in Contacts)
            {
                //we dont count "private ip addresses" as "real connections"
                //this should avoid, that pcs of computing pools only connect to each other and not to the internet
                if (entry.Value.IsOffline == false && !IpTools.IsPrivateIP(entry.Value.IPAddress))
                {
                    counter++;
                }
            }
            return counter;
        }

        /// <summary>
        /// Returns the PeerId of this ConnectionManager
        /// </summary>
        /// <returns></returns>
        public byte[] GetPeerId()
        {
            return PeerId;
        }

        /// <summary>
        /// Return a list of all online contacts
        /// </summary>
        /// <returns></returns>
        public List<Contact> GetAllOnlineContacts()
        {
            List<Contact> contacts = new List<Contact>();
            foreach (Contact contact in Contacts.Values)
            {
                if (contact.IsOffline == false)
                {
                    contacts.Add(contact);
                }
            }
            return contacts;
        }

        /// <summary>
        /// Blocks, until a DataMessage was received.
        /// Then, it returns a Data object containing the payload of the message.
        /// Returns null if the ConnectionManager was stopped
        /// </summary>
        /// <returns></returns>
        public Data ReceiveData()
        {
            while (Running)
            {
                if (DataMessagesIngoing.TryDequeue(out DataMessage datamessage))
                {
                    Data data = new Data
                    {
                        Payload = datamessage.Payload,
                        PeerId = datamessage.MessageHeader.SenderPeerId
                    };
                    return data;
                }
                else
                {
                    Thread.Sleep(Constants.CONNECTIONMANAGER_WORKER_THREAD_SLEEPTIME);
                }
            }
            return null;
        }

        /// <summary>
        /// Sends data to all neighbors if PeerID == null
        /// Otherwise sends data only to specific peer if known; but to all known ip/ports of this peer
        /// </summary>
        /// <param name="data"></param>
        public void SendData(byte[] data, byte[] PeerId = null)
        {
            if (PeerId != null && PeerId.Length != 16)
            {
                throw new Exception(string.Format("Invalid PeerID length. Expected 16 but obtained {0}", PeerId.Length));
            }
            DataMessage dataMessage = new DataMessage
            {
                Payload = data
            };
            if (PeerId != null)
            {
                dataMessage.MessageHeader.ReceiverPeerId = PeerId;
            }
            DataMessagesOutgoing.Enqueue(dataMessage);
        }

        /// <summary>
        /// Check, if this ConnectionManager is currently running
        /// </summary>
        /// <returns></returns>
        public bool IsRunning()
        {
            return Running;
        }

        internal void OnConnectionsNumberChanged(List<Contact> contacts)
        {
            if (ConnectionsNumberChanged != null)
            {
                ConnectionsNumberChanged.BeginInvoke(this, new ConnectionsNumberChangedEventArgs() { Contacts = contacts }, null, null);
            }
        }

        /// <summary>
        /// Calls DoUpdateObservableContactList either from ui or current thread to update it
        /// </summary>
        internal void OnUpdateObservableContactList()
        {
            //If we are in a WPF application, we update the observable collection in UI thread
            if (Application.Current != null)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    DoUpdateObservableContactList();
                }));
            }
            else
            {
                DoUpdateObservableContactList();
            }
        }

        /// <summary>
        /// Adds peers that just came online and removes peers that are offline
        /// </summary>
        private void DoUpdateObservableContactList()
        {
            List<Contact> removeList = new List<Contact>();
            foreach (Contact contact in ObservableContactList)
            {
                if (contact.IsOffline)
                {
                    removeList.Add(contact);
                }
            }
            foreach (Contact contact in removeList)
            {
                ObservableContactList.Remove(contact);
            }
            foreach (Contact contact in Contacts.Values)
            {
                if (!contact.IsOffline && !ObservableContactList.Contains(contact))
                {
                    ObservableContactList.Add(contact);
                }
            }
        }

        /// <summary>
        /// Returns observable Contact list
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<Contact> GetContacts()
        {
            return ObservableContactList;
        }
    }

    /// <summary>
    /// Object containing data (byte array) and the PeerId of the sender/receiver of the data
    /// </summary>
    public class Data
    {
        public byte[] PeerId;
        public byte[] Payload;
    }
}