/*
   Copyright 2020 Nils Kopal <Nils.Kopal<at>CrypTool.org

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
using System.Runtime.CompilerServices;
using System.Text;

namespace CrypTool.Plugins.HomophonicSubstitutionAnalyzer
{
    public class HillClimber
    {
        private bool _stop = false;

        private HomophoneMapping[] _globalbestkey;
        private Text _globalbestplaintext;
        private double _globalbestkeycost;

        public event EventHandler<ProgressChangedEventArgs> Progress;
        public event EventHandler<NewBestValueEventArgs> NewBestValue;
        public Grams Grams { get; set; }
        public AnalyzerConfiguration AnalyzerConfiguration { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="keylength"></param>
        public HillClimber(AnalyzerConfiguration configuration)
        {
            AnalyzerConfiguration = configuration;
        }

        /// <summary>
        /// Attacks the homophone substitution using hillclimbing
        /// </summary>        
        public void Execute()
        {
            //0) initialize everything            
            Random random = new Random(Guid.NewGuid().GetHashCode());
            SimulatedAnnealing simulatedAnnealing = new SimulatedAnnealing(AnalyzerConfiguration.StartTemperature, AnalyzerConfiguration.Steps);
            KeyLetterDistributor keyLetterDistributor = new KeyLetterDistributor();
            HomophoneMapping[] bestkey = new HomophoneMapping[AnalyzerConfiguration.Keylength];
            double bestkeycost = double.MinValue;
            _globalbestkey = new HomophoneMapping[AnalyzerConfiguration.Keylength];
            _globalbestkeycost = double.MinValue;
            _stop = false;

            DateTime nextUpdateTime = DateTime.Now.AddSeconds(1);

            //1) generate start key
            List<int> numbers = new List<int>();
            keyLetterDistributor.Init(AnalyzerConfiguration.KeyLetterLimits);
            for (int i = 0; i < AnalyzerConfiguration.Keylength; i++)
            {
                numbers.Add(keyLetterDistributor.GetNextLetter());
            }
            if (AnalyzerConfiguration.UseNulls)
            {
                for (int i = 0; i < 2; i++)
                {
                    numbers.Add(Tools.MapIntoNumberSpace("#", AnalyzerConfiguration.PlaintextMapping)[0]); //we use the #-symbol as null
                }
            }
            keyLetterDistributor.Init(AnalyzerConfiguration.KeyLetterLimits);

            HomophoneMapping[] runkey = new HomophoneMapping[AnalyzerConfiguration.Keylength];
            for (int i = 0; i < AnalyzerConfiguration.Keylength; i++)
            {
                runkey[i] = new HomophoneMapping(AnalyzerConfiguration.Ciphertext, i, -1);
            }

            //3) apply locked homophones on generated key
            for (int i = 0; i < AnalyzerConfiguration.LockedHomophoneMappings.Length; i++)
            {
                if (AnalyzerConfiguration.LockedHomophoneMappings[i] != -1)
                {
                    numbers.Remove(AnalyzerConfiguration.LockedHomophoneMappings[i]);
                    runkey[i].PlainLetter = AnalyzerConfiguration.LockedHomophoneMappings[i];
                }
                else
                {
                    int rnd = random.Next(0, numbers.Count);
                    runkey[i].PlainLetter = numbers[rnd];
                    numbers.RemoveAt(rnd);
                }
            }

            int noglobalbestcounter = 0;
            int nullsymbol = AnalyzerConfiguration.UseNulls ? Tools.MapIntoNumberSpace("#", AnalyzerConfiguration.PlaintextMapping)[0] : -1;

            //3) do hillcimbing
            Text plaintext = DecryptHomophonicSubstitution(runkey);
            do
            {
                //3.1) permutate key                
                for (int i = 0; i < AnalyzerConfiguration.Keylength - 1; i++)
                {
                    for (int j = i + 1; j < AnalyzerConfiguration.Keylength; j++)
                    {
                        if (AnalyzerConfiguration.LockedHomophoneMappings[i] != -1 ||
                            AnalyzerConfiguration.LockedHomophoneMappings[j] != -1 ||
                            runkey[i].PlainLetter == runkey[j].PlainLetter)
                        {
                            //we don't change locked homophone mappings in the key
                            //we don't exchange plainletters if they are equal
                            continue;
                        }

                        // swap the i-th element with the j-th element
                        (runkey[i].PlainLetter, runkey[j].PlainLetter) = (runkey[j].PlainLetter, runkey[i].PlainLetter);

                        // decrypt the ciphertext inplace
                        DecryptHomophonicSubstitutionInPlace(plaintext, runkey, i, j);

                        // compute cost value to rate the key (fitness)
                        double costvalue = Grams.CalculateCost(plaintext.ToIntegerList(nullsymbol));

                        if (simulatedAnnealing.AcceptWithTemperature(costvalue, bestkeycost))
                        {
                            bestkeycost = costvalue;
                            bestkey = CreateDeepKeyCopy(runkey);
                        }
                        else
                        {
                            //revert the key to the old one by swapping
                            (runkey[i].PlainLetter, runkey[j].PlainLetter) = (runkey[j].PlainLetter, runkey[i].PlainLetter);
                            DecryptHomophonicSubstitutionInPlace(plaintext, runkey, i, j);
                        }
                    }

                    if (_stop)
                    {
                        break;
                    }
                }

                //3.2) Check, if we have a new global best one
                if (bestkeycost > _globalbestkeycost)
                {
                    _globalbestkeycost = bestkeycost;
                    _globalbestkey = CreateDeepKeyCopy(bestkey);
                    _globalbestplaintext = DecryptHomophonicSubstitution(_globalbestkey);

                    if (NewBestValue != null)
                    {
                        OnNewBestValue();
                    }
                    noglobalbestcounter = 0;
                }
                else
                {
                    noglobalbestcounter++;
                    if (noglobalbestcounter == 50)
                    {
                        runkey[runkey.Length - 1].PlainLetter = keyLetterDistributor.GetNextLetter();
                        runkey[runkey.Length - 2].PlainLetter = keyLetterDistributor.GetNextLetter();
                        runkey[runkey.Length - 3].PlainLetter = keyLetterDistributor.GetNextLetter();
                        noglobalbestcounter = 0;
                    }
                }

                //3.3) update progress in ui
                if (DateTime.Now > nextUpdateTime)
                {
                    if (Progress != null && !_stop)
                    {
                        Progress.Invoke(this, new ProgressChangedEventArgs() { Percentage = simulatedAnnealing.GetProgress() });
                    }
                    nextUpdateTime = DateTime.Now.AddSeconds(1);
                }

            } while (!_stop && simulatedAnnealing.IsHot());

            //set final progress to 1.0
            if (Progress != null && !_stop)
            {
                Progress.Invoke(this, new ProgressChangedEventArgs() { Percentage = 1, Terminated = true });
            }

            //force current value as output
            OnNewBestValue(true);
            _stop = true;
        }

        /// <summary>
        /// Notifies the component that we have a new global new best value
        /// </summary>
        /// <param name="forceOutput"></param>
        private void OnNewBestValue(bool forceOutput = false)
        {
            int[] globalbestplaintextNumbers = _globalbestplaintext.ToIntegerArray();
            string strplaintext = Tools.MapNumbersIntoTextSpace(globalbestplaintextNumbers, AnalyzerConfiguration.PlaintextMapping);
            string strPlaintextMapping = CreateKeyString(_globalbestkey, AnalyzerConfiguration.PlaintextMapping);
            string strciphertextalphabet = AnalyzerConfiguration.CiphertextAlphabet.Substring(0, _globalbestkey.Length);
            double costvalue = _globalbestkeycost;

            NewBestValue.Invoke(this,
                new NewBestValueEventArgs()
                {
                    Plaintext = strplaintext,
                    PlaintextAsNumbers = globalbestplaintextNumbers,
                    PlaintextMapping = strPlaintextMapping,
                    CiphertextAlphabet = strciphertextalphabet,
                    CostValue = costvalue,
                    ForceOutput = forceOutput
                });
        }

        /// <summary>
        /// Decrypts the homophone cipher using the given key
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Text DecryptHomophonicSubstitution(HomophoneMapping[] key)
        {
            Text plaintext = new Text();
            foreach (HomophoneMapping mapping in key)
            {
                foreach (int position in mapping.Positions)
                {
                    plaintext[position] = new int[] { mapping.PlainLetter };
                }
            }
            return plaintext;
        }

        /// <summary>
        /// Decrypts the homophones inplace
        /// </summary>
        /// <param name="plaintext"></param>
        /// <param name="key"></param>
        /// <param name="i"></param>
        /// <param name="j"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DecryptHomophonicSubstitutionInPlace(Text plaintext, HomophoneMapping[] key, int i, int j)
        {
            foreach (int position in key[i].Positions)
            {
                plaintext[position] = new int[] { key[i].PlainLetter };
            }
            foreach (int position in key[j].Positions)
            {
                plaintext[position] = new int[] { key[j].PlainLetter };
            }
        }

        /// <summary>
        /// Creates a deep copy of the given key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private HomophoneMapping[] CreateDeepKeyCopy(HomophoneMapping[] key)
        {
            if (key == null)
            {
                return null;
            }
            HomophoneMapping[] copy = new HomophoneMapping[key.Length];
            for (int i = 0; i < key.Length; i++)
            {
                copy[i] = (HomophoneMapping)key[i].Clone();
            }
            return copy;
        }

        /// <summary>
        /// Creates a string of the given key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="Alphabet"></param>
        /// <returns></returns>
        private string CreateKeyString(HomophoneMapping[] key, string Alphabet)
        {
            StringBuilder builder = new StringBuilder();
            foreach (HomophoneMapping mapping in key)
            {
                try
                {
                    builder.Append(Alphabet[mapping.PlainLetter]);
                }
                catch (IndexOutOfRangeException)
                {
                    //if the letter is not in alphabet, we just add the last 
                    builder.Append(Alphabet[Alphabet.Length - 1]);
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// Stops the analyzer
        /// </summary>
        public void Stop()
        {
            _stop = true;
        }
    }

    /// <summary>
    /// EventArgs for the progress change 
    /// </summary>
    public class ProgressChangedEventArgs : EventArgs
    {
        public bool Terminated { get; set; }
        public double Percentage { get; set; }

        public ProgressChangedEventArgs()
        {
            Terminated = false;
            Percentage = 0;
        }
    }

    /// <summary>
    /// EventArgs for a new "best value"
    /// </summary>
    public class NewBestValueEventArgs : EventArgs
    {
        public bool NewTopEntry { get; set; }
        public string Plaintext { get; set; }
        public int[] PlaintextAsNumbers { get; set; }
        public string PlaintextMapping { get; set; }
        public string CiphertextAlphabet { get; set; }
        public double CostValue { get; set; }
        public List<string> FoundWords { get; set; }
        public string SubstitutionKey { get; set; }
        public bool ForceOutput { get; set; }
    }

    /// <summary>
    /// EventArgs for a change of the user. This means, the user
    /// changed the plaintext mapping of a homophone
    /// </summary>
    public class UserChangedTextEventArgs : NewBestValueEventArgs
    {
    }

    /// <summary>
    /// A mapping of a plainletter to a ciphertext letter
    /// </summary>
    public class HomophoneMapping : ICloneable
    {
        public int CipherLetter;
        public int PlainLetter;
        public int[] Positions;

        /// <summary>
        /// Default constructor
        /// </summary>
        public HomophoneMapping()
        {

        }

        /// <summary>
        /// Creates a new HomophoneMapping and memorizes the position of the cipherletter
        /// in the given ciphertext
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <param name="cipherLetter"></param>
        /// <param name="plainLetter"></param>
        public HomophoneMapping(Text ciphertext, int cipherLetter, int plainLetter)
        {
            CipherLetter = cipherLetter;
            PlainLetter = plainLetter;
            List<int> positions = new List<int>();
            int length = ciphertext.GetSymbolsCount();
            for (int i = 0; i < length; i++)
            {
                if (ciphertext[i][0] == cipherLetter)
                {
                    positions.Add(i);
                }
            }
            Positions = positions.ToArray();
        }

        /// <summary>
        /// Creates a clone of this mapping
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            HomophoneMapping clone = new HomophoneMapping
            {
                CipherLetter = CipherLetter,
                PlainLetter = PlainLetter,
                Positions = (int[])Positions.Clone()
            };
            return clone;
        }
    }

    /// <summary>
    /// Min and max value LetterLimits for letters in the key
    /// </summary>
    public class LetterLimits
    {
        public int Letter { get; set; }
        public int MinValue { get; set; }
        public int MaxValue { get; set; }
    }

    /// <summary>
    /// The KeyLetterDistributor returns key letters for the creation of Random keys based on
    /// min and max values numbers of letters
    /// </summary>
    public class KeyLetterDistributor
    {
        private readonly Random _random = new Random();
        private int[] distribution;
        public List<LetterLimits> LetterLimits = new List<LetterLimits>();

        /// <summary>
        /// Default constructor
        /// </summary>
        public KeyLetterDistributor()
        {

        }

        /// <summary>
        /// Initializes the distributor using the given LetterLimits
        /// </summary>
        /// <param name="letterLimits"></param>
        public void Init(List<LetterLimits> letterLimits)
        {
            LetterLimits = letterLimits;
            distribution = new int[LetterLimits.Count];
        }

        /// <summary>
        /// Returns a letter based on the given LetterLimits
        /// </summary>
        /// <returns></returns>
        public int GetNextLetter()
        {
            //Step 1: return using min values
            int position = _random.Next(0, LetterLimits.Count);
            for (int i = 0; i < LetterLimits.Count; i++)
            {
                if (distribution[position] < LetterLimits[position].MinValue)
                {
                    int retvalue = LetterLimits[position].Letter;
                    distribution[position]++;
                    return retvalue;
                }
                position = (position + 1) % LetterLimits.Count;
            }

            //Step 2: return using max values
            position = _random.Next(0, LetterLimits.Count);
            for (int i = 0; i < LetterLimits.Count; i++)
            {
                if (distribution[position] < LetterLimits[position].MaxValue)
                {
                    int retvalue = LetterLimits[position].Letter;
                    distribution[position]++;
                    return retvalue;
                }
                position = (position + 1) % LetterLimits.Count;
            }

            //Step 3: We do not find any one, thus we just return a Random value
            position = _random.Next(0, LetterLimits.Count);
            int finalretvalue = LetterLimits[position].Letter;
            distribution[position]++;
            return finalretvalue;
        }
    }

    /// <summary>
    /// Configuration class containing all configuration parameters needed by the analyzer
    /// </summary>
    public class AnalyzerConfiguration
    {
        public AnalysisMode AnalysisMode { get; set; }
        public int Keylength { get; private set; }
        public string PlaintextMapping { get; set; }
        public string PlaintextAlphabet { get; set; }
        public string CiphertextAlphabet { get; set; }
        public int TextColumns { get; set; }
        public int Steps { get; set; }
        public int Restarts { get; set; }
        public int MinWordLength { get; set; }
        public int MaxWordLength { get; set; }        
        public int WordCountToFind { get; set; }
        public int NomenclatureElementsThreshold { get; set; }
        public List<LetterLimits> KeyLetterLimits { get; set; }
        public int[] LockedHomophoneMappings { get; set; }
        public Text Ciphertext { get; private set; }
        public double StartTemperature { get; set; }
        public char Separator { get; set; }
        public bool UseNulls { get; set; }
        public List<int> LinebreakPositions { get; set; }
        public bool KeepLinebreaks { get; set; }

        /// <summary>
        /// Creates a new AnalyzerConfiugraion using the given keylength and ciphertext
        /// </summary>
        /// <param name="keylength"></param>
        /// <param name="ciphertext"></param>
        public AnalyzerConfiguration(int keylength, Text ciphertext)
        {
            Keylength = keylength;
            Ciphertext = ciphertext;
            LockedHomophoneMappings = new int[keylength];
            KeyLetterLimits = new List<LetterLimits>();
            Separator = ' ';

            for (int i = 0; i < keylength; i++)
            {
                LockedHomophoneMappings[i] = -1;
            }

            for (int i = 0; i < Ciphertext.GetSymbolsCount(); i++)
            {
                Ciphertext[i] = new int[] { Ciphertext[i][0] % keylength };
            }
        }
    }
}
