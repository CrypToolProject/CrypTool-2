using System.Collections.Generic;
using System.IO;
using Interfaces;
using Newtonsoft.Json;

namespace _3Round16BitSPN
{
    public static class DataLoader
    {
        public static void saveDifferentialAttackRoundConfiguration(DifferentialAttackRoundConfiguration data, string fileName)
        {
            using (StreamWriter file = File.CreateText(fileName))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Converters.Add(new Newtonsoft.Json.Converters.JavaScriptDateTimeConverter());
                serializer.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                serializer.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto;
                serializer.Formatting = Newtonsoft.Json.Formatting.Indented;
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

        public static void saveFilteredPairList(List<Pair> pairList, string fileName)
        {
            using (StreamWriter file = File.CreateText(fileName))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Converters.Add(new Newtonsoft.Json.Converters.JavaScriptDateTimeConverter());
                serializer.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                serializer.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto;
                serializer.Formatting = Newtonsoft.Json.Formatting.Indented;
                serializer.Serialize(file, pairList);
            }
        }

        public static List<Pair> loadFilteredPairList(string fileName)
        {
            List<Pair> pairList = null;

            using (StreamReader file = File.OpenText(fileName))
            {
                string data = file.ReadToEnd();
                pairList = JsonConvert.DeserializeObject<List<Pair>>(File.ReadAllText(fileName), new Newtonsoft.Json.JsonSerializerSettings
                {
                    TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
                    NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                });
            }

            return pairList;
        }
    }
}
