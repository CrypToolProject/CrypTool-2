/*
   Copyright 2024 CrypTool 2 Team <ct2contact@CrypTool.org>

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
using LanguageStatisticsLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrypTool.AnalysisMonoalphabeticSubstitution
{
    internal class GeneticAttacker
    {
        #region Variables

        // Stop flag
        private bool stopFlag;

        // Genetic algorithm parameters
        private const int population_size = 300;
        private const int mutateProbability = 80;
        private const int maxGenerations = 40;
        private const double changeBorder = 0.001;
        private int repetitions = 1;
        private const int maxTextLength = 500;

        // Working Variables
        private static readonly Random rnd = new Random();
        private readonly List<int[]> banlist = new List<int[]>();

        // Input Property Variables
        private Alphabet plaintext_alphabet = null;
        private Alphabet ciphertext_alphabet = null;
        private Text ciphertext = null;
        private Grams grams;
        private bool standardAlphabet = false;

        // Output Property Variables
        private Text plaintext = null;
        private int[] best_key = null;
        private int currun_keys_tested = 0;

        // Delegate
        private PluginProgress pluginProgress;
        private UpdateKeyDisplay updateKeyDisplay;

        // Class for a population
        private class Population
        {
            public int[][] keys;
            public double[] fitness;
            public int[] prob;
            public double dev;
            public double fit_lastbestkey;
        }

        #endregion

        #region Constructor

        public GeneticAttacker()
        {

        }

        #endregion 

        #region Input Properties

        public Alphabet Plaintext_Alphabet
        {
            get => plaintext_alphabet;
            set => plaintext_alphabet = value;
        }

        public Alphabet Ciphertext_Alphabet
        {
            get => ciphertext_alphabet;
            set => ciphertext_alphabet = value;
        }

        public Text Ciphertext
        {
            get => ciphertext;
            set => ciphertext = value;
        }

        public Grams Grams
        {
            get => grams;
            set => grams = value;
        }

        public bool StandardAlphabet
        {
            get => standardAlphabet;
            set => standardAlphabet = value;
        }

        public bool StopFlag
        {
            get => stopFlag;
            set => stopFlag = value;
        }

        #endregion

        #region Output Properties

        public Text Plaintext
        {
            get => plaintext;
            private set { }
        }

        public int Currun_Keys
        {
            get => currun_keys_tested;
            private set { }
        }

        public UpdateKeyDisplay UpdateKeyDisplay
        {
            get => updateKeyDisplay;
            set => updateKeyDisplay = value;
        }

        public PluginProgress PluginProgressCallback
        {
            get => pluginProgress;
            set => pluginProgress = value;
        }

        #endregion

        #region Main Methods

        public void Analyze()
        {
            //Set progress to 50%
            pluginProgress(50, 100);

            // Adjust analyzer parameters to ciphertext length
            AdjustAnalyzerParameters(ciphertext.Length);
            currun_keys_tested = 0;

            // Initialization of repetition data structures
            int[][] bestkeys = new int[repetitions][];
            double[] bestkeys_fit = new double[repetitions];

            // Execute analysis
            for (int curRep = 0; curRep < repetitions; curRep++)
            {
                Population population = new Population();
                SetUpEnvironment(population, GeneticAttacker.population_size);

                CreateInitialGeneration(population, ciphertext, ciphertext_alphabet, plaintext_alphabet);

                double change = population.dev;
                int curGen = 1;
                Population nextGen = population;

                while ((change > GeneticAttacker.changeBorder) && (curGen < GeneticAttacker.maxGenerations))
                {
                    if (StopFlag)
                    {
                        return;
                    }

                    nextGen = CreateNextGeneration(nextGen, ciphertext, ciphertext_alphabet, false);
                    change = nextGen.dev;
                    curGen++;

                    pluginProgress(50.0 + (((double)(curRep + 1) * curGen) / (repetitions * GeneticAttacker.maxGenerations)) / 2 * 100, 100.0);
                }

                nextGen = CreateNextGeneration(nextGen, ciphertext, ciphertext_alphabet, true);

                plaintext = DecryptCiphertext(nextGen.keys[0], ciphertext, ciphertext_alphabet);

                bestkeys[curRep] = nextGen.keys[0];
                bestkeys_fit[curRep] = nextGen.fitness[0];

                Text plainTxt = DecryptCiphertext(bestkeys[curRep], ciphertext, ciphertext_alphabet);
                string plain = plainTxt.ToString(plaintext_alphabet);
                string key_string = CreateAlphabetOutput(bestkeys[curRep], plaintext_alphabet);

                // Report keyCan
                KeyCandidate newKeyCan = new KeyCandidate(bestkeys[curRep], bestkeys_fit[curRep], plain, key_string)
                {
                    GenAttack = true
                };
                updateKeyDisplay(newKeyCan);
            }
        }

        #endregion

        #region AlgorithmMethods

        private void SetUpEnvironment(Population pop, int size)
        {
            // Initialize data structures
            best_key = new int[size];
            pop.keys = new int[size][];
            pop.fitness = new double[size];

            // Create probability array to choose crossover keys
            pop.prob = new int[(size * (size + 1)) / 2];

            for (int i = 0, k = 0; i < size; i++)
            {
                for (int j = 0; j < size - i; j++)
                {
                    pop.prob[k++] = i;
                }
            }
        }

        private void CreateInitialGeneration(Population pop, Text ciphertext, Alphabet cipher_alpha, Alphabet plain_alpha)
        {
            // Create initial population keys
            int[] newkey;
            int keylength = cipher_alpha.Length;

            // Create the other population keys at random
            for (int i = 0; i < pop.keys.Length; i++)
            {
                newkey = CreateInitialKeyRandom(keylength);
                while (banlist.Contains(newkey))
                {
                    newkey = CreateInitialKeyRandom(keylength);
                }
                banlist.Add(newkey);
                pop.keys[i] = newkey;
            }

            // Calculate fitness of population keys
            for (int i = 0; i < pop.keys.Length; i++)
            {
                pop.fitness[i] = grams.CalculateCost(DecryptCiphertext(pop.keys[i], ciphertext, cipher_alpha).ToIntArray());
            }

            // Sort keys according to their fitness
            int[] helper1;
            double helper2;

            for (int i = 0; i < pop.keys.Length; i++)
            {
                for (int j = 0; j < pop.keys.Length; j++)
                {
                    if (pop.fitness[i] > pop.fitness[j])
                    {
                        helper1 = pop.keys[i];
                        pop.keys[i] = pop.keys[j];
                        pop.keys[j] = helper1;
                        helper2 = pop.fitness[i];
                        pop.fitness[i] = pop.fitness[j];
                        pop.fitness[j] = helper2;
                    }
                }
            }

            // Calculate change in development
            pop.fit_lastbestkey = pop.fitness[0];
            pop.dev = Math.Abs(pop.fitness[0]);
        }

        private Population CreateNextGeneration(Population pop, Text ciphertext, Alphabet cipher_alpha, bool last)
        {
            Population next = new Population
            {
                prob = pop.prob,
                fitness = new double[pop.keys.Length],
                keys = new int[pop.keys.Length][]
            };

            // Create population_size x children through crossover and mutate children
            int p1;
            int p2;
            int i1;
            int i2;
            int size = pop.prob.Length;
            int helper3;

            for (int i = 0; i < next.keys.Length; i++)
            {
                i1 = GeneticAttacker.rnd.Next(size);
                i2 = GeneticAttacker.rnd.Next(size);
                p1 = pop.prob[i1];
                p2 = pop.prob[i2];

                next.keys[i] = CombineKeys(pop.keys[p1], pop.fitness[p1], pop.keys[p2], pop.fitness[p2], this.ciphertext, ciphertext_alphabet);

                if (!last)
                {
                    for (int j = 0; j < next.keys[i].Length; j++)
                    {
                        if (GeneticAttacker.rnd.Next(GeneticAttacker.mutateProbability) == 0)
                        {
                            p1 = GeneticAttacker.rnd.Next(next.keys[i].Length);
                            p2 = GeneticAttacker.rnd.Next(next.keys[i].Length);
                            helper3 = next.keys[i][p1];
                            next.keys[i][p1] = next.keys[i][p2];
                            next.keys[i][p2] = helper3;
                        }
                    }
                }
            }

            // Calculate fitness of population
            for (int i = 0; i < next.keys.Length; i++)
            {
                next.fitness[i] = grams.CalculateCost(DecryptCiphertext(next.keys[i], ciphertext, cipher_alpha).ToIntArray());
            }

            // Sort keys according to their fitness
            int[] helper1;
            double helper2;

            for (int i = 0; i < next.keys.Length; i++)
            {
                for (int j = 0; j < next.keys.Length; j++)
                {
                    if (next.fitness[i] > next.fitness[j])
                    {
                        helper1 = next.keys[i];
                        next.keys[i] = next.keys[j];
                        next.keys[j] = helper1;
                        helper2 = next.fitness[i];
                        next.fitness[i] = next.fitness[j];
                        next.fitness[j] = helper2;
                    }
                }
            }

            // Calculate change in development
            next.dev = Math.Abs(Math.Abs(next.fitness[0]) - Math.Abs(pop.fit_lastbestkey));
            next.fit_lastbestkey = next.fitness[0];

            return next;
        }

        private void AdjustAnalyzerParameters(int textlength)
        {
            // Change parameters according to the ciphertext length
            if (textlength >= 200)
            {
                repetitions = 1;
            }
            else if (textlength >= 100)
            {
                repetitions = 2;
            }
            else if (textlength >= 90)
            {
                repetitions = 10;
            }
            else if (textlength >= 70)
            {
                repetitions = 20;
            }
            else
            {
                repetitions = 30;
            }
        }

        #endregion

        #region Support Methods

        private int[] CreateInitialKeyRandom(int keylength)
        {
            bool vorhanden = false;
            int[] res = new int[keylength];

            for (int i = 0; i < res.Length; i++)
            {
                int value;

                do
                {
                    vorhanden = false;
                    value = rnd.Next(res.Length);

                    for (int j = 0; j < i; j++)
                    {
                        if (res[j] == value)
                        {
                            vorhanden = true;
                            break;
                        }
                    }

                } while (vorhanden == true);

                res[i] = value;
            }
            return res;
        }

        private void MakeComplete(int[] key)
        {
            List<int> let = new List<int>();
            for (int i = 0; i < key.Length; i++)
            {
                let.Add(i);
            }
            for (int i = 0; i < key.Length; i++)
            {
                if (key[i] != -1)
                {
                    let.Remove(key[i]);
                }
            }
            for (int i = 0; i < key.Length; i++)
            {
                if (key[i] == -1)
                {
                    key[i] = let[0];
                    let.RemoveAt(0);
                }
            }
        }

        private bool KeyHasNoInformation(int[] key)
        {
            for (int i = 0; i < key.Length; i++)
            {
                if (key[i] != -1)
                {
                    return false;
                }
            }
            return true;
        }

        private int[] KeyCreatedByMatch(int[] key, Alphabet cipher_alpha, string ct_word, Alphabet plain_alpha, string dic_word)
        {
            int[] newkey = new int[key.Length];
            for (int i = 0; i < newkey.Length; i++)
            {
                newkey[i] = key[i];
            }
            int ct_index;
            int pt_index;
            for (int i = 0; i < ct_word.Length; i++)
            {
                ct_index = cipher_alpha.GetPositionOfLetter(ct_word.Substring(i, 1));
                pt_index = plain_alpha.GetPositionOfLetter(dic_word.Substring(i, 1));
                if ((!newkey.Contains(pt_index)) && (newkey[ct_index] == -1))
                {
                    newkey[ct_index] = pt_index;
                }

                if ((newkey[ct_index] != pt_index) && (newkey[ct_index] != -1))
                {
                    return null;
                }


            }

            return newkey;
        }

        private Text DecryptCiphertext(int[] key, Text ciphertext, Alphabet ciphertext_alphabet)
        {
            currun_keys_tested++;
            int index = -1;
            Text plaintext = ciphertext.CopyTo();

            for (int i = 0; i < ciphertext.Length; i++)
            {
                index = ciphertext.GetLetterAt(i);
                if (index >= 0)
                {
                    plaintext.ChangeLetterAt(i, key[index]);
                }
            }

            return plaintext;
        }

        public string DecryptCiphertext(int[] key)
        {
            Text plaintext = DecryptCiphertext(key, ciphertext, ciphertext_alphabet);
            return plaintext.ToString(plaintext_alphabet);
        }

        private int[] CombineKeys(int[] p1, double fit_p1, int[] p2, double fit_p2, Text ciphertext, Alphabet ciphertext_alphabet)
        {
            int[] res = new int[this.ciphertext_alphabet.Length];
            int[] less_fit;
            double fitness;
            double new_fitness;
            Text plaintext;

            if (fit_p1 > fit_p2)
            {
                p1.CopyTo(res, 0);
                less_fit = p2;
                fitness = fit_p1;
            }
            else
            {
                p2.CopyTo(res, 0);
                less_fit = p1;
                fitness = fit_p2;
            }

            int index = -1;
            for (int i = 0; i < res.Length; i++)
            {
                if (res[i] != less_fit[i])
                {
                    for (int j = 0; j < res.Length; j++)
                    {
                        if (res[j] == less_fit[i])
                        {
                            index = j;
                            break;
                        }
                    }
                    int helper = res[i];
                    res[i] = res[index];
                    res[index] = helper;
                    plaintext = DecryptCiphertext(res, ciphertext, ciphertext_alphabet);
                    new_fitness = grams.CalculateCost(plaintext.ToIntArray());
                    if (fitness > new_fitness)
                    {
                        helper = res[i];
                        res[i] = res[index];
                        res[index] = helper;
                    }

                }
            }

            return res;
        }

        private string CreateAlphabetOutput(int[] key, Alphabet ciphertext_alphabet)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < key.Length; i++)
            {
                sb.Append(ciphertext_alphabet.GetLetterFromPosition(key[i]));
            }

            return sb.ToString();
        }

        #endregion
    }
}