using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;
using Newtonsoft.Json;

namespace HeysToyCipher
{
    static class DataLoader
    {
        public static void saveDifferentialAttackRoundConfiguration(DifferentialAttackRoundConfiguration data, string fileName)
        {
            using (StreamWriter file = File.AppendText(fileName))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Converters.Add(new Newtonsoft.Json.Converters.JavaScriptDateTimeConverter());
                serializer.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                serializer.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto;
                serializer.Formatting = Newtonsoft.Json.Formatting.None;
                serializer.Serialize(file, data);
            }

            
        }

        public static DifferentialAttackRoundConfiguration loadDifferentialAttackRoundConfiguration(string fileName)
        {
            DifferentialAttackRoundConfiguration config = null;

            using (StreamReader file = File.OpenText(fileName))
            {
                string data = file.ReadToEnd();
                config = JsonConvert.DeserializeObject<DifferentialAttackRoundConfiguration>(File.ReadAllText(fileName), new Newtonsoft.Json.JsonSerializerSettings
                {
                    TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
                    NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                });
            }

            return config;
        }
    }
}
