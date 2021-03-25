using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;using System.Threading;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections;
using BitcoinBlockChainAnalyser;

namespace BitcoinDownloadServer
{
    class Server
    {
        //private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly Logger log = new Logger();
        private static string genesisTransaction = "{\"result\":{\"txid\":\"4a5e1e4baab89f3a32518a88c31bc87f618f76673e2cc77ab2127b7afdeda33b\",\"hash\":\"4a5e1e4baab89f3a32518a88c31bc87f618f76673e2cc77ab2127b7afdeda33b\",\"version\":1,\"size\":285,\"vsize\":285,\"locktime\":0,\"vin\":[{\"coinbase\":\"04ffff001d0104\",\"sequence\":4294967295}],\"vout\":[{\"value\":50.00000000,\"n\":0,\"scriptPubKey\":{\"asm\":\"0496b538e853519c726a2c91e61ec11600ae1390813a627c66fb8be7947be63c52da7589379515d4e0a604f8141781e62294721166bf621e73a82cbf2342c858eeOP_CHECKSIG\",\"hex\":\"410496b538e853519c726a2c91e61ec11600ae1390813a627c66fb8be7947be63c52da7589379515d4e0a604f8141781e62294721166bf621e73a82cbf2342c858eeac\",\"reqSigs\":1,\"type\":\"pubkey\",\"addresses\":[\"1A1zP1eP5QGefi2DMPTfTL5SLmv7DivfNa\"]}}],\"hex\":\"01000000010000000000000000000000000000000000000000000000000000000000000000ffffffff0704ffff001d0104ffffffff0100f2052a0100000043410496b538e853519c726a2c91e61ec11600ae1390813a627c66fb8be7947be63c52da7589379515d4e0a604f8141781e62294721166bf621e73a82cbf2342c858eeac00000000\",\"blockhash\":\"000000000019d6689c085ae165831e934ff763ae46a2a6c172b3f1b60a8ce26f\",\"confirmations\":531370,\"time\":1231006505,\"blocktime\":1231006505},\"error\":null,\"id\":null}";

        /*
         * This method creates a listener so that the requests from CT2 can be accepted and processed
         */
        static void Main(string[] args)
        {
            Logger.SetLogLevel(Logtype.Info);

            BitcoinDownloadServer.Properties.Settings.Default.Reload();
            string ServerIp = BitcoinDownloadServer.Properties.Settings.Default.bitcoinApiServerUrl;
            string UserName = BitcoinDownloadServer.Properties.Settings.Default.bitcoinApiUsername;

            log.LogText("Started with serverip=" + ServerIp, Logtype.Info);
            log.LogText("Started with username=" + UserName, Logtype.Info);

            TcpListener listener = null;
            try
            {
                listener = new TcpListener(IPAddress.Any, 8080);
                listener.Start();
                log.LogText("Service was started", Logtype.Info);
                log.LogText("Waiting for incoming client connections...", Logtype.Info);

                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    log.LogText("Accepted new Client connection...", Logtype.Info);
                    Thread thread = new Thread(ClientRequests);
                    thread.IsBackground = true;
                    thread.Start(client);
                }

            }
            catch (Exception e)
            {
                log.LogText("Error while starting the Service: "+ e.Message, Logtype.Error);
            }
            finally
            {
                if (listener != null)
                    listener.Stop();
            }

        }

         /*
         * This method controls the client requests and uses the message type to decide 
         * which additional method is called to retrieve data from the server.
         */
        private static void ClientRequests(object argument)
        {
            TcpClient client = (TcpClient)argument;
            try
            {
                var networkStream = client.GetStream();
                var message = new Message();
                do
                {
                    message = Message.ReceiveMessage(networkStream);
                    String s = System.Text.Encoding.UTF8.GetString(message.Payload, 0, message.Header.Size);

                    Message response = null;

                    if (message.Header.MessageType == MessageType.GetblockcountRequestMessage)
                    {
                        response = Getblockcount();
                    }
                    else if (message.Header.MessageType == MessageType.GetblockRequestMessage)
                    {
                        int number;
                        if(int.TryParse(s, out number))
                        {
                            string hash = Getblockhash(number);
                            if (!hash.Equals("see serverlog"))
                            {
                                response = Getblock(hash);
                            }

                        }
                        else
                        {
                            response = Getblock(s);
                        }
                        
                    }
                    else if (message.Header.MessageType == MessageType.GettransactionRequestMessage)
                    {
                        response = GetTransaction(s);
                    }else if (message.Header.MessageType == MessageType.GettxoutRequestMessage)
                    {
                        response = Gettxout(s);
                    }
                    if(response != null)
                    {
                        Message.SendMessage(networkStream, response);
                    }
                    else
                    {
                        networkStream.Close();
                    }




                } while (client.Connected);
                //close the complete client connection
                client.Close();
                log.LogText("Closing client Connection!", Logtype.Info);
            }
            catch (Exception e)
            {
                client.Close();
                log.LogText("Exiting thread: "+e.Message, Logtype.Error);
            }
            finally
            {
                if (client != null)
                    client.Close();
            }
        }

