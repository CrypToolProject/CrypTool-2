using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Collections.Generic;

class CrypToolServer
{
    #region Properties

    public int Port {get;set;}

    #endregion

    #region Events

    public delegate void JobCompletedDelegate(EndPoint ipep, JobResult j, String name);
    public event JobCompletedDelegate OnJobCompleted;

    public delegate bool ClientConnectedDelegate(EndPoint ipep, String name, String password);
    public ClientConnectedDelegate OnClientAuth;

    public delegate void EndPointDelegate(EndPoint ipep);
    public event EndPointDelegate OnClientDisconnected;

    public event EndPointDelegate OnClientRequestedJob;

    public delegate void StringDelegate(String str);
    public event StringDelegate OnErrorLog;
    
    #endregion

    #region Variables

    private Dictionary<EndPoint, TcpClient> connectedClients = new Dictionary<EndPoint, TcpClient>();
    private TcpListener tcpListener;

    enum State
    {
        Created,
        Running,
        Exiting,
        Dead
    }
    private State state = State.Created;
    #endregion

    ///<summary>
    /// Starts the server. Will block as long as the server runs, you might want to start this in an additional thread.
    ///</summary>
    public void Run()
    {
        if (OnJobCompleted == null ||
            OnClientAuth == null ||
            OnClientDisconnected == null ||
            OnClientRequestedJob == null ||
            OnErrorLog == null)
        {
            throw new Exception("One of the mandatory events was not bound");
        }

        try
        {
            lock (this)
            {
                if (state != State.Created)
                {
                    if (OnErrorLog != null)
                    {
                        OnErrorLog("Invalid state: " + state);
                    }
                    return;
                }

                // has to be in lock, otherwise a Stop call can cause an object
                // disposed exception on tcpListener.start()
                state = State.Running;
                tcpListener = new TcpListener(IPAddress.Any, Port);
                tcpListener.Start();
            }


            while (state == State.Running)
            {
                TcpClient client = tcpListener.AcceptTcpClient();
                lock (connectedClients)
                {
                    connectedClients.Add(client.Client.RemoteEndPoint, client);
                }
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClient));
                clientThread.Start(client);
            }
        }
        catch (Exception e)
        {
            if (state == State.Running && OnErrorLog != null)
            {
                OnErrorLog("CrypToolServer: Got Exception while running "+e.Message);
            }
        }
        finally
        {
            try
            {
                state = State.Dead;
                tcpListener.Stop();
            }
            catch (Exception)
            {
            }
            lock (connectedClients)
            {
                foreach (var client in connectedClients)
                    client.Value.Close();
            }
        }
    }
    
    public void SendJob(JobInput j, EndPoint i)
    {
        TcpClient client = null;
        lock(connectedClients)
        {
            if (!connectedClients.TryGetValue(i, out client))
            {
                if (OnErrorLog != null)
                    OnErrorLog("Tried to send job to not present external client " + i);
                return;
            }
        }
        try
        {
            lock (client)
            {
                var wrapped = new PlatformIndependentWrapper(client);

                wrapped.WriteInt((int)ServerOpcodes.NEW_JOB);
                wrapped.WriteString(j.Guid);
                wrapped.WriteString(j.Src);
                wrapped.WriteInt(j.Key.Length);
                wrapped.WriteBytes(j.Key);
                wrapped.WriteInt(j.LargerThen ? 1 : 0);
                wrapped.WriteInt(j.Size);
                wrapped.WriteInt(j.ResultSize);
            }
        }
        catch (Exception e)
        {
            if (OnErrorLog != null)
                OnErrorLog("Got " + e.GetType() + " while sending job to " + i);
            client.Close();
            // nothing more to do here. HandleClient will remove this from connectedClients
        }
    }

    private void HandleClient(object obj)
    {
        var client = obj as TcpClient;
        EndPoint ep = client.Client.RemoteEndPoint;

        bool identified = false;
        String name = string.Empty;
        try
        {
            var wrapped = new PlatformIndependentWrapper(client);
            while (true)
            {
                switch ((ClientOpcodes)wrapped.ReadInt())
                {
                    case ClientOpcodes.HELLO:
                        {
                            name = wrapped.ReadString();
                            String password = wrapped.ReadString();
                            if (OnClientAuth == null || !OnClientAuth(ep, name, password))
                            {
                                wrapped.WriteInt((int)ServerOpcodes.WRONG_PASSWORD);
                                return;
                            }
                            identified = true;
                        }
                        break;

                    case ClientOpcodes.JOB_RESULT:
                        {
                            if (!identified)
                            {
                                if (OnErrorLog != null)
                                {
                                    OnErrorLog("Client '" + ep + "' tried to post result without identification");
                                }
                                return;
                            }

                            var jobGuid = wrapped.ReadString();
                            var resultList = new List<KeyValuePair<float, int>>();
                            var resultListLength = wrapped.ReadInt();
                            for (int c = 0; c < resultListLength; c++)
                            {
                                var key = wrapped.ReadInt();
                                var cost = wrapped.ReadFloat();
                                resultList.Add(new KeyValuePair<float, int>(cost, key));
                            }

                            JobResult rs = new JobResult();
                            rs.Guid = jobGuid;
                            rs.ResultList = resultList;

                            if (OnJobCompleted != null)
                            {
                                OnJobCompleted(ep, rs, name);
                            }
                        }
                        break;

                    case ClientOpcodes.JOB_REQUEST:
                        {
                            if (!identified)
                            {
                                if (OnErrorLog != null)
                                {
                                    OnErrorLog("Client '" + ep + "' tried to request job without identification");
                                }
                                return;
                            }

                            if (OnClientRequestedJob != null)
                            {
                                OnClientRequestedJob(ep);
                            }
                        }
                        break;
                }
            }
        }
        catch (SocketException)
        {
            // left blank intentionally. Will be thrown on client disconnect.
        }
        catch (Exception e)
        {
            if (OnErrorLog != null)
            {
                OnErrorLog("Client '" + ep + "' caused exception " + e);
            }
        }
        finally
        {
            // just to be sure..
            client.Close();

            lock (connectedClients)
            {
                connectedClients.Remove(ep);
            }

            if (OnClientDisconnected != null)
                OnClientDisconnected(ep);
        }
    }

    /// <summary>
    /// Closes this server. Any concurrent call to Run() in any other thread will return.
    /// </summary>
    public void Shutdown()
    {
        lock (this)
        {
            if (state == State.Dead ||
                state == State.Exiting)
            {
                return;
            }
            state = State.Exiting;
            try
            {
                tcpListener.Stop();
            }
            catch (Exception)
            {
            }
        }
    }
}
