using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using CrypTool.PluginBase.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO.Compression;

namespace CrypTool.Plugins.PlayfairAnalyzer
{
    class Stats
    {
        private static uint[] pentagramStatsFlat;
        private static uint[] hexagramStatsFlat;

        static Stats()
        {
            if (pentagramStatsFlat == null)
                pentagramStatsFlat = Load5GramsGZ("Data\\penta.gz");
                //readHexaBin("Data\\hexa.bin");
        }

        static bool readHexaBin(String filename)
        {
            //short[] hexaShort = new short[26 * 26 * 26 * 26 * 26 * 26];
            hexagramStatsFlat = new uint[26 * 26 * 26 * 26 * 26]; // use only pentagrams as hexagrams lead to memory exception
            
            filename = Path.Combine(DirectoryHelper.DirectoryCrypPlugins, filename);
            using (var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                using (var reader = new BinaryReader(fileStream))
                {
                    for (int i = 0; i < hexagramStatsFlat.Length; i++)
                    {
                        for (int j = 0; j < 26; j++)
                        {
                            var bytes = reader.ReadBytes(2);
                            Array.Reverse(bytes);   // adjust endianness
                            hexagramStatsFlat[i] += BitConverter.ToUInt16(bytes, 0);
                        }
                    }
                }
            }

            /*
            // save pentagrams as compressed file
            BinaryFormatter bf = new BinaryFormatter();

            using (var fs = new FileStream("penta.gz", FileMode.Create))
            {
                using (var gz = new GZipStream(fs, CompressionMode.Compress))
                {
                    bf.Serialize(gz, hexagramStatsFlat);
                }
            }
            */

            //for (int i = 0; i < 26 * 26 * 26 * 26 * 26 * 26; i++)
            //    hexagramStatsFlat[i] = hexaShort[i] * 10;

            return true;
        }

        public static uint[] Load5GramsGZ(string filename)
        {
            uint[] freq;

            BinaryFormatter bf = new BinaryFormatter();

            using (FileStream fs = new FileStream(Path.Combine(DirectoryHelper.DirectoryCrypPlugins, filename), FileMode.Open, FileAccess.Read))
            {
                using (var gz = new GZipStream(fs, CompressionMode.Decompress))
                {
                    freq = (uint[])bf.Deserialize(gz);
                }
            }

            return freq;
        }

        public static long evalPlaintextPentagram(byte[] plaintext, int plaintextLength)
        {
            int l1 = 0;
            int l2 = 0;
            int l3 = 0;
            int l4 = 0;

            long val = 0;

            for (int i = 0; i < plaintextLength; i++)
            {
                int l5 = plaintext[i];
                if (l5 < 26)
                {
                    if (i >= 4)
                        val += pentagramStatsFlat[((((((l1 * 26) + l2) * 26) + l3) * 26) + l4) * 26 + l5];
                }
                else
                    l5 = Utils.Z;

                l1 = l2;
                l2 = l3;
                l3 = l4;
                l4 = l5;
            }

            return (val*18) / ((plaintextLength - 4) * 26);
            //return val / (plaintextLength - 4);
        }

        public static long evalPlaintextHexagram(byte[] plaintext, int plaintextLength)
        {
            int l1 = 0;
            int l2 = 0;
            int l3 = 0;
            int l4 = 0;
            int l5 = 0;

            long val = 0;

            for (int i = 0; i < plaintextLength; i++)
            {
                int l6 = plaintext[i];
                if (l6 < 26)
                {
                    if (i >= 5)
                        val += hexagramStatsFlat[(((((((l1 * 26) + l2) * 26) + l3) * 26) + l4) * 26 + l5) * 26 + l6];
                }
                else
                    l6 = Utils.Z;

                l1 = l2;
                l2 = l3;
                l3 = l4;
                l4 = l5;
                l5 = l6;
            }

            return val / (plaintextLength - 5);
        }
    }
}