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

using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

#endregion

namespace NetworkSender
{
    /// <summary>
    ///   singleton that stores active connections for both NetworkSender and NetworkReceiver
    /// </summary>
    public class NetworkConnectionStore
    {
        #region singleton

        private static NetworkConnectionStore instance;

        private NetworkConnectionStore() { }

        public static NetworkConnectionStore Instance => instance ?? (instance = new NetworkConnectionStore());

        #endregion

        private readonly List<NetworkConnection> connections = new List<NetworkConnection>();

        /// <summary>
        /// Gets the connection. Returns null if id doesnt exists
        /// </summary>
        /// <param name="ID">The identifier.</param>
        /// <returns></returns>
        public NetworkConnection GetConnection(int ID)
        {
            if (connections.Count >= ID)
            {
                return connections[ID - 1];
            }
            return null;
        }

        /// <summary>
        /// Adds the connection to the shared store. also sets the ID of the connection.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns>The ID of the connection</returns>
        public int AddConnection(NetworkConnection connection)
        {
            lock (connections)
            {
                connection.ID = connections.Count + 1;
                connections.Add(connection);
            }
            return connection.ID;
        }

        /// <summary>
        /// Stops this instance, by closing all active connections
        /// </summary> 
        public void Stop()
        {
            lock (connections)
            {
                List<NetworkConnection> toRemove = new List<NetworkConnection>();
                foreach (NetworkConnection networkConnection in connections)
                {
                    try
                    {
                        networkConnection.Close();
                    }
                    finally
                    {
                        toRemove.Add(networkConnection);
                    }
                }
                connections.RemoveAll(toRemove.Contains);
            }
        }
    }

    public abstract class NetworkConnection
    {
        public int ID { get; set; }
        public IPEndPoint RemoteEndPoint { get; set; }

        public abstract void Close();
    }

    public class UDPConnection : NetworkConnection
    {
        public UdpClient UDPClient { get; set; }
        public override void Close()
        {
            UDPClient.Close();
        }
    }

    public class TCPConnection : NetworkConnection
    {
        public TcpClient TCPClient { get; set; }
        public override void Close()
        {
            TCPClient.Close();
        }
    }

}