using System;
using System.IO;
using System.Threading;

namespace PlayfairAnalysis.Common
{
    public class Stats
    {
        private readonly AnalysisInstance instance;
        private readonly Utils utils;

        public long[] monogramStats = new long[Utils.TEXT_ALPHABET_SIZE];
        public long[] bigramStats = new long[Utils.TEXT_ALPHABET_SIZE * 32];
        private short[] hexagramStats = null;
        public long evaluations = 0;

        public Stats(AnalysisInstance instance)
        {
            this.instance = instance;
            utils = new Utils(0);
        }

        public bool readHexagramStatsFile(string filename, CancellationToken ct)
        {
            long start = DateTime.Now.Ticks;

            instance.CtAPI.printf("Loading hexagram stats file {0}\n",
                    filename);

            int totalShortRead = 0;

            try
            {
                //FileInputStream _is = new FileInputStream(new File(filename));

                //Buffer.BlockCopy()


                hexagramStats = new short[26 * 26 * 26 * 26 * 26 * 26];

                using (BinaryReader reader = new BinaryReader(File.OpenRead(filename)))
                {
                    byte[] stats = reader.ReadBytes(hexagramStats.Length * 2);
                    for (int i = 0; i < stats.Length; i += 2)
                    {
                        //Convert big endian to little endian values:
                        byte tmp = stats[i];
                        stats[i] = stats[i + 1];
                        stats[i + 1] = tmp;
                    }
                    Buffer.BlockCopy(stats, 0, hexagramStats, 0, stats.Length);
                }

                /*
                int[] hist = new int[100000];
                for (short h : hexagramStats) {
                    hist[h]++;
                }
                for (int i = 0; i < hist.Length; i++) {
                    if (hist[i] > 0) {
                        Console.Out.WriteLine("%,8d %,10d/%,10d\n", i, hist[i], hexagramStats.Length);
                    }
                }
                */


            }
            catch (IOException ex)
            {
                instance.CtAPI.goodbyeFatalError("Unable to read hexa file {0} - {1}", filename, ex.ToString());
            }
            instance.CtAPI.printf("Hexagram stats file {0} loaded successfully ({1} seconds), size = {2} bytes\n",
                    filename, TimeSpan.FromTicks(DateTime.Now.Ticks - start).TotalSeconds, totalShortRead * 2);
            instance.CtAPI.println("");
            instance.CtAPI.println("");
            return true;
        }

        private const int POWER_26_5 = 26 * 26 * 26 * 26 * 26;

        public long evalPlaintextHexagram(int[] plaintext, int plaintextLength)
        {

            instance.CtAPI.shutdownIfNeeded();
            if (plaintextLength < 6)
            {
                return 0;  //Do not evaluate
            }

            evaluations++;

            int index = (((((((plaintext[0] * 26) + plaintext[1]) * 26) + plaintext[2]) * 26) + plaintext[3]) * 26 + plaintext[4]);
            long val = 0;
            for (int i = 5; i < plaintextLength; i++)
            {
                index = (index % POWER_26_5) * 26 + plaintext[i];
                val += hexagramStats[index];
            }
            return (val * 1000) / (plaintextLength - 5);

        }

        public long evalPlaintextHexagram(int[] plaintext)
        {
            return evalPlaintextHexagram(plaintext, plaintext.Length);
        }

        public (TimeSpan elapsed, long evaluations) evaluationsSummary()
        {
            TimeSpan elapsed = utils.getElapsed();
            return (elapsed, evaluations);
            //return $"[{elapsed.TotalSeconds:N0} sec][{evaluations / 1000:N0} K decryptions ({evaluations / elapsed.TotalMilliseconds:N0} K/sec)]";
        }

        private int readBigramFile(string fileName)
        {

            string line = "";
            int items = 0;

            try
            {
                using (StreamReader bufferedReader = new StreamReader(fileName))
                {
                    while ((line = bufferedReader.ReadLine()) != null)
                    {

                        line = line.ToUpper();
                        string[] split = line.Split(new[] { "[ ]+" }, StringSplitOptions.None);
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
            }
            catch (IOException ex)
            {
                instance.CtAPI.goodbyeFatalError("Unable to read bigram file {0} - {1}", fileName, ex.ToString());
            }

            instance.CtAPI.printf("Bigram file read: {0}, items  = {1}  \n", fileName, items);

            convertToLog(bigramStats);

            return items;

        }

        private int readMonogramFile(string fileName, bool m209)
        {

            string line;
            int items = 0;

            try
            {
                using (StreamReader bufferedReader = new StreamReader(fileName))
                {
                    while ((line = bufferedReader.ReadLine()) != null)
                    {

                        line = line.ToUpper();
                        string[] split = line.Split(new[] { "[ ]+" }, StringSplitOptions.None);
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
            }
            catch (IOException ex)
            {
                instance.CtAPI.goodbyeFatalError("Unable to read mono file {0} - {1}", fileName, ex.ToString());
            }

            instance.CtAPI.printf("mono file read: {0}, items  = {1}  \n", fileName, items);

            convertToLog(monogramStats);

            return items;

        }

        private int readFileForStats(string fileName, bool m209)
        {


            string line;
            int length = 0;
            string from = "èéìùòàëáöæëüãþôâäíûóšøůěňïçñíàçèìåáßŕúµýˆ^άλêéąîőčžâªªºžńάλληφοράθęźðöżõřáěšďťˇי".ToUpper();
            string to = "eeiuoaeaoaeuapoaaiuosouenicniaceiaasrupyxxageeaioczaaaoznxxxxxxxxxxzoozoraesdtxe".ToUpper();


            try
            {
                using (StreamReader bufferedReader = new StreamReader(fileName))
                {
                    int l2 = -1;
                    while ((line = bufferedReader.ReadLine()) != null)
                    {

                        foreach (char c_iter in line.ToUpper())
                        {
                            char c = c_iter;
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
            }
            catch (IOException ex)
            {
                instance.CtAPI.goodbyeFatalError("Unable to read text file for stats  {0} - {1}", fileName, ex.ToString());
            }

            convertToLog(bigramStats);
            convertToLog(monogramStats);

            instance.CtAPI.printf("Text file read for stats {0}, length = {1} \n", fileName, length);

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

        public bool load(string dirname, Language language, bool m209)
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
                instance.CtAPI.goodbyeFatalError("Cannot load stats - language: " + language);
            }
            return true;
        }

    }

}
