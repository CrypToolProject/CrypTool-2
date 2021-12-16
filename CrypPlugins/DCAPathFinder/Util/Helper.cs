/*
   Copyright 2019 Christian Bender christian1.bender@student.uni-siegen.de

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

using DCAPathFinder.Logic;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DCAPathFinder.Util
{
    public static class Helper
    {
        /// <summary>
        /// Converts a bool array to string representation
        /// </summary>
        /// <param name="inputArray"></param>
        /// <returns></returns>
        public static string BoolArrayToString(bool[] inputArray)
        {
            string result = "";

            for (int i = (inputArray.Length - 1); i >= 0; i--)
            {
                if (inputArray[i])
                {
                    result += "1";
                }
                else
                {
                    result += "0";
                }
            }

            return result;
        }

        /// <summary>
        /// Deserializes a saved DifferentialAttackRoundConfiguration from disk with a given filename
        /// </summary>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        public static DifferentialAttackRoundConfiguration LoadConfigurationFromDisk(string resourceName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string res = assembly.GetManifestResourceNames().Single(str => str.EndsWith(resourceName));

            DifferentialAttackRoundConfiguration data = null;

            string configurationString;
            using (Stream stream = assembly.GetManifestResourceStream(res))
            using (StreamReader reader = new StreamReader(stream))
            {
                configurationString = reader.ReadToEnd();
            }

            data = JsonConvert.DeserializeObject<DifferentialAttackRoundConfiguration>(configurationString, new Newtonsoft.Json.JsonSerializerSettings
            {
                TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
            });

            return data;
        }
    }
}
