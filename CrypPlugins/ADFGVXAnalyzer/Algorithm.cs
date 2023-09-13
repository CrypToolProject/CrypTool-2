/*
   Copyright 2018 Dominik Vogt <ct2contact@CrypTool.org>

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
using common;
using System;

namespace ADFGVXAnalyzer
{
    public class IndexOfCoinzidenz
    {

        public const double GERMAN = 7.6;
        public const double ENGLISH = 6.5;
        public const double FRANCE = 7.8;
        public const double ITALIA = 7.6;
        public const double SPAIN = 7.5;

        public static double Getlanguage(int language)
        {
            switch (language)
            {
                case 0:
                    return GERMAN;
                case 1:
                    return ENGLISH;
                case 2:
                    return FRANCE;
                case 3:
                    return ITALIA;
                case 4:
                    return SPAIN;
                default:
                    return ENGLISH;
            }
        }

    }

    public class ThreadingHelper
    {
        public long[] decryptions;
        public int taskcount;
        public double bestOverall = 0.0;
        public object bestOverallLock = new object();
        public object decryptionsLock = new object();
        private readonly ADFGVXAnalyzer analyzer;

        public ThreadingHelper(int taskcount, ADFGVXAnalyzer analyzer)
        {
            this.taskcount = taskcount;
            decryptions = new long[taskcount];
            this.analyzer = analyzer;

        }

        public void UpdateDisplayEnd(int keylength, long decryptions, long alldecryptions)
        {
            alldecryptions *= this.decryptions.Length;
            analyzer.UpdateDisplayEnd(keylength, decryptions, alldecryptions);
            analyzer.ProgressChanged(decryptions, alldecryptions);
        }
    }

    public class Algorithm
    {
        private readonly int keyLength;
        private readonly int language;
        private readonly int restarts;
        private readonly int deviation;
        private readonly ADFGVXVector[] ciphers;
        private readonly Alphabet36Vector allPlain;
        private readonly Alphabet36Vector plain;
        private readonly ADFGVXVector interimCipher;
        private readonly Logger log;
        private readonly ThreadingHelper threadingHelper;
        private readonly ADFGVXAnalyzer analyzer;

        private readonly int taskId;

        public Algorithm(int keyLength, string[] messages, Logger log, int taskId, ThreadingHelper threadingHelper, ADFGVXANalyzerSettings settings, ADFGVXAnalyzer analyzer)
        {
            this.analyzer = analyzer;
            this.threadingHelper = threadingHelper;
            this.taskId = taskId;
            this.log = log;
            language = settings.Language;
            deviation = settings.Deviation;
            restarts = settings.Restarts;
            this.keyLength = keyLength;
            ciphers = new ADFGVXVector[messages.Length];
            int totalPlainLength = 0;
            int maxPlainLength = 0;
            for (int m = 0; m < messages.Length; m++)
            {
                ciphers[m] = new ADFGVXVector(messages[m].Replace(" ", ""), false);
                totalPlainLength += ciphers[m].length / 2;
                maxPlainLength = Math.Max(maxPlainLength, ciphers[m].length / 2);
            }
            allPlain = new Alphabet36Vector(totalPlainLength, true);
            plain = new Alphabet36Vector(maxPlainLength, false);
            interimCipher = new ADFGVXVector(maxPlainLength * 2, false);
        }

        private double Eval(ADFGVX key)
        {
            threadingHelper.decryptions[taskId - 1]++;
            allPlain.length = 0;
            foreach (ADFGVXVector cipher in ciphers)
            {
                key.Decrypt(cipher, interimCipher, plain);
                allPlain.append(plain);
            }
            allPlain.stats();
            return (6000.0 * allPlain.IoC1 + 180000.0 * allPlain.IoC2);
        }

        public void SANgramsIC()
        {

            AlphabetVector keepTranspositionKey = new AlphabetVector(keyLength, false);
            AlphabetVector newTranspositionKey = new AlphabetVector(keyLength, false);

            ADFGVX key = new ADFGVX("", keyLength);
            TranspositionTransformations transforms = new TranspositionTransformations(keyLength, true, true, true);

            for (int cycles = 1; cycles <= restarts; cycles++)
            {
                if (cycles % 10 == 0)
                {
                    analyzer.LogText += Environment.NewLine + "Task id: " + taskId + " starting with cycle: " + cycles;
                }
                key.RandomTranspositionKey();
                double score = Eval(key);

                double startTemp = 500.0;
                double endTemp = 20.0;
                double delta = 20.0;

                double temp = startTemp;
                for (int step = 0; temp >= endTemp; step++, temp -= delta)
                {
                    transforms.randomize();
                    int size = transforms.size();

                    for (int i = 0; i < size; i++)
                    {
                        keepTranspositionKey.copy(key.transpositionKey);
                        transforms.transform(keepTranspositionKey.TextInInt, newTranspositionKey.TextInInt, keyLength, i);
                        key.SetTranspositionKey(newTranspositionKey);
                        double newScore = Eval(key);
                        if (SimulatedAnnealing.Accept(newScore, score, temp))
                        {
                            score = newScore;
                            if (score > threadingHelper.bestOverall)
                            {
                                PrintIfBest(key, cycles, step, score, temp, deviation);
                            }
                        }
                        else
                        {
                            key.SetTranspositionKey(keepTranspositionKey);
                        }
                    }
                    // Update PresentationView
                    long alldecryptions = 0;
                    lock (threadingHelper.decryptionsLock)
                    {
                        foreach (long d in threadingHelper.decryptions)
                        {
                            alldecryptions += d;
                        }
                        threadingHelper.UpdateDisplayEnd(keyLength, alldecryptions, restarts * (long)(startTemp / delta) * size + restarts);
                    }
                }
            }
        }

        private void PrintIfBest(ADFGVX key, int cycles, int step, double score, double temp, int deviation)
        {
            lock (threadingHelper.bestOverallLock)
            {
                if (score > threadingHelper.bestOverall)
                {
                    threadingHelper.bestOverall = score;
                    analyzer.AddNewBestListEntry(Math.Round(threadingHelper.bestOverall, 0),
                        Math.Round(allPlain.IoC1, 2), Math.Round(allPlain.IoC2, 2), key.transpositionKey.ToString(), allPlain.ToString());
                    if (allPlain.IoC1 >= IndexOfCoinzidenz.Getlanguage(language) * ((float)(100 - deviation) / 100))
                    {
                        analyzer.TranspositionResult = allPlain.ToString();
                    }
                    analyzer.Transpositionkey = key.transpositionKey.ToString();
                    analyzer.LogText += Environment.NewLine + "Task: " + taskId + Environment.NewLine + "cycle: " + cycles +
                                        Environment.NewLine + "temp: " + temp + Environment.NewLine + "trans key: " + key.transpositionKey +
                                        Environment.NewLine + "bestOverall: " + threadingHelper.bestOverall +
                                        Environment.NewLine + "IoC1 and IoC2: " + allPlain.IoC1 + " " + allPlain.IoC2;
                }
            }
        }
    }
}
