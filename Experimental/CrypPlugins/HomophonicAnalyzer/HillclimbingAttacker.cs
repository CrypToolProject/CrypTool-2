using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Diagnostics;
using System.Security.Cryptography;
using CrypTool.PluginBase.IO;

namespace CrypTool.Plugins.HomophonicAnalyzer
{
    class HillclimbingAttacker
    {
        #region Variables

        // Delegate
        private bool stopFlag;
        private PluginProgress pluginProgress;
        private UpdateKeyDisplay updateKeyDisplay;

        private string ciphertextString = null;
        private string plaintextalphabet = null;
        private string ciphertextalphabet = null;
        private int restarts;
        private double[,,] _trigrams;
        private double[,,,] _quadgrams;

        private long totalKeys;
        Random random = new Random();
        int[] freqs;
        int[] ciphertext, plaintext;
        int[] key, copykey, bestkey;

        #endregion Variables;

        #region Input Properties

        public long TotalKeys
        {
            get { return this.totalKeys; }
        }

        public string Ciphertext
        {
            get { return this.ciphertextString; }
            set { this.ciphertextString = value; }
        }

        public string PlaintextAlphabet
        {
            get { return this.plaintextalphabet; }
            set { this.plaintextalphabet = value; }
        }

        public string CiphertextAlphabet
        {
            get { return this.ciphertextalphabet; }
            set { this.ciphertextalphabet = value; }
        }

        public int Restarts
        {
            get { return this.restarts; }
            set { this.restarts = value; }
        }

        public Boolean StopFlag
        {
            get { return this.stopFlag; }
            set { this.stopFlag = value; }
        }
        #endregion Input Properties

        #region Output Properties

        public UpdateKeyDisplay UpdateKeyDisplay
        {
            get { return this.updateKeyDisplay; }
            set { this.updateKeyDisplay = value; }
        }

        public PluginProgress PluginProgressCallback
        {
            get { return this.pluginProgress; }
            set { this.pluginProgress = value; }
        }
        #endregion Output Properties

        public void Execute(int algorithm)
        {
            freqs = new int[PlaintextAlphabet.Length];
            
            //Load Costfunction
            Load3Grams();
            Load4Grams();

            //Take input and prepare
            ciphertext = MapTextIntoNumberSpace(RemoveInvalidChars(ciphertextString, ciphertextalphabet), ciphertextalphabet);
            plaintext = new int[ciphertext.Length];

            bestkey = new int[ciphertextalphabet.Length];
            copykey = new int[ciphertextalphabet.Length];
            key = copykey;

            //key = new int[] { 8, 19, 24, 0, 13, 22, 1, 2, 4, 6, 3, 5, 8, 11, 12, 14, 15, 17, 18, 21, 7, 20, 19, 23 };
            plaintext = Decrypt(ciphertext, key, ciphertext.Length);
            Console.WriteLine(getScore(plaintext));

            if (algorithm == 0)
            {
                Execute_HillClimbingDeterministic();
            }
            else if (algorithm == 1)
            {
                Execute_HillClimbingRandom();
            }
            else if (algorithm == 2)
            {
                Execute_SimulatedAnnealing();
            }

            pluginProgress(1, 1);
        }

