using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrypTool.PluginBase.IO;
using CrypTool.PluginBase.Utils;

namespace CrypTool.AnalysisMonoalphabeticSubstitution
{
    class HillclimbingAttacker
    {
        #region Variables

        // Delegate
        private bool stopFlag;
        private PluginProgress pluginProgress;
        private UpdateKeyDisplay updateKeyDisplay;
        

        //Input
        private string ciphertextString = null;
        private string ciphertextalphabet = null;
        private string plaintextalphabet = null;
        private int restarts;
        public Grams grams; // GPU requires quadgrams

        //InplaceSymbols
        int[,] inplaceSpots;
        int[] inplaceAmountOfSymbols;

        //Output
        private long totalKeys;

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

        public string CiphertextAlphabet
        {
            get { return this.ciphertextalphabet; }
            set { this.ciphertextalphabet = value; }
        }

        public string PlaintextAlphabet
        {
            get { return this.plaintextalphabet; }
            set { this.plaintextalphabet = value; }
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


        public void ExecuteOnCPU()
        {
            #region{Variables}

            double globalbestkeycost = double.MinValue;
            int[] bestkey = new int[plaintextalphabet.Length];
            inplaceAmountOfSymbols = new int[plaintextalphabet.Length];
            int alphabetlength = plaintextalphabet.Length; //No need for calculating a few million times. (Performance)
            bool foundbetter;
            bool foundInplace = false;
            totalKeys = 0;
            Random random = new Random();

            #endregion{Variables}

            //Take input and prepare
            //int[] ciphertext = MapTextIntoNumberSpace(RemoveInvalidChars(ciphertextString.ToLower(), ciphertextalphabet), ciphertextalphabet);
            PluginBase.Utils.Alphabet ciphertextAlphabet = new PluginBase.Utils.Alphabet(ciphertextalphabet);
            PluginBase.Utils.Alphabet plaintextAlphabet = new PluginBase.Utils.Alphabet(plaintextalphabet);
            PluginBase.Utils.Text cipherText = new PluginBase.Utils.Text(ciphertextString, ciphertextAlphabet);
            int[] ciphertext = cipherText.ValidLetterArray;

            int length = ciphertext.Length;
            int[] plaintext = new int[length];
            inplaceSpots = new int[plaintextalphabet.Length, length];

            for (int restart = 0; restart < restarts; restart++)
            {
                pluginProgress(restart, restarts);

                //Generate random key:
                int[] runkey = BuildRandomKey(random);
                double bestkeycost = double.MinValue;

                //Create first plaintext and analyze places of symbols:
                plaintext = UseKeyOnCipher(ciphertext, runkey);
                AnalyzeSymbolPlaces(plaintext, length);

                do
                {
                    foundbetter = false;
                    for (int i = 0; i < alphabetlength; i++)
                    {
                        foundInplace = false;
                        int[] copykey = (int[])runkey.Clone();
                        for (int j = 0; j < alphabetlength; j++)
                        {
                            if (i == j) continue;

                            //create child key
                            Swap(copykey, i, j);

                            int sub1 = copykey[i];
                            int sub2 = copykey[j];

                            //Inplace swap in text
                            for (int m = 0; m < inplaceAmountOfSymbols[sub1]; m++)
                                plaintext[inplaceSpots[sub1, m]] = sub2;

                            for (int m = 0; m < inplaceAmountOfSymbols[sub2]; m++)
                                plaintext[inplaceSpots[sub2, m]] = sub1;

                            //Calculate the costfunction
                            double costvalue = grams.CalculateCost(plaintext);

                            if (bestkeycost < costvalue) //When found a better key adopt it.
                            {
                                bestkeycost = costvalue;
                                bestkey = (int[])copykey.Clone();
                                foundbetter = true;
                                foundInplace = true;
                            }

                            //Revert the CopyKey substitution
                            Swap(copykey, i, j);

                            for (int m = 0; m < inplaceAmountOfSymbols[sub2]; m++)
                                plaintext[inplaceSpots[sub2, m]] = sub2;

                            for (int m = 0; m < inplaceAmountOfSymbols[sub1]; m++)
                                plaintext[inplaceSpots[sub1, m]] = sub1;

                            totalKeys++; //Count Keys for Performance output
                        }

                        //Fast converge take over new key + therefore new resulting plaintext
                        if (foundInplace)
                        {
                            runkey = bestkey;
                            plaintext = UseKeyOnCipher(ciphertext, runkey);
                            AnalyzeSymbolPlaces(plaintext, length);
                        }
                    }
                } while (foundbetter);

                if (StopFlag) return;

                if (globalbestkeycost < bestkeycost)
                {
                    globalbestkeycost = bestkeycost;
                    //Add to bestlist-output:
                    string keystring = MapNumbersIntoTextSpace(bestkey, plaintextalphabet);
                    if (keystring.Contains("ABIRD"))
                    {
                        //Console.WriteLine(keystring);
                    }
                    KeyCandidate newKeyCan = new KeyCandidate(bestkey, bestkeycost, MapNumbersIntoTextSpace(UseKeyOnCipher(ciphertext, bestkey), plaintextalphabet), keystring);
                    newKeyCan.HillAttack = true;
                    updateKeyDisplay(newKeyCan);
                }
            }
        }

        #region Methods & Functions

        private int[] BuildRandomKey(Random randomdev)
        {
            List<int> list = new List<int>();
            for (int i = 0; i < plaintextalphabet.Length; i++) list.Add(i);

            int[] key = new int[plaintextalphabet.Length];

            for (int i = (plaintextalphabet.Length - 1); i >= 0; i--)
            {
                int random = randomdev.Next(0, i + 1);
                key[i] = list[random];
                list.RemoveAt(random);
            }

            return key;
        }

        private void Swap(int[] key, int i, int j)
        {
            int tmp = key[i];
            key[i] = key[j];
            key[j] = tmp;
        }

        private int[] UseKeyOnCipher(int[] ciphertext, int[] key)
        {
            int[] plaintext = new int[ciphertext.Length];

            for (int i = 0; i < ciphertext.Length; i++)
            {
                if (ciphertext[i] < key.Length)
                {
                    plaintext[i] = key[ciphertext[i]];
                }
                else
                {
                    plaintext[i] = key[key.Length - 1];
                }
            }

            return plaintext;
        }

        /// <summary>
        /// Maps a given array of numbers into the "textspace" defined by the alphabet
        /// </summary>
        /// <param name="numbers"></param>
        /// <param name="alphabet"></param>
        /// <returns></returns>
        public static string MapNumbersIntoTextSpace(int[] numbers, string alphabet)
        {
            var builder = new StringBuilder();
            foreach (var i in numbers)
            {
                builder.Append(alphabet[i]);
            }
            return builder.ToString();
        }

        private void AnalyzeSymbolPlaces(int[] text, int length)
        {
            for (int i = 0; i < plaintextalphabet.Length; i++)
                inplaceAmountOfSymbols[i] = 0;

            for (int i = 0; i < length; i++)
            {
                int p = text[i];
                if (p < 0 || p > length)
                {
                    //Console.WriteLine("Error: illegal symbol found");
                    break;
                }
                inplaceSpots[p, inplaceAmountOfSymbols[p]++] = i;
            }
        }

        #endregion Methods & Functions
    }
}