        /*
         * This method establishes the connection to the Blockchain server
         * return webRequest
         */
        private static HttpWebRequest ConnectToServer()
        {

            HttpWebRequest webRequest = null;
            try
            {
                // The properties for server connection
                string ServerIp = BitcoinDownloadServer.Properties.Settings.Default.bitcoinApiServerUrl;
                string UserName = BitcoinDownloadServer.Properties.Settings.Default.bitcoinApiUsername;
                string Password = BitcoinDownloadServer.Properties.Settings.Default.bitcoinApiPassword;

                webRequest = (HttpWebRequest)WebRequest.Create(ServerIp);
                webRequest.Credentials = new NetworkCredential(UserName, Password);

                webRequest.ContentType = "application/json-rpc";
                webRequest.Method = "POST";

                return webRequest;
            }catch(Exception e)
            {
                log.LogText("ConnectToServer(): "+e.Message, Logtype.Error);
                return webRequest;
            }
        }

        /*
         * This method defines the beginning of the beginning of the message
         * return Json Object
         */
        private static JObject BitcoinApiCall(string methodName)
        {
            JObject joe = new JObject();
            joe.Add(new JProperty("jsonrpc", "1.0"));
            joe.Add(new JProperty("id", "1"));
            joe.Add(new JProperty("method", methodName));

            return joe;
        }


        /*
         * The AddParameter methods can be used to attach a wide variety of parameters.
         * This is necessary because the parameters differ in the variable type from request to request.
         * return Json Object
         */
        private static JObject AddParameter(string parameter, JObject joe)
        {
            JArray props = new JArray();
            props.Add(parameter);
            joe.Add(new JProperty("params", props));
            return joe;
        }

        private static JObject AddParameter(int parameter, JObject joe)
        {
            JArray props = new JArray();
            props.Add(parameter);
            joe.Add(new JProperty("params", props));
            return joe;
        }

        private static JObject AddParameter(string parameter, bool value , JObject joe)
        {
            JArray props = new JArray();
            props.Add(parameter);
            props.Add(value);
            joe.Add(new JProperty("params", props));
            return joe;
        }

        private static JObject AddParameter(string parameter,int vout, bool value, JObject joe)
        {
            JArray props = new JArray();
            props.Add(parameter);
            props.Add(vout);
            props.Add(value);
            joe.Add(new JProperty("params", props));
            return joe;
        }

        /*
         * This method serializes and sends the data to the blockchain server and receives the response. 
         */
        private static string SendRequestAndGetResponse(JObject joe, HttpWebRequest webRequest)
        {

            try
            {
                // serialize JSON for request
                string s = JsonConvert.SerializeObject(joe);
                byte[] byteArray = Encoding.UTF8.GetBytes(s);
                webRequest.ContentLength = byteArray.Length;
                Stream dataStream = webRequest.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
            }
            catch (Exception e)
            {
                log.LogText("BitcoinApi not available: " + e.Message, Logtype.Error);
                return "API connection";
            }
            try
            { 
                // deserialze the response
                WebResponse webResponse = webRequest.GetResponse();
                StreamReader sReader = new StreamReader(webResponse.GetResponseStream(), true);
                string responseValue = sReader.ReadToEnd();
                return responseValue;
            }
            catch (Exception e)
            {
                log.LogText("error while deserialze: " + e.Message, Logtype.Error);
                return "response message";
            }
        }

        /*
         * The following methods generate the various Api calls and use the methods available above.
         * 
         */

        //Returns the number of the most recent block
        private static Message Getblockcount()
        {
            Message message = new Message();
            message.Header.MessageType = MessageType.GetblockcountResponseMessage;
            try
            {
                //send a request and get a response from api
                HttpWebRequest webRequest = ConnectToServer();
                JObject joe = BitcoinApiCall("getblockcount");
                string apiResponse = SendRequestAndGetResponse(joe, webRequest);
                if(!(apiResponse.Equals("API connection") || apiResponse.Equals("response message")))
                {
                    JObject getblockcount = JObject.Parse(apiResponse);
                    string clientResponse = (string)getblockcount.GetValue("result");
                    //create a message

                    byte[] byteArray = Encoding.ASCII.GetBytes(clientResponse);
                    message.Payload = byteArray;
                }
                else
                {
                    message = SetErrorPayload(message, apiResponse);
                }
            }
            catch (Exception e)
            {
                log.LogText("Error while executing Getblockcount method: " + e.Message, Logtype.Error);
                message = SetErrorPayload(message, "see serverlog");
            }
            return message;
        }