        public void Execute_HillClimbingDeterministic()
        {
            totalKeys = 0;
            bool foundBetter;
            double costvalue, bestcostvalue;

            for (int restart = 0; restart < restarts; restart++)
            {
                key = BuildRandomKey();

                //key = Enumerable.Range(0, key.Length).ToArray();
                //for(int i=0;i<key.Length;i++)
                //{
                //    int j = random.Next(key.Length);
                //    int tmp = key[i]; key[i] = key[j]; key[j] = tmp;
                //}

                bestcostvalue = double.MinValue;

                do
                {
                    foundBetter = false;

                    for (int i = 0; i < key.Length; i++)
                    {
                        for (int j = i+1; j < key.Length; j++)
                        {
                            if (StopFlag) return;

                            Array.Copy(key, copykey, key.Length);   // save key
                            
                            // modify key
                            int tmp = key[i]; key[i] = key[j]; key[j] = tmp;

                            totalKeys++;

                            plaintext = Decrypt(ciphertext, key, ciphertext.Length);
                            costvalue = getScore(plaintext);

                            if (bestcostvalue < costvalue)
                            {
                                bestcostvalue = costvalue;
                                Array.Copy(key, bestkey, key.Length);
                                Array.Copy(key, copykey, key.Length);   // save key
                                foundBetter = true;

                                //Add to bestlist-output:
                                KeyCandidate newKeyCan = new KeyCandidate(bestkey, bestcostvalue,
                                    ConvertNumbersToLetters(plaintext, plaintextalphabet), ConvertNumbersToLetters(bestkey, plaintextalphabet));
                                newKeyCan.HillAttack = true;
                                this.updateKeyDisplay(newKeyCan);
                            }

                            Array.Copy(copykey, key, key.Length);   // restore key
                        }
                    }

                    Array.Copy(bestkey, key, key.Length);   // key = bestkey

                } while (foundBetter);

                pluginProgress(restart, restarts);
            }
        }
        
        public void Execute_HillClimbingRandom()
        {
            //int num = 100000;
            //Random rnd = new Random();
            //Stopwatch sw = new Stopwatch();
            //sw.Start();
            //for(int i=0;i<num;i++)
            //    BuildRandomKey(rnd);
            //sw.Stop();
            //Console.WriteLine(sw.ElapsedMilliseconds);
            //sw.Reset();
            //sw.Start();
            //for (int i = 0; i < num; i++)
            //    BuildRandomKey2(rnd);
            //sw.Stop();
            //Console.WriteLine(sw.ElapsedMilliseconds);

            //Dictionary<string, double> d = new Dictionary<string, double>();

            //for (int i = 0; i < 26; i++)
            //{
            //    for (int j = 0; j < 26; j++)
            //    {
            //        for (int k = 0; k < 26; k++)
            //        {
            //            for (int l = 0; l < 26; l++)
            //            {
            //                char[] c = new char[] { (Char)(65 + i), (Char)(65 + j), (Char)(65 + k), (Char)(65 + l) };
            //                string s = new string(c);
            //                d[s] = _quadgrams[i, j, k, l];
            //            }
            //        }
            //    }
            //}

            //foreach (var kk in d.Keys.OrderByDescending(k => d[k]).Take(20))
            //{
            //    Console.WriteLine(kk + ": " + d[kk]);
            //}
            
            double costvalue, bestcostvalue, globalbestcostvalue = double.MinValue;
            int maximumNoImprovementCounter = 5000;


            totalKeys = 0;

            for (int restart = 0; restart < restarts; restart++)
            {
                key = BuildRandomKey();
                bestcostvalue = double.MinValue;
                
                for(int noImprovementCounter = 0; noImprovementCounter < maximumNoImprovementCounter; noImprovementCounter++)
                {
                    if (StopFlag) return;

                    Array.Copy(key, copykey, key.Length);   // save key

                    // modify key
                    int n = random.Next(1, 3);
                    for (int i = 0; i < n; i++)
                        key[random.Next(key.Length)] = random.Next(plaintextalphabet.Length);
                    //    Swap(key, random.Next(key.Length), random.Next(key.Length));

                    totalKeys++;

                    plaintext = Decrypt(ciphertext, key, ciphertext.Length);
                    costvalue = getScore(plaintext);

                    if (bestcostvalue < costvalue)
                    {
                        bestcostvalue = costvalue;
                        noImprovementCounter = 0;
                        Array.Copy(key, bestkey, key.Length);
                        Array.Copy(key, copykey, key.Length);

                        if (globalbestcostvalue < bestcostvalue)
                            globalbestcostvalue = bestcostvalue;

                        //Add to bestlist-output:
                        KeyCandidate newKeyCan = new KeyCandidate(bestkey, bestcostvalue,
                            ConvertNumbersToLetters(plaintext, plaintextalphabet), ConvertNumbersToLetters(bestkey, plaintextalphabet));
                        newKeyCan.HillAttack = true;
                        this.updateKeyDisplay(newKeyCan);
                    }

                    Array.Copy(copykey, key, key.Length);   // restore key
                }

                pluginProgress(restart, restarts);
            }
        }

