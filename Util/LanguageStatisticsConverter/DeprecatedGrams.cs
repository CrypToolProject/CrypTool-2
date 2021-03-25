using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

//This file contains all loading routines of the old, deprecated language statistics file format.

namespace LanguageStatisticsConverter
{

    /// <summary>
    /// Abstract super class for nGrams classes
    /// </summary>
    public abstract class Grams
    {
        public string Alphabet { get; protected set; }

        public abstract int GramLength { get; }

        public abstract IEnumerable<float> GetFlattenedFrequencies();
    }

    public class UniGrams : Grams
    {
        public float[] Frequencies;

        public override int GramLength => 1;

        public UniGrams(string filepath)
        {
            LoadGZ(filepath);
        }
        public override IEnumerable<float> GetFlattenedFrequencies()
        {
            return Frequencies.Cast<float>();
        }

        private void LoadGZ(string filepath)
        {
            uint[] data;
            uint max;
            ulong sum;

            BinaryFormatter bf = new BinaryFormatter();

            using (FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            using (var gz = new GZipStream(fs, CompressionMode.Decompress))
            {
                Alphabet = (string)bf.Deserialize(gz);
                max = (uint)bf.Deserialize(gz);
                sum = (ulong)bf.Deserialize(gz);
                if (max == 0 && sum == 0)
                {
                    Frequencies = (float[])bf.Deserialize(gz);
                }
                else
                {
                    data = (uint[])bf.Deserialize(gz);
                    Frequencies = new float[Alphabet.Length];
                    for (int a = 0; a < Alphabet.Length; a++)
                        Frequencies[a] = (float)Math.Log((data[a] + 0.001) / max);
                }
            }

        }
    }

    public class BiGrams : Grams
    {
        public float[,] Frequencies;

        public override int GramLength => 2;

        public BiGrams(string filepath)
        {
            LoadGZ(filepath);
        }

        public override IEnumerable<float> GetFlattenedFrequencies()
        {
            return Frequencies.Cast<float>();
        }

        private void LoadGZ(string filepath)
        {
            uint[,] data;
            uint max;
            ulong sum;

            BinaryFormatter bf = new BinaryFormatter();

            using (FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            using (var gz = new GZipStream(fs, CompressionMode.Decompress))
            {
                Alphabet = (string)bf.Deserialize(gz);
                max = (uint)bf.Deserialize(gz);
                sum = (ulong)bf.Deserialize(gz);
                if (max == 0 && sum == 0)
                {
                    Frequencies = (float[,])bf.Deserialize(gz);
                }
                else
                {
                    data = (uint[,])bf.Deserialize(gz);
                    Frequencies = new float[Alphabet.Length, Alphabet.Length];

                    for (int a = 0; a < Alphabet.Length; a++)
                        for (int b = 0; b < Alphabet.Length; b++)
                            Frequencies[a, b] = (float)Math.Log((data[a, b] + 0.001) / max);
                }
            }
        }
    }

    public class TriGrams : Grams
    {
        public float[,,] Frequencies;

        public override int GramLength => 3;

        public TriGrams(string filepath)
        {
            LoadGZ(filepath);
        }

        public override IEnumerable<float> GetFlattenedFrequencies()
        {
            return Frequencies.Cast<float>();
        }

        private void LoadGZ(string filepath)
        {
            uint[,,] data;
            uint max;
            ulong sum;

            BinaryFormatter bf = new BinaryFormatter();

            using (FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            using (var gz = new GZipStream(fs, CompressionMode.Decompress))
            {
                Alphabet = (string)bf.Deserialize(gz);
                max = (uint)bf.Deserialize(gz);
                sum = (ulong)bf.Deserialize(gz);
                if (max == 0 && sum == 0)
                {
                    Frequencies = (float[,,])bf.Deserialize(gz);
                }
                else
                {
                    data = (uint[,,])bf.Deserialize(gz);
                    Frequencies = new float[Alphabet.Length, Alphabet.Length, Alphabet.Length];

                    for (int a = 0; a < Alphabet.Length; a++)
                        for (int b = 0; b < Alphabet.Length; b++)
                            for (int c = 0; c < Alphabet.Length; c++)
                                Frequencies[a, b, c] = (float)Math.Log((data[a, b, c] + 0.001) / max);
                }

            }
        }
    }

    public class QuadGrams : Grams
    {
        public float[,,,] Frequencies;

        public override int GramLength => 4;

        public QuadGrams(string filepath)
        {
            LoadGZ(filepath);
        }

        public override IEnumerable<float> GetFlattenedFrequencies()
        {
            return Frequencies.Cast<float>();
        }

        private void LoadGZ(string filepath)
        {
            uint[,,,] data;
            uint max;
            ulong sum;

            BinaryFormatter bf = new BinaryFormatter();

            using (FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            using (var gz = new GZipStream(fs, CompressionMode.Decompress))
            {
                Alphabet = (string)bf.Deserialize(gz);
                max = (uint)bf.Deserialize(gz);
                sum = (ulong)bf.Deserialize(gz);
                if (max == 0 && sum == 0)
                {
                    Frequencies = (float[,,,])bf.Deserialize(gz);
                }
                else
                {
                    data = (uint[,,,])bf.Deserialize(gz);
                    Frequencies = new float[Alphabet.Length, Alphabet.Length, Alphabet.Length, Alphabet.Length];

                    for (int a = 0; a < Alphabet.Length; a++)
                        for (int b = 0; b < Alphabet.Length; b++)
                            for (int c = 0; c < Alphabet.Length; c++)
                                for (int d = 0; d < Alphabet.Length; d++)
                                    Frequencies[a, b, c, d] = (float)Math.Log((data[a, b, c, d] + 0.001) / max);
                }
            }
        }
    }

    public class PentaGrams : Grams
    {
        public float[,,,,] Frequencies;

        public override int GramLength => 5;

        public PentaGrams(string filepath)
        {
            LoadGZ(filepath);
        }

        public override IEnumerable<float> GetFlattenedFrequencies()
        {
            return Frequencies.Cast<float>();
        }

        private void LoadGZ(string filepath)
        {
            uint[,,,,] data;
            uint max;
            ulong sum;

            BinaryFormatter bf = new BinaryFormatter();

            using (FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            using (var gz = new GZipStream(fs, CompressionMode.Decompress))
            {
                Alphabet = (string)bf.Deserialize(gz);
                max = (uint)bf.Deserialize(gz);
                sum = (ulong)bf.Deserialize(gz);

                if (max == 0 && sum == 0)
                {
                    Frequencies = (float[,,,,])bf.Deserialize(gz);
                }
                else
                {
                    data = (uint[,,,,])bf.Deserialize(gz);
                    Frequencies = new float[Alphabet.Length, Alphabet.Length, Alphabet.Length, Alphabet.Length, Alphabet.Length];

                    for (int a = 0; a < Alphabet.Length; a++)
                        for (int b = 0; b < Alphabet.Length; b++)
                            for (int c = 0; c < Alphabet.Length; c++)
                                for (int d = 0; d < Alphabet.Length; d++)
                                    for (int e = 0; e < Alphabet.Length; e++)
                                        Frequencies[a, b, c, d, e] = (float)Math.Log((data[a, b, c, d, e] + 0.001) / max);
                }
            }
        }
    }
}
