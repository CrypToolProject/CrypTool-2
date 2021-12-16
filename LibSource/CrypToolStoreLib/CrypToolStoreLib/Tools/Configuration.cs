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
using System.IO;

namespace CrypToolStoreLib.Tools
{
    public class Configuration
    {
        private readonly Logger Logger = Logger.GetLogger();
        private static Configuration Instance = null;
        private readonly ConcurrentDictionary<string, string> ConfigurationDictionary = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// Sets the name of the config file
        /// </summary>
        public static string ConfigFileName
        {
            get;
            set;
        }

        /// <summary>
        /// Static constructor
        /// sets default values
        /// </summary>
        static Configuration()
        {
            ConfigFileName = "config.cfg";
        }

        /// <summary>
        /// Creates a new Configuration by reading the config file
        /// private => singleton pattern
        /// </summary>
        private Configuration()
        {
            try
            {
                string[] entries = File.ReadAllLines(ConfigFileName);
                int line = 1;
                Logger.LogText(string.Format("Reading configfile {0}", ConfigFileName), this, Logtype.Info);
                foreach (string entry in entries)
                {
                    if (entry.TrimStart().StartsWith("#"))
                    {
                        //config file allows comments beginning with #
                        continue;
                    }
                    string[] keyvalue = entry.Split('=');
                    if (keyvalue.Length != 2)
                    {
                        Logger.LogText(string.Format("Invalid entry in config file line {0}. It will be ignored.", line), this, Logtype.Warning);
                    }
                    else
                    {
                        Logger.LogText(string.Format("Read config entry of line {0} for {1}", line, keyvalue[0]), this, Logtype.Info);
                        ConfigurationDictionary[keyvalue[0].ToLower()] = keyvalue[1];
                    }
                    line++;
                }
                Logger.LogText(string.Format("Finished reading configfile {0}", ConfigFileName), this, Logtype.Info);
            }
            catch (Exception ex)
            {
                Logger.LogText(string.Format("Exception occured during reading of config file: {0}", ex.Message), this, Logtype.Error);
            }
        }

        /// <summary>
        /// Returns a config entry
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetConfigEntry(string key)
        {
            if (ConfigurationDictionary.ContainsKey(key.ToLower()))
            {
                return ConfigurationDictionary[key.ToLower()];
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Sets a config entry
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetConfigurationEntry(string key, string value)
        {
            ConfigurationDictionary[key] = value;
        }

        /// <summary>
        /// Returns the instance of the Configration
        /// </summary>
        /// <returns></returns>
        public static Configuration GetConfiguration()
        {
            if (Instance == null)
            {
                Instance = new Configuration();
            }
            return Instance;
        }
    }
}