        public void Execute_SimulatedAnnealing()
        {
            double workingScore, maxScore;

            double inittemp = 6;
            double epsilon = 0.1;
            int iterations = 1000;
            double factor = 0.999;
            double printCounter = 0;
            
            totalKeys = 0;

            for (int restart = 0; restart < restarts; restart++)
            {
                key = BuildRandomKey();
                maxScore = double.MinValue;

                plaintext = Decrypt(ciphertext, key, ciphertext.Length);
                double currentScore = getScore(plaintext);
                maxScore = currentScore;

                int cnt = 0;

                for (double temp = inittemp; temp > epsilon; temp *= factor)
                {
                    for (int t = 0; t < iterations; t++, cnt++)
                    {
                        if (StopFlag) return;

                        Array.Copy(key, copykey, key.Length);   // save key

                        // modify key
                        key[random.Next(key.Length)] = random.Next(plaintextalphabet.Length);

                        totalKeys++;

                        plaintext = Decrypt(ciphertext, key, ciphertext.Length);
                        workingScore = getScore(plaintext);

                        if (maxScore < workingScore)
                        {
                            maxScore = workingScore;
                            //Add to bestlist-output:
                            KeyCandidate newKeyCan = new KeyCandidate(bestkey, maxScore, ConvertNumbersToLetters(plaintext, plaintextalphabet), ConvertNumbersToLetters(bestkey, plaintextalphabet));
                            newKeyCan.HillAttack = true;
                            this.updateKeyDisplay(newKeyCan);
                        }

                        if (++printCounter == 5000)
                        {
                            printCounter = 0;
                            Console.Write("\r" + workingScore + " " + currentScore + " " + maxScore);
                        }

                        double diff = workingScore - currentScore;
                        double m = Math.Exp(diff / temp);
                        bool accept = (diff >= 0) || (random.NextDouble() < Math.Exp(diff / temp));

                        if (accept)
                        {
                            currentScore = workingScore;
                            Array.Copy(key, bestkey, key.Length);
                            Array.Copy(key, copykey, key.Length);
                        }

                        Array.Copy(copykey, key, key.Length);   // restore key
                    }
                }

                pluginProgress(restart, restarts);
            }
        }

        #region Methods & Functions

        public static string RemoveInvalidChars(string text, string alphabet)
        {
            return new String(text.Where(c => alphabet.Contains(c)).ToArray());
        }

        public static int[] MapTextIntoNumberSpace(string text, string alphabet)
        {
            return text.Select(c => alphabet.IndexOf(c)).ToArray();
        }

        private int[] BuildRandomKey()
        {
            return ciphertextalphabet.Select(_ => random.Next(plaintextalphabet.Length)).ToArray();
        }

        private void Swap(int[] key, int i, int j)
        {
            int tmp = key[i];
            key[i] = key[j];
            key[j] = tmp;
        }

        private int[] Decrypt(int[] ciphertext, int[] key, int length)
        {
            int[] plaintext = new int[length];

            for (int i = 0; i < length; i++)
                plaintext[i] = key[ciphertext[i]];

            return plaintext;
        }
        
        /// <summary>
        /// Maps a given array of numbers into the "textspace" defined by the alphabet
        /// </summary>
        /// <param name="numbers"></param>
        /// <param name="alphabet"></param>
        /// <returns></returns>
        public static string ConvertNumbersToLetters(int[] numbers, string alphabet)
        {
            return new string(numbers.Select(i => alphabet[i]).ToArray());
        }