        //gets the block hash matching the block number
        private static string Getblockhash(int number)
        {

            try
            {
                //Step 1: we need the blockhash from the blocknumber
                HttpWebRequest webRequest = ConnectToServer();
                JObject joeHash = BitcoinApiCall("getblockhash");
                joeHash = AddParameter(number, joeHash);
                string hash = SendRequestAndGetResponse(joeHash, webRequest);
                if (!(hash.Equals("API connection") || hash.Equals("response message")))
                {
                    JObject getblockhash = JObject.Parse(hash);

                    //bockhash are written in the result parameter of the json string
                    return getblockhash.GetValue("result").ToString();
                }
                else
                {
                    return hash;
                }
            }
            catch (Exception e)
            {
                log.LogText("Error while executing Getblockhash method: "+e.Message, Logtype.Error);
                
            }
            return "see serverlog";

        }

        //gets the block data by specifying the block hash
        private static Message Getblock(string hash)
        {
            Message message = new Message();
            message.Header.MessageType = MessageType.GetblockResponseMessage;
            try
            {
                //we can push the block informations with the hash result
                JObject joeBlock = BitcoinApiCall("getblock");
                joeBlock = AddParameter(hash, joeBlock);
                HttpWebRequest webRequest = ConnectToServer();
                string getblock = SendRequestAndGetResponse(joeBlock, webRequest);
                if (!(getblock.Equals("API connection") || getblock.Equals("response message")))
                {
                    //create a message
                    byte[] byteArray = Encoding.ASCII.GetBytes(getblock);
                    message.Payload = byteArray;
                }
                else
                {
                    message = SetErrorPayload(message, getblock);
                }
            }
            catch (Exception e)
            {
                log.LogText("Error while executing Getblock method: "+e.Message, Logtype.Error);
                message = SetErrorPayload(message, "see serverlog");
            }
            return message;
        }

        //gets the transaction data by specifying the transaction hash
        private static Message GetTransaction(string hash)
        {
            Message message = new Message();
            message.Header.MessageType = MessageType.GettransactionResponseMessage;
            try
            {
                HttpWebRequest webRequest = ConnectToServer();
                JObject joeTransaction = BitcoinApiCall("getrawtransaction");
                string getTransaction;
                if (hash.Equals("4a5e1e4baab89f3a32518a88c31bc87f618f76673e2cc77ab2127b7afdeda33b"))
                {
                    getTransaction = genesisTransaction;
                }
                else
                {
                    joeTransaction = AddParameter(hash, true, joeTransaction);
                    getTransaction = SendRequestAndGetResponse(joeTransaction, webRequest);
                }
                if (!(getTransaction.Equals("API connection") || getTransaction.Equals("response message")))
                {
                    //create a message
                    byte[] byteArray = Encoding.ASCII.GetBytes(getTransaction);
                    message.Payload = byteArray;
                }
                else
                {
                    message = SetErrorPayload(message, getTransaction);
                }
            }
            catch (Exception e)
            {
                log.LogText("Error while executing GetTransaction method: " + e.Message, Logtype.Error);
                message = SetErrorPayload(message, "see serverlog");
            }

            return message;
        }

        //Get only the information from an output of a transaction
        private static Message Gettxout(string payload)
        {
            Message message = new Message();
            message.Header.MessageType = MessageType.GettxoutResponsetMessage;
            try
            {
                HttpWebRequest webRequest = ConnectToServer();
                JObject joeTxOut = BitcoinApiCall("gettxout");
                JObject joePayload = JObject.Parse(payload);

                joeTxOut = AddParameter(joePayload.GetValue("txid").ToString(), (int)joePayload.GetValue("vout"), true, joeTxOut);
                string getTxOut = SendRequestAndGetResponse(joeTxOut, webRequest);
                if (!(getTxOut.Equals("API connection") || getTxOut.Equals("response message")))
                {
                    //create a message
                    byte[] byteArray = Encoding.ASCII.GetBytes(getTxOut);
                    message.Payload = byteArray;
                }
                else
                {
                    message = SetErrorPayload(message, getTxOut);
                }
            }
            catch (Exception e)
            {
                log.LogText("Error while executing Gettxout method: " + e.Message, Logtype.Error);
                message = SetErrorPayload(message, "see serverlog");
            }

            return message;
        }


        private static Message SetErrorPayload(Message message,string payload)
        {
            byte[] byteArray = Encoding.ASCII.GetBytes(payload);
            message.Payload = byteArray;

            return message;
        }

    }
}
