/*
   Copyright 2020 George Lasry
   Converted in 2020 from Java to C# by Nils Kopal

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
using System.IO;

namespace EnigmaAnalyzerLib.Common
{
    public class Stats
    {
        public static long[] monogramStats = new long[Utils.TEXT_ALPHABET_SIZE];
        public static long[] bigramStats = new long[Utils.TEXT_ALPHABET_SIZE * 32];
        private static readonly short[] hexagramStats = null;
        public static long evaluations = 0;

        public static bool readHexagramStatsFile(string filename)
        {
            DateTime start = DateTime.Now;

            /*Console.WriteLine("Loading hexagram stats file {0} ({1} free shorts before loading)",
                    filename, Runtime.getRuntime().freeMemory());

            int totalShortRead = 0;

            try {
                FileStream inputstream = new FileStream(filename, FileMode.Open, FileAccess.Read);

                hexagramStats = new short[26 * 26 * 26 * 26 * 26 * 26];

                int CHUNK_SIZE = 65536;

                short[] hexagramStatsBuffer = new short[CHUNK_SIZE];
                short[] shorts = new short[CHUNK_SIZE * 2];

                int read;
                while ((read = inputstream.Read(shorts, 0, shorts.Length)) > 0)
                {
                    //auskommentiert von Nils
                    shortBuffer myshortBuffer = shortBuffer.wrap(shorts);
                    ShortBuffer myShortBuffer = myshortBuffer.asShortBuffer();
                    myShortBuffer.Get(hexagramStatsBuffer);
                    System.arraycopy(hexagramStatsBuffer, 0, hexagramStats, totalShortRead, read / 2);
                    totalShortRead += read / 2;
                }
                inputstream.Close();

                /*
                int[] hist = new int[100000];
                for (short h : hexagramStats) {
                    hist[h]++;
                }
                for (int i = 0; i < hist.Length; i++) {
                    if (hist[i] > 0) {
                        Console.WriteLine("%,8d %,10d/%,10d", i, hist[i], hexagramStats.Length);
                    }
                }
                
            } catch (IOException ex) {
                Console.WriteLine("Unable to read hexa file {0} - {0}", filename, ex.ToString());
            }
            /*Console.WriteLine("Hexagram stats file {0} loaded successfully (%f seconds), size = %,d shorts (%,d free shorts after loading)",
                    filename, (System.currentTimeMillis() - start) / 1_000.0, totalShortRead * 2, Runtime.getRuntime().freeMemory());
            Console.WriteLineln("");
            Console.WriteLineln("");*/
            return true;
        }

        private static readonly int POWER_26_5 = 26 * 26 * 26 * 26 * 26;

        public static long evalPlaintextHexagram(int[] plaintext, int plaintextLength)
        {

            //CtAPI.shutdownIfNeeded();
            Stats.evaluations++;

            int index = (((((((plaintext[0] * 26) + plaintext[1]) * 26) + plaintext[2]) * 26) + plaintext[3]) * 26 + plaintext[4]);
            long val = 0;
            for (int i = 5; i < plaintextLength; i++)
            {
                index = (index % POWER_26_5) * 26 + plaintext[i];
                val += hexagramStats[index];
            }
            return (val * 1000) / (plaintextLength - 5);

        }

        public static long evalPlaintextHexagram(int[] plaintext)
        {
            return evalPlaintextHexagram(plaintext, plaintext.Length);
        }

        public static string evaluationsSummary()
        {
            long elapsed = Utils.getElapsedMillis();
            if (elapsed <= 0)
            {
                elapsed = 1;
            }
            return string.Format("[{0} sec.][{1} decryptions ({2}/sec.)]", elapsed / 1000, Stats.evaluations / 1000, Stats.evaluations / elapsed);
        }

        private static int readBigramFile(string fileName)
        {

            string line = "";
            int items = 0;

            try
            {
                FileStream fileReader = new FileStream(fileName, FileMode.Open, FileAccess.Read);

                StreamReader bufferedReader = new StreamReader(fileReader);

                while ((line = bufferedReader.ReadLine()) != null)
                {

                    line = line.ToUpper();
                    string[] split = line.Split(new char[] { '[', ' ', ']', '+' });
                    int l1 = Utils.TEXT_ALPHABET.IndexOf(split[0][0]);
                    int l2 = Utils.TEXT_ALPHABET.IndexOf(split[0][1]);
                    if (l1 < 0 || l2 < 0)
                    {
                        continue;
                    }
                    long freq = long.Parse(split[1]);

                    bigramStats[(l1 << 5) + l2] += freq;
                    items++;
                }

                bufferedReader.Close();
            }
            catch (IOException ex)
            {
                Console.WriteLine("Unable to read bigram file {0} - {1}", fileName, ex.ToString());
            }

            Console.WriteLine("Bigram file read: {0}, items  = {1}  ", fileName, items);

            convertToLog(bigramStats);

            return items;

        }

        private static int readMonogramFile(string fileName, bool m209)
        {

            string line;
            int items = 0;

            try
            {
                FileStream fileReader = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                StreamReader bufferedReader = new StreamReader(fileReader);

                while ((line = bufferedReader.ReadLine()) != null)
                {

                    line = line.ToUpper();
                    string[] split = line.Split(new char[] { '[', ' ', ']', '+' });
                    int l1 = Utils.TEXT_ALPHABET.IndexOf(split[0][0]);
                    if (l1 < 0)
                    {
                        continue;
                    }
                    long freq = long.Parse(split[1]);

                    monogramStats[l1] += freq;
                    items++;
                }

                bufferedReader.Close();
            }
            catch (IOException ex)
            {
                Console.WriteLine("Unable to read mono file {0} - {1}", fileName, ex.ToString());
            }

            Console.WriteLine("mono file read: {0}, items  = {1}  ", fileName, items);

            convertToLog(monogramStats);

            return items;

        }

        private static int readFileForStats(string fileName, bool m209)
        {


            string line;
            int length = 0;
            string from = "èéìùòàëáöæëüãþôâäíûóšøůěňïçñíàçèìåáßŕúµýˆ^άλêéąîőčžâªªºžńάλληφοράθęźðöżõřáěšďťˇי".ToUpper();
            string to = "eeiuoaeaoaeuapoaaiuosouenicniaceiaasrupyxxageeaioczaaaoznxxxxxxxxxxzoozoraesdtxe".ToUpper();


            try
            {
                FileStream fileReader = new FileStream(fileName, FileMode.Open, FileAccess.Read);

                StreamReader bufferedReader = new StreamReader(fileReader);
                int l2 = -1;
                while ((line = bufferedReader.ReadLine()) != null)
                {

                    foreach (char c_iterate in line.ToUpper().ToCharArray())
                    {
                        char c = c_iterate;
                        if (m209)
                        {

                            if (c == ' ' || c == ',' || c == '.')
                            {
                                c = 'Z';
                            }
                        }

                        int rep = from.IndexOf(c);
                        if (rep != -1)
                        {
                            c = to[rep];
                        }
                        int l1 = l2;
                        l2 = Utils.TEXT_ALPHABET.IndexOf(c);
                        if (l1 != -1 && l2 != -1)
                        {
                            monogramStats[l1]++;
                            bigramStats[(l1 << 5) + l2]++;
                            length++;
                        }
                    }
                }

                // Always close files.
                bufferedReader.Close();
            }
            catch (IOException ex)
            {
                Console.WriteLine("Unable to read text file for stats  {0} - {1}", fileName, ex.ToString());
            }

            convertToLog(bigramStats);
            convertToLog(monogramStats);

            Console.WriteLine("Text file read for stats {0}, length = {1} ", fileName, length);

            return length;
        }

        private static void convertToLog(long[] stats)
        {
            long minVal = long.MaxValue;
            foreach (long stat in stats)
            {
                if ((stat > 0) && (stat < minVal))
                {
                    minVal = stat;
                }
            }

            for (int i = 0; i < stats.Length; i++)
            {
                if (stats[i] > 0)
                {
                    stats[i] = (long)(10000.0 * Math.Log((1.0 * stats[i]) / (1.0 * minVal)));
                }
            }

        }

        public static bool load(string dirname, Language language, bool m209)
        {
            int n = 1;
            switch (language)
            {
                case Language.ENGLISH:
                    //n *= readFileForStats("book.txt", m209);
                    n *= readBigramFile(dirname + "/" + "english_bigrams.txt");
                    n *= readMonogramFile(dirname + "/" + "english_monograms.txt", m209);
                    break;
                case Language.FRENCH:
                    n *= readBigramFile(dirname + "/" + "french_bigrams.txt");
                    n *= readMonogramFile(dirname + "/" + "french_monograms.txt", m209);
                    break;
                case Language.ITALIAN:
                    n *= readFileForStats(dirname + "/" + "italianbook.txt", m209);
                    break;
                case Language.GERMAN:
                    n *= readFileForStats(dirname + "/" + "germanbook.txt", m209);
                    //n *= readBigramFile(dirname + "/" + "german_bigrams.txt");
                    //n *= readMonogramFile(dirname + "/" + "german_monograms.txt", m209);
                    break;
            }
            if (m209)
            {
                monogramStats['E' - 'A'] = Math.Max(60000, monogramStats['E' - 'A']);
                monogramStats['Z' - 'A'] = Math.Max(80000, monogramStats['Z' - 'A']);
            }

            if (n == 0)
            {
                Console.WriteLine("Cannot load stats - language: " + language);
            }
            return true;
        }

    }
}