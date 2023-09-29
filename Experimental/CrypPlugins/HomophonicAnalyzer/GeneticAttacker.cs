using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Diagnostics;
using System.Security.Cryptography;

namespace CrypTool.Plugins.HomophonicAnalyzer
{
    class GeneticAttacker
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
        private static Random rnd = new Random();
        private List<int[]> banlist = new List<int[]>();

        // Input Property Variables
        private Alphabet plaintext_alphabet = null;
        private Alphabet ciphertext_alphabet = null;
        private Text ciphertext = null;
        private Frequencies language_frequencies = null;
        private Boolean standardAlphabet = false;
        
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
            get { return this.plaintext_alphabet; }
            set { this.plaintext_alphabet = value; }
        }

        public Alphabet Ciphertext_Alphabet
        {
            get { return this.ciphertext_alphabet; }
            set {this.ciphertext_alphabet = value;}
        }

        public Text Ciphertext
        {
            get { return this.ciphertext; }
            set { this.ciphertext = value; }
        }

        public Frequencies Language_Frequencies
        {
            get { return this.language_frequencies; }
            set { this.language_frequencies = value; }
        }

        public Boolean StandardAlphabet
        {
            get { return this.standardAlphabet; }
            set { this.standardAlphabet = value; }
        }

        public Boolean StopFlag
        {
            get { return this.stopFlag; }
            set { this.stopFlag = value; } 
        }

        #endregion

        #region Output Properties
        
        public Text Plaintext
        {
            get { return this.plaintext; }
            private set { }
        }

        public int Currun_Keys
        {
            get { return this.currun_keys_tested; }
            private set { }
        }

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

        #endregion

        #region Main Methods

        public void Analyze()
        {
            //Set progress to 50%
            this.pluginProgress(50.0, 100.0);

            // Adjust analyzer parameters to ciphertext length
            AdjustAnalyzerParameters(this.ciphertext.Length);
            this.currun_keys_tested = 0;

            // Initialization of repetition data structures
            int[][] bestkeys = new int[this.repetitions][];
            double[] bestkeys_fit = new double[this.repetitions];

            // Execute analysis
            for (int curRep = 0; curRep < this.repetitions; curRep++)
            {
                if (this.stopFlag == true)
                {
                    return;
                }

                Population population = new Population();
                SetUpEnvironment(population, GeneticAttacker.population_size);

                CreateInitialGeneration(population, this.ciphertext, this.ciphertext_alphabet, this.plaintext_alphabet, null);

                double change = population.dev;
                int curGen = 1;
                Population nextGen = population;

                while ((change > GeneticAttacker.changeBorder) && (curGen < GeneticAttacker.maxGenerations))
                {
                    if (this.stopFlag == true)
                    {
                        return;
                    }

                    nextGen = CreateNextGeneration(nextGen, this.ciphertext, this.ciphertext_alphabet, false);
                    change = nextGen.dev;
                    curGen++;

                    this.pluginProgress(50.0 + (((double)(curRep + 1) * curGen) / (this.repetitions * GeneticAttacker.maxGenerations)) / 2 * 100, 100.0);
                }

                nextGen = CreateNextGeneration(nextGen, this.ciphertext, this.ciphertext_alphabet, true);
            
                this.plaintext = DecryptCiphertext(nextGen.keys[0], this.ciphertext, this.ciphertext_alphabet);
                
                bestkeys[curRep] = nextGen.keys[0];
                bestkeys_fit[curRep] = nextGen.fitness[0];

                Text plainTxt = DecryptCiphertext(bestkeys[curRep], this.ciphertext, this.ciphertext_alphabet);
                String plain = plainTxt.ToString(this.plaintext_alphabet);
                String key_string = CreateAlphabetOutput(bestkeys[curRep], this.plaintext_alphabet);

                // Report keyCan
                KeyCandidate newKeyCan = new KeyCandidate(bestkeys[curRep], bestkeys_fit[curRep], plain, key_string);
                newKeyCan.GenAttack = true;
                this.updateKeyDisplay(newKeyCan);
            }
        }

        #endregion

        #region AlgorithmMethods

        private void SetUpEnvironment(Population pop, int size)
        {
            // Initialize data structures
            this.best_key = new int[size];
            pop.keys = new int[size][];
            pop.fitness = new double[size];
       
            // Create probability array to choose crossover keys
            int s = 0;
            int[] quantElements = new int[size];
            for (int i = 0; i < size; i++)
            {
                s += i;
                quantElements[i] = size - i;
            }
            s += size;
            pop.prob = new int[s];
            int index = 0;
            for (int i = 0; i < quantElements.Length; i++)
            {
                for (int j = 0; j < quantElements[i]; j++)
                {
                    pop.prob[index] = i;
                    index++;
                }
            }
        }

        private void CreateInitialGeneration(Population pop, Text ciphertext, Alphabet cipher_alpha, Alphabet plain_alpha, Frequencies freq)
        {
            // Create initial population keys
            int[] newkey;
            int keylength = cipher_alpha.Length;
            
            // Create the other population keys at random
            for (int i = 0; i < pop.keys.Length; i++)
            {
                newkey = this.CreateInitialKeyRandom(keylength);
                while (this.banlist.Contains(newkey)){
                    newkey = this.CreateInitialKeyRandom(keylength);
                }
                this.banlist.Add(newkey);
                pop.keys[i] = newkey;
            }

            // Calculate fitness of population keys
            for (int i = 0; i < pop.keys.Length; i++)
            {
                pop.fitness[i] = this.language_frequencies.CalculateFitnessOfKey(DecryptCiphertext(pop.keys[i], ciphertext, cipher_alpha));
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
            Population next = new Population();
            

            next.prob = pop.prob;
            next.fitness = new double[pop.keys.Length];
            next.keys = new int[pop.keys.Length][];

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
    
                next.keys[i] = CombineKeys(pop.keys[p1], pop.fitness[p1], pop.keys[p2], pop.fitness[p2], this.ciphertext, this.ciphertext_alphabet);

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
                next.fitness[i] = this.language_frequencies.CalculateFitnessOfKey(DecryptCiphertext(next.keys[i],ciphertext, cipher_alpha));
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
                this.repetitions = 1;
            }
            else if ((textlength >= 100) && (textlength < 200))
            {
                this.repetitions = 2;
            }
            else if ((textlength >= 90) && (textlength < 100))
            {
                this.repetitions = 10;
            }
            else if ((textlength >= 70) && (textlength < 80))
            {
                this.repetitions = 20;
            }
            else if (textlength < 70)
            {
                this.repetitions = 30;
            }
        }

        #endregion

        #region Support Methods

        private int[] CreateInitialKeyRandom(int keylength)
        {
            Boolean vorhanden = false;
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
            this.currun_keys_tested++;
            int index = -1;
            Text plaintext = ciphertext.CopyTo();

            for (int i=0;i<ciphertext.Length;i++)
            {
                index = ciphertext.GetLetterAt(i);
                if (index >= 0)
                {
                    plaintext.ChangeLetterAt(i, key[index]);
                }
            }

            return plaintext;
        }

        public String DecryptCiphertext(int[] key)
        {
            Text plaintext = DecryptCiphertext(key, this.ciphertext, this.ciphertext_alphabet);
            return plaintext.ToString(this.plaintext_alphabet);
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
                p1.CopyTo(res,0);
                less_fit = p2;
                fitness = fit_p1;
            }
            else
            {
                p2.CopyTo(res,0);
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
                    new_fitness = this.language_frequencies.CalculateFitnessOfKey(plaintext);
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

        private String CreateAlphabetOutput(int[] key, Alphabet ciphertext_alphabet)
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