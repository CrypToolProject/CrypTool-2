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
using CrypToolStoreLib.Tools;
using System;
using System.Security.Cryptography.X509Certificates;

namespace CrypToolStoreBuildSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger.LogFilePrefix = "CrypToolStoreBuildSystem";
            Logger.EnableFileLog = true;
            X509Certificate2 serverCertificate;
            try
            {
                serverCertificate = new X509Certificate2(Configuration.GetConfiguration().GetConfigEntry("ServerCertificate"));
            }
            catch (Exception ex)
            {
                Logger.GetLogger().LogText(string.Format("Exception while loading server certificate", ex.Message), null, Logtype.Error);
                return;
            }

            CrypToolStoreBuildServer server = new CrypToolStoreBuildServer(serverCertificate);
            server.Start();
            while (true)
            {
                Console.ReadLine();
            }
        }
    }
}
