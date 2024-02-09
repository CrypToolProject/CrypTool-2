/*
   Copyright CrypTool 2 Team josef.matwich@gmail.com

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
using M209AnalyzerLib.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace M209AnalyzerLib.Common
{
    public class Stats
    {
        public long[] MonogramStats { get; set; } = new long[Utils.TEXT_ALPHABET_SIZE];
        public long[] BigramStats { get; set; } = new long[Utils.TEXT_ALPHABET_SIZE * 32];
        public long Evaluations { get; set; } = 0;

        private short[] HexagramStats = null;

        private readonly int POWER_26_5 = 26 * 26 * 26 * 26 * 26;

        public long EvalPlaintextHexagram(int[] plaintext, int plaintextLength)
        {
            Evaluations++;

            int index = (((((((plaintext[0] * 26) + plaintext[1]) * 26) + plaintext[2]) * 26) + plaintext[3]) * 26 + plaintext[4]);
            long val = 0;
            for (int i = 5; i < plaintextLength; i++)
            {
                index = (index % POWER_26_5) * 26 + plaintext[i];
                val += HexagramStats[index];
            }
            return (val * 1000) / (plaintextLength - 5);
        }

        public long EvalPlaintextHexagram(int[] plaintext)
        {
            return EvalPlaintextHexagram(plaintext, plaintext.Length);
        }

        public string EvaluationsSummary()
        {
            long elapsed = Utils.GgetElapsedMillis();
            return String.Format("[%,d sec.][%,dK decryptions (%,dK/sec.)]", elapsed / 1000, Evaluations / 1000, Evaluations / elapsed);
        }

        public int ReadBigramFile(string fileName)
        {
            int items = 0;

            List<string> lines = File.ReadAllLines(fileName).ToList();

            foreach (string lineTmp in lines)
            {
                string line = lineTmp.ToUpper();
                Regex regex = new Regex("[ ]+");
                string[] split = regex.Split(line);
                int l1 = Utils.TEXT_ALPHABET.IndexOf(split[0].ElementAt(0));
                int l2 = Utils.TEXT_ALPHABET.IndexOf(split[0].ElementAt(1));
                if (l1 < 0 || l2 < 0)
                {
                    continue;
                }
                long freq = long.Parse(split[1]);

                BigramStats[(l1 << 5) + l2] += freq;
                items++;
            }

            Console.WriteLine($"Bigram file read: {fileName}, items  = {items}  \n");

            ConvertToLog(BigramStats);

            return items;
        }

        public int ReadMonogramFromResource(string resource)
        {
            string line;
            int items = 0;
            try
            {
                StringReader stringReader = new StringReader(resource);

                while (((line = stringReader.ReadLine()) != null))
                {
                    line = line.ToUpper();
                    Regex regex = new Regex("[ ]+");
                    string[] split = regex.Split(line);
                    int l1 = Utils.TEXT_ALPHABET.IndexOf(split[0].ElementAt(0));
                    if (l1 < 0)
                    {
                        continue;
                    }
                    long freq = long.Parse(split[1]);

                    MonogramStats[l1] += freq;
                    items++;
                }
                // Always close files.
                stringReader.Close();
            }
            catch (IOException ex)
            {
                Console.WriteLine("Unable to read tridict from resources: {0}", ex.ToString());
            }
            Console.WriteLine($"mono stats loaded from resource. items  = {items}  \n");

            ConvertToLog(MonogramStats);

            return items;
        }

        public int ReadMonogramFile(string fileName)
        {
            string line;
            int items = 0;

            List<string> lines = File.ReadAllLines(fileName).ToList();

            foreach (string lineTmp in lines)
            {
                line = lineTmp.ToUpper();
                Regex regex = new Regex("[ ]+");
                string[] split = regex.Split(line);
                int l1 = Utils.TEXT_ALPHABET.IndexOf(split[0].ElementAt(0));
                if (l1 < 0)
                {
                    continue;
                }
                long freq = long.Parse(split[1]);

                MonogramStats[l1] += freq;
                items++;
            }

            Console.WriteLine($"mono file read: {fileName}, items  = {items}  \n");

            ConvertToLog(MonogramStats);

            return items;
        }

        public int ReadFileForStats(string fileName, bool isM209)
        {
            int length = 0;
            string from = "èéìùòàëáöæëüãþôâäíûóšøůěňïçñíàçèìåáßŕúµýˆ^άλêéąîőčžâªªºžńάλληφοράθęźðöżõřáěšďťˇי".ToUpper();
            string to = "eeiuoaeaoaeuapoaaiuosouenicniaceiaasrupyxxageeaioczaaaoznxxxxxxxxxxzoozoraesdtxe".ToUpper();


            List<string> lines = File.ReadAllLines(fileName).ToList();

            int l2 = -1;
            foreach (string lineTmp in lines)
            {
                char[] charArray = lineTmp.ToUpper().ToCharArray();
                for (int i = 0; i < charArray.Length; i++)
                {
                    if (isM209)
                    {
                        if (charArray[i] == ' ' || charArray[i] == ',' || charArray[i] == '.')
                        {
                            charArray[i] = 'Z';
                        }
                    }

                    int rep = from.IndexOf(charArray[i]);
                    if (rep != -1)
                    {
                        charArray[i] = to.ElementAt(rep);
                    }
                    int l1 = l2;
                    l2 = Utils.TEXT_ALPHABET.IndexOf(charArray[i]);
                    if (l1 != -1 && l2 != -1)
                    {
                        MonogramStats[l1]++;
                        BigramStats[(l1 << 5) + l2]++;
                        length++;
                    }
                }
            }

            ConvertToLog(BigramStats);
            ConvertToLog(MonogramStats);

            Console.WriteLine($"Text file read for stats {fileName}, length = {length} \n");

            return length;
        }

        /// <summary>
        /// Exchange every value in stats array by the logarithm of this value
        /// </summary>
        /// <param name="stats">Array of values that have to be exchanged by log value of the value.</param>
        private void ConvertToLog(long[] stats)
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
                    stats[i] = (long)(10_000.0 * Math.Log((1.0 * stats[i]) / (1.0 * minVal)));
                }
            }

        }

        /// <summary>
        /// Load grams from file
        /// </summary>
        /// <param name="dirname">Directory of gram file</param>
        /// <param name="language">Gram file for language</param>
        /// <param name="isM209">Is this for the m209?</param>
        /// <returns></returns>
        public bool Load(string dirname, Language language, bool isM209)
        {
            int n = 1;
            switch (language)
            {
                case Language.ENGLISH:
                    n *= ReadBigramFile($"{dirname}/english_bigrams.txt");
                    n *= ReadMonogramFile($"{dirname}/english_monograms.txt");
                    break;
                case Language.FRENCH:
                    n *= ReadBigramFile($"{dirname}/french_bigrams.txt");
                    n *= ReadMonogramFile($"{dirname}/french_monograms.txt");
                    break;
                case Language.ITALIAN:
                    n *= ReadFileForStats($"{dirname}/italianbook.txt", isM209);
                    break;
                case Language.GERMAN:
                    n *= ReadFileForStats($"{dirname}/germanbook.txt", isM209);
                    break;
            }

            // If m209 the Z needs other value, because Z is also the space and the letter
            if (isM209)
            {
                Console.WriteLine('E' - 'A');
                MonogramStats['E' - 'A'] = Math.Max(60000, MonogramStats['E' - 'A']);
                MonogramStats['Z' - 'A'] = Math.Max(80000, MonogramStats['Z' - 'A']);
            }

            if (n == 0)
            {
                throw new Exception($"Cannot load stats - language: {language}");
            }
            return true;
        }

        /// <summary>
        /// Load grams from resource
        /// </summary>
        /// <param name="language">Gram file for language</param>
        /// <param name="isM209">Is this for the m209?</param>
        /// <returns></returns>
        public bool Load(Language language, bool isM209)
        {
            int n = 1;
            switch (language)
            {
                case Language.ENGLISH:
                    n *= ReadMonogramFromResource(Properties.Resources.english_monograms);
                    break;
                case Language.FRENCH:
                    n *= ReadMonogramFromResource(Properties.Resources.french_monograms);
                    break;
                case Language.ITALIAN:
                    break;
                case Language.GERMAN:
                    break;
            }

            // If m209 the Z needs other value, because Z is also the space and the letter
            if (isM209)
            {
                Console.WriteLine('E' - 'A');
                MonogramStats['E' - 'A'] = Math.Max(60000, MonogramStats['E' - 'A']);
                MonogramStats['Z' - 'A'] = Math.Max(80000, MonogramStats['Z' - 'A']);
            }

            if (n == 0)
            {
                throw new Exception($"Cannot load stats - language: {language}");
            }
            return true;
        }
    }
}
