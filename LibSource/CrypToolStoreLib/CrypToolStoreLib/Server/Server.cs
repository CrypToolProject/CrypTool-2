/*
   Copyright 2021 Nils Kopal <kopal<AT>cryptool.org>

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
using CrypToolStoreLib.Database;
using CrypToolStoreLib.DataObjects;
using CrypToolStoreLib.Network;
using CrypToolStoreLib.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace CrypToolStoreLib.Server
{
    public class CrypToolStoreServer
    {
        private readonly Logger logger = Logger.GetLogger();
        private readonly BlockingCollection<ClientHandler> _registeredClientHandlers = new BlockingCollection<ClientHandler>();
        private readonly BlockingCollection<Task> _handlerTasks = new BlockingCollection<Task>();

        /// <summary>
        /// Server key used for the ssl stream
        /// </summary>
        public X509Certificate2 ServerKey
        {
            get;
            set;
        }

        /// <summary>
        /// Responsible for incoming TCP connections
        /// </summary>
        private TcpListener TCPListener
        {
            get;
            set;
        }

        /// <summary>
        /// Port to connect to
        /// </summary>
        public int Port
        {
            get;
            set;
        }

        /// <summary>
        /// Flag to set state of server to running/not running
        /// </summary>
        internal bool Running
        {
            get;
            set;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public CrypToolStoreServer()
        {
            Port = Constants.SERVER_DEFAULT_PORT;
        }

        /// <summary>
        /// Starts the server
        /// </summary>
        public void Start()
        {
            if (Running)
            {
                return;
            }
            logger.LogText("Starting listen thread", this, Logtype.Info);
            Running = true;

            Thread listenThread = new Thread(ListenThread)
            {
                IsBackground = true
            };
            listenThread.Start();
            Thread.Sleep(1000); //just to let the thread start
            logger.LogText("Listen thread started", this, Logtype.Info);

            Thread housekeeperThread = new Thread(HousekeeperThread)
            {
                IsBackground = true
            };
            housekeeperThread.Start();
            Thread.Sleep(1000); //just to let the thread start
            logger.LogText("Housekeeper thread started", this, Logtype.Info);

        }

        /// <summary>
        /// Listens for new incoming connections
        /// </summary>
        private void ListenThread()
        {
            try
            {
                TCPListener = new TcpListener(IPAddress.Any, Port);
                TCPListener.Start();

                while (Running)
                {
                    try
                    {
                        TcpClient client = TCPListener.AcceptTcpClient();
                        client.ReceiveTimeout = 1000;
                        client.SendTimeout = 1000;
                        logger.LogText(string.Format("New client connected from IP/Port={0}", client.Client.RemoteEndPoint), this, Logtype.Info);
                        CancellationToken token;
                        _handlerTasks.Add(
                            Task.Run(() =>
                            {
                                ClientHandler handler = new ClientHandler();
                                try
                                {

                                    RegisterClientHandler(handler);
                                    handler.CrypToolStoreServer = this;
                                    try
                                    {
                                        handler.HandleClient(client);
                                    }
                                    catch (Exception ex)
                                    {
                                        logger.LogText(string.Format("Exception during handling of client from IP/Port={0} : {1}", client.Client.RemoteEndPoint, ex.Message), this, Logtype.Error);
                                        logger.LogException(ex, this, Logtype.Error);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.LogText(string.Format("Exception during handling of client from IP/Port={0} : {1}", client.Client.RemoteEndPoint, ex.Message), this, Logtype.Error);
                                    logger.LogException(ex, this, Logtype.Error);
                                }
                                try
                                {
                                    UnregisterClientHandler(handler);
                                }
                                catch (Exception ex)
                                {
                                    logger.LogText(string.Format("Exception during unregistration of client client handler from IP/Port={0}: {1}", client.Client.RemoteEndPoint, ex.Message), this, Logtype.Error);
                                    logger.LogException(ex, this, Logtype.Error);
                                }
                            }, token));
                    }
                    catch (Exception ex)
                    {
                        if (Running)
                        {
                            logger.LogText(string.Format("Exception in ListenThread: {0}", ex.Message), this, Logtype.Error);
                            logger.LogException(ex, this, Logtype.Error);
                        }
                    }
                }
                logger.LogText("ListenThread terminated", this, Logtype.Info);
            }
            catch (Exception ex)
            {
                if (Running)
                {
                    logger.LogText(string.Format("Exception in ListenThread: {0}", ex.Message), this, Logtype.Error);
                    logger.LogException(ex, this, Logtype.Error);
                }
            }
        }

        /// <summary>
        /// Clears tasks that have finished by calling their Dispose method and 
        /// removing them from the internal list
        /// </summary>
        private void HousekeeperThread()
        {
            try
            {
                List<Task> tasksToRemove = new List<Task>();
                while (Running)
                {
                    try
                    {
                        //find tasks to dispose and remove
                        //and dispose them
                        foreach (Task task in _handlerTasks)
                        {
                            if (task.IsCompleted || task.IsCanceled || task.IsFaulted)
                            {
                                try
                                {
                                    task.Dispose();                                    
                                }
                                catch (Exception ex)
                                {
                                    logger.LogText(string.Format("Exception during Disposal of task: {0}", ex.Message), this, Logtype.Error);
                                    logger.LogException(ex, this, Logtype.Error);
                                }
                                tasksToRemove.Add(task);
                            }
                        }
                        //remove all tasks from the internal list
                        foreach (Task task in tasksToRemove)
                        {
                            Task taskToRemove = task;
                            _handlerTasks.TryTake(out taskToRemove);
                        }
                        tasksToRemove.Clear();
                        Thread.Sleep(1000);
                    }
                    catch (Exception ex)
                    {
                        if (Running)
                        {
                            logger.LogText(string.Format("Exception in HousekeeperThread: {0}", ex.Message), this, Logtype.Error);
                            logger.LogException(ex, this, Logtype.Error);
                        }
                    }
                }
                logger.LogText("HousekeeperThread terminated", this, Logtype.Info);
            }
            catch (Exception ex)
            {
                if (Running)
                {
                    logger.LogText(string.Format("Exception in HousekeeperThread: {0}", ex.Message), this, Logtype.Error);
                    logger.LogException(ex, this, Logtype.Error);
                }
            }
        }

        /// <summary>
        /// Registers a client handler
        /// needed for checking, how many handlers are running during shutdown
        /// </summary>
        /// <param name="handler"></param>
        public void RegisterClientHandler(ClientHandler handler)
        {
            lock (this)
            {
                _registeredClientHandlers.Add(handler);
            }
        }

        /// <summary>
        /// Unregisters a client handler
        /// needed for checking, how many handlers are running during shutdown
        /// </summary>
        /// <param name="handler"></param>
        public void UnregisterClientHandler(ClientHandler handler)
        {
            lock (this)
            {
                ClientHandler removeHandler = handler;
                _registeredClientHandlers.TryTake(out removeHandler);
            }
        }

        /// <summary>
        /// Stops the server
        /// </summary>
        public void Stop()
        {
            if (!Running)
            {
                return;
            }
            try
            {
                logger.LogText("Stopping server", this, Logtype.Info);
                Running = false;

                logger.LogText("Stopping TCPListener", this, Logtype.Info);
                TCPListener.Stop();
                logger.LogText("TCPListener stopped", this, Logtype.Info);

                logger.LogText("Waiting for all ClientHandlers to go down", this, Logtype.Info);
                //we wait 5 seconds for all handlers to go down... then we just move on
                int waitcounter = 0;
                while (_registeredClientHandlers.Count > 0 && waitcounter < 100)
                {
                    Thread.Sleep(50);
                    waitcounter++;
                }

                if (_registeredClientHandlers.Count == 0)
                {
                    logger.LogText("All ClientHandlers are down", this, Logtype.Info);
                }
                else
                {
                    logger.LogText(string.Format("We still had {0} ClientHandlers runnning... but we go on with the stopping of the server", _registeredClientHandlers.Count), this, Logtype.Error);
                }
                logger.LogText("Server stopped", this, Logtype.Info);
            }
            catch (Exception ex)
            {
                logger.LogText(string.Format("Exception during stopping of server: {0}", ex.Message), this, Logtype.Error);
                logger.LogException(ex, this, Logtype.Error);
            }
        }
    }

    /// <summary>
    /// A single client handler is responsible for the communication with one client
    /// </summary>
    public class ClientHandler
    {
        private readonly Logger Logger = Logger.GetLogger();
        private readonly CrypToolStoreDatabase Database = CrypToolStoreDatabase.GetDatabase();
        private bool ClientIsAuthenticated { get; set; }
        private bool ClientIsAdmin { get; set; }
        private string Username { get; set; }
        private IPAddress IPAddress { get; set; }

        /// <summary>
        /// This hashset memorizes the tries of a password from a dedicated IP
        /// </summary>
        private static readonly ConcurrentDictionary<IPAddress, PasswordTry> PasswordTries = new ConcurrentDictionary<IPAddress, PasswordTry>();

        private readonly int PASSWORD_RETRY_INTERVAL = 5; //minutes
        private readonly int ALLOWED_PASSWORD_RETRIES = 3;

        /// <summary>
        /// Reference to the server object
        /// </summary>
        public CrypToolStoreServer CrypToolStoreServer
        {
            get;
            set;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public ClientHandler()
        {
            Username = "anonymous"; //default username is anonymous
            ClientIsAuthenticated = false;
            ClientIsAdmin = false;
        }

        /// <summary>
        /// This method receives messages from the client and handles it
        /// </summary>
        /// <param name="client"></param>
        public void HandleClient(TcpClient client)
        {
            IPAddress = ((IPEndPoint)client.Client.RemoteEndPoint).Address;
            using (SslStream sslstream = new SslStream(client.GetStream()))
            {
                //Step 0: Authenticate SSLStream as server
                sslstream.ReadTimeout = Constants.CLIENTHANDLER_READ_TIMEOUT;
                sslstream.WriteTimeout = Constants.CLIENTHANDLER_WRITE_TIMEOUT;
                sslstream.AuthenticateAsServer(CrypToolStoreServer.ServerKey, false, false);
                try
                {
                    while (CrypToolStoreServer.Running && client.Connected && sslstream.CanRead && sslstream.CanWrite)
                    {
                        //Receive message
                        Message message = ReceiveMessage(sslstream);

                        //Handle received message
                        if (message != null)
                        {
                            HandleMessage(message, sslstream);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogText(string.Format("Exception during HandleClient: {0}", ex.Message), this, Logtype.Error);
                    Logger.LogException(ex, this, Logtype.Error);
                    return;
                }
                finally
                {
                    if (sslstream != null)
                    {
                        sslstream.Close();
                        sslstream.Dispose();
                    }
                    if (client != null)
                    {
                        client.Close();
                        client.Dispose();
                    }
                    Logger.LogText("Client disconnected", this, Logtype.Info);
                }
            }
        }

        /// <summary>
        /// Receives a message from the ssl stream
        /// </summary>
        /// <param name="sslstream"></param>
        /// <returns></returns>
        private Message ReceiveMessage(SslStream sslstream)
        {
            //Step 1: Read message header                    
            byte[] headerbytes = new byte[21]; //a message header is 21 bytes
            int bytesread = 0;

            while (bytesread < 21)
            {
                int readbytes = sslstream.Read(headerbytes, bytesread, 21 - bytesread);
                if (readbytes == 0)
                {
                    //stream was closed
                    return null;
                }
                bytesread += readbytes;
            }

            //Step 2: Deserialize message header and get payloadsize
            MessageHeader header = new MessageHeader();
            header.Deserialize(headerbytes);
            int payloadsize = header.PayloadSize;
            if (payloadsize > Constants.SERVER_MESSAGE_MAX_PAYLOAD_SIZE)
            {
                //if we receive a message larger than MAX_PAYLOAD_SIZE we throw an exception which terminates the session
                throw new Exception(string.Format("Receiving a message with a payload which is larger (={0} bytes) than the SERVER_MAX_PAYLOAD_SIZE={1} bytes", payloadsize, Constants.SERVER_MESSAGE_MAX_PAYLOAD_SIZE));
            }

            //Step 3: Read complete message
            byte[] messagebytes = new byte[payloadsize + 21];
            Array.Copy(headerbytes, 0, messagebytes, 0, 21);

            while (bytesread < payloadsize + 21)
            {
                int readbytes = sslstream.Read(messagebytes, bytesread, payloadsize + 21 - bytesread);
                if (readbytes == 0)
                {
                    //stream was closed
                    return null;
                }
                bytesread += readbytes;
            }

            //Step 4: Deserialize Message
            Message message = Message.DeserializeMessage(messagebytes);
            Logger.LogText(string.Format("Received a \"{0}\" message from the client", message.MessageHeader.MessageType.ToString()), this, Logtype.Debug);

            return message;
        }

        /// <summary>
        /// Responsible for handling received messages by calling message handlers based on the message type
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sslstream"></param>
        private void HandleMessage(Message message, SslStream sslStream)
        {
            switch (message.MessageHeader.MessageType)
            {
                case MessageType.Login:
                    HandleLoginMessage((LoginMessage)message, sslStream);
                    break;
                case MessageType.Logout:
                    HandleLogoutMessage((LogoutMessage)message, sslStream);
                    break;
                case MessageType.CreateNewDeveloper:
                    HandleCreateNewDeveloperMessage((CreateNewDeveloperMessage)message, sslStream);
                    break;
                case MessageType.UpdateDeveloper:
                    HandleUpdateDeveloperMessage((UpdateDeveloperMessage)message, sslStream);
                    break;
                case MessageType.DeleteDeveloper:
                    HandleDeleteDeveloperMessage((DeleteDeveloperMessage)message, sslStream);
                    break;
                case MessageType.RequestDeveloper:
                    HandleRequestDeveloperMessage((RequestDeveloperMessage)message, sslStream);
                    break;
                case MessageType.RequestDeveloperList:
                    HandleRequestDeveloperListMessage((RequestDeveloperListMessage)message, sslStream);
                    break;
                case MessageType.CreateNewPlugin:
                    HandleCreateNewPluginMessage((CreateNewPluginMessage)message, sslStream);
                    break;
                case MessageType.UpdatePlugin:
                    HandleUpdatePluginMessage((UpdatePluginMessage)message, sslStream);
                    break;
                case MessageType.RequestPlugin:
                    HandleRequestPluginMessage((RequestPluginMessage)message, sslStream);
                    break;
                case MessageType.RequestPublishedPluginList:
                    HandleRequestPublishedPluginListMessage((RequestPublishedPluginListMessage)message, sslStream);
                    break;
                case MessageType.RequestPublishedPlugin:
                    HandleRequestPublishedPluginMessage((RequestPublishedPluginMessage)message, sslStream);
                    break;
                case MessageType.DeletePlugin:
                    HandleDelePluginMessage((DeletePluginMessage)message, sslStream);
                    break;
                case MessageType.RequestPluginList:
                    HandleRequestPluginListMessage((RequestPluginListMessage)message, sslStream);
                    break;
                case MessageType.CreateNewSource:
                    HandleCreateNewSourceMessage((CreateNewSourceMessage)message, sslStream);
                    break;
                case MessageType.UpdateSource:
                    HandleUpdateSourceMessage((UpdateSourceMessage)message, sslStream);
                    break;
                case MessageType.UpdateSourcePublishState:
                    HandleUpdateSourcePublishStateMessage((UpdateSourcePublishStateMessage)message, sslStream);
                    break;
                case MessageType.DeleteSource:
                    HandleDeleteSourceMessage((DeleteSourceMessage)message, sslStream);
                    break;
                case MessageType.RequestSource:
                    HandleRequestSourceMessage((RequestSourceMessage)message, sslStream);
                    break;
                case MessageType.RequestSourceList:
                    HandleRequestSourceListMessage((RequestSourceListMessage)message, sslStream);
                    break;
                case MessageType.CreateNewResource:
                    HandleCreateNewResourceMessage((CreateNewResourceMessage)message, sslStream);
                    break;
                case MessageType.UpdateResource:
                    HandleUpdateResourceMessage((UpdateResourceMessage)message, sslStream);
                    break;
                case MessageType.DeleteResource:
                    HandleDeletResourceMessage((DeleteResourceMessage)message, sslStream);
                    break;
                case MessageType.RequestResource:
                    HandleRequestResourceMessage((RequestResourceMessage)message, sslStream);
                    break;
                case MessageType.RequestResourceList:
                    HandleRequestResourceListMessage((RequestResourceListMessage)message, sslStream);
                    break;
                case MessageType.RequestPublishedResourceList:
                    HandleRequestPublishedResourceListMessage((RequestPublishedResourceListMessage)message, sslStream);
                    break;
                case MessageType.RequestPublishedResource:
                    HandleRequestPublishedResourceMessage((RequestPublishedResourceMessage)message, sslStream);
                    break;
                case MessageType.CreateNewResourceData:
                    HandleCreateNewResourceDataMessage((CreateNewResourceDataMessage)message, sslStream);
                    break;
                case MessageType.UpdateResourceData:
                    HandleUpdateResourceDataMessage((UpdateResourceDataMessage)message, sslStream);
                    break;
                case MessageType.UpdateResourceDataPublishState:
                    HandleUpdateResourceDataPublishState((UpdateResourceDataPublishStateMessage)message, sslStream);
                    break;
                case MessageType.DeleteResourceData:
                    HandleDeleteResourceDataMessage((DeleteResourceDataMessage)message, sslStream);
                    break;
                case MessageType.RequestResourceData:
                    HandleRequestResourceDataMessage((RequestResourceDataMessage)message, sslStream);
                    break;
                case MessageType.RequestResourceDataList:
                    HandleRequestResourceDataListMessage((RequestResourceDataListMessage)message, sslStream);
                    break;
                case MessageType.StartUploadSourceZipfile:
                    HandleStartUploadSourceZipFileMessage((StartUploadSourceZipfileMessage)message, sslStream);
                    break;
                case MessageType.StartUploadAssemblyZipfile:
                    HandleStartUploadAssemblyZipFileMessage((StartUploadAssemblyZipfileMessage)message, sslStream);
                    break;
                case MessageType.StartUploadResourceDataFile:
                    HandleStartUploadResourceDataFileMessage((StartUploadResourceDataFileMessage)message, sslStream);
                    break;
                case MessageType.RequestDownloadSourceZipfile:
                    HandleRequestDownloadSourceZipfileMessage((RequestDownloadSourceZipfileMessage)message, sslStream);
                    break;
                case MessageType.RequestDownloadAssemblyZipfile:
                    HandleRequestDownloadAssemblyZipfileMessage((RequestDownloadAssemblyZipfileMessage)message, sslStream);
                    break;
                case MessageType.RequestDownloadResourceDataFile:
                    HandleRequestDownloadResourceDataFileMessage((RequestDownloadResourceDataFileMessage)message, sslStream);
                    break;

                default:
                    HandleUnknownMessage(message, sslStream);
                    break;
            }
        }

        /// <summary>
        /// Handles messages of unknown message type
        /// Sends that we do not know the type of message to the client
        /// Also writes a log entry
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sslStream"></param>
        private void HandleUnknownMessage(Message message, SslStream sslStream)
        {
            Logger.LogText(string.Format("Received message of unknown type {0} from user {1} from IP={2}", message.MessageHeader.MessageType, Username, IPAddress), this, Logtype.Debug);
            ServerErrorMessage error = new ServerErrorMessage
            {
                Message = string.Format("Unknown type of message: {0}", message.MessageHeader.MessageType)
            };
            SendMessage(error, sslStream);
        }

        /// <summary>
        /// Handles login attempts
        /// Each IP is only allowed to try ALLOWED_PASSWORD_RETRIES passwords
        /// After ALLOWED_PASSWORD_RETRIES wrong passwords, the authentication is always refused within the next PASSWORD_RETRY_INTERVAL minutes
        /// Login attempts refresh the interval
        /// </summary>
        /// <param name="loginMessage"></param>
        /// <param name="sslStream"></param>
        private void HandleLoginMessage(LoginMessage loginMessage, SslStream sslStream)
        {
            Logger.LogText(string.Format("Login attempt from Ip={0}", IPAddress), this, Logtype.Debug);

            //Initially, we set everything to false
            Username = "anonymous"; //default username is anonymous
            ClientIsAuthenticated = false;
            ClientIsAdmin = false;

            string username = loginMessage.Username.ToLower();  //all usernames have to be lowercase
            string password = loginMessage.Password;

            if (PasswordTries.ContainsKey(IPAddress))
            {
                if (DateTime.Now > PasswordTries[IPAddress].LastTryDateTime.AddMinutes(PASSWORD_RETRY_INTERVAL))
                {
                    PasswordTries.TryRemove(IPAddress, out PasswordTry passwordTry);
                }
                else if (PasswordTries[IPAddress].Number >= ALLOWED_PASSWORD_RETRIES)
                {
                    // after 3 tries, we just close the connection and refresh the timer
                    PasswordTries[IPAddress].LastTryDateTime = DateTime.Now;
                    PasswordTries[IPAddress].Number++;
                    Logger.LogText(string.Format("{0}. try of a username/password (username={1}) combination from IP {2} - kill the sslStream now", PasswordTries[IPAddress].Number, username, IPAddress), this, Logtype.Warning);
                    sslStream.Close();
                    return;
                }
            }

            if (Database.CheckDeveloperPassword(username, password) == true)
            {
                ClientIsAuthenticated = true;
                Username = username;
                ResponseLoginMessage response = new ResponseLoginMessage
                {
                    LoginOk = true,
                    Message = "Login credentials correct"
                };
                Logger.LogText(string.Format("User {0} successfully authenticated from IP={1}", username, IPAddress), this, Logtype.Info);
                Developer developer = Database.GetDeveloper(username);
                if (developer.IsAdmin)
                {
                    response.IsAdmin = true;
                    ClientIsAdmin = true;
                    Logger.LogText(string.Format("User {0} is admin", username), this, Logtype.Info);
                }
                SendMessage(response, sslStream);
            }
            else
            {
                if (!PasswordTries.ContainsKey(IPAddress))
                {
                    PasswordTries.TryAdd(IPAddress, new PasswordTry() { Number = 1, LastTryDateTime = DateTime.Now });
                }
                else
                {
                    PasswordTries[IPAddress].Number++;
                    PasswordTries[IPAddress].LastTryDateTime = DateTime.Now;
                }
                ResponseLoginMessage response = new ResponseLoginMessage
                {
                    LoginOk = false,
                    Message = "Login credentials incorrect"
                };
                Logger.LogText(string.Format("{0}. try of a username/password (username={1}) combination from IP={2}", PasswordTries[IPAddress].Number, username, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
            }
        }

        /// <summary>
        /// Handles logouts; set ClientIsAuthenticated to false
        /// </summary>
        /// <param name="logoutMessage"></param>
        /// <param name="sslStream"></param>
        private void HandleLogoutMessage(LogoutMessage logoutMessage, SslStream sslStream)
        {
            Logger.LogText(string.Format("User {0} from IP={1} logged out", Username, IPAddress), this, Logtype.Info);
            Username = "anonymous"; //default username is anonymous
            ClientIsAuthenticated = false;
            ClientIsAdmin = false;
            sslStream.Close();
        }

        /// <summary>
        /// Handles CreateNewDeveloperMessages
        /// If the user is authenticated and he is admin, it tries to create a new developer in the database        
        /// Then, it sends a response message which contains if it succeeded or failed
        /// </summary>
        /// <param name="createNewDeveloperMessage"></param>
        /// <param name="sslStream"></param>
        private void HandleCreateNewDeveloperMessage(CreateNewDeveloperMessage createNewDeveloperMessage, SslStream sslStream)
        {
            Logger.LogText(string.Format("User {0} tries to create a developer", Username), this, Logtype.Debug);

            //Only authenticated admins are allowed to create new developers
            if (!ClientIsAuthenticated || !ClientIsAdmin)
            {
                ResponseDeveloperModificationMessage response = new ResponseDeveloperModificationMessage
                {
                    ModifiedDeveloper = false,
                    Message = "Unauthorized to create new developers. Please authenticate yourself as admin"
                };
                Logger.LogText(string.Format("Unauthorized user {0} tried to create new developer={1} from IP={2}", Username, createNewDeveloperMessage.Developer.Username, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }
            //Here, the user is authenticated and he is an admin; thus, creation of new developer in database is started
            try
            {
                Developer developer = createNewDeveloperMessage.Developer;
                Database.CreateDeveloper(developer.Username, developer.Firstname, developer.Lastname, developer.Email, developer.Password, developer.IsAdmin);
                Logger.LogText(string.Format("User {0} created new developer in database: {1}", Username, developer), this, Logtype.Info);
                ResponseDeveloperModificationMessage response = new ResponseDeveloperModificationMessage
                {
                    ModifiedDeveloper = true,
                    Message = string.Format("Created new developer={0} in database", developer.Username)
                };
                SendMessage(response, sslStream);
            }
            catch (Exception ex)
            {
                //creation failed; logg to logfile and return exception to client
                ResponseDeveloperModificationMessage response = new ResponseDeveloperModificationMessage
                {
                    ModifiedDeveloper = false
                };
                Logger.LogText(string.Format("User {0} tried to create a new developer. But an exception occured: {1}", Username, ex.Message), this, Logtype.Error);
                Logger.LogException(ex, this, Logtype.Error);
                response.Message = "Exception during creation of new developer";
                SendMessage(response, sslStream);
            }
        }

        /// <summary>
        /// Handles UpdateDeveloperMessages
        /// If the user is authenticated and he is admin, it tries to update an existing developer in the database        
        /// Then, it sends a response message which contains if it succeeded or failed
        /// </summary>
        /// <param name="updateDeveloperMessage"></param>
        /// <param name="sslStream"></param>
        private void HandleUpdateDeveloperMessage(UpdateDeveloperMessage updateDeveloperMessage, SslStream sslStream)
        {
            Logger.LogText(string.Format("User {0} tries to update a developer", Username), this, Logtype.Debug);

            //Only authenticated admins are allowed to create new developers
            if (!ClientIsAuthenticated || (!ClientIsAdmin && Username != updateDeveloperMessage.Developer.Username))
            {
                ResponseDeveloperModificationMessage response = new ResponseDeveloperModificationMessage
                {
                    ModifiedDeveloper = false,
                    Message = "Unauthorized to update a developer. Please authenticate yourself as admin or try only to update yourself"
                };
                Logger.LogText(string.Format("Unauthorized user {0} tried to update developer={1} from IP={2}", Username, updateDeveloperMessage.Developer.Username, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }
            //Here, the user is authenticated and he is an admin; thus, update of existing in database is started
            try
            {
                Developer developer = updateDeveloperMessage.Developer;
                if (ClientIsAdmin)
                {
                    //Only admins are allowed to set/remove admin flags
                    Database.UpdateDeveloper(developer.Username, developer.Firstname, developer.Lastname, developer.Email, developer.IsAdmin);
                }
                else
                {
                    Database.UpdateDeveloperNoAdmin(developer.Username, developer.Firstname, developer.Lastname, developer.Email);
                }
                if (!string.IsNullOrEmpty(developer.Password))
                {
                    Database.UpdateDeveloperPassword(developer.Username, developer.Password);
                }
                Logger.LogText(string.Format("User {0} updated existing developer in database: {1}", Username, developer.Username), this, Logtype.Info);
                ResponseDeveloperModificationMessage response = new ResponseDeveloperModificationMessage
                {
                    ModifiedDeveloper = true,
                    Message = string.Format("Updated developer in database: {0}", developer.Username)
                };
                SendMessage(response, sslStream);
            }
            catch (Exception ex)
            {
                //update failed; logg to logfile and return exception to client
                ResponseDeveloperModificationMessage response = new ResponseDeveloperModificationMessage
                {
                    ModifiedDeveloper = false
                };
                Logger.LogText(string.Format("User {0} tried to update an existing developer. But an exception occured: {1}", Username, ex.Message), this, Logtype.Error);
                Logger.LogException(ex, this, Logtype.Error);
                response.Message = "Exception during update of existing developer";
                SendMessage(response, sslStream);
            }
        }

        /// <summary>
        /// Handles DeleteDeveloperMessages
        /// If the user is authenticated and he is admin, it tries to delete an existing developer in the database        
        /// Then, it sends a response message which contains if it succeeded or failed
        /// </summary>
        /// <param name="deleteDeveloperMessage"></param>
        /// <param name="sslStream"></param>
        private void HandleDeleteDeveloperMessage(DeleteDeveloperMessage deleteDeveloperMessage, SslStream sslStream)
        {
            Logger.LogText(string.Format("User {0} tries to delete a developer", Username), this, Logtype.Debug);

            //Only authenticated admins are allowed to create new developers
            if (!ClientIsAuthenticated || !ClientIsAdmin)
            {
                ResponseDeveloperModificationMessage response = new ResponseDeveloperModificationMessage
                {
                    ModifiedDeveloper = false,
                    Message = "Unauthorized to delete developers. Please authenticate yourself as admin"
                };
                Logger.LogText(string.Format("Unauthorized user {0} tried to delete developer={1} from IP={2}", Username, deleteDeveloperMessage.Developer.Username, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }
            //Here, the user is authenticated and he is an admin; thus, deletion of existing user in database is started
            try
            {
                Developer developer = deleteDeveloperMessage.Developer;
                Database.DeleteDeveloper(developer.Username);
                Logger.LogText(string.Format("User {0} deleted existing developer in database: {1}", Username, developer.Username), this, Logtype.Info);
                ResponseDeveloperModificationMessage response = new ResponseDeveloperModificationMessage
                {
                    ModifiedDeveloper = true,
                    Message = string.Format("Deleted developer={0} in database", developer.Username)
                };
                SendMessage(response, sslStream);
            }
            catch (Exception ex)
            {
                //deletion failed; logg to logfile and return exception to client
                ResponseDeveloperModificationMessage response = new ResponseDeveloperModificationMessage
                {
                    ModifiedDeveloper = false
                };
                Logger.LogText(string.Format("User {0} tried to delete an existing developer. But an exception occured: {1}", Username, ex.Message), this, Logtype.Error);
                Logger.LogException(ex, this, Logtype.Error);
                response.Message = "Exception during deletion of existing developer";
                SendMessage(response, sslStream);
            }
        }

        /// <summary>
        /// Handles RequestDeveloperMessages
        /// If the user is authenticated and he is admin, it tries to get an existing developer from the database        
        /// Then, it sends a response message which contains it and if it succeeded or failed
        /// </summary>
        /// <param name="requestDeveloperMessage"></param>
        /// <param name="sslStream"></param>
        private void HandleRequestDeveloperMessage(RequestDeveloperMessage requestDeveloperMessage, SslStream sslStream)
        {
            Logger.LogText(string.Format("User {0} requests a developer", Username), this, Logtype.Debug);

            //Only authenticated admins are allowed to request any user; users may only request their data
            if (!ClientIsAuthenticated || (!ClientIsAdmin && Username != requestDeveloperMessage.Username))
            {
                ResponseDeveloperMessage response = new ResponseDeveloperMessage
                {
                    DeveloperExists = false,
                    Message = "Unauthorized to get developer. Please authenticate yourself as admin"
                };
                Logger.LogText(string.Format("Unauthorized user {0} tried to request developer={1} from IP={2}", Username, requestDeveloperMessage.Username, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }
            //Here, the user is authenticated and he is an admin; thus, requesting existing user from database
            try
            {
                Developer developer = Database.GetDeveloper(requestDeveloperMessage.Username);

                if (developer == null)
                {
                    Logger.LogText(string.Format("User {0} requested a non existing developer from database: {1}", Username, requestDeveloperMessage.Username), this, Logtype.Info);
                    ResponseDeveloperMessage response = new ResponseDeveloperMessage
                    {
                        Message = string.Format("Developer does not exist: {0}", requestDeveloperMessage.Username),
                        DeveloperExists = false
                    };
                    SendMessage(response, sslStream);
                }
                else
                {
                    Logger.LogText(string.Format("User {0} requested an existing developer from database: {1}", Username, developer), this, Logtype.Debug);
                    ResponseDeveloperMessage response = new ResponseDeveloperMessage
                    {
                        Message = string.Format("Return developer={0}", developer.Username),
                        DeveloperExists = true,
                        Developer = developer
                    };
                    SendMessage(response, sslStream);
                }
            }
            catch (Exception ex)
            {
                //deletion failed; logg to logfile and return exception to client
                ResponseDeveloperMessage response = new ResponseDeveloperMessage
                {
                    DeveloperExists = false
                };
                Logger.LogText(string.Format("User {0} tried to get an existing developer. But an exception occured: {1}", Username, ex.Message), this, Logtype.Error);
                Logger.LogException(ex, this, Logtype.Error);
                response.Message = "Exception during request of existing developer:";
                SendMessage(response, sslStream);
            }
        }

        /// <summary>
        /// Handles RequestDeveloperListMessages
        /// If the user is authenticated and he is admin, it tries to get a list of developers from the database        
        /// Then, it sends a response message which contains it and if it succeeded or failed
        /// </summary>
        /// <param name="requestDeveloperListMessage"></param>
        /// <param name="sslStream"></param>
        private void HandleRequestDeveloperListMessage(RequestDeveloperListMessage requestDeveloperListMessage, SslStream sslStream)
        {
            Logger.LogText(string.Format("User {0} requests a developer list", Username), this, Logtype.Debug);

            //Only authenticated admins are allowed to create new developers
            if (!ClientIsAuthenticated || !ClientIsAdmin)
            {
                ResponseDeveloperListMessage response = new ResponseDeveloperListMessage
                {
                    AllowedToViewList = false,
                    Message = "Unauthorized to get developer list. Please authenticate yourself as admin"
                };
                Logger.LogText(string.Format("Unauthorized user {0} tried to request developer list from IP={1}", Username, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }
            //Here, the user is authenticated and he is an admin; thus, requesting existing user from database
            try
            {
                List<Developer> developerList = Database.GetDevelopers();

                Logger.LogText(string.Format("User {0} requested a developer list", Username), this, Logtype.Debug);
                ResponseDeveloperListMessage response = new ResponseDeveloperListMessage
                {
                    Message = string.Format("Return developer list"),
                    AllowedToViewList = true,
                    DeveloperList = developerList
                };
                SendMessage(response, sslStream);

            }
            catch (Exception ex)
            {
                //deletion failed; logg to logfile and return exception to client
                ResponseDeveloperListMessage response = new ResponseDeveloperListMessage
                {
                    AllowedToViewList = false
                };
                Logger.LogText(string.Format("User {0} tried to get a developer list. But an exception occured: {1}", Username, ex.Message), this, Logtype.Error);
                Logger.LogException(ex, this, Logtype.Error);
                response.Message = "Exception during request of developer list";
                SendMessage(response, sslStream);
            }
        }

        /// <summary>
        /// Handles CreateNewPluginMessages
        /// If the user is authenticated, it tries to create a new plugin in the database        
        /// Then, it sends a response message which contains if it succeeded or failed
        /// </summary>
        /// <param name="createNewPluginMessage"></param>
        /// <param name="sslStream"></param>
        private void HandleCreateNewPluginMessage(CreateNewPluginMessage createNewPluginMessage, SslStream sslStream)
        {
            Logger.LogText(string.Format("User {0} tries to create a plugin", Username), this, Logtype.Debug);

            //Only authenticated users are allowed to create new plugins
            if (!ClientIsAuthenticated)
            {
                ResponsePluginModificationMessage response = new ResponsePluginModificationMessage
                {
                    ModifiedPlugin = false,
                    Message = "Unauthorized to create new plugins. Please authenticate yourself"
                };
                Logger.LogText(string.Format("Unauthorized user {0} tried to create new plugin={1} from IP={2}", Username, createNewPluginMessage.Plugin.Name, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }
            //Here, the user is authenticated; thus, creation of new plugin in database is started
            try
            {
                Plugin plugin = createNewPluginMessage.Plugin;
                Database.CreatePlugin(Username, plugin.Name, plugin.ShortDescription, plugin.LongDescription, plugin.Authornames, plugin.Authoremails, plugin.Authorinstitutes, plugin.Icon);
                Logger.LogText(string.Format("User {0} created new plugin in database: {1}", Username, plugin), this, Logtype.Info);
                ResponsePluginModificationMessage response = new ResponsePluginModificationMessage
                {
                    ModifiedPlugin = true,
                    Message = string.Format("Created new plugin={0} in database", plugin.Id)
                };
                SendMessage(response, sslStream);
            }
            catch (Exception ex)
            {
                //creation failed; logg to logfile and return exception to client
                ResponsePluginModificationMessage response = new ResponsePluginModificationMessage
                {
                    ModifiedPlugin = false
                };
                Logger.LogText(string.Format("User {0} tried to create a new plugin. But an exception occured: {1}", Username, ex.Message), this, Logtype.Error);
                Logger.LogException(ex, this, Logtype.Error);
                response.Message = "Exception during creation of new plugin";
                SendMessage(response, sslStream);
            }
        }

        /// <summary>
        /// Handles UpdatePluginMessages
        /// If the user is authenticated, it tries to update an existing plugin in the database
        /// Users can only update their plugins; admins can update all plugins
        /// Then, it sends a response message which contains if it succeeded or failed
        /// </summary>
        /// <param name="updatePluginMessage"></param>
        /// <param name="sslStream"></param>
        private void HandleUpdatePluginMessage(UpdatePluginMessage updatePluginMessage, SslStream sslStream)
        {
            Logger.LogText(string.Format("User {0} tries to update a plugin", Username), this, Logtype.Debug);

            //Only authenticated users are allowed to update plugins
            if (!ClientIsAuthenticated)
            {
                ResponsePluginModificationMessage response = new ResponsePluginModificationMessage
                {
                    ModifiedPlugin = false,
                    Message = "Unauthorized to update that plugin"
                };
                Logger.LogText(string.Format("Unauthorized user {0} tried to update plugin={1} from IP={2}", Username, updatePluginMessage.Plugin.Id, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }
            //check, if plugin to update exist
            Plugin plugin = Database.GetPlugin(updatePluginMessage.Plugin.Id);
            if (plugin == null)
            {
                ResponsePluginModificationMessage response = new ResponsePluginModificationMessage
                {
                    ModifiedPlugin = false,
                    Message = "Unauthorized to update that plugin" // we send an "unauthorized"; thus, it is not possible to search database for existing ids
                };
                Logger.LogText(string.Format("User {0} tried to update non-existing plugin={1} from IP={2}", Username, updatePluginMessage.Plugin.Id, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }
            //"normal" users are only allowed to update their own plugins
            if (ClientIsAdmin == false && plugin.Username != Username)
            {
                ResponsePluginModificationMessage response = new ResponsePluginModificationMessage
                {
                    ModifiedPlugin = false,
                    Message = "Unauthorized to update that plugin"
                };
                Logger.LogText(string.Format("Unauthorized user {0} tried to update plugin={1} from IP={2}", Username, updatePluginMessage.Plugin.Id, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }

            //Here, the user is authorized; thus, update of existing plugin in database is started
            try
            {
                if (plugin.Icon.Length > Constants.CLIENTHANDLER_MAX_ICON_FILE_SIZE)
                {
                    ResponsePluginModificationMessage icon_too_big_response = new ResponsePluginModificationMessage
                    {
                        ModifiedPlugin = false
                    };
                    Logger.LogText(string.Format("User {0} tried to upload an icon > {0} byte", Constants.CLIENTHANDLER_MAX_ICON_FILE_SIZE), this, Logtype.Error);
                    icon_too_big_response.Message = string.Format("Icon file size > {0} byte now allowed!", Constants.CLIENTHANDLER_MAX_ICON_FILE_SIZE);
                    SendMessage(icon_too_big_response, sslStream);
                    return;
                }
                plugin = updatePluginMessage.Plugin;
                Database.UpdatePlugin(plugin.Id, plugin.Name, plugin.ShortDescription, plugin.LongDescription, plugin.Authornames, plugin.Authoremails, plugin.Authorinstitutes, plugin.Icon);
                plugin = Database.GetPlugin(plugin.Id);
                Logger.LogText(string.Format("User {0} updated existing plugin={1} in database", Username, plugin.Id), this, Logtype.Info);
                ResponsePluginModificationMessage response = new ResponsePluginModificationMessage
                {
                    ModifiedPlugin = true,
                    Message = string.Format("Updated plugin={0} in database", plugin.Id)
                };
                SendMessage(response, sslStream);
            }
            catch (Exception ex)
            {
                //update failed; logg to logfile and return exception to client
                ResponsePluginModificationMessage response = new ResponsePluginModificationMessage
                {
                    ModifiedPlugin = false
                };
                Logger.LogText(string.Format("User {0} tried to update an existing plugin. But an exception occured: {1}", Username, ex.Message), this, Logtype.Error);
                Logger.LogException(ex, this, Logtype.Error);
                response.Message = "Exception during update of existing plugin";
                SendMessage(response, sslStream);
            }
        }

        /// <summary>
        /// Handles DeletePluginMessages
        /// If the user is authenticated, it tries to delete an existing plugin in the database
        /// Users can only delete their plugins; admins can delete all plugins
        /// Then, it sends a response message which contains if it succeeded or failed
        /// </summary>
        /// <param name="deletePluginMessage"></param>
        /// <param name="sslStream"></param>
        private void HandleDelePluginMessage(DeletePluginMessage deletePluginMessage, SslStream sslStream)
        {
            Logger.LogText(string.Format("User {0} tries to delete a plugin", Username), this, Logtype.Debug);

            //Only authenticated users are allowed to delete plugins
            if (!ClientIsAuthenticated)
            {
                ResponsePluginModificationMessage response = new ResponsePluginModificationMessage
                {
                    ModifiedPlugin = false,
                    Message = "Unauthorized to delete that plugin"
                };
                Logger.LogText(string.Format("Unauthorized user {0} tried to delete plugin={1} from IP={2}", Username, deletePluginMessage.Plugin.Id, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }
            //check, if plugin to update exist
            Plugin plugin = Database.GetPlugin(deletePluginMessage.Plugin.Id);
            if (plugin == null)
            {
                ResponsePluginModificationMessage response = new ResponsePluginModificationMessage
                {
                    ModifiedPlugin = false,
                    Message = "Unauthorized to delete that plugin" // we send an "unauthorized"; thus, it is not possible to search database for existing ids
                };
                Logger.LogText(string.Format("User {0} tried to delete non-existing plugin={1} from IP={2}", Username, deletePluginMessage.Plugin.Id, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }
            //"normal" users are only allowed to update their own plugins
            if (ClientIsAdmin == false && plugin.Username != Username)
            {
                ResponsePluginModificationMessage response = new ResponsePluginModificationMessage
                {
                    ModifiedPlugin = false,
                    Message = "Unauthorized to delete that plugin"
                };
                Logger.LogText(string.Format("Unauthorized user {0} tried to delete plugin={1} from IP={2}", Username, deletePluginMessage.Plugin.Id, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }

            //Here, the user is authorized; thus, deletion of existing plugin in database is started
            try
            {
                plugin = deletePluginMessage.Plugin;
                Database.DeletePlugin(deletePluginMessage.Plugin.Id);
                Logger.LogText(string.Format("User {0} deleted existing plugin={1} in database", Username, plugin.Id), this, Logtype.Info);
                ResponsePluginModificationMessage response = new ResponsePluginModificationMessage
                {
                    ModifiedPlugin = true,
                    Message = string.Format("Deleted plugin in database: {0}", plugin.Id)
                };
                SendMessage(response, sslStream);
            }
            catch (Exception ex)
            {
                //Deletion failed; logg to logfile and return exception to client
                ResponsePluginModificationMessage response = new ResponsePluginModificationMessage
                {
                    ModifiedPlugin = false
                };
                Logger.LogText(string.Format("User {0} tried to delete an existing plugin={1}. But an exception occured: {2}", Username, deletePluginMessage.Plugin.Id, ex.Message), this, Logtype.Error);
                Logger.LogException(ex, this, Logtype.Error);
                response.Message = "Exception during delete of existing plugin";
                SendMessage(response, sslStream);
            }
        }

        /// <summary>
        /// Handles RequestPluginMessages
        /// Returns the plugin if it exists in the database
        /// Everyone is able to get plugins
        /// </summary>
        /// <param name="requestPluginMessage"></param>
        /// <param name="sslStream"></param>
        private void HandleRequestPluginMessage(RequestPluginMessage requestPluginMessage, SslStream sslStream)
        {
            Logger.LogText(string.Format("User {0} tries to request a plugin={1}", Username, requestPluginMessage.Id), this, Logtype.Debug);

            try
            {
                Plugin plugin = Database.GetPlugin(requestPluginMessage.Id);
                if (plugin == null)
                {
                    ResponsePluginMessage response = new ResponsePluginMessage
                    {
                        PluginExists = false
                    };
                    Logger.LogText(string.Format("User {0} tried to get a non-existing plugin", Username), this, Logtype.Warning);
                    response.Message = string.Format("Plugin {0} does not exist", requestPluginMessage.Id);
                    SendMessage(response, sslStream);
                }
                else
                {
                    if (plugin.Username == Username || ClientIsAdmin)
                    {
                        ResponsePluginMessage response = new ResponsePluginMessage
                        {
                            Plugin = plugin,
                            PluginExists = true
                        };
                        string message = string.Format("Responding with plugin={0}", plugin.Id);
                        Logger.LogText(message, this, Logtype.Debug);
                        response.Message = message;
                        SendMessage(response, sslStream);
                    }
                    else
                    {
                        ResponsePluginMessage response = new ResponsePluginMessage
                        {
                            PluginExists = false
                        };
                        Logger.LogText(string.Format("Unauthorized user {0} tried to get a plugin={1}", Username, requestPluginMessage.Id), this, Logtype.Warning);
                        response.Message = string.Format("Plugin {0} does not exist", requestPluginMessage.Id);
                        SendMessage(response, sslStream);
                    }
                }
            }
            catch (Exception ex)
            {
                //request failed; logg to logfile and return exception to client
                ResponsePluginMessage response = new ResponsePluginMessage();
                Logger.LogText(string.Format("User {0} tried to get an existing plugin. But an exception occured: {1}", Username, ex.Message), this, Logtype.Error);
                Logger.LogException(ex, this, Logtype.Error);
                response.Message = "Exception during request of existing plugin";
                SendMessage(response, sslStream);
            }
        }

        /// <summary>
        /// Handles RequestPluginListMessages
        /// responses with lists of plugins
        /// </summary>
        /// <param name="requestPluginListMessage"></param>
        /// <param name="sslStream"></param>
        private void HandleRequestPluginListMessage(RequestPluginListMessage requestPluginListMessage, SslStream sslStream)
        {
            Logger.LogText(string.Format("User {0} requested a list of plugins", Username), this, Logtype.Debug);

            try
            {
                List<Plugin> plugins = Database.GetPlugins(requestPluginListMessage.Username.Equals("*") ? null : requestPluginListMessage.Username);
                if (!ClientIsAdmin)
                {
                    plugins = (from p in plugins where p.Username == Username select p).ToList();
                }
                ResponsePluginListMessage response = new ResponsePluginListMessage
                {
                    Plugins = plugins
                };
                string message = string.Format("Responding with plugin list containing {0} elements", plugins.Count);
                Logger.LogText(message, this, Logtype.Debug);
                response.Message = message;
                SendMessage(response, sslStream);
            }
            catch (Exception ex)
            {
                //request failed; logg to logfile and return exception to client
                ResponsePluginMessage response = new ResponsePluginMessage();
                Logger.LogText(string.Format("User {0} tried to get a plugin list. But an exception occured: {1}", Username, ex.Message), this, Logtype.Error);
                Logger.LogException(ex, this, Logtype.Error);
                response.Message = "Exception during request of source list";
                SendMessage(response, sslStream);
            }
        }

        /// <summary>
        /// Handles RequestPublishedPluginListMessage
        /// responses with lists of published plugins
        /// </summary>
        /// <param name="requestPublishedPluginListMessage"></param>
        /// <param name="sslStream"></param>
        private void HandleRequestPublishedPluginListMessage(RequestPublishedPluginListMessage requestPublishedPluginListMessage, SslStream sslStream)
        {
            Logger.LogText(string.Format("User {0} requested a list of plugins for publishstate=", Username, requestPublishedPluginListMessage.PublishState.ToString()), this, Logtype.Debug);

            try
            {
                if (requestPublishedPluginListMessage.PublishState == PublishState.NOTPUBLISHED)
                {
                    List<PluginAndSource> pluginsAndSources = new List<PluginAndSource>();
                    ResponsePublishedPluginListMessage response = new ResponsePublishedPluginListMessage
                    {
                        PluginsAndSources = pluginsAndSources
                    };
                    string message = string.Format("Requesting unpublished plugins is not possible");
                    Logger.LogText(message, this, Logtype.Debug);
                    response.Message = message;
                    SendMessage(response, sslStream);
                }
                else
                {
                    List<PluginAndSource> pluginsAndSources = Database.GetPublishedPlugins(requestPublishedPluginListMessage.PublishState);
                    foreach (PluginAndSource pluginAndSource in pluginsAndSources)
                    {
                        try
                        {
                            string filename = Constants.CLIENTHANDLER_PLUGIN_ASSEMBLIES_FOLDER + Path.DirectorySeparatorChar + "Assembly-" + pluginAndSource.Plugin.Id + "-" + pluginAndSource.Source.PluginVersion + ".zip";
                            FileInfo fileInfo = new FileInfo(filename);
                            pluginAndSource.FileSize = fileInfo.Length;
                        }
                        catch (Exception)
                        {
                            pluginAndSource.FileSize = 0;
                        }
                    }
                    ResponsePublishedPluginListMessage response = new ResponsePublishedPluginListMessage
                    {
                        PluginsAndSources = pluginsAndSources
                    };
                    string message = string.Format("Responding with published plugin list containing {0} elements", pluginsAndSources.Count);
                    Logger.LogText(message, this, Logtype.Debug);
                    response.Message = message;
                    SendMessage(response, sslStream);
                }
            }
            catch (Exception ex)
            {
                //request failed; logg to logfile and return exception to client
                ResponsePublishedPluginListMessage response = new ResponsePublishedPluginListMessage();
                Logger.LogText(string.Format("User {0} tried to get a published plugin list. But an exception occured: {1}", Username, ex.Message), this, Logtype.Error);
                Logger.LogException(ex, this, Logtype.Error);
                response.Message = "Exception during request of source list";
                SendMessage(response, sslStream);
            }
        }

        /// <summary>
        /// Handles RequestPublishedPluginMessages
        /// Returns the plugin and source if it exists in the database
        /// </summary>
        /// <param name="requestPublishedPluginMessage"></param>
        /// <param name="sslStream"></param>
        private void HandleRequestPublishedPluginMessage(RequestPublishedPluginMessage requestPublishedPluginMessage, SslStream sslStream)
        {
            Logger.LogText(string.Format("User {0} tries to request a plugin={1}", Username, requestPublishedPluginMessage.Id), this, Logtype.Debug);

            try
            {
                PluginAndSource pluginAndSource = Database.GetPublishedPlugin(requestPublishedPluginMessage.Id, requestPublishedPluginMessage.PublishState);
                if (pluginAndSource == null)
                {
                    ResponsePublishedPluginMessage response = new ResponsePublishedPluginMessage
                    {
                        PluginAndSourceExist = false
                    };
                    Logger.LogText(string.Format("User {0} tried to get a non-existing published plugin", Username), this, Logtype.Warning);
                    response.Message = string.Format("Published plugin {0} does not exist", requestPublishedPluginMessage.Id);
                    SendMessage(response, sslStream);
                }
                else
                {
                    ResponsePublishedPluginMessage response = new ResponsePublishedPluginMessage
                    {
                        PluginAndSource = pluginAndSource,
                        PluginAndSourceExist = true
                    };
                    string message = string.Format("Responding with plugin={0}, source={1}-{2}", pluginAndSource.Plugin.Id, pluginAndSource.Source.PluginId, pluginAndSource.Source.PluginVersion);
                    Logger.LogText(message, this, Logtype.Debug);
                    response.Message = message;
                    SendMessage(response, sslStream);

                }
            }
            catch (Exception ex)
            {
                //request failed; logg to logfile and return exception to client
                ResponsePublishedPluginMessage response = new ResponsePublishedPluginMessage();
                Logger.LogText(string.Format("User {0} tried to get a published plugin. But an exception occured: {1}", Username, ex.Message), this, Logtype.Error);
                Logger.LogException(ex, this, Logtype.Error);
                response.Message = "Exception during request of published plugin";
                SendMessage(response, sslStream);
            }
        }

        /// <summary>
        /// Handles CreateNewSourceMessages
        /// checks, if corresponding plugin exist and is owned by the user. If yes, it creates the source.
        /// Also admin are able to create sources
        /// </summary>
        /// <param name="createNewSourceMessage"></param>
        /// <param name="sslStream"></param>
        private void HandleCreateNewSourceMessage(CreateNewSourceMessage createNewSourceMessage, SslStream sslStream)
        {
            Logger.LogText(string.Format("User {0} tries to create a source", Username), this, Logtype.Debug);

            //Only authenticated users are allowed to create new plugins
            if (!ClientIsAuthenticated)
            {
                ResponseSourceModificationMessage response = new ResponseSourceModificationMessage
                {
                    ModifiedSource = false,
                    Message = "Unauthorized to create new plugins. Please authenticate yourself"
                };
                Logger.LogText(string.Format("Unauthorized user {0} tried to create new source={1}-{2} from IP={3}", Username, createNewSourceMessage.Source.PluginId, createNewSourceMessage.Source.PluginVersion, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }

            //check, if plugin exists and is owned by the user
            Source source = createNewSourceMessage.Source;
            Plugin plugin = Database.GetPlugin(source.PluginId);

            //Plugin does not exist
            if (plugin == null)
            {
                ResponseSourceModificationMessage response = new ResponseSourceModificationMessage
                {
                    ModifiedSource = false,
                    Message = string.Format("Plugin with id={0} does not exist", source.PluginId)
                };
                Logger.LogText(string.Format("User {0} tried to create new source={1}-{2} from IP={3} for a non-existing plugin id={4}", Username, createNewSourceMessage.Source.PluginId, createNewSourceMessage.Source.PluginVersion, IPAddress, source.PluginId), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }

            //Plugin is not owned by the user and user is not admin
            if (plugin.Username != Username && !ClientIsAdmin)
            {
                ResponseSourceModificationMessage response = new ResponseSourceModificationMessage
                {
                    ModifiedSource = false,
                    Message = "Not authorized"
                };
                Logger.LogText(string.Format("User {0} tried to create new source={1}-{2} from IP={3} for a plugin={4} that he does not own ", Username, createNewSourceMessage.Source.PluginId, createNewSourceMessage.Source.PluginVersion, IPAddress, source.PluginId), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }

            //Here, everything is fine; thus, we try to create the source
            try
            {
                Database.CreateSource(source);
                Logger.LogText(string.Format("User {0} created new source={1}-{2}", Username, source.PluginId, source.PluginVersion), this, Logtype.Info);
                ResponseSourceModificationMessage response = new ResponseSourceModificationMessage
                {
                    ModifiedSource = true,
                    Message = string.Format("Created new source={0}-{1} in database", source.PluginId, source.PluginVersion)
                };
                SendMessage(response, sslStream);
            }
            catch (Exception ex)
            {
                //creation failed; logg to logfile and return exception to client
                ResponseSourceModificationMessage response = new ResponseSourceModificationMessage
                {
                    ModifiedSource = false
                };
                Logger.LogText(string.Format("User {0} tried to create a new source. But an exception occured: {1}", Username, ex.Message), this, Logtype.Error);
                Logger.LogException(ex, this, Logtype.Error);
                response.Message = "Exception during creation of new source";
                SendMessage(response, sslStream);
            }
        }

        /// <summary>
        /// Handles UpdateSourceMessages
        /// If the user is authenticated, it tries to update an existing source in the database
        /// Users can only update their sources; admins can update all sources
        /// Then, it sends a response message which contains if it succeeded or failed
        /// </summary>
        /// <param name="updateSourceMessage"></param>
        /// <param name="sslStream"></param>
        private void HandleUpdateSourceMessage(UpdateSourceMessage updateSourceMessage, SslStream sslStream)
        {
            Logger.LogText(string.Format("User {0} tries to update a source", Username), this, Logtype.Debug);

            //Only authenticated users are allowed to update sources
            if (!ClientIsAuthenticated)
            {
                ResponseSourceModificationMessage response = new ResponseSourceModificationMessage
                {
                    ModifiedSource = false,
                    Message = "Unauthorized to update that source"
                };
                Logger.LogText(string.Format("Unauthorized user {0} tried to update source={1}-{2} from IP={3}", Username, updateSourceMessage.Source.PluginId, updateSourceMessage.Source.PluginVersion, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }
            //check, if source to update exist
            Plugin plugin = Database.GetPlugin(updateSourceMessage.Source.PluginId);
            Source source = Database.GetSource(updateSourceMessage.Source.PluginId, updateSourceMessage.Source.PluginVersion);
            if (source == null)
            {
                ResponseSourceModificationMessage response = new ResponseSourceModificationMessage
                {
                    ModifiedSource = false,
                    Message = "Unauthorized to update that source" // we send an "unauthorized"; thus, it is not possible to search database for existing ids
                };
                Logger.LogText(string.Format("User {0} tried to update non-existing source={1}-{2} from IP={3}", Username, updateSourceMessage.Source.PluginId, updateSourceMessage.Source.PluginVersion, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }
            //"normal" users are only allowed to update their own sources
            if (ClientIsAdmin == false && plugin.Username != Username)
            {
                ResponseSourceModificationMessage response = new ResponseSourceModificationMessage
                {
                    ModifiedSource = false,
                    Message = "Unauthorized to update that source"
                };
                Logger.LogText(string.Format("Unauthorized user {0} tried to update source={1}-{2} from IP={3}", Username, updateSourceMessage.Source.PluginId, updateSourceMessage.Source.PluginVersion, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }

            //Here, the user is authorized; thus, update of existing source in database is started
            try
            {
                source = updateSourceMessage.Source;
                Database.UpdateSource(source.PluginId, source.PluginVersion, source.ZipFileName, source.BuildState, source.BuildLog, source.BuildVersion);
                Logger.LogText(string.Format("User {0} updated existing source={1}-{2} in database", Username, source.PluginId, source.PluginVersion), this, Logtype.Info);
                ResponseSourceModificationMessage response = new ResponseSourceModificationMessage
                {
                    ModifiedSource = true,
                    Message = string.Format("Updated source={0}-{1} in database {0}", source.PluginId, source.PluginVersion)
                };
                SendMessage(response, sslStream);
            }
            catch (Exception ex)
            {
                //update failed; logg to logfile and return exception to client
                ResponseSourceModificationMessage response = new ResponseSourceModificationMessage
                {
                    ModifiedSource = false
                };
                Logger.LogText(string.Format("User {0} tried to update an existing source. But an exception occured: {1}", Username, ex.Message), this, Logtype.Error);
                Logger.LogException(ex, this, Logtype.Error);
                response.Message = "Exception during update of existing source";
                SendMessage(response, sslStream);
            }
        }

        /// <summary>
        /// Handles UpdateSourcePublishStateMessage
        /// only admins are allowed to update the publish state of a source
        /// </summary>
        /// <param name="updateSourceMessage"></param>
        /// <param name="sslStream"></param>
        private void HandleUpdateSourcePublishStateMessage(UpdateSourcePublishStateMessage updateSourceMessage, SslStream sslStream)
        {
            Logger.LogText(string.Format("User {0} tries to update the publish state of a source", Username), this, Logtype.Debug);

            //Only authenticated users are allowed to update sources
            if (!ClientIsAuthenticated)
            {
                ResponseSourceModificationMessage response = new ResponseSourceModificationMessage
                {
                    ModifiedSource = false,
                    Message = "Unauthorized to update that source"
                };
                Logger.LogText(string.Format("Unauthorized user {0} tried to update the publish state of source={1}-{2} from IP={3}", Username, updateSourceMessage.Source.PluginId, updateSourceMessage.Source.PluginVersion, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }
            //check, if source to update exist
            Plugin plugin = Database.GetPlugin(updateSourceMessage.Source.PluginId);
            Source source = Database.GetSource(updateSourceMessage.Source.PluginId, updateSourceMessage.Source.PluginVersion);
            if (source == null)
            {
                ResponseSourceModificationMessage response = new ResponseSourceModificationMessage
                {
                    ModifiedSource = false,
                    Message = "Unauthorized to update that source" // we send an "unauthorized"; thus, it is not possible to search database for existing ids
                };
                Logger.LogText(string.Format("User {0} tried to update the public state of a non-existing source={1}-{2} from IP={3}", Username, updateSourceMessage.Source.PluginId, updateSourceMessage.Source.PluginVersion, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }
            //only admins are allowed to update the publish state
            if (ClientIsAdmin == false)
            {
                ResponseSourceModificationMessage response = new ResponseSourceModificationMessage
                {
                    ModifiedSource = false,
                    Message = "Unauthorized to update that source"
                };
                Logger.LogText(string.Format("Unauthorized user {0} tried to update source={1}-{2} from IP={3}", Username, updateSourceMessage.Source.PluginId, updateSourceMessage.Source.PluginVersion, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }

            //Here, the user is authorized; thus, update of existing source in database is started
            try
            {
                source = updateSourceMessage.Source;

                PublishState publishState;
                switch (source.PublishState)
                {
                    case "DEVELOPER":
                        publishState = PublishState.DEVELOPER;
                        break;
                    case "NIGHTLY":
                        publishState = PublishState.NIGHTLY;
                        break;
                    case "BETA":
                        publishState = PublishState.BETA;
                        break;
                    case "RELEASE":
                        publishState = PublishState.RELEASE;
                        break;
                    default:
                    case "NOTPUBLISHED":
                        publishState = PublishState.NOTPUBLISHED;
                        break;
                }

                Database.UpdateSource(source.PluginId, source.PluginVersion, publishState);
                Logger.LogText(string.Format("User {0} updated publish state of existing source={1}-{2} in database", Username, source.PluginId, source.PluginVersion), this, Logtype.Info);
                ResponseSourceModificationMessage response = new ResponseSourceModificationMessage
                {
                    ModifiedSource = true,
                    Message = string.Format("Updated publish state of source={0}-{1} in database", source.PluginId, source.PluginVersion)
                };
                SendMessage(response, sslStream);
            }
            catch (Exception ex)
            {
                //update failed; logg to logfile and return exception to client
                ResponseSourceModificationMessage response = new ResponseSourceModificationMessage
                {
                    ModifiedSource = false
                };
                Logger.LogText(string.Format("User {0} tried to update the publish state of an existing source. But an exception occured: {1}", Username, ex.Message), this, Logtype.Error);
                Logger.LogException(ex, this, Logtype.Error);
                response.Message = "Exception during update of publish state of existing source";
                SendMessage(response, sslStream);
            }
        }

        /// <summary>
        /// Handles DeleteSourceMessages
        /// If the user is authenticated, it tries to delete an existing source in the database
        /// Users are only allowed to delete their sources
        /// Admins are allowed to delete any source
        /// Then, it sends a response message which contains if it succeeded or failed
        /// </summary>
        /// <param name="deleteSourceMessage"></param>
        /// <param name="sslStream"></param>
        private void HandleDeleteSourceMessage(DeleteSourceMessage deleteSourceMessage, SslStream sslStream)
        {
            Logger.LogText(string.Format("User {0} tries to delete a source", Username), this, Logtype.Debug);

            //Only authenticated users are allowed to delete sources
            if (!ClientIsAuthenticated)
            {
                ResponseSourceModificationMessage response = new ResponseSourceModificationMessage
                {
                    ModifiedSource = false,
                    Message = "Unauthorized to delete that source"
                };
                Logger.LogText(string.Format("Unauthorized user {0} tried to delete source={1}-{2} from IP={3}", Username, deleteSourceMessage.Source.PluginId, deleteSourceMessage.Source.PluginVersion, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }
            //check, if source to delete exist
            Plugin plugin = Database.GetPlugin(deleteSourceMessage.Source.PluginId);
            Source source = Database.GetSource(deleteSourceMessage.Source.PluginId, deleteSourceMessage.Source.PluginVersion);
            if (source == null)
            {
                ResponseSourceModificationMessage response = new ResponseSourceModificationMessage
                {
                    ModifiedSource = false,
                    Message = "Unauthorized to delete that source" // we send an "unauthorized"; thus, it is not possible to search database for existing ids
                };
                Logger.LogText(string.Format("User {0} tried to delete non-existing source={1}-{2} from IP={3}", Username, deleteSourceMessage.Source.PluginId, deleteSourceMessage.Source.PluginVersion, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }
            //"normal" users are only allowed to update their own sources
            if (ClientIsAdmin == false && plugin.Username != Username)
            {
                ResponseSourceModificationMessage response = new ResponseSourceModificationMessage
                {
                    ModifiedSource = false,
                    Message = "Unauthorized to delete that source"
                };
                Logger.LogText(string.Format("Unauthorized user {0} tried to delete source={1}-{2} from IP={3}", Username, deleteSourceMessage.Source.PluginId, deleteSourceMessage.Source.PluginVersion, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }

            //Here, the user is authorized; thus, deletion of existing source in database as well as deletion of files is started
            try
            {
                //1. delete files in file system
                string filename = Constants.CLIENTHANDLER_PLUGIN_SOURCE_FOLDER + Path.DirectorySeparatorChar + "Source-" + source.PluginId + "-" + source.PluginVersion + ".zip";
                if (File.Exists(filename))
                {
                    Logger.LogText(string.Format("Deleting source zipfile: {0}", filename), this, Logtype.Info);
                    File.Delete(filename);
                    Logger.LogText(string.Format("Deleted source zipfile: {0}", filename), this, Logtype.Info);
                }
                filename = Constants.CLIENTHANDLER_PLUGIN_ASSEMBLIES_FOLDER + Path.DirectorySeparatorChar + "Assembly-" + source.PluginId + "-" + source.PluginVersion + ".zip";
                if (File.Exists(filename))
                {
                    Logger.LogText(string.Format("Deleting assembly zipfile: {0}", filename), this, Logtype.Info);
                    File.Delete(filename);
                    Logger.LogText(string.Format("Deleted assembly zipfile: {0}", filename), this, Logtype.Info);
                }
                //2. delete source in database
                Database.DeleteSource(deleteSourceMessage.Source.PluginId, deleteSourceMessage.Source.PluginVersion);
                Logger.LogText(string.Format("User {0} deleted existing source in database: {1}-{2}", Username, deleteSourceMessage.Source.PluginId, deleteSourceMessage.Source.PluginVersion), this, Logtype.Info);
                ResponseSourceModificationMessage response = new ResponseSourceModificationMessage
                {
                    ModifiedSource = true,
                    Message = string.Format("Deleted source={0}-{1} in database", source.PluginId, source.PluginVersion)
                };
                SendMessage(response, sslStream);
            }
            catch (Exception ex)
            {
                //Deletion failed; logg to logfile and return exception to client
                ResponseSourceModificationMessage response = new ResponseSourceModificationMessage
                {
                    ModifiedSource = false
                };
                Logger.LogText(string.Format("User {0} tried to delete an existing source={1}-{2}. But an exception occured: {3}", Username, deleteSourceMessage.Source.PluginId, deleteSourceMessage.Source.PluginVersion, ex.Message), this, Logtype.Error);
                Logger.LogException(ex, this, Logtype.Error);
                response.Message = "Exception during delete of existing source";
                SendMessage(response, sslStream);
            }
        }

        /// <summary>
        /// Handles RequestSourceMessages
        /// Returns the source if it exists in the database
        /// Only the owners of a plugin or admins are allowed to get the sources
        /// </summary>
        /// <param name="requestSourceMessage"></param>
        /// <param name="sslStream"></param>
        private void HandleRequestSourceMessage(RequestSourceMessage requestSourceMessage, SslStream sslStream)
        {
            Logger.LogText(string.Format("User {0} requested a source={1}-{2}", Username, requestSourceMessage.PluginId, requestSourceMessage.PluginVersion), this, Logtype.Debug);

            try
            {
                Source source = Database.GetSource(requestSourceMessage.PluginId, requestSourceMessage.PluginVersion);
                if (source == null)
                {
                    ResponseSourceMessage response = new ResponseSourceMessage
                    {
                        SourceExists = false
                    };
                    Logger.LogText(string.Format("User {0} tried to get a non-existing source={1}-{2}", Username, requestSourceMessage.PluginId, requestSourceMessage.PluginVersion), this, Logtype.Warning);
                    response.Message = "Unauthorized to get that source";
                    SendMessage(response, sslStream);
                }
                else
                {
                    //Check, if plugin is owned by user or user is admin
                    Plugin plugin = Database.GetPlugin(source.PluginId);
                    if (ClientIsAdmin || plugin.Username == Username)
                    {
                        ResponseSourceMessage response = new ResponseSourceMessage
                        {
                            Source = source,
                            SourceExists = true
                        };
                        string message = string.Format("Responding with source {0}-{1}", source.PluginId, source.PluginVersion);
                        Logger.LogText(message, this, Logtype.Debug);
                        response.Message = message;
                        SendMessage(response, sslStream);
                    }
                    else
                    {
                        ResponseSourceMessage response = new ResponseSourceMessage
                        {
                            SourceExists = false
                        };
                        Logger.LogText(string.Format("Unauthorized user {0} tried to get a source {1}-{2}", Username, requestSourceMessage.PluginId, requestSourceMessage.PluginVersion), this, Logtype.Warning);
                        response.Message = "Unauthorized to get that source";
                        SendMessage(response, sslStream);
                    }
                }
            }
            catch (Exception ex)
            {
                //request failed; logg to logfile and return exception to client
                ResponseSourceMessage response = new ResponseSourceMessage();
                Logger.LogText(string.Format("User {0} tried to get an existing source={1}-{2}. But an exception occured: {3}", Username, requestSourceMessage.PluginId, requestSourceMessage.PluginVersion, ex.Message), this, Logtype.Error);
                Logger.LogException(ex, this, Logtype.Error);
                response.Message = "Exception during request of existing source";
                SendMessage(response, sslStream);
            }
        }

        /// <summary>
        /// Handles RequestSourceListMessages
        /// responses with lists of sources
        /// Only sources are returned that are owned by the user
        /// Admins may receice everything
        /// </summary>
        /// <param name="requestSourceListMessage"></param>
        /// <param name="sslStream"></param>
        private void HandleRequestSourceListMessage(RequestSourceListMessage requestSourceListMessage, SslStream sslStream)
        {
            Logger.LogText(string.Format("User {0} requested a list of sources", Username), this, Logtype.Debug);

            //Only authenticated admins are allowed to receive source lists
            if (!ClientIsAuthenticated)
            {
                ResponseSourceListMessage response = new ResponseSourceListMessage
                {
                    AllowedToViewList = false,
                    Message = "Unauthorized to get resource list. Please authenticate yourself"
                };
                Logger.LogText(string.Format("Unauthorized user {0} tried to request source list of plugin={1} from IP={2}", Username, requestSourceListMessage.PluginId, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }
            try
            {
                if (!ClientIsAdmin)
                {
                    Plugin plugin = Database.GetPlugin(requestSourceListMessage.PluginId);
                    if (plugin.Username != Username)
                    {
                        ResponseSourceListMessage response = new ResponseSourceListMessage
                        {
                            AllowedToViewList = false,
                            Message = "Unauthorized to get resource list. Please authenticate yourself"
                        };
                        Logger.LogText(string.Format("Unauthorized user {0} tried to request source list of plugin={1} from IP={2}", Username, requestSourceListMessage.PluginId, IPAddress), this, Logtype.Warning);
                        SendMessage(response, sslStream);
                        return;
                    }
                }
                List<Source> sources = new List<Source>();
                if (requestSourceListMessage.PluginId != -1)
                {
                    //return all sources of a dedicated plugin
                    sources = Database.GetSources(requestSourceListMessage.PluginId);
                }
                else if (ClientIsAdmin && !string.IsNullOrEmpty(requestSourceListMessage.BuildState))
                {
                    //return all sources in a dedicated BuildState; mainly needed by buildserver
                    //only admin users are allowed to do this
                    sources = Database.GetSources(requestSourceListMessage.BuildState);
                }
                ResponseSourceListMessage responseSourceListMessage = new ResponseSourceListMessage
                {
                    AllowedToViewList = true
                };
                string message = string.Format("Responding with source list containing {0} elements", sources.Count);
                Logger.LogText(message, this, Logtype.Debug);
                responseSourceListMessage.Message = message;
                responseSourceListMessage.SourceList = sources;
                SendMessage(responseSourceListMessage, sslStream);
            }
            catch (Exception ex)
            {
                //request failed; logg to logfile and return exception to client
                ResponseSourceListMessage response = new ResponseSourceListMessage();
                Logger.LogText(string.Format("User {0} tried to get a source list. But an exception occured: {1}", Username, ex.Message), this, Logtype.Error);
                Logger.LogException(ex, this, Logtype.Error);
                response.Message = "Exception during request of source list";
                SendMessage(response, sslStream);
            }
        }

        /// <summary>
        /// Handles CreateNewResourceMessages
        /// If the user is authenticated, it tries to create a new resource in the database        
        /// Then, it sends a response message which contains if it succeeded or failed
        /// </summary>
        /// <param name="createNewResourceMessage"></param>
        /// <param name="sslStream"></param>
        private void HandleCreateNewResourceMessage(CreateNewResourceMessage createNewResourceMessage, SslStream sslStream)
        {
            Logger.LogText(string.Format("User {0} tries to create a resource={1}", createNewResourceMessage.Resource.Name, Username), this, Logtype.Debug);

            //Only authenticated users are allowed to create new plugins
            if (!ClientIsAuthenticated)
            {
                ResponseResourceModificationMessage response = new ResponseResourceModificationMessage
                {
                    ModifiedResource = false,
                    Message = "Unauthorized to create new resources. Please authenticate yourself"
                };
                Logger.LogText(string.Format("Unauthorized user {0} tried to create new resource={1} from IP={2}", Username, createNewResourceMessage.Resource.Name, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }
            //Here, the user is authenticated; thus, creation of new resource in database is started
            try
            {
                Resource resource = createNewResourceMessage.Resource;
                Database.CreateResource(Username, resource.Name, resource.Description);
                Logger.LogText(string.Format("User {0} created new resource={1} in database", Username, resource.Name), this, Logtype.Info);
                ResponseResourceModificationMessage response = new ResponseResourceModificationMessage
                {
                    ModifiedResource = true,
                    Message = string.Format("Created new resource={0} in database", resource.Name)
                };
                SendMessage(response, sslStream);
            }
            catch (Exception ex)
            {
                //creation failed; logg to logfile and return exception to client
                ResponseResourceModificationMessage response = new ResponseResourceModificationMessage
                {
                    ModifiedResource = false
                };
                Logger.LogText(string.Format("User {0} tried to create a new resource. But an exception occured: {1}", Username, ex.Message), this, Logtype.Error);
                Logger.LogException(ex, this, Logtype.Error);
                response.Message = "Exception during creation of new resource";
                SendMessage(response, sslStream);
            }
        }

        /// <summary>
        /// Handles UpdateResourceMessages
        /// If the user is authenticated, it tries to update an existing resource in the database
        /// Users can only update their resources; admins can update all resources
        /// Then, it sends a response message which contains if it succeeded or failed
        /// </summary>
        /// <param name="updateResourceMessage"></param>
        /// <param name="sslStream"></param>
        private void HandleUpdateResourceMessage(UpdateResourceMessage updateResourceMessage, SslStream sslStream)
        {
            Logger.LogText(string.Format("User {0} tries to update a resource={1}", Username, updateResourceMessage.Resource.Id), this, Logtype.Debug);

            //Only authenticated users are allowed to update resources
            if (!ClientIsAuthenticated)
            {
                ResponseResourceModificationMessage response = new ResponseResourceModificationMessage
                {
                    ModifiedResource = false,
                    Message = "Unauthorized to update that resource"
                };
                Logger.LogText(string.Format("Unauthorized user {0} tried to update resource={1} from IP={2}", Username, updateResourceMessage.Resource, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }
            //check, if resource to update exist
            Resource resource = Database.GetResource(updateResourceMessage.Resource.Id);
            if (resource == null)
            {
                ResponseResourceModificationMessage response = new ResponseResourceModificationMessage
                {
                    ModifiedResource = false,
                    Message = "Unauthorized to update that resource" // we send an "unauthorized"; thus, it is not possible to search database for existing ids
                };
                Logger.LogText(string.Format("User {0} tried to update non-existing resource={1} from IP={2}", Username, updateResourceMessage.Resource, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }
            //"normal" users are only allowed to update their own resources
            if (ClientIsAdmin == false && resource.Username != Username)
            {
                ResponseResourceModificationMessage response = new ResponseResourceModificationMessage
                {
                    ModifiedResource = false,
                    Message = "Unauthorized to update that resource"
                };
                Logger.LogText(string.Format("Unauthorized user {0} tried to update resource={1} from IP={2}", Username, updateResourceMessage.Resource, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }

            //Here, the user is authorized; thus, update of existing resource in database is started
            try
            {
                resource = updateResourceMessage.Resource;
                Database.UpdateResource(resource.Id, resource.Name, resource.Description);
                resource = Database.GetResource(resource.Id);
                Logger.LogText(string.Format("User {0} updated existing resource={1} in database", Username, resource.Id), this, Logtype.Info);
                ResponseResourceModificationMessage response = new ResponseResourceModificationMessage
                {
                    ModifiedResource = true,
                    Message = string.Format("Updated resource={0} in database.", resource.Id)
                };
                SendMessage(response, sslStream);
            }
            catch (Exception ex)
            {
                //update failed; logg to logfile and return exception to client
                ResponseResourceModificationMessage response = new ResponseResourceModificationMessage
                {
                    ModifiedResource = false
                };
                Logger.LogText(string.Format("User {0} tried to update an existing resource={1}. But an exception occured: {2}", Username, resource.Id, ex.Message), this, Logtype.Error);
                Logger.LogException(ex, this, Logtype.Error);
                response.Message = "Exception during update of existing resource";
                SendMessage(response, sslStream);
            }
        }

        /// <summary>
        /// Handles DeleteResourceMessages
        /// If the user is authenticated, it tries to delete an existing resource in the database
        /// Users can only delete their resources; admins can delete all resources
        /// Then, it sends a response message which contains if it succeeded or failed
        /// </summary>
        /// <param name="deleteResourceMessage"></param>
        /// <param name="sslStream"></param>
        private void HandleDeletResourceMessage(DeleteResourceMessage deleteResourceMessage, SslStream sslStream)
        {
            Logger.LogText(string.Format("User {0} tries to delete a resource={1}", Username, deleteResourceMessage.Resource.Id), this, Logtype.Debug);

            //Only authenticated users are allowed to delete resources
            if (!ClientIsAuthenticated)
            {
                ResponseResourceModificationMessage response = new ResponseResourceModificationMessage
                {
                    ModifiedResource = false,
                    Message = "Unauthorized to delete that resource"
                };
                Logger.LogText(string.Format("Unauthorized user {0} tried to delete resource={1} from IP={2}", Username, deleteResourceMessage.Resource.Id, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }
            //check, if resource to update exist
            Resource resource = Database.GetResource(deleteResourceMessage.Resource.Id);
            if (resource == null)
            {
                ResponseResourceModificationMessage response = new ResponseResourceModificationMessage
                {
                    ModifiedResource = false,
                    Message = "Unauthorized to delete that resource" // we send an "unauthorized"; thus, it is not possible to search database for existing ids
                };
                Logger.LogText(string.Format("User {0} tried to delete non-existing resource={1} from IP={2}", Username, deleteResourceMessage.Resource.Id, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }
            //"normal" users are only allowed to update their own resources
            if (ClientIsAdmin == false && resource.Username != Username)
            {
                ResponseResourceModificationMessage response = new ResponseResourceModificationMessage
                {
                    ModifiedResource = false,
                    Message = "Unauthorized to delete that resource"
                };
                Logger.LogText(string.Format("Unauthorized user {0} tried to delete resource={1} from IP={2}", Username, deleteResourceMessage.Resource.Id, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }

            //Here, the user is authorized; thus, deletion of existing resource in database is started
            try
            {
                resource = deleteResourceMessage.Resource;
                Database.DeleteResource(deleteResourceMessage.Resource.Id);
                Logger.LogText(string.Format("User {0} deleted existing resource in database: {1}", Username, resource.Id), this, Logtype.Info);
                ResponseResourceModificationMessage response = new ResponseResourceModificationMessage
                {
                    ModifiedResource = true,
                    Message = string.Format("Deleted resource in database: {0}", resource.Id)
                };
                SendMessage(response, sslStream);
            }
            catch (Exception ex)
            {
                //Deletion failed; logg to logfile and return exception to client
                ResponseResourceModificationMessage response = new ResponseResourceModificationMessage
                {
                    ModifiedResource = false
                };
                Logger.LogText(string.Format("User {0} tried to delete an existing resource={1}. But an exception occured: {2}", Username, deleteResourceMessage.Resource.Id, ex.Message), this, Logtype.Error);
                Logger.LogException(ex, this, Logtype.Error);
                response.Message = "Exception during delete of existing resource";
                SendMessage(response, sslStream);
            }
        }

        /// <summary>
        /// Handles RequestResourceMessages
        /// Returns the resource if it exists in the database
        /// Everyone is able to get resources
        /// </summary>
        /// <param name="requestResourceMessage"></param>
        /// <param name="sslStream"></param>
        private void HandleRequestResourceMessage(RequestResourceMessage requestResourceMessage, SslStream sslStream)
        {
            Logger.LogText(string.Format("User {0} tries to request a resource={1}", Username, requestResourceMessage.Id), this, Logtype.Debug);

            try
            {
                Resource resource = Database.GetResource(requestResourceMessage.Id);
                if (resource == null)
                {
                    ResponseResourceMessage response = new ResponseResourceMessage
                    {
                        ResourceExists = false
                    };
                    Logger.LogText(string.Format("User {0} tried to get a non-existing resource", Username), this, Logtype.Warning);
                    response.Message = string.Format("Resource {0} does not exist", requestResourceMessage.Id);
                    SendMessage(response, sslStream);
                }
                else
                {

                    if (resource.Username != Username && !ClientIsAdmin)
                    {
                        ResponseResourceMessage response = new ResponseResourceMessage
                        {
                            ResourceExists = false
                        };
                        Logger.LogText(string.Format("Unauthorized user {0} tried to get a resource={1}", Username, requestResourceMessage.Id), this, Logtype.Warning);
                        response.Message = string.Format("Resource {0} does not exist", requestResourceMessage.Id);
                        SendMessage(response, sslStream);
                    }
                    else
                    {
                        ResponseResourceMessage response = new ResponseResourceMessage
                        {
                            Resource = resource,
                            ResourceExists = true
                        };
                        string message = string.Format("Responding with resource={0}", resource.Id);
                        Logger.LogText(message, this, Logtype.Debug);
                        response.Message = message;
                        SendMessage(response, sslStream);
                    }
                }
            }
            catch (Exception ex)
            {
                //request failed; logg to logfile and return exception to client
                ResponseResourceMessage response = new ResponseResourceMessage();
                Logger.LogText(string.Format("User {0} tried to get an existing resource. But an exception occured: {1}", Username, ex.Message), this, Logtype.Error);
                Logger.LogException(ex, this, Logtype.Error);
                response.Message = "Exception during request of existing resource";
                SendMessage(response, sslStream);
            }
        }

        /// <summary>
        /// Handles RequestResourceListMessages
        /// responses with lists of resources
        /// </summary>
        /// <param name="requestResourceListMessage"></param>
        /// <param name="sslStream"></param>
        private void HandleRequestResourceListMessage(RequestResourceListMessage requestResourceListMessage, SslStream sslStream)
        {
            Logger.LogText(string.Format("User {0} requested a list of resources", Username), this, Logtype.Debug);

            try
            {
                List<Resource> resources = Database.GetResources(requestResourceListMessage.Username.Equals("*") ? null : requestResourceListMessage.Username);
                if (!ClientIsAdmin)
                {
                    resources = (from p in resources where p.Username == Username select p).ToList();
                }
                ResponseResourceListMessage response = new ResponseResourceListMessage
                {
                    Resources = resources
                };
                string message = string.Format("Responding with resource list containing {0} elements", resources.Count);
                Logger.LogText(message, this, Logtype.Debug);
                response.Message = message;
                SendMessage(response, sslStream);
            }
            catch (Exception ex)
            {
                //request failed; logg to logfile and return exception to client
                ResponseResourceMessage response = new ResponseResourceMessage();
                Logger.LogText(string.Format("User {0} tried to get a resource list. But an exception occured: {1}", Username, ex.Message), this, Logtype.Error);
                Logger.LogException(ex, this, Logtype.Error);
                response.Message = "Exception during request of resource list";
                SendMessage(response, sslStream);
            }
        }

        /// <summary>
        /// Handles RequestPublishedResourceListMessage
        /// responses with lists of published resources
        /// </summary>
        /// <param name="requestResourceListMessage"></param>
        /// <param name="sslStream"></param>
        private void HandleRequestPublishedResourceListMessage(RequestPublishedResourceListMessage requestPublishedResourceListMessage, SslStream sslStream)
        {
            Logger.LogText(string.Format("User {0} requested a list of resources for publishstate=", Username, requestPublishedResourceListMessage.PublishState.ToString()), this, Logtype.Debug);

            try
            {
                if (requestPublishedResourceListMessage.PublishState == PublishState.NOTPUBLISHED)
                {
                    List<ResourceAndResourceData> resourcesAndResourceDatas = new List<ResourceAndResourceData>();
                    ResponsePublishedResourceListMessage response = new ResponsePublishedResourceListMessage
                    {
                        ResourcesAndResourceDatas = resourcesAndResourceDatas
                    };
                    string message = string.Format("Requesting unpublished resources is not possible");
                    Logger.LogText(message, this, Logtype.Debug);
                    response.Message = message;
                    SendMessage(response, sslStream);
                }
                else
                {
                    List<ResourceAndResourceData> resourcesAndResourceDatas = Database.GetPublishedResources(requestPublishedResourceListMessage.PublishState);
                    foreach (ResourceAndResourceData resourceAndResourceData in resourcesAndResourceDatas)
                    {
                        try
                        {
                            string filename = Constants.CLIENTHANDLER_RESOURCEDATA_FOLDER + Path.DirectorySeparatorChar + "ResourceData-" + resourceAndResourceData.ResourceData.ResourceId + "-" + resourceAndResourceData.ResourceData.ResourceVersion + ".bin";
                            FileInfo fileInfo = new FileInfo(filename);
                            resourceAndResourceData.FileSize = fileInfo.Length;
                        }
                        catch (Exception)
                        {
                            resourceAndResourceData.FileSize = 0;
                        }
                    }
                    ResponsePublishedResourceListMessage response = new ResponsePublishedResourceListMessage
                    {
                        ResourcesAndResourceDatas = resourcesAndResourceDatas
                    };
                    string message = string.Format("Responding with published resource list containing {0} elements", resourcesAndResourceDatas.Count);
                    Logger.LogText(message, this, Logtype.Debug);
                    response.Message = message;
                    SendMessage(response, sslStream);
                }
            }
            catch (Exception ex)
            {
                //request failed; logg to logfile and return exception to client
                ResponsePublishedResourceListMessage response = new ResponsePublishedResourceListMessage();
                Logger.LogText(string.Format("User {0} tried to get a published resource list. But an exception occured: {1}", Username, ex.Message), this, Logtype.Error);
                Logger.LogException(ex, this, Logtype.Error);
                response.Message = "Exception during request of source list";
                SendMessage(response, sslStream);
            }
        }

        /// <summary>
        /// Handles RequestPublishedResourceMessages
        /// Returns the resource and source if it exists in the database
        /// </summary>
        /// <param name="requestPublishedResourceMessage"></param>
        /// <param name="sslStream"></param>
        private void HandleRequestPublishedResourceMessage(RequestPublishedResourceMessage requestPublishedResourceMessage, SslStream sslStream)
        {
            Logger.LogText(string.Format("User {0} tries to request a resource={1}", Username, requestPublishedResourceMessage.Id), this, Logtype.Debug);

            try
            {
                ResourceAndResourceData resourceAndResourceData = Database.GetPublishedResource(requestPublishedResourceMessage.Id, requestPublishedResourceMessage.PublishState);
                if (resourceAndResourceData == null)
                {
                    ResponsePublishedResourceMessage response = new ResponsePublishedResourceMessage
                    {
                        ResourceAndResourceDataExist = false
                    };
                    Logger.LogText(string.Format("User {0} tried to get a non-existing published resource", Username), this, Logtype.Warning);
                    response.Message = string.Format("Published resource {0} does not exist", requestPublishedResourceMessage.Id);
                    SendMessage(response, sslStream);
                }
                else
                {
                    ResponsePublishedResourceMessage response = new ResponsePublishedResourceMessage
                    {
                        ResourceAndResourceData = resourceAndResourceData,
                        ResourceAndResourceDataExist = true
                    };
                    string message = string.Format("Responding with resource={0}, source={1}-{2}", resourceAndResourceData.Resource.Id, resourceAndResourceData.ResourceData.ResourceId, resourceAndResourceData.ResourceData.ResourceVersion);
                    Logger.LogText(message, this, Logtype.Debug);
                    response.Message = message;
                    SendMessage(response, sslStream);

                }
            }
            catch (Exception ex)
            {
                //request failed; logg to logfile and return exception to client
                ResponsePublishedResourceMessage response = new ResponsePublishedResourceMessage();
                Logger.LogText(string.Format("User {0} tried to get a published resource. But an exception occured: {1}", Username, ex.Message), this, Logtype.Error);
                Logger.LogException(ex, this, Logtype.Error);
                response.Message = "Exception during request of published resource";
                SendMessage(response, sslStream);
            }
        }

        /// <summary>
        /// Handles CreateNewResourceDataMessages
        /// If the user is authenticated, it tries to create a new resource data in the database        
        /// Then, it sends a response message which contains if it succeeded or failed
        /// </summary>
        /// <param name="createNewResourceDataMessage"></param>
        /// <param name="sslStream"></param>
        private void HandleCreateNewResourceDataMessage(CreateNewResourceDataMessage createNewResourceDataMessage, SslStream sslStream)
        {
            Logger.LogText(string.Format("User {0} tries to create a resource data={1}-{2}", Username, createNewResourceDataMessage.ResourceData.ResourceId, createNewResourceDataMessage.ResourceData.ResourceVersion), this, Logtype.Debug);

            //Only authenticated users are allowed to create new plugins
            if (!ClientIsAuthenticated)
            {
                ResponseResourceDataModificationMessage response = new ResponseResourceDataModificationMessage
                {
                    ModifiedResourceData = false,
                    Message = "Unauthorized to create new resource data. Please authenticate yourself"
                };
                Logger.LogText(string.Format("Unauthorized user {0} tried to create new resource data={1}-{2} from IP={3}", Username, createNewResourceDataMessage.ResourceData.ResourceId, createNewResourceDataMessage.ResourceData.ResourceVersion, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }

            //check, if resource exists and is owned by the user
            ResourceData resourceData = createNewResourceDataMessage.ResourceData;
            Resource resource = Database.GetResource(resourceData.ResourceId);

            //Plugin does not exist
            if (resource == null)
            {
                ResponseResourceDataModificationMessage response = new ResponseResourceDataModificationMessage
                {
                    ModifiedResourceData = false,
                    Message = string.Format("Resource with id={0} does not exist", resourceData.ResourceId)
                };
                Logger.LogText(string.Format("User {0} tried to create new resource data={1}-{2} from IP={3} for a non-existing resource", Username, createNewResourceDataMessage.ResourceData.ResourceId, createNewResourceDataMessage.ResourceData.ResourceVersion, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }

            //Plugin is not owned by the user and user is not admin
            if (resource.Username != Username && !ClientIsAdmin)
            {
                ResponseResourceDataModificationMessage response = new ResponseResourceDataModificationMessage
                {
                    ModifiedResourceData = false,
                    Message = "Not authorized"
                };
                Logger.LogText(string.Format("User {0} tried to create new resource data={1}-{2} from IP={3} for a resource that he does not own ", Username, createNewResourceDataMessage.ResourceData.ResourceId, createNewResourceDataMessage.ResourceData.ResourceVersion, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }

            //Here, everything is fine; thus, we try to create the resource data
            try
            {
                Database.CreateResourceData(createNewResourceDataMessage.ResourceData.ResourceId, createNewResourceDataMessage.ResourceData.ResourceVersion, createNewResourceDataMessage.ResourceData.DataFilename, DateTime.Now);
                Logger.LogText(string.Format("User {0} created new resource data for resource={1}-{2} in database", Username, createNewResourceDataMessage.ResourceData.ResourceId, createNewResourceDataMessage.ResourceData.ResourceVersion), this, Logtype.Info);
                ResponseResourceDataModificationMessage response = new ResponseResourceDataModificationMessage
                {
                    ModifiedResourceData = true,
                    Message = string.Format("Created new resource data={0}-{1} in database", resourceData.ResourceId, resourceData.ResourceVersion)
                };
                SendMessage(response, sslStream);
            }
            catch (Exception ex)
            {
                //creation failed; logg to logfile and return exception to client
                ResponseResourceDataModificationMessage response = new ResponseResourceDataModificationMessage
                {
                    ModifiedResourceData = false
                };
                Logger.LogText(string.Format("User {0} tried to create a new resource data={1}-{2}. But an exception occured: {3}", Username, createNewResourceDataMessage.ResourceData.ResourceId, createNewResourceDataMessage.ResourceData.ResourceVersion, ex.Message), this, Logtype.Error);
                Logger.LogException(ex, this, Logtype.Error);
                response.Message = "Exception during creation of new resource data";
                SendMessage(response, sslStream);
            }
        }

        /// <summary>
        /// Handles UpdateResourceDataMessages
        /// If the user is authenticated, it tries to update an existing resource data in the database
        /// Users can only update their resource data; admins can update all resource data
        /// Then, it sends a response message which contains if it succeeded or failed
        /// </summary>
        /// <param name="updateResourceDataMessage"></param>
        /// <param name="sslStream"></param>
        private void HandleUpdateResourceDataMessage(UpdateResourceDataMessage updateResourceDataMessage, SslStream sslStream)
        {
            Logger.LogText(string.Format("User {0} tries to update a resource data={1}-{2}", Username, updateResourceDataMessage.ResourceData.ResourceId, updateResourceDataMessage.ResourceData.ResourceVersion), this, Logtype.Debug);

            //Only authenticated users are allowed to update resourceDatas
            if (!ClientIsAuthenticated)
            {
                ResponseResourceDataModificationMessage response = new ResponseResourceDataModificationMessage
                {
                    ModifiedResourceData = false,
                    Message = "Unauthorized to update that resourceData"
                };
                Logger.LogText(string.Format("Unauthorized user {0} tried to update resource data={1}-{2} from IP={3}", Username, updateResourceDataMessage.ResourceData.ResourceId, updateResourceDataMessage.ResourceData.ResourceVersion, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }
            //check, if resourceData to update exist
            Resource resource = Database.GetResource(updateResourceDataMessage.ResourceData.ResourceId);
            ResourceData resourceData = Database.GetResourceData(updateResourceDataMessage.ResourceData.ResourceId, updateResourceDataMessage.ResourceData.ResourceVersion);
            if (resourceData == null)
            {
                ResponseResourceDataModificationMessage response = new ResponseResourceDataModificationMessage
                {
                    ModifiedResourceData = false,
                    Message = "Unauthorized to update that resourceData" // we send an "unauthorized"; thus, it is not possible to search database for existing ids
                };
                Logger.LogText(string.Format("User {0} tried to update non-existing resource data={1}-{2} from IP={3}", Username, updateResourceDataMessage.ResourceData.ResourceId, updateResourceDataMessage.ResourceData.ResourceVersion, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }
            //"normal" users are only allowed to update their own resourceDatas
            if (ClientIsAdmin == false && resource.Username != Username)
            {
                ResponseResourceDataModificationMessage response = new ResponseResourceDataModificationMessage
                {
                    ModifiedResourceData = false,
                    Message = "Unauthorized to update that plugin"
                };
                Logger.LogText(string.Format("Unauthorized user {0} tried to update resource data={1}-{2} from IP={3}", Username, updateResourceDataMessage.ResourceData.ResourceId, updateResourceDataMessage.ResourceData.ResourceVersion, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }

            //Here, the user is authorized; thus, update of existing resourceData in database is started
            try
            {
                resourceData = updateResourceDataMessage.ResourceData;
                Database.UpdateResourceData(resourceData.ResourceId, resourceData.ResourceVersion, resourceData.DataFilename, DateTime.Now, resourceData.PublishState);
                Logger.LogText(string.Format("User {0} updated existing resource data={1}-{2} in database", Username, updateResourceDataMessage.ResourceData.ResourceId, updateResourceDataMessage.ResourceData.ResourceVersion), this, Logtype.Info);
                ResponseResourceDataModificationMessage response = new ResponseResourceDataModificationMessage
                {
                    ModifiedResourceData = true,
                    Message = string.Format("Updated resource data={0}-{1} in database", updateResourceDataMessage.ResourceData.ResourceId, updateResourceDataMessage.ResourceData.ResourceVersion)
                };
                SendMessage(response, sslStream);
            }
            catch (Exception ex)
            {
                //update failed; logg to logfile and return exception to client
                ResponseResourceDataModificationMessage response = new ResponseResourceDataModificationMessage
                {
                    ModifiedResourceData = false
                };
                Logger.LogText(string.Format("User {0} tried to update an existing resource data={1}-{2}. But an exception occured: {3}", Username, updateResourceDataMessage.ResourceData.ResourceId, updateResourceDataMessage.ResourceData.ResourceVersion, ex.Message), this, Logtype.Error);
                Logger.LogException(ex, this, Logtype.Error);
                response.Message = "Exception during update of existing resource data";
                SendMessage(response, sslStream);
            }
        }

        /// <summary>
        /// Handles UpdateResourceDataPublishStateMessages
        /// Updates the dedicated ResourceData's publish state to the given one
        /// </summary>
        /// <param name="updateResourceDataPublishStateMessage"></param>
        /// <param name="sslStream"></param>
        private void HandleUpdateResourceDataPublishState(UpdateResourceDataPublishStateMessage updateResourceDataPublishStateMessage, SslStream sslStream)
        {
            Logger.LogText(string.Format("User {0} tries to update the publish state of a source", Username), this, Logtype.Debug);

            //Only authenticated users are allowed to update resourcedatas
            if (!ClientIsAuthenticated)
            {
                ResponseSourceModificationMessage response = new ResponseSourceModificationMessage
                {
                    ModifiedSource = false,
                    Message = "Unauthorized to update that resourcedata"
                };
                Logger.LogText(string.Format("Unauthorized user {0} tried to update the publish state of resourcedata={1}-{2} from IP={3}", Username, updateResourceDataPublishStateMessage.ResourceData.ResourceId, updateResourceDataPublishStateMessage.ResourceData.ResourceVersion, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }
            //check, if resourcedata to update exist
            Plugin plugin = Database.GetPlugin(updateResourceDataPublishStateMessage.ResourceData.ResourceId);
            ResourceData resourceData = Database.GetResourceData(updateResourceDataPublishStateMessage.ResourceData.ResourceId, updateResourceDataPublishStateMessage.ResourceData.ResourceVersion);
            if (resourceData == null)
            {
                ResponseSourceModificationMessage response = new ResponseSourceModificationMessage
                {
                    ModifiedSource = false,
                    Message = "Unauthorized to update that resourcedata" // we send an "unauthorized"; thus, it is not possible to search database for existing ids
                };
                Logger.LogText(string.Format("User {0} tried to update the public state of a non-existing resourcedata={1}-{2} from IP={3}", Username, updateResourceDataPublishStateMessage.ResourceData.ResourceId, updateResourceDataPublishStateMessage.ResourceData.ResourceVersion, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }
            //only admins are allowed to update the publish state
            if (ClientIsAdmin == false)
            {
                ResponseSourceModificationMessage response = new ResponseSourceModificationMessage
                {
                    ModifiedSource = false,
                    Message = "Unauthorized to update that resourcedata"
                };
                Logger.LogText(string.Format("Unauthorized user {0} tried to update resourcedata={1}-{2} from IP={3}", Username, updateResourceDataPublishStateMessage.ResourceData.ResourceId, updateResourceDataPublishStateMessage.ResourceData.ResourceVersion, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }

            //Here, the user is authorized; thus, update of existing source in database is started
            try
            {
                resourceData = updateResourceDataPublishStateMessage.ResourceData;

                PublishState publishState;
                switch (resourceData.PublishState)
                {
                    case "DEVELOPER":
                        publishState = PublishState.DEVELOPER;
                        break;
                    case "NIGHTLY":
                        publishState = PublishState.NIGHTLY;
                        break;
                    case "BETA":
                        publishState = PublishState.BETA;
                        break;
                    case "RELEASE":
                        publishState = PublishState.RELEASE;
                        break;
                    default:
                    case "NOTPUBLISHED":
                        publishState = PublishState.NOTPUBLISHED;
                        break;
                }

                Database.UpdateResourceData(resourceData.ResourceId, resourceData.ResourceVersion, publishState);
                Logger.LogText(string.Format("User {0} updated publish state of existing resourcedata={1}-{2} in database", Username, resourceData.ResourceId, resourceData.ResourceVersion), this, Logtype.Info);
                ResponseResourceDataModificationMessage response = new ResponseResourceDataModificationMessage
                {
                    ModifiedResourceData = true,
                    Message = string.Format("Updated publish state of resourcedata={0}-{1} in database", resourceData.ResourceId, resourceData.ResourceVersion)
                };
                SendMessage(response, sslStream);
            }
            catch (Exception ex)
            {
                //update failed; logg to logfile and return exception to client
                ResponseSourceModificationMessage response = new ResponseSourceModificationMessage
                {
                    ModifiedSource = false
                };
                Logger.LogText(string.Format("User {0} tried to update the publish state of an existing source. But an exception occured: {1}", Username, ex.Message), this, Logtype.Error);
                Logger.LogException(ex, this, Logtype.Error);
                response.Message = "Exception during update of publish state of existing source";
                SendMessage(response, sslStream);
            }
        }

        /// <summary>
        /// Handles DeleteResourceDataMessage
        /// If the user is authenticated, it tries to delete an existing resource data in the database
        /// Users are only allowed to delete their resource datas
        /// Admins are allowed to delete any resource data
        /// Then, it sends a response message which contains if it succeeded or failed
        /// </summary>
        /// <param name="deleteResourceDataMessage"></param>
        /// <param name="sslStream"></param>
        private void HandleDeleteResourceDataMessage(DeleteResourceDataMessage deleteResourceDataMessage, SslStream sslStream)
        {
            Logger.LogText(string.Format("User {0} tries to delete a resource data={1}-{2}", deleteResourceDataMessage.ResourceData.ResourceId, deleteResourceDataMessage.ResourceData.ResourceVersion, Username), this, Logtype.Debug);

            //Only authenticated users are allowed to delete sourceDatas
            if (!ClientIsAuthenticated)
            {
                ResponseResourceDataModificationMessage response = new ResponseResourceDataModificationMessage
                {
                    ModifiedResourceData = false,
                    Message = "Unauthorized to delete that resource data"
                };
                Logger.LogText(string.Format("Unauthorized user {0} tried to delete resource data={1}-{2} from IP={3}", Username, deleteResourceDataMessage.ResourceData.ResourceId, deleteResourceDataMessage.ResourceData.ResourceVersion, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }
            //check, if sourceData to delete exist
            Resource resource = Database.GetResource(deleteResourceDataMessage.ResourceData.ResourceId);
            ResourceData sourceData = Database.GetResourceData(deleteResourceDataMessage.ResourceData.ResourceId, deleteResourceDataMessage.ResourceData.ResourceVersion);
            if (sourceData == null)
            {
                ResponseResourceDataModificationMessage response = new ResponseResourceDataModificationMessage
                {
                    ModifiedResourceData = false,
                    Message = "Unauthorized to delete that resource data" // we send an "unauthorized"; thus, it is not possible to search database for existing ids
                };
                Logger.LogText(string.Format("User {0} tried to delete non-existing resource data={1}-{2} from IP={3}", Username, deleteResourceDataMessage.ResourceData.ResourceId, deleteResourceDataMessage.ResourceData.ResourceVersion, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }
            //"normal" users are only allowed to update their own sourceDatas
            if (ClientIsAdmin == false && resource.Username != Username)
            {
                ResponseResourceDataModificationMessage response = new ResponseResourceDataModificationMessage
                {
                    ModifiedResourceData = false,
                    Message = "Unauthorized to delete that resource data"
                };
                Logger.LogText(string.Format("Unauthorized user {0} tried to delete resource data={1}-{2} from IP={3}", Username, deleteResourceDataMessage.ResourceData.ResourceVersion, deleteResourceDataMessage.ResourceData.ResourceVersion, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }

            //Here, the user is authorized; thus, deletion of existing sourceData in database is started
            try
            {
                //1. delete files in file system
                string filename = Constants.CLIENTHANDLER_RESOURCEDATA_FOLDER + Path.DirectorySeparatorChar + "ResourceData-" + deleteResourceDataMessage.ResourceData.ResourceId + "-" + deleteResourceDataMessage.ResourceData.ResourceVersion + ".bin";
                if (File.Exists(filename))
                {
                    Logger.LogText(string.Format("Deleting resource data file: {0}", filename), this, Logtype.Info);
                    File.Delete(filename);
                    Logger.LogText(string.Format("Deleted resource data file: {0}", filename), this, Logtype.Info);
                }

                //2. delete entry in database
                Database.DeleteResourceData(deleteResourceDataMessage.ResourceData.ResourceId, deleteResourceDataMessage.ResourceData.ResourceVersion);
                Logger.LogText(string.Format("User {0} deleted existing resource data={1}-{2} in database", Username, deleteResourceDataMessage.ResourceData.ResourceId, deleteResourceDataMessage.ResourceData.ResourceVersion), this, Logtype.Info);
                ResponseResourceDataModificationMessage response = new ResponseResourceDataModificationMessage
                {
                    ModifiedResourceData = true,
                    Message = string.Format("Deleted source data={0}-{1} in database", deleteResourceDataMessage.ResourceData.ResourceId, deleteResourceDataMessage.ResourceData.ResourceVersion)
                };
                SendMessage(response, sslStream);
            }
            catch (Exception ex)
            {
                //Deletion failed; logg to logfile and return exception to client
                ResponseResourceDataModificationMessage response = new ResponseResourceDataModificationMessage
                {
                    ModifiedResourceData = false
                };
                Logger.LogText(string.Format("User {0} tried to delete an existing resource data={1}-{2}. But an exception occured: {3}", Username, deleteResourceDataMessage.ResourceData.ResourceId, deleteResourceDataMessage.ResourceData.ResourceVersion, ex.Message), this, Logtype.Error);
                Logger.LogException(ex, this, Logtype.Error);
                response.Message = "Exception during delete of existing resource data";
                SendMessage(response, sslStream);
            }
        }

        /// <summary>
        /// Handles RequestResourceDataMessage
        /// Returns the resource data if it exists in the database
        /// Only the owners of a plugin or admins are allowed to get the sources
        /// </summary>
        /// <param name="requestSourceMessage"></param>
        /// <param name="sslStream"></param>
        private void HandleRequestResourceDataMessage(RequestResourceDataMessage requestResourceDataMessage, SslStream sslStream)
        {
            Logger.LogText(string.Format("User {0} requested a resource data={1}-{2}", requestResourceDataMessage.ResourceId, requestResourceDataMessage.ResourceVersion, Username), this, Logtype.Debug);

            try
            {
                ResourceData resourceData = Database.GetResourceData(requestResourceDataMessage.ResourceId, requestResourceDataMessage.ResourceVersion);
                if (resourceData == null)
                {
                    ResponseResourceDataMessage response = new ResponseResourceDataMessage
                    {
                        ResourceDataExists = false
                    };
                    Logger.LogText(string.Format("User {0} tried to get a non-existing resource data={1}-{2}", Username, requestResourceDataMessage.ResourceId, requestResourceDataMessage.ResourceVersion), this, Logtype.Warning);
                    response.Message = "Unauthorized to get that resourceData";
                    SendMessage(response, sslStream);
                }
                else
                {
                    //Check, if resource is owned by user or user is admin
                    Resource resource = Database.GetResource(resourceData.ResourceId);
                    if (ClientIsAdmin || resource.Username == Username)
                    {
                        ResponseResourceDataMessage response = new ResponseResourceDataMessage
                        {
                            ResourceData = resourceData,
                            ResourceDataExists = true
                        };
                        string message = string.Format("Responding with resource data={0}-{1}", requestResourceDataMessage.ResourceId, requestResourceDataMessage.ResourceVersion);
                        Logger.LogText(message, this, Logtype.Debug);
                        response.Message = message;
                        SendMessage(response, sslStream);
                    }
                    else
                    {
                        ResponseResourceDataMessage response = new ResponseResourceDataMessage
                        {
                            ResourceDataExists = false
                        };
                        Logger.LogText(string.Format("Unauthorized user {0} tried to get a resource data={1}-{2}", Username, requestResourceDataMessage.ResourceId, requestResourceDataMessage.ResourceVersion), this, Logtype.Warning);
                        response.Message = "Unauthorized to get that resource data";
                        SendMessage(response, sslStream);
                    }
                }
            }
            catch (Exception ex)
            {
                //request failed; logg to logfile and return exception to client
                ResponseResourceDataMessage response = new ResponseResourceDataMessage();
                Logger.LogText(string.Format("User {0} tried to get an existing resource data={1}-{2}. But an exception occured: {3}", Username, requestResourceDataMessage.ResourceId, requestResourceDataMessage.ResourceVersion, ex.Message), this, Logtype.Error);
                Logger.LogException(ex, this, Logtype.Error);
                response.Message = "Exception during request of existing resource data";
                SendMessage(response, sslStream);
            }
        }

        /// <summary>
        /// Handles RequestResourceDataListMessage
        /// responses with lists of resource data
        /// Only source data are returned that are owned by the user
        /// Admins may receice everything
        /// </summary>
        /// <param name="requestSourceListMessage"></param>
        /// <param name="sslStream"></param>
        private void HandleRequestResourceDataListMessage(RequestResourceDataListMessage requestResourceDataListMessage, SslStream sslStream)
        {
            Logger.LogText(string.Format("User {0} requested a list of resource data", Username), this, Logtype.Debug);

            //Only authenticated admins are allowed to receive ResourceData lists
            if (!ClientIsAuthenticated)
            {
                ResponseResourceDataListMessage response = new ResponseResourceDataListMessage
                {
                    AllowedToViewList = false,
                    Message = "Unauthorized to get resource data list. Please authenticate yourself"
                };
                Logger.LogText(string.Format("Unauthorized user {0} tried to request resource data list of resource={1} from IP={2}", Username, requestResourceDataListMessage.ResourceId, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }
            try
            {
                if (!ClientIsAdmin)
                {
                    Resource Resource = Database.GetResource(requestResourceDataListMessage.ResourceId);
                    if (Resource.Username != Username)
                    {
                        ResponseResourceDataListMessage response = new ResponseResourceDataListMessage
                        {
                            AllowedToViewList = false,
                            Message = "Unauthorized to get resource data list. Please authenticate yourself"
                        };
                        Logger.LogText(string.Format("Unauthorized user {0} tried to request resource data list of resource={1} from IP={2}", Username, requestResourceDataListMessage.ResourceId, IPAddress), this, Logtype.Warning);
                        SendMessage(response, sslStream);
                        return;
                    }
                }
                List<ResourceData> ResourceDatas = Database.GetResourceDatas(requestResourceDataListMessage.ResourceId);
                ResponseResourceDataListMessage responseResourceDataListMessage = new ResponseResourceDataListMessage
                {
                    AllowedToViewList = true
                };
                string message = string.Format("Responding with resource data list containing {0} elements", ResourceDatas.Count);
                Logger.LogText(message, this, Logtype.Debug);
                responseResourceDataListMessage.Message = message;
                responseResourceDataListMessage.ResourceDataList = ResourceDatas;
                SendMessage(responseResourceDataListMessage, sslStream);
            }
            catch (Exception ex)
            {
                //request failed; logg to logfile and return exception to client
                ResponseResourceDataListMessage response = new ResponseResourceDataListMessage();
                Logger.LogText(string.Format("User {0} tried to get a resource data list. But an exception occured: {1}", Username, ex.Message), this, Logtype.Error);
                Logger.LogException(ex, this, Logtype.Error);
                response.Message = "Exception during request of resource data list";
                SendMessage(response, sslStream);
            }
        }

        /// <summary>
        /// Handles uploading of source zipfiles
        /// </summary>
        /// <param name="startUploadSourceZipfileMessage"></param>
        private void HandleStartUploadSourceZipFileMessage(StartUploadSourceZipfileMessage startUploadSourceZipfileMessage, SslStream sslStream)
        {
            DateTime uploadStartTime = DateTime.Now;
            Logger.LogText(string.Format("User {0} starts uploading a source zipfile ({1} byte) for source={2}-{3}", Username, startUploadSourceZipfileMessage.FileSize, startUploadSourceZipfileMessage.Source.PluginId, startUploadSourceZipfileMessage.Source.PluginVersion), this, Logtype.Info);
            string tempfilename = string.Empty;

            //Only authenticated users are allowed to upload a zipfile
            if (!ClientIsAuthenticated)
            {
                ResponseUploadDownloadDataMessage response = new ResponseUploadDownloadDataMessage
                {
                    Success = false,
                    Message = "Unauthorized to upload a source zipfile. Please authenticate yourself"
                };
                Logger.LogText(string.Format("Unauthorized user {0} tried to upload a source zipfile for source={1}-{2} from IP={3}", Username, startUploadSourceZipfileMessage.Source.PluginId, startUploadSourceZipfileMessage.Source.PluginVersion, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }
            try
            {
                Plugin plugin = Database.GetPlugin(startUploadSourceZipfileMessage.Source.PluginId);

                //only admins and users that own the resource are allowed to a upload zipfile
                if (!ClientIsAdmin && !(Username == plugin.Username))
                {
                    ResponseUploadDownloadDataMessage response = new ResponseUploadDownloadDataMessage
                    {
                        Success = false,
                        Message = "Unauthorized to upload a source zipfile for that source"
                    };
                    Logger.LogText(string.Format("Unauthorized user {0} tried to upload a source zipfile for source={1}-{2} from IP={3}", Username, startUploadSourceZipfileMessage.Source.PluginId, startUploadSourceZipfileMessage.Source.PluginVersion, IPAddress), this, Logtype.Warning);
                    SendMessage(response, sslStream);
                    return;
                }
                Source source = Database.GetSource(startUploadSourceZipfileMessage.Source.PluginId, startUploadSourceZipfileMessage.Source.PluginVersion);

                ResponseUploadDownloadDataMessage responseSuccess = new ResponseUploadDownloadDataMessage
                {
                    Success = true,
                    Message = "Authorized to upload data"
                };
                SendMessage(responseSuccess, sslStream);

                long filesize = startUploadSourceZipfileMessage.FileSize;
                string filename = "Source-" + source.PluginId + "-" + source.PluginVersion + ".zip";
                tempfilename = filename + "_" + DateTime.Now.Ticks;

                CheckSourceFolder();
                long writtenFilesize = 0;
                using (FileStream fileStream = new FileStream(Constants.CLIENTHANDLER_PLUGIN_SOURCE_FOLDER + Path.DirectorySeparatorChar + tempfilename, FileMode.CreateNew, FileAccess.Write))
                {

                    while (true)
                    {
                        //Receive message from stream
                        Message message = ReceiveMessage(sslStream);

                        //case 1: we receive a data message
                        if (message.MessageHeader.MessageType == MessageType.UploadDownloadData)
                        {
                            UploadDownloadDataMessage uploadDownloadDataMessage = (UploadDownloadDataMessage)message;
                            fileStream.Write(uploadDownloadDataMessage.Data, 0, uploadDownloadDataMessage.Data.Length);
                            writtenFilesize += uploadDownloadDataMessage.Data.Length;
                            //received wrong message, abort
                            ResponseUploadDownloadDataMessage response = new ResponseUploadDownloadDataMessage
                            {
                                Success = true,
                                Message = "OK"
                            };
                            SendMessage(response, sslStream);
                            if (writtenFilesize == filesize)
                            {
                                //we received the exact filesize, thus, we assume everything is OK
                                break; // upload completed
                            }
                            if (writtenFilesize > filesize)
                            {
                                //here, something went wrong
                                //client sent more bytes than he initially told us to send
                                ResponseUploadDownloadDataMessage wrongUploadSizeResponseUploadDownloadDataMessage = new ResponseUploadDownloadDataMessage();
                                Logger.LogText(string.Format("User {0} sent too much data. Exptected {1} but already received {2} Abort now", Username, filesize, writtenFilesize), this, Logtype.Error);
                                wrongUploadSizeResponseUploadDownloadDataMessage.Success = false;
                                wrongUploadSizeResponseUploadDownloadDataMessage.Message = "Exception during upload of zipfile. You sent too much data";
                                SendMessage(wrongUploadSizeResponseUploadDownloadDataMessage, sslStream);
                                return; // error: wrong message
                            }
                        }
                        //case 2: we receive a stop message
                        else if (message.MessageHeader.MessageType == MessageType.StopUploadDownload)
                        {
                            //received wrong message, abort
                            ResponseUploadDownloadDataMessage response = new ResponseUploadDownloadDataMessage();
                            Logger.LogText(string.Format("User {0} stopped the upload of source zipfile for source: {1}-{2}", Username, source.PluginId, source.PluginVersion), this, Logtype.Info);
                            return; // stopped by user
                        }
                        //case 3: we receive something wrong...
                        else
                        {
                            //when we received a wrong message, we abort
                            ResponseUploadDownloadDataMessage response = new ResponseUploadDownloadDataMessage();
                            Logger.LogText(string.Format("User {0} sent a wrong message. Expected UploadDownloadDataMessage but received {1}", Username, message.MessageHeader.MessageType), this, Logtype.Error);
                            response.Success = false;
                            response.Message = "Exception during upload of zipfile";
                            SendMessage(response, sslStream);
                            return; // error: wrong message
                        }
                    }
                }

                //when we are here, the upload went well,
                //thus we can delete the old file, if it exists, and rename the temp file

                if (File.Exists(Constants.CLIENTHANDLER_PLUGIN_SOURCE_FOLDER + Path.DirectorySeparatorChar + filename))
                {
                    Logger.LogText(string.Format("File {0} already exists. Delete it now", filename), this, Logtype.Info);
                    File.Delete(Constants.CLIENTHANDLER_PLUGIN_SOURCE_FOLDER + Path.DirectorySeparatorChar + filename);
                    Logger.LogText(string.Format("Deleted file {0}", filename), this, Logtype.Info);
                }

                Logger.LogText(string.Format("Renaming file {0} to {1}", tempfilename, filename), this, Logtype.Info);
                File.Move(Constants.CLIENTHANDLER_PLUGIN_SOURCE_FOLDER + Path.DirectorySeparatorChar + tempfilename, Constants.CLIENTHANDLER_PLUGIN_SOURCE_FOLDER + Path.DirectorySeparatorChar + filename);
                Logger.LogText(string.Format("Renamed file {0} to {1}", tempfilename, filename), this, Logtype.Info);

                Logger.LogText(string.Format("Updating Source={0}-{1} in database", source.PluginId, source.PluginVersion), this, Logtype.Info);
                Database.UpdateSource(source.PluginId, source.PluginVersion, filename, BuildState.UPLOADED.ToString(), string.Format("Uploaded by {0}", Username), DateTime.Now);
                Logger.LogText(string.Format("Updated Source={0}-{1} in database", source.PluginId, source.PluginVersion), this, Logtype.Info);

                Logger.LogText(string.Format("User {0} uploaded a {1} byte source zip for source={2}-{3} in {4}", Username, writtenFilesize, source.PluginId, source.PluginVersion, DateTime.Now - uploadStartTime), this, Logtype.Info);

            }
            catch (Exception ex)
            {
                //request failed; logg to logfile and return exception to client
                ResponseUploadDownloadDataMessage response = new ResponseUploadDownloadDataMessage
                {
                    Success = false
                };
                Logger.LogText(string.Format("User {0} tried to upload a source zipfile. But an exception occured: {1}", Username, ex.Message), this, Logtype.Error);
                Logger.LogException(ex, this, Logtype.Error);
                response.Message = "Exception during upload of source zipfile";
                SendMessage(response, sslStream);
            }
            finally
            {
                //If something went wrong, maybe the tempfile still exists
                //thus, we delete it here
                if (tempfilename != string.Empty && File.Exists(Constants.CLIENTHANDLER_PLUGIN_SOURCE_FOLDER + Path.DirectorySeparatorChar + tempfilename))
                {
                    Logger.LogText(string.Format("Delete temp file {0}", Constants.CLIENTHANDLER_PLUGIN_SOURCE_FOLDER + Path.DirectorySeparatorChar + tempfilename), this, Logtype.Info);
                    File.Delete(Constants.CLIENTHANDLER_PLUGIN_SOURCE_FOLDER + Path.DirectorySeparatorChar + tempfilename);
                    Logger.LogText(string.Format("Deleted temp file {0}", Constants.CLIENTHANDLER_PLUGIN_SOURCE_FOLDER + Path.DirectorySeparatorChar + tempfilename), this, Logtype.Info);
                }
            }
        }

        /// <summary>
        /// Handles uploading of source zipfiles
        /// </summary>
        /// <param name="startUploadAssemblyZipfileMessage"></param>
        private void HandleStartUploadAssemblyZipFileMessage(StartUploadAssemblyZipfileMessage startUploadAssemblyZipfileMessage, SslStream sslStream)
        {
            DateTime uploadStartTime = DateTime.Now;
            Logger.LogText(string.Format("User {0} starts uploading an assembly zipfile ({1} byte) for source={2}-{3}", Username, startUploadAssemblyZipfileMessage.FileSize, startUploadAssemblyZipfileMessage.Source.PluginId, startUploadAssemblyZipfileMessage.Source.PluginVersion), this, Logtype.Info);
            string tempfilename = string.Empty;

            //Only authenticated users are allowed to upload a zipfile
            if (!ClientIsAuthenticated)
            {
                ResponseUploadDownloadDataMessage response = new ResponseUploadDownloadDataMessage
                {
                    Success = false,
                    Message = "Unauthorized to upload an assembly zipfile. Please authenticate yourself"
                };
                Logger.LogText(string.Format("Unauthorized user {0} tried to upload an assembly zipfile for source={1}-{2} from IP={3}", Username, startUploadAssemblyZipfileMessage.Source.PluginId, startUploadAssemblyZipfileMessage.Source.PluginVersion, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }
            try
            {
                Plugin plugin = Database.GetPlugin(startUploadAssemblyZipfileMessage.Source.PluginId);

                //only admins and users that own the resource are allowed to a upload zipfile
                if (!ClientIsAdmin && !(Username == plugin.Username))
                {
                    ResponseUploadDownloadDataMessage response = new ResponseUploadDownloadDataMessage
                    {
                        Success = false,
                        Message = "Unauthorized to upload an assembly zipfile for that source"
                    };
                    Logger.LogText(string.Format("Unauthorized user {0} tried to upload an assembly zipfile for source={1}-{2} from IP={3}", Username, startUploadAssemblyZipfileMessage.Source.PluginId, startUploadAssemblyZipfileMessage.Source.PluginVersion, IPAddress), this, Logtype.Warning);
                    SendMessage(response, sslStream);
                    return;
                }
                Source source = Database.GetSource(startUploadAssemblyZipfileMessage.Source.PluginId, startUploadAssemblyZipfileMessage.Source.PluginVersion);

                ResponseUploadDownloadDataMessage responseSuccess = new ResponseUploadDownloadDataMessage
                {
                    Success = true,
                    Message = "Authorized to upload data"
                };
                SendMessage(responseSuccess, sslStream);

                long filesize = startUploadAssemblyZipfileMessage.FileSize;
                string filename = "Assembly-" + source.PluginId + "-" + source.PluginVersion + ".zip";
                tempfilename = filename + "_" + DateTime.Now.Ticks;

                CheckAssembliesFolder();
                long writtenFilesize = 0;
                using (FileStream fileStream = new FileStream(Constants.CLIENTHANDLER_PLUGIN_ASSEMBLIES_FOLDER + Path.DirectorySeparatorChar + tempfilename, FileMode.CreateNew, FileAccess.Write))
                {

                    while (true)
                    {
                        //Receive message from stream
                        Message message = ReceiveMessage(sslStream);

                        //case 1: we receive a data message
                        if (message.MessageHeader.MessageType == MessageType.UploadDownloadData)
                        {
                            UploadDownloadDataMessage uploadDownloadDataMessage = (UploadDownloadDataMessage)message;
                            fileStream.Write(uploadDownloadDataMessage.Data, 0, uploadDownloadDataMessage.Data.Length);
                            writtenFilesize += uploadDownloadDataMessage.Data.Length;
                            //received wrong message, abort
                            ResponseUploadDownloadDataMessage response = new ResponseUploadDownloadDataMessage
                            {
                                Success = true,
                                Message = "OK"
                            };
                            SendMessage(response, sslStream);
                            if (writtenFilesize == filesize)
                            {
                                //we received the exact filesize, thus, we assume everything is OK
                                break; // upload completed
                            }
                            if (writtenFilesize > filesize)
                            {
                                //here, something went wrong
                                //client sent more bytes than he initially told us to send
                                ResponseUploadDownloadDataMessage wrongUploadSizeResponseUploadDownloadDataMessage = new ResponseUploadDownloadDataMessage();
                                Logger.LogText(string.Format("User {0} sent too much data. Exptected {1} but already received {2} Abort now", Username, filesize, writtenFilesize), this, Logtype.Error);
                                wrongUploadSizeResponseUploadDownloadDataMessage.Success = false;
                                wrongUploadSizeResponseUploadDownloadDataMessage.Message = "Exception during upload of zipfile. You sent too much data";
                                SendMessage(wrongUploadSizeResponseUploadDownloadDataMessage, sslStream);
                                return; // error: wrong message
                            }
                        }
                        //case 2: we receive a stop message
                        else if (message.MessageHeader.MessageType == MessageType.StopUploadDownload)
                        {
                            //received wrong message, abort
                            ResponseUploadDownloadDataMessage response = new ResponseUploadDownloadDataMessage();
                            Logger.LogText(string.Format("User {0} stopped the upload of assembly zipfile for source: {1}-{2}", Username, source.PluginId, source.PluginVersion), this, Logtype.Info);
                            return; // stopped by user
                        }
                        //case 3: we receive something wrong...
                        else
                        {
                            //when we received a wrong message, we abort
                            ResponseUploadDownloadDataMessage response = new ResponseUploadDownloadDataMessage();
                            Logger.LogText(string.Format("User {0} sent a wrong message. Expected UploadDownloadDataMessage but received {1}", Username, message.MessageHeader.MessageType), this, Logtype.Error);
                            response.Success = false;
                            response.Message = "Exception during upload of zipfile";
                            SendMessage(response, sslStream);
                            return; // error: wrong message
                        }
                    }
                }

                //when we are here, the upload went well,
                //thus we can delete the old file, if it exists, and rename the temp file

                if (File.Exists(Constants.CLIENTHANDLER_PLUGIN_ASSEMBLIES_FOLDER + Path.DirectorySeparatorChar + filename))
                {
                    Logger.LogText(string.Format("File {0} already exists. Delete it now", filename), this, Logtype.Info);
                    File.Delete(Constants.CLIENTHANDLER_PLUGIN_ASSEMBLIES_FOLDER + Path.DirectorySeparatorChar + filename);
                    Logger.LogText(string.Format("Deleted file {0}", filename), this, Logtype.Info);
                }

                Logger.LogText(string.Format("Renaming file {0} to {1}", tempfilename, filename), this, Logtype.Info);
                File.Move(Constants.CLIENTHANDLER_PLUGIN_ASSEMBLIES_FOLDER + Path.DirectorySeparatorChar + tempfilename, Constants.CLIENTHANDLER_PLUGIN_ASSEMBLIES_FOLDER + Path.DirectorySeparatorChar + filename);
                Logger.LogText(string.Format("Renamed file {0} to {1}", tempfilename, filename), this, Logtype.Info);

                Logger.LogText(string.Format("Updating source={0}-{1} in database", source.PluginId, source.PluginVersion), this, Logtype.Info);
                Database.UpdateSource(source.PluginId, source.PluginVersion, filename);
                Logger.LogText(string.Format("Updated source={0}-{1} in database", source.PluginId, source.PluginVersion), this, Logtype.Info);

                Logger.LogText(string.Format("User {0} uploaded a {1} byte assembly zipfile for source={2}-{3} in {4}", Username, writtenFilesize, source.PluginId, source.PluginVersion, DateTime.Now - uploadStartTime), this, Logtype.Info);

            }
            catch (Exception ex)
            {
                //request failed; logg to logfile and return exception to client
                ResponseUploadDownloadDataMessage response = new ResponseUploadDownloadDataMessage
                {
                    Success = false
                };
                Logger.LogText(string.Format("User {0} tried to upload a zipfile. But an exception occured: {1}", Username, ex.Message), this, Logtype.Error);
                Logger.LogException(ex, this, Logtype.Error);
                response.Message = "Exception during upload of zipfile";
                SendMessage(response, sslStream);
            }
            finally
            {
                //If something went wrong, maybe the tempfile still exists
                //thus, we delete it here
                if (tempfilename != string.Empty && File.Exists(Constants.CLIENTHANDLER_PLUGIN_SOURCE_FOLDER + Path.DirectorySeparatorChar + tempfilename))
                {
                    Logger.LogText(string.Format("Delete temp file {0}", Constants.CLIENTHANDLER_PLUGIN_SOURCE_FOLDER + Path.DirectorySeparatorChar + tempfilename), this, Logtype.Info);
                    File.Delete(Constants.CLIENTHANDLER_PLUGIN_SOURCE_FOLDER + Path.DirectorySeparatorChar + tempfilename);
                    Logger.LogText(string.Format("Deleted temp file {0}", Constants.CLIENTHANDLER_PLUGIN_SOURCE_FOLDER + Path.DirectorySeparatorChar + tempfilename), this, Logtype.Info);
                }
            }
        }

        /// <summary>
        /// Handles uploading of resourcedata files
        /// </summary>
        /// <param name="startUploadResourceDataFileMessage"></param>
        /// <param name="sslStream"></param>
        private void HandleStartUploadResourceDataFileMessage(StartUploadResourceDataFileMessage startUploadResourceDataFileMessage, SslStream sslStream)
        {
            DateTime uploadStartTime = DateTime.Now;
            Logger.LogText(string.Format("User {0} starts uploading a resourcedata file ({1} byte) for resourcedata={2}-{3}", Username, startUploadResourceDataFileMessage.FileSize, startUploadResourceDataFileMessage.ResourceData.ResourceId, startUploadResourceDataFileMessage.ResourceData.ResourceVersion), this, Logtype.Info);
            string tempfilename = string.Empty;

            //Only authenticated users are allowed to upload a zipfile
            if (!ClientIsAuthenticated)
            {
                ResponseUploadDownloadDataMessage response = new ResponseUploadDownloadDataMessage
                {
                    Success = false,
                    Message = "Unauthorized to upload resourcedata file. Please authenticate yourself"
                };
                Logger.LogText(string.Format("Unauthorized user {0} tried to upload a resourcedata file for resourcedata={1}-{2} from IP={3}", Username, startUploadResourceDataFileMessage.ResourceData.ResourceId, startUploadResourceDataFileMessage.ResourceData.ResourceVersion, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }
            try
            {
                Resource resource = Database.GetResource(startUploadResourceDataFileMessage.ResourceData.ResourceId);

                //only admins and users that own the resourcedata are allowed to upload a file
                if (!ClientIsAdmin && !(Username == resource.Username))
                {
                    ResponseUploadDownloadDataMessage response = new ResponseUploadDownloadDataMessage
                    {
                        Success = false,
                        Message = "Unauthorized to upload a resourcedata file for that resource"
                    };
                    Logger.LogText(string.Format("Unauthorized user {0} tried to upload a resource data file for resource={1}-{2} from IP={3}", Username, startUploadResourceDataFileMessage.ResourceData.ResourceId, startUploadResourceDataFileMessage.ResourceData.ResourceVersion, IPAddress), this, Logtype.Warning);
                    SendMessage(response, sslStream);
                    return;
                }
                ResourceData resourceData = Database.GetResourceData(startUploadResourceDataFileMessage.ResourceData.ResourceId, startUploadResourceDataFileMessage.ResourceData.ResourceVersion);

                ResponseUploadDownloadDataMessage responseSuccess = new ResponseUploadDownloadDataMessage
                {
                    Success = true,
                    Message = "Authorized to upload data"
                };
                SendMessage(responseSuccess, sslStream);

                long filesize = startUploadResourceDataFileMessage.FileSize;
                string filename = "ResourceData-" + resourceData.ResourceId + "-" + resourceData.ResourceVersion + ".bin";
                tempfilename = filename + "_" + DateTime.Now.Ticks;

                CheckResourceDataFolder();

                long writtenFilesize = 0;
                using (FileStream fileStream = new FileStream(Constants.CLIENTHANDLER_RESOURCEDATA_FOLDER + Path.DirectorySeparatorChar + tempfilename, FileMode.CreateNew, FileAccess.Write))
                {

                    while (true)
                    {
                        //Receive message from stream
                        Message message = ReceiveMessage(sslStream);

                        //case 1: we receive a data message
                        if (message.MessageHeader.MessageType == MessageType.UploadDownloadData)
                        {
                            UploadDownloadDataMessage uploadDownloadDataMessage = (UploadDownloadDataMessage)message;
                            fileStream.Write(uploadDownloadDataMessage.Data, 0, uploadDownloadDataMessage.Data.Length);
                            writtenFilesize += uploadDownloadDataMessage.Data.Length;
                            //received wrong message, abort
                            ResponseUploadDownloadDataMessage response = new ResponseUploadDownloadDataMessage
                            {
                                Success = true,
                                Message = "OK"
                            };
                            SendMessage(response, sslStream);
                            if (writtenFilesize == filesize)
                            {
                                //we received the exact filesize, thus, we assume everything is OK
                                break; // upload completed
                            }
                            if (writtenFilesize > filesize)
                            {
                                //here, something went wrong
                                //client sent more bytes than he initially told us to send
                                ResponseUploadDownloadDataMessage wrongUploadSizeResponseUploadDownloadDataMessage = new ResponseUploadDownloadDataMessage();
                                Logger.LogText(string.Format("User {0} sent too much data. Exptected {1} but already received {2} Abort now", Username, filesize, writtenFilesize), this, Logtype.Error);
                                wrongUploadSizeResponseUploadDownloadDataMessage.Success = false;
                                wrongUploadSizeResponseUploadDownloadDataMessage.Message = "Exception during upload of file. You sent too much data";
                                SendMessage(wrongUploadSizeResponseUploadDownloadDataMessage, sslStream);
                                return; // error: wrong message
                            }
                        }
                        //case 2: we receive a stop message
                        else if (message.MessageHeader.MessageType == MessageType.StopUploadDownload)
                        {
                            //received wrong message, abort
                            ResponseUploadDownloadDataMessage response = new ResponseUploadDownloadDataMessage();
                            Logger.LogText(string.Format("User {0} stopped the upload of resourcedata file for source: {1}-{2}", Username, resourceData.ResourceId, resourceData.ResourceVersion), this, Logtype.Info);
                            return; // stopped by user
                        }
                        //case 3: we receive something wrong...
                        else
                        {
                            //when we received a wrong message, we abort
                            ResponseUploadDownloadDataMessage response = new ResponseUploadDownloadDataMessage();
                            Logger.LogText(string.Format("User {0} sent a wrong message. Expected UploadDownloadDataMessage but received {1}", Username, message.MessageHeader.MessageType), this, Logtype.Error);
                            response.Success = false;
                            response.Message = "Exception during upload of zipfile";
                            SendMessage(response, sslStream);
                            return; // error: wrong message
                        }
                    }
                }

                //when we are here, the upload went well,
                //thus we can delete the old file, if it exists, and rename the temp file

                if (File.Exists(Constants.CLIENTHANDLER_RESOURCEDATA_FOLDER + Path.DirectorySeparatorChar + filename))
                {
                    Logger.LogText(string.Format("File {0} already exists. Delete it now", filename), this, Logtype.Info);
                    File.Delete(Constants.CLIENTHANDLER_RESOURCEDATA_FOLDER + Path.DirectorySeparatorChar + filename);
                    Logger.LogText(string.Format("Deleted file {0}", filename), this, Logtype.Info);
                }

                Logger.LogText(string.Format("Renaming file {0} to {1}", tempfilename, filename), this, Logtype.Info);
                File.Move(Constants.CLIENTHANDLER_RESOURCEDATA_FOLDER + Path.DirectorySeparatorChar + tempfilename, Constants.CLIENTHANDLER_RESOURCEDATA_FOLDER + Path.DirectorySeparatorChar + filename);
                Logger.LogText(string.Format("Renamed file {0} to {1}", tempfilename, filename), this, Logtype.Info);

                Logger.LogText(string.Format("Updating resourcedata={0}-{1} in database", resourceData.ResourceId, resourceData.ResourceVersion), this, Logtype.Info);
                Database.UpdateResourceData(resourceData.ResourceId, resourceData.ResourceVersion, filename);
                Logger.LogText(string.Format("Updated resourcedata={0}-{1} in database", resourceData.ResourceId, resourceData.ResourceVersion), this, Logtype.Info);

                Logger.LogText(string.Format("User {0} uploaded a {1} byte resourcedata file for resourcedata={2}-{3} in {4}", Username, writtenFilesize, resourceData.ResourceId, resourceData.ResourceVersion, DateTime.Now - uploadStartTime), this, Logtype.Info);

            }
            catch (Exception ex)
            {
                //request failed; logg to logfile and return exception to client
                ResponseUploadDownloadDataMessage response = new ResponseUploadDownloadDataMessage
                {
                    Success = false
                };
                Logger.LogText(string.Format("User {0} tried to upload a resourcedata file. But an exception occured: {1}", Username, ex.Message), this, Logtype.Error);
                Logger.LogException(ex, this, Logtype.Error);
                response.Message = "Exception during upload of resourcedata file";
                SendMessage(response, sslStream);
            }
            finally
            {
                //If something went wrong, maybe the tempfile still exists
                //thus, we delete it here
                if (tempfilename != string.Empty && File.Exists(Constants.CLIENTHANDLER_RESOURCEDATA_FOLDER + Path.DirectorySeparatorChar + tempfilename))
                {
                    Logger.LogText(string.Format("Delete temp file {0}", Constants.CLIENTHANDLER_RESOURCEDATA_FOLDER + Path.DirectorySeparatorChar + tempfilename), this, Logtype.Info);
                    File.Delete(Constants.CLIENTHANDLER_RESOURCEDATA_FOLDER + Path.DirectorySeparatorChar + tempfilename);
                    Logger.LogText(string.Format("Deleted temp file {0}", Constants.CLIENTHANDLER_RESOURCEDATA_FOLDER + Path.DirectorySeparatorChar + tempfilename), this, Logtype.Info);
                }
            }
        }

        /// <summary>
        /// Handles downloading of source zipfiles
        /// </summary>
        /// <param name="requestDownloadSourceZipfileMessage"></param>
        /// <param name="sslStream"></param>
        private void HandleRequestDownloadSourceZipfileMessage(RequestDownloadSourceZipfileMessage requestDownloadSourceZipfileMessage, SslStream sslStream)
        {
            DateTime downloadStartTime = DateTime.Now;
            Logger.LogText(string.Format("User {0} starts downloading a zipfile for source={1}-{2}", Username, requestDownloadSourceZipfileMessage.Source.PluginId, requestDownloadSourceZipfileMessage.Source.PluginVersion), this, Logtype.Info);

            //Only authenticated users are allowed to download a zipfile
            if (!ClientIsAuthenticated)
            {
                ResponseUploadDownloadDataMessage response = new ResponseUploadDownloadDataMessage
                {
                    Success = false,
                    Message = "Unauthorized to download a zipfile. Please authenticate yourself"
                };
                Logger.LogText(string.Format("Unauthorized user {0} tried to download a zipfile for source={1}-{2} from IP={3}", Username, requestDownloadSourceZipfileMessage.Source.PluginId, requestDownloadSourceZipfileMessage.Source.PluginVersion, IPAddress), this, Logtype.Warning);
                SendMessage(response, sslStream);
                return;
            }
            try
            {
                Plugin plugin = Database.GetPlugin(requestDownloadSourceZipfileMessage.Source.PluginId);
                //check, if user is admin or plugin is owned by user
                if (!ClientIsAdmin && !(Username == plugin.Username))
                {
                    ResponseUploadDownloadDataMessage response = new ResponseUploadDownloadDataMessage
                    {
                        Success = false,
                        Message = "Unauthorized to download zipfile for that source"
                    };
                    Logger.LogText(string.Format("Unauthorized user {0} tried to download a zipfile for source={1}-{2} from IP={3}", Username, requestDownloadSourceZipfileMessage.Source.PluginId, requestDownloadSourceZipfileMessage.Source.PluginVersion, IPAddress), this, Logtype.Warning);
                    SendMessage(response, sslStream);
                    return;
                }

                //check, if source exists
                Source source = Database.GetSource(requestDownloadSourceZipfileMessage.Source.PluginId, requestDownloadSourceZipfileMessage.Source.PluginVersion);
                if (source == null)
                {
                    ResponseUploadDownloadDataMessage response = new ResponseUploadDownloadDataMessage
                    {
                        Success = false,
                        Message = "Source does not exist"
                    };
                    Logger.LogText(string.Format("User {0} tried to download a zipfile for a non-existing source={1}-{2} from IP={3}", Username, requestDownloadSourceZipfileMessage.Source.PluginId, requestDownloadSourceZipfileMessage.Source.PluginVersion, IPAddress), this, Logtype.Warning);
                    SendMessage(response, sslStream);
                    return;
                }

                string filename = source.ZipFileName;

                //no zipfile previously uploaded
                if (filename.Equals(string.Empty))
                {
                    ResponseUploadDownloadDataMessage response = new ResponseUploadDownloadDataMessage
                    {
                        Success = false,
                        Message = "No zipfile has been previously uploaded for this source"
                    };
                    Logger.LogText(string.Format("User {0} tried to download a non existing zipfile for a source={1}-{2} from IP={3}", Username, requestDownloadSourceZipfileMessage.Source.PluginId, requestDownloadSourceZipfileMessage.Source.PluginVersion, IPAddress), this, Logtype.Warning);
                    SendMessage(response, sslStream);
                    return;
                }

                //check if file in file system exists
                if (!File.Exists(Constants.CLIENTHANDLER_PLUGIN_SOURCE_FOLDER + Path.DirectorySeparatorChar + filename))
                {
                    ResponseUploadDownloadDataMessage response = new ResponseUploadDownloadDataMessage
                    {
                        Success = false,
                        Message = "Source zipfile does not exist. Please contact a CrypToolStore admin"
                    };
                    Logger.LogText(string.Format("User {0} tried to download a zipfile for a source={1}-{2} that does not exists in file system from IP={3}", Username, requestDownloadSourceZipfileMessage.Source.PluginId, requestDownloadSourceZipfileMessage.Source.PluginVersion, IPAddress), this, Logtype.Error);
                    SendMessage(response, sslStream);
                    return;
                }

                FileInfo fileInfo = new FileInfo(Constants.CLIENTHANDLER_PLUGIN_SOURCE_FOLDER + Path.DirectorySeparatorChar + filename);
                long filesize = fileInfo.Length;
                long totalbytesread = 0;

                using (FileStream fileStream = new FileStream(Constants.CLIENTHANDLER_PLUGIN_SOURCE_FOLDER + Path.DirectorySeparatorChar + filename, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[Constants.CLIENTHANDLER_FILE_BUFFER_SIZE];
                    while (totalbytesread < filesize)
                    {
                        //read a block of data
                        int bytesread = 0;
                        int current_bytesread = 0;

                        while ((current_bytesread = fileStream.Read(buffer, bytesread, Constants.CLIENTHANDLER_FILE_BUFFER_SIZE - bytesread)) > 0 && bytesread < Constants.CLIENTHANDLER_FILE_BUFFER_SIZE)
                        {
                            bytesread += current_bytesread;
                            totalbytesread += current_bytesread;
                        }

                        byte[] data;
                        if (bytesread < Constants.CLIENTHANDLER_FILE_BUFFER_SIZE)
                        {
                            data = new byte[bytesread];
                            Array.Copy(buffer, 0, data, 0, bytesread);
                        }
                        else
                        {
                            data = buffer;
                        }

                        //send the block of data
                        UploadDownloadDataMessage uploadDownloadDataMessage = new UploadDownloadDataMessage
                        {
                            Data = data,
                            Offset = totalbytesread,
                            FileSize = filesize
                        };

                        SendMessage(uploadDownloadDataMessage, sslStream);

                        //check, if block of data was received without error
                        Message response = ReceiveMessage(sslStream);

                        //Received null = connection closed
                        if (response == null)
                        {
                            Logger.LogText("Received null. Connection closed by server", this, Logtype.Info);
                            return;
                        }

                        //Received ResponseUploadDownloadDataMessage
                        if (response.MessageHeader.MessageType == MessageType.ResponseUploadDownloadData)
                        {
                            ResponseUploadDownloadDataMessage responseUploadDownloadDataMessage = (ResponseUploadDownloadDataMessage)response;
                            if (responseUploadDownloadDataMessage.Success == false)
                            {
                                string failmsg = string.Format("Download of source zipfile of source={0}-{1} failed, reason: {2}", source.PluginId, source.PluginVersion, responseUploadDownloadDataMessage.Message);
                                Logger.LogText(failmsg, this, Logtype.Info);
                                return;
                            }
                        }
                        else if (response.MessageHeader.MessageType == MessageType.StopUploadDownload)
                        {
                            Logger.LogText(string.Format("User aborted download of source zipfile of source={0}-{1}", source.PluginId, source.PluginVersion), this, Logtype.Info);
                            return;
                        }
                        else
                        {
                            //Received another (wrong) message
                            string msg = string.Format("Response message to UploadDownloadDataMessage was not a ResponseUploadDownloadDataMessage or a StopUploadDownloadMessage. Message was: {0}", response.MessageHeader.MessageType.ToString());
                            Logger.LogText(msg, this, Logtype.Info);
                            return;
                        }
                    }
                }

                //Received another (wrong) message                    
                Logger.LogText(string.Format("User {0} completely downloaded {1} in {2}", Username, filename, DateTime.Now - downloadStartTime), this, Logtype.Info);
            }
            catch (Exception ex)
            {
                //request failed; logg to logfile and return exception to client
                ResponseUploadDownloadDataMessage response = new ResponseUploadDownloadDataMessage
                {
                    Success = false
                };
                Logger.LogText(string.Format("User {0} tried to download a source zipfile. But an exception occured: {1}", Username, ex.Message), this, Logtype.Error);
                Logger.LogException(ex, this, Logtype.Error);
                response.Message = "Exception during download of source zipfile";
                SendMessage(response, sslStream);
            }
        }

        /// <summary>
        /// Handles downloading of assembly zipfiles
        /// </summary>
        /// <param name="requestDownloadAssemblyZipfileMessage"></param>
        /// <param name="sslStream"></param>
        private void HandleRequestDownloadAssemblyZipfileMessage(RequestDownloadAssemblyZipfileMessage requestDownloadAssemblyZipfileMessage, SslStream sslStream)
        {
            DateTime downloadStartTime = DateTime.Now;
            Logger.LogText(string.Format("User {0} starts downloading an assembly zipfile for source={1}-{2}", Username, requestDownloadAssemblyZipfileMessage.Source.PluginId, requestDownloadAssemblyZipfileMessage.Source.PluginVersion), this, Logtype.Info);

            try
            {
                Plugin plugin = Database.GetPlugin(requestDownloadAssemblyZipfileMessage.Source.PluginId);

                //check, if plugin exists                
                if (plugin == null)
                {
                    ResponseUploadDownloadDataMessage response = new ResponseUploadDownloadDataMessage
                    {
                        Success = false,
                        Message = "Assembly does not exist"
                    };
                    Logger.LogText(string.Format("User {0} tried to download an assembly zipfile for a non-existing plugin={1} from IP={2}", Username, requestDownloadAssemblyZipfileMessage.Source.PluginId, IPAddress), this, Logtype.Warning);
                    SendMessage(response, sslStream);
                    return;
                }

                Source source = Database.GetSource(requestDownloadAssemblyZipfileMessage.Source.PluginId, requestDownloadAssemblyZipfileMessage.Source.PluginVersion);

                //check, if source exists                
                if (source == null)
                {
                    ResponseUploadDownloadDataMessage response = new ResponseUploadDownloadDataMessage
                    {
                        Success = false,
                        Message = "Assembly does not exist"
                    };
                    Logger.LogText(string.Format("User {0} tried to download an assembly zipfile for a non-existing source={1}-{2} from IP={3}", Username, requestDownloadAssemblyZipfileMessage.Source.PluginId, requestDownloadAssemblyZipfileMessage.Source.PluginVersion, IPAddress), this, Logtype.Warning);
                    SendMessage(response, sslStream);
                    return;
                }

                //check, if user is admin or plugin is owned by user or plugin is published (publishstate != NOTPUBLISED)
                if (!ClientIsAdmin && !(Username == plugin.Username) && source.PublishState.ToLower().Equals(PublishState.NOTPUBLISHED.ToString().ToLower()))
                {
                    ResponseUploadDownloadDataMessage response = new ResponseUploadDownloadDataMessage
                    {
                        Success = false,
                        Message = "Unauthorized to download assembly zipfile for that source"
                    };
                    Logger.LogText(string.Format("Unauthorized user {0} tried to download an assembly zipfile for source={1}-{2} from IP={3}", Username, requestDownloadAssemblyZipfileMessage.Source.PluginId, requestDownloadAssemblyZipfileMessage.Source.PluginVersion, IPAddress), this, Logtype.Warning);
                    SendMessage(response, sslStream);
                    return;
                }
                string filename = source.AssemblyFileName;

                //no zipfile previously uploaded
                if (filename.Equals(string.Empty))
                {
                    ResponseUploadDownloadDataMessage response = new ResponseUploadDownloadDataMessage
                    {
                        Success = false,
                        Message = "No assembly zipfile has been previously uploaded for this source"
                    };
                    Logger.LogText(string.Format("User {0} tried to download a non existing assembly zipfile for a source={1}-{2} from IP={3}", Username, requestDownloadAssemblyZipfileMessage.Source.PluginId, requestDownloadAssemblyZipfileMessage.Source.PluginVersion, IPAddress), this, Logtype.Warning);
                    SendMessage(response, sslStream);
                    return;
                }

                //check if file in file system exists
                if (!File.Exists(Constants.CLIENTHANDLER_PLUGIN_ASSEMBLIES_FOLDER + Path.DirectorySeparatorChar + filename))
                {
                    ResponseUploadDownloadDataMessage response = new ResponseUploadDownloadDataMessage
                    {
                        Success = false,
                        Message = "Assembly zipfile does not exist. Please contact a CrypToolStore admin"
                    };
                    Logger.LogText(string.Format("User {0} tried to download an assembly zipfile for a source={1}-{2} that does not exists in file system from IP={3}", Username, requestDownloadAssemblyZipfileMessage.Source.PluginId, requestDownloadAssemblyZipfileMessage.Source.PluginVersion, IPAddress), this, Logtype.Error);
                    SendMessage(response, sslStream);
                    return;
                }

                FileInfo fileInfo = new FileInfo(Constants.CLIENTHANDLER_PLUGIN_ASSEMBLIES_FOLDER + Path.DirectorySeparatorChar + filename);
                long filesize = fileInfo.Length;
                long totalbytesread = 0;

                using (FileStream fileStream = new FileStream(Constants.CLIENTHANDLER_PLUGIN_ASSEMBLIES_FOLDER + Path.DirectorySeparatorChar + filename, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[Constants.CLIENTHANDLER_FILE_BUFFER_SIZE];
                    while (totalbytesread < filesize)
                    {
                        //read a block of data
                        int bytesread = 0;
                        int current_bytesread = 0;

                        while ((current_bytesread = fileStream.Read(buffer, bytesread, Constants.CLIENTHANDLER_FILE_BUFFER_SIZE - bytesread)) > 0 && bytesread < Constants.CLIENTHANDLER_FILE_BUFFER_SIZE)
                        {
                            bytesread += current_bytesread;
                            totalbytesread += current_bytesread;
                        }

                        byte[] data;
                        if (bytesread < Constants.CLIENTHANDLER_FILE_BUFFER_SIZE)
                        {
                            data = new byte[bytesread];
                            Array.Copy(buffer, 0, data, 0, bytesread);
                        }
                        else
                        {
                            data = buffer;
                        }

                        //send the block of data
                        UploadDownloadDataMessage uploadDownloadDataMessage = new UploadDownloadDataMessage
                        {
                            Data = data,
                            Offset = totalbytesread,
                            FileSize = filesize
                        };

                        SendMessage(uploadDownloadDataMessage, sslStream);

                        //check, if block of data was received without error
                        Message response = ReceiveMessage(sslStream);

                        //Received null = connection closed
                        if (response == null)
                        {
                            Logger.LogText("Received null. Connection closed by server", this, Logtype.Info);
                            return;
                        }

                        //Received ResponseUploadDownloadDataMessage
                        if (response.MessageHeader.MessageType == MessageType.ResponseUploadDownloadData)
                        {
                            ResponseUploadDownloadDataMessage responseUploadDownloadDataMessage = (ResponseUploadDownloadDataMessage)response;
                            if (responseUploadDownloadDataMessage.Success == false)
                            {
                                string failmsg = string.Format("Download of assembly zipfile for source={0}-{1} failed, reason: {2}", source.PluginId, source.PluginVersion, responseUploadDownloadDataMessage.Message);
                                Logger.LogText(failmsg, this, Logtype.Info);
                                return;
                            }
                        }
                        else if (response.MessageHeader.MessageType == MessageType.StopUploadDownload)
                        {
                            Logger.LogText(string.Format("User aborted download of assembly zipfile of source={0}-{1}", source.PluginId, source.PluginVersion), this, Logtype.Info);
                            return;
                        }
                        else
                        {
                            //Received another (wrong) message
                            string msg = string.Format("Response message to UploadDownloadDataMessage was not a ResponseUploadDownloadDataMessage or a StopUploadDownloadMessage. Message was: {0}", response.MessageHeader.MessageType.ToString());
                            Logger.LogText(msg, this, Logtype.Info);
                            return;
                        }
                    }
                }

                //Received another (wrong) message                    
                Logger.LogText(string.Format("User {0} completely downloaded {1} in {2}", Username, filename, DateTime.Now - downloadStartTime), this, Logtype.Info);
            }
            catch (Exception ex)
            {
                //request failed; logg to logfile and return exception to client
                ResponseUploadDownloadDataMessage response = new ResponseUploadDownloadDataMessage
                {
                    Success = false
                };
                Logger.LogText(string.Format("User {0} tried to download an assembly zipfile. But an exception occured: {1}", Username, ex.Message), this, Logtype.Error);
                Logger.LogException(ex, this, Logtype.Error);
                response.Message = "Exception during download of assembly zipfile";
                SendMessage(response, sslStream);
            }
        }

        /// <summary>
        /// Handles downloading of resourcedata files
        /// </summary>
        /// <param name="requestDownloadResourceDataFileMessage"></param>
        /// <param name="sslStream"></param>
        private void HandleRequestDownloadResourceDataFileMessage(RequestDownloadResourceDataFileMessage requestDownloadResourceDataFileMessage, SslStream sslStream)
        {
            DateTime downloadStartTime = DateTime.Now;
            Logger.LogText(string.Format("User {0} starts downloading a resourcedata file for resourcedata={1}-{2}", Username, requestDownloadResourceDataFileMessage.ResourceData.ResourceId, requestDownloadResourceDataFileMessage.ResourceData.ResourceVersion), this, Logtype.Info);

            try
            {
                Resource resource = Database.GetResource(requestDownloadResourceDataFileMessage.ResourceData.ResourceId);
                if (resource == null)
                {
                    ResponseUploadDownloadDataMessage response = new ResponseUploadDownloadDataMessage
                    {
                        Success = false,
                        Message = "Resource does not exist"
                    };
                    Logger.LogText(string.Format("User {0} tried to download resourcedata file for a non-existing resource={1} from IP={2}", Username, requestDownloadResourceDataFileMessage.ResourceData.ResourceId, IPAddress), this, Logtype.Warning);
                    SendMessage(response, sslStream);
                    return;
                }

                //check, if source exists
                ResourceData resourceData = Database.GetResourceData(requestDownloadResourceDataFileMessage.ResourceData.ResourceId, requestDownloadResourceDataFileMessage.ResourceData.ResourceVersion);
                if (resourceData == null)
                {
                    ResponseUploadDownloadDataMessage response = new ResponseUploadDownloadDataMessage
                    {
                        Success = false,
                        Message = "Resourcedata does not exist"
                    };
                    Logger.LogText(string.Format("User {0} tried to download resourcedata file for a non-existing resourcedata={1}-{2} from IP={3}", Username, requestDownloadResourceDataFileMessage.ResourceData.ResourceId, requestDownloadResourceDataFileMessage.ResourceData.ResourceVersion, IPAddress), this, Logtype.Warning);
                    SendMessage(response, sslStream);
                    return;
                }


                //check, if user is admin or resource is owned by user or plugin is published (publishstate != NOTPUBLISED)
                if (!ClientIsAdmin && !(Username == resource.Username) && resourceData.PublishState.ToLower().Equals(PublishState.NOTPUBLISHED.ToString().ToLower()))
                {
                    ResponseUploadDownloadDataMessage response = new ResponseUploadDownloadDataMessage
                    {
                        Success = false,
                        Message = "Unauthorized to download file for that resourcedata"
                    };
                    Logger.LogText(string.Format("Unauthorized user {0} tried to download a resourcedata file for resourcedata={1}-{2} from IP={3}", Username, requestDownloadResourceDataFileMessage.ResourceData.ResourceId, requestDownloadResourceDataFileMessage.ResourceData.ResourceVersion, IPAddress), this, Logtype.Warning);
                    SendMessage(response, sslStream);
                    return;
                }

                string filename = resourceData.DataFilename;

                //no file previously uploaded
                if (filename.Equals(string.Empty))
                {
                    ResponseUploadDownloadDataMessage response = new ResponseUploadDownloadDataMessage
                    {
                        Success = false,
                        Message = "No resourcedata file has been previously uploaded for this resourcedata"
                    };
                    Logger.LogText(string.Format("User {0} tried to download a non existing resourcedata file for a resourcedata={1}-{2} from IP={3}", Username, requestDownloadResourceDataFileMessage.ResourceData.ResourceId, requestDownloadResourceDataFileMessage.ResourceData.ResourceVersion, IPAddress), this, Logtype.Warning);
                    SendMessage(response, sslStream);
                    return;
                }

                //check if file in file system exists
                if (!File.Exists(Constants.CLIENTHANDLER_RESOURCEDATA_FOLDER + Path.DirectorySeparatorChar + filename))
                {
                    ResponseUploadDownloadDataMessage response = new ResponseUploadDownloadDataMessage
                    {
                        Success = false,
                        Message = "Resourcedata file does not exist. Please contact a CrypToolStore admin"
                    };
                    Logger.LogText(string.Format("User {0} tried to download resourcedata file for a resourcedata={1}-{2} that does not exists in file system from IP={3}", Username, requestDownloadResourceDataFileMessage.ResourceData.ResourceId, requestDownloadResourceDataFileMessage.ResourceData.ResourceVersion, IPAddress), this, Logtype.Error);
                    SendMessage(response, sslStream);
                    return;
                }

                FileInfo fileInfo = new FileInfo(Constants.CLIENTHANDLER_RESOURCEDATA_FOLDER + Path.DirectorySeparatorChar + filename);
                long filesize = fileInfo.Length;
                long totalbytesread = 0;

                using (FileStream fileStream = new FileStream(Constants.CLIENTHANDLER_RESOURCEDATA_FOLDER + Path.DirectorySeparatorChar + filename, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[Constants.CLIENTHANDLER_FILE_BUFFER_SIZE];
                    while (totalbytesread < filesize)
                    {
                        //read a block of data
                        int bytesread = 0;
                        int current_bytesread = 0;

                        while ((current_bytesread = fileStream.Read(buffer, bytesread, Constants.CLIENTHANDLER_FILE_BUFFER_SIZE - bytesread)) > 0 && bytesread < Constants.CLIENTHANDLER_FILE_BUFFER_SIZE)
                        {
                            bytesread += current_bytesread;
                            totalbytesread += current_bytesread;
                        }

                        byte[] data;
                        if (bytesread < Constants.CLIENTHANDLER_FILE_BUFFER_SIZE)
                        {
                            data = new byte[bytesread];
                            Array.Copy(buffer, 0, data, 0, bytesread);
                        }
                        else
                        {
                            data = buffer;
                        }

                        //send the block of data
                        UploadDownloadDataMessage uploadDownloadDataMessage = new UploadDownloadDataMessage
                        {
                            Data = data,
                            Offset = totalbytesread,
                            FileSize = filesize
                        };

                        SendMessage(uploadDownloadDataMessage, sslStream);

                        //check, if block of data was received without error
                        Message response = ReceiveMessage(sslStream);

                        //Received null = connection closed
                        if (response == null)
                        {
                            Logger.LogText("Received null. Connection closed by server", this, Logtype.Info);
                            return;
                        }

                        //Received ResponseUploadDownloadDataMessage
                        if (response.MessageHeader.MessageType == MessageType.ResponseUploadDownloadData)
                        {
                            ResponseUploadDownloadDataMessage responseUploadDownloadDataMessage = (ResponseUploadDownloadDataMessage)response;
                            if (responseUploadDownloadDataMessage.Success == false)
                            {
                                string failmsg = string.Format("Download of resourcedata file for resourcedata={0}-{1} failed, reason: {2}", resourceData.ResourceId, resourceData.ResourceVersion, responseUploadDownloadDataMessage.Message);
                                Logger.LogText(failmsg, this, Logtype.Info);
                                return;
                            }
                        }
                        else if (response.MessageHeader.MessageType == MessageType.StopUploadDownload)
                        {
                            Logger.LogText(string.Format("User aborted download of resourcedata file of resourcedata={0}-{1}", resourceData.ResourceId, resourceData.ResourceVersion), this, Logtype.Info);
                            return;
                        }
                        else
                        {
                            //Received another (wrong) message
                            string msg = string.Format("Response message to UploadDownloadDataMessage was not a ResponseUploadDownloadDataMessage or a StopUploadDownloadMessage. Message was: {0}", response.MessageHeader.MessageType.ToString());
                            Logger.LogText(msg, this, Logtype.Info);
                            return;
                        }
                    }
                }
                //Received another (wrong) message                    
                Logger.LogText(string.Format("User {0} completely downloaded {1} in {2}", Username, filename, DateTime.Now - downloadStartTime), this, Logtype.Info);
            }
            catch (Exception ex)
            {
                //request failed; logg to logfile and return exception to client
                ResponseUploadDownloadDataMessage response = new ResponseUploadDownloadDataMessage
                {
                    Success = false
                };
                Logger.LogText(string.Format("User {0} tried to download an resourcedata file. But an exception occured: {1}", Username, ex.Message), this, Logtype.Error);
                Logger.LogException(ex, this, Logtype.Error);
                response.Message = "Exception during download of resourcedata file";
                SendMessage(response, sslStream);
            }
        }

        /// <summary>
        /// Checks, if the Source folder exists; if not it creates it
        /// </summary>
        private void CheckSourceFolder()
        {
            if (!Directory.Exists(Constants.CLIENTHANDLER_PLUGIN_SOURCE_FOLDER))
            {
                Logger.LogText(string.Format("PLUGIN_SOURCE_FOLDER={0} does not exist. Create it now", Constants.CLIENTHANDLER_PLUGIN_SOURCE_FOLDER), this, Logtype.Info);
                Directory.CreateDirectory(Constants.CLIENTHANDLER_PLUGIN_SOURCE_FOLDER);
                Logger.LogText(string.Format("PLUGIN_SOURCE_FOLDER={0} created", Constants.CLIENTHANDLER_PLUGIN_SOURCE_FOLDER), this, Logtype.Info);
            }
        }

        /// <summary>
        /// Checks, if the Assemblies folder exists; if not it creates it
        /// </summary>
        private void CheckAssembliesFolder()
        {
            if (!Directory.Exists(Constants.CLIENTHANDLER_PLUGIN_ASSEMBLIES_FOLDER))
            {
                Logger.LogText(string.Format("PLUGIN_ASSEMBLIES_FOLDER={0} does not exist. Create it now", Constants.CLIENTHANDLER_PLUGIN_ASSEMBLIES_FOLDER), this, Logtype.Info);
                Directory.CreateDirectory(Constants.CLIENTHANDLER_PLUGIN_ASSEMBLIES_FOLDER);
                Logger.LogText(string.Format("PLUGIN_ASSEMBLIES_FOLDER={0} created", Constants.CLIENTHANDLER_PLUGIN_ASSEMBLIES_FOLDER), this, Logtype.Info);
            }
        }

        /// <summary>
        /// Checks, if the ResourceData folder exists; if not it creates it
        /// </summary>
        private void CheckResourceDataFolder()
        {
            if (!Directory.Exists(Constants.CLIENTHANDLER_RESOURCEDATA_FOLDER))
            {
                Logger.LogText(string.Format("RESOURCEDATA_FOLDER={0} does not exist. Create it now", Constants.CLIENTHANDLER_RESOURCEDATA_FOLDER), this, Logtype.Info);
                Directory.CreateDirectory(Constants.CLIENTHANDLER_RESOURCEDATA_FOLDER);
                Logger.LogText(string.Format("RESOURCEDATA_FOLDER={0} created", Constants.CLIENTHANDLER_RESOURCEDATA_FOLDER), this, Logtype.Info);
            }
        }

        /// <summary>
        /// Sends a message to the client
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sslStream"></param>
        private void SendMessage(Message message, SslStream sslStream)
        {
            byte[] messagebytes = message.Serialize();
            sslStream.Write(messagebytes);
            sslStream.Flush();
            Logger.LogText(string.Format("Sent a \"{0}\" message to the client", message.MessageHeader.MessageType.ToString()), this, Logtype.Debug);
        }
    }
}