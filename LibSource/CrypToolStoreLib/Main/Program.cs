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
using CrypToolStoreLib.Database;
using CrypToolStoreLib.DataObjects;
using CrypToolStoreLib.Tools;
using CrypToolStoreLib.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrypToolStoreLib.Client;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.Threading;
using CrypToolStoreLib;

namespace CrpyStoreLib
{
    /// <summary>
    /// Main program of the CrypToolStore Server
    ///
    /// To create a new server, you need
    /// - create a new database containing the CrypToolStore tables/database
    /// - compile "Main" project
    /// - put assemblies on a linux server
    /// - adapt the config file on that server
    /// - start :-)
    /// 
    /// </summary>
    public class Program
    {
        private static Logger logger;
        private static Configuration Config = Configuration.GetConfiguration();
        private static Program program = new Program();

        static void Main(string[] args)
        {
            program.Run();
        }

        public void Run()
        {
            Logger.LogFilePrefix = "CrypToolStoreServer";
            Logger.EnableFileLog = true;
            string logdirectory = Config.GetConfigEntry("Logdirectory");
            if (string.IsNullOrEmpty(logdirectory))
            {
                Logger.LogDirectory = logdirectory;
            }
            else
            {
                Logger.LogDirectory = "Logs";
            }

            Logger.SetLogLevel(Logtype.Info);
            string logtype = Config.GetConfigEntry("Logtype");
            if (logtype != null)
            {
                switch (logtype.ToLower())
                {
                    case "debug":
                        Logger.SetLogLevel(Logtype.Debug);
                        break;
                    case "info":
                        Logger.SetLogLevel(Logtype.Info);
                        break;
                    case "warning":
                        Logger.SetLogLevel(Logtype.Warning);
                        break;
                    case "error":
                        Logger.SetLogLevel(Logtype.Error);
                        break;
                }                    
            }
            logger = Logger.GetLogger();
            
            CrypToolStoreDatabase database = CrypToolStoreDatabase.GetDatabase();
            if (!database.InitAndConnect(Config.GetConfigEntry("DB_Server"), Config.GetConfigEntry("DB_Name"), Config.GetConfigEntry("DB_User"), Config.GetConfigEntry("DB_Password"), int.Parse(Config.GetConfigEntry("DB_Connections"))))
            {
                logger.LogText("Could not connect to mysql database at startup... retrying it later", this, Logtype.Error);
            }

            CrypToolStoreServer server = null;
            
            try
            {
                X509Certificate2 cert = new X509Certificate2(Config.GetConfigEntry("Cert_File"), Config.GetConfigEntry("Cert_Password"));
                server = new CrypToolStoreServer();
                server.Port = int.Parse(Config.GetConfigEntry("Listenport"));
                server.ServerKey = cert;
                server.Start();

                while (true)
                {
                    //you have to enter quit to go down
                    string line = Console.ReadLine();
                    if (string.IsNullOrEmpty(line))
                    {
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        if (line.ToLower().Equals("quit"))
                        {
                            break;
                        }
                        else
                        {
                            Console.WriteLine(string.Format(
                                "Command >{0}< unknown... you can only enter >quit< to stop the server...", line));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogText(string.Format("Exception while starting/running the server: {0}", ex.Message), this, Logtype.Error);
            }
            finally
            {
                if (server != null)
                {
                    server.Stop();
                }
                database.Close();
            }
        }
    }
}