        double getScore(int[] s)
        {
            double score = 0;

            //score += getScore5(s);
            score += CalculateCost4Grams(s) / 2;
            score += CalculateCost3Grams(s) / 4;
            //score += getScore2(s) / 8;
            score /= s.Length;
            score /= 1e18;
            //score = Math.Exp(score);

            getFreqs(s);
            //double f = IoC(s) - 0.0746;
            //f /= 5;
            //f *= f;
            //score *= 1 / (f + 1);
            double ioc = IoC(s);
            ioc -= 0.0665;
            if (ioc < 0) ioc = 0;
            ioc *= ioc;
            ioc *= ioc;
            //ioc = Math.Sqrt(ioc);
            //ioc = Math.Sqrt(ioc);
            //ioc = Math.Sqrt(ioc);
            double factor = 1 - ioc;
            if (factor < 0) factor = 0;
            //if (factor < 0) factor = 0;
            //double factor = 1 - (0.1 * Math.Abs(ioc - 0.0665));
            //double factor = 1 - ioc;
            score *= factor;
            //double chi2 = ChiSquare(s);
            //score += 10000-ChiSquare(s);

            return score;
        }

        public double IoC(int[] s)
        {
            float ic = 0;

            foreach (int f in freqs)
                ic += f * (f-1);

            ic /= s.Length * (s.Length - 1);

            return ic;
        }
        
        public void getFreqs(int[] s)
        {
            for (int i = 0; i < plaintextalphabet.Length; i++) freqs[i] = 0;

            foreach (var c in s)
                freqs[c]++;
        }

        double CalculateCost3Grams(int[] plaintext)
        {
            double value = 0;
            var end = plaintext.Length - 2;

            for (var i = 0; i < end; i++)
                value += _trigrams[plaintext[i], plaintext[i + 1], plaintext[i + 2]];

            return value;
        }

        double CalculateCost4Grams(int[] plaintext)
        {
            double value = 0;
            var end = plaintext.Length - 3;

            for (var i = 0; i < end; i++)
                value += _quadgrams[plaintext[i], plaintext[i + 1], plaintext[i + 2], plaintext[i + 3]];

            return value;
        }

        private void Load3Grams()
        {
            string filename = "";
            int n = 26;
            if (plaintextalphabet.Length == 26) { filename = "en-3gram-nocs.bin"; n = 26; }
            if (plaintextalphabet.Length == 27) { filename = "es-3gram-nocs.bin"; n = 27; }
            if (plaintextalphabet.Length == 30) { filename = "de-3gram-nocs.bin"; n = 26; }  // Fehler in "de-3gram-nocs.bin": Es sind nur 26^4 Samples enthalten, erwartet werden aber 30^4!

            _trigrams = new double[plaintextalphabet.Length, plaintextalphabet.Length, plaintextalphabet.Length];

            using (var fileStream = new FileStream(Path.Combine(DirectoryHelper.DirectoryLanguageStatistics, filename), FileMode.Open, FileAccess.Read))
            {
                using (var reader = new BinaryReader(fileStream))
                {
                    for (int i = 0; i < n; i++)
                    {
                        for (int j = 0; j < n; j++)
                        {
                            for (int k = 0; k < n; k++)
                            {
                                var bytes = reader.ReadBytes(8);
                                _trigrams[i, j, k] = BitConverter.ToDouble(bytes, 0);
                            }
                        }
                    }
                }
            }
        }

        private void Load4Grams()
        {
            string filename = "";
            if (plaintextalphabet.Length == 26) filename = "en-4gram-nocs.bin";
            if (plaintextalphabet.Length == 27) filename = "es-4gram-nocs.bin";
            if (plaintextalphabet.Length == 30) filename = "de-4gram-nocs.bin";

            _quadgrams = new double[plaintextalphabet.Length, plaintextalphabet.Length, plaintextalphabet.Length, plaintextalphabet.Length];

            using (var fileStream = new FileStream(Path.Combine(DirectoryHelper.DirectoryLanguageStatistics, filename), FileMode.Open, FileAccess.Read))
            {
                using (var reader = new BinaryReader(fileStream))
                {
                    for (int i = 0; i < plaintextalphabet.Length; i++)
                    {
                        for (int j = 0; j < plaintextalphabet.Length; j++)
                        {
                            for (int k = 0; k < plaintextalphabet.Length; k++)
                            {
                                for (int l = 0; l < plaintextalphabet.Length; l++)
                                {
                                    var bytes = reader.ReadBytes(8);
                                    _quadgrams[i, j, k, l] = BitConverter.ToDouble(bytes, 0);
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion Methods & Functions
    }
}