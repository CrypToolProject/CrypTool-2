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
using System.Text;

namespace CrypTool.AnalysisMonoalphabeticSubstitution
{
    /**
     *  Dictionary attacker is based on the Decrypto project and "Robust Dictionary Attack of Short Simple
     *  Substitution Ciphers" of Edwin Olson
     **/

    internal delegate int DetermineNextSubstitution(bool[] solvedWords, Mapping map, out int index, SolveData data);

    internal class DictionaryAttacker
    {
        #region Variables

        // Stop Flag
        private bool stopFlag;

        // Input Variables
        private Dictionary lDic;
        private Grams grams;
        private Text ctext;
        private Alphabet calpha;
        private Alphabet palpha;

        // Output Variables
        private bool completeKey = false;
        private bool partialKey = false;
        private UpdateKeyDisplay updateKeyDisplay;

        // Working Variables
        private readonly List<int[]> result;
        private readonly List<Word> words;
        private byte[] historder;
        private int[] histogram;
        private readonly Random random;
        private DetermineNextSubstitution DetermineNextSubstitution;
        private PluginProgress pluginProgress;
        private int currentProgress;
        private int maxProgressIt;

        // Parameter dictionary attack
        private const int maxIterations = 100;
        private const int maxRandomIterations = 300;
        private bool pruneRootNode = true;
        private bool pruneEachNode = true;
        private bool scrambleWordOrder = false;
        private const int maxReduceIterations = 50;

        #endregion

        #region Constructor

        public DictionaryAttacker()
        {
            words = new List<Word>();
            result = new List<int[]>();
            random = new Random();
        }

        #endregion

        #region Properties

        public Dictionary languageDictionary
        {
            get => lDic;
            set => lDic = value;
        }

        public Grams Grams
        {
            get => grams;
            set => grams = value;
        }

        public Text ciphertext
        {
            get => ctext;
            set => ctext = value;
        }

        public Alphabet ciphertext_alphabet
        {
            get => calpha;
            set => calpha = value;
        }

        public Alphabet plaintext_alphabet
        {
            get => palpha;
            set => palpha = value;
        }

        public int NumberOfWords
        {
            get => words.Count;
            set {; }
        }

        public bool CompleteKey
        {
            get => completeKey;
            set {; }
        }

        public bool PartialKey
        {
            get => partialKey;
            set {; }
        }

        public PluginProgress PluginProgressCallback
        {
            get => pluginProgress;
            set => pluginProgress = value;
        }

        public UpdateKeyDisplay UpdateKeyDisplay
        {
            get => updateKeyDisplay;
            set => updateKeyDisplay = value;
        }

        public bool StopFlag
        {
            get => stopFlag;
            set => stopFlag = value;
        }

        #endregion

        #region Methods

        public void PrepareAttack()
        {
            // Test input
            if (calpha == null || ctext == null || grams == null || lDic == null)
            {
                return;
            }

            List<byte[]> text_in_words = ctext.ToSingleWords(calpha);

            // Add only unique words
            WordComparer word_comparer = new WordComparer();
            for (int i = 0; i < text_in_words.Count; i++)
            {
                Word w = new Word(i, text_in_words[i]);
                bool vorhanden = false;
                for (int j = 0; j < words.Count; j++)
                {
                    if (word_comparer.Compare(w, words[j]) == 0)
                    {
                        vorhanden = true;
                        break;
                    }
                }

                if (!vorhanden)
                {
                    words.Add(w);
                }

                if (StopFlag)
                {
                    return;
                }
            }

            // Look up words with same pattern in dictionary
            foreach (Word w in words)
            {
                w.Candidates = GetCandidates(w);
                if (w.Candidates == null || w.Candidates.Length == 0)
                {
                    w.Enabled = false;
                }
            }

            // Generate order of ciphertext letters
            int[] letter_frequencies = new int[calpha.GetAlphabetQuantity()];

            foreach (Word w in words)
            {
                for (int i = 0; i < w.ByteValue.Length; i++)
                {
                    if (w.ByteValue[i] < letter_frequencies.Length)
                    {
                        letter_frequencies[w.ByteValue[i]]++;
                    }
                }
            }
            histogram = letter_frequencies;

            historder = new byte[letter_frequencies.Length];
            for (int i = 0; i < historder.Length; i++)
            {
                historder[i] = (byte)i;
            }

            for (int i = 0; i < letter_frequencies.Length; i++)
            {
                for (int j = i + 1; j < letter_frequencies.Length; j++)
                {
                    if (letter_frequencies[j] > letter_frequencies[i])
                    {
                        int helper = letter_frequencies[i];
                        letter_frequencies[i] = letter_frequencies[j];
                        letter_frequencies[j] = helper;
                        helper = historder[i];
                        historder[i] = historder[j];
                        historder[j] = (byte)helper;
                    }
                }
            }

            // Calculate max progress
            int nr = words.Count;
            int v1 = 0;
            int v2 = 0;
            if (nr < 200)
            {
                for (int i = 0; i < nr; i++)
                {
                    if (nr < 70)
                    {
                        for (int j = i + 1; j < nr; j++)
                        {
                            if (nr < 30)
                            {
                                for (int t = j + 1; t < nr; t++)
                                {
                                    v2++;
                                }
                            }
                            v1++;
                        }
                    }
                }
            }
            maxProgressIt = (1 + nr + v1 + v2 + 300);
        }

        public void SolveDeterministicFull()
        {
            pruneEachNode = true;
            pruneRootNode = true;
            scrambleWordOrder = false;

            DetermineNextSubstitution = DetermineNextSubstitutionDeterministic;

            ReenableWords();
            Solve();

            // Set progress
            currentProgress++;
            double state = ((double)currentProgress) / maxProgressIt;
            PluginProgressCallback(state, 200.0);
        }

        public void SolveDeterministicWithDisabledWords()
        {
            pruneEachNode = true;
            pruneRootNode = true;
            scrambleWordOrder = false;

            DetermineNextSubstitution = DetermineNextSubstitutionDeterministic;
            // Max 3 disabled words
            // Disable all combinations of 1 word
            if (words.Count >= 2 && words.Count < 200)
            {
                for (int i = 0; i < words.Count; i++)
                {
                    if (StopFlag)
                    {
                        break;
                    }

                    ReenableWords();
                    words[i].Enabled = false;
                    Solve();

                    // Set progress
                    currentProgress++;
                    double state = (((double)currentProgress) / maxProgressIt) * 100;
                    PluginProgressCallback(state, 200.0);
                }
            }
            // Disable all combinations of 2 words
            if (words.Count >= 3 && words.Count < 70)
            {
                for (int i = 0; i < words.Count; i++)
                {
                    if (StopFlag)
                    {
                        break;
                    }

                    for (int j = i + 1; j < words.Count; j++)
                    {
                        ReenableWords();
                        words[i].Enabled = false;
                        words[j].Enabled = false;
                        Solve();

                        // Set progress
                        currentProgress++;
                        double state = (((double)currentProgress) / maxProgressIt) * 100;
                        PluginProgressCallback(state, 200.0);
                    }
                }
            }
            // Disable all combinations of 3 words
            if (words.Count >= 4 && words.Count < 30)
            {
                for (int i = 0; i < words.Count; i++)
                {
                    if (StopFlag)
                    {
                        break;
                    }

                    for (int j = i + 1; j < words.Count; j++)
                    {
                        for (int x = j + 1; x < words.Count; x++)
                        {
                            ReenableWords();
                            words[i].Enabled = false;
                            words[j].Enabled = false;
                            words[x].Enabled = false;
                            Solve();

                            // Set progress
                            currentProgress++;
                            double state = (((double)currentProgress) / maxProgressIt) * 100;
                            PluginProgressCallback(state, 200.0);
                        }
                    }
                }
            }
        }

        public void SolveRandomized()
        {
            pruneEachNode = false;
            pruneRootNode = false;
            scrambleWordOrder = true;

            DetermineNextSubstitution = DetermineNextSubstituationRandomly;

            for (int i = 0; i < DictionaryAttacker.maxRandomIterations; i++)
            {
                if (StopFlag)
                {
                    break;
                }

                Solve();

                // Set progress
                currentProgress++;
                double state = (((double)currentProgress) / maxProgressIt) * 100;
                PluginProgressCallback(state, 200.0);
            }
        }

        private void Solve()
        {
            //First Mapping
            Mapping startMap = new Mapping(calpha.GetAlphabetQuantity());
            startMap.SetFull();

            // Prepare solution base data
            SolveData basis = new SolveData
            {
                solvedWords = new bool[words.Count],
                numSolvedWords = 0,
                mapping = startMap,
                firstcand = new int[words.Count]
            };
            for (int i = 0; i < words.Count; i++)
            {
                basis.firstcand[i] = 0;
            }
            Stack<SolveData> stack = new Stack<SolveData>();
            stack.Push(basis);

            // Reduce root node
            if (pruneRootNode)
            {
                bool inconsistent = ReduceCandidates(basis.mapping, basis.solvedWords, basis);
                if (inconsistent)
                {
                    basis.mapping = startMap.Copy();
                }
            }

            int rounds = 0;
            int action;

            do
            {
                if (StopFlag)
                {
                    break;
                }

                rounds++;

                //// Store new data object to stack
                SolveData data = stack.Peek().Copy();

                //// Break if all words are solved
                if (data.numSolvedWords == words.Count)
                {
                    AddKeyToResult(data.mapping, true);
                    break;
                }

                // Define next action
                action = DetermineNextSubstitution(data.solvedWords, data.mapping, out int index, data);
                if (action == -1)
                {
                    AddKeyToResult(data.mapping, true);
                    break;
                }
                else if (action == -2)
                {
                    AddKeyToResult(data.mapping, false);
                    break;
                }
                else if (action == 1)
                {
                    //// Implement action - Word
                    Word w = words[index];
                    data.solvedWords[index] = true;
                    data.numSolvedWords++;

                    if (!w.Enabled)
                    {
                        continue;
                    }

                    Mapping helper = data.mapping.Copy();
                    bool leaf = true;

                    if (scrambleWordOrder)
                    {
                        for (int c = data.firstcand[index]; c < w.Candidates.Length; c++)
                        {
                            int d = random.Next(w.Candidates.Length - data.firstcand[index]) + data.firstcand[index];
                            Candidate cc = w.Candidates[c];
                            Candidate cd = w.Candidates[d];
                            w.Candidates[d] = cc;
                            w.Candidates[c] = cd;
                        }
                    }

                    for (int c = data.firstcand[index]; c < w.Candidates.Length; c++)
                    {
                        Candidate candidate = w.Candidates[c];

                        if (!helper.IsMappingOK(w.ByteValue, candidate.ByteValue))
                        {
                            continue;
                        }

                        leaf = false;

                        helper.SetMapping(w.ByteValue, candidate.ByteValue);

                        bool inconsistent = false;
                        if (pruneEachNode)
                        {
                            inconsistent = ReduceCandidates(helper, data.solvedWords, data);
                        }

                        if (!inconsistent)
                        {
                            data.mapping = helper;
                            data.firstcand[index] = c;
                            stack.Push(data);
                            leaf = false;
                            break;
                        }

                        helper.SetTo(data.mapping);
                    }

                    if (leaf)
                    {
                        AddKeyToResult(helper, false);
                        stack.Pop();
                    }
                }
                else if (action == 2)
                {
                    /// Implement action - letter
                    byte[] a = new byte[1];
                    byte[] b = new byte[1];
                    a[0] = (byte)(index);

                    Mapping helper = data.mapping.Copy();
                    bool leaf = true;

                    for (int c = 0; c < calpha.GetAlphabetQuantity(); c++)
                    {
                        b[0] = (byte)c;

                        if (helper.IsMappingOK(a, b))
                        {
                            helper.SetMapping(a, b);

                            bool inconsistent = ReduceCandidates(helper, data.solvedWords, data);

                            if (!inconsistent)
                            {
                                data.mapping = helper;
                                leaf = false;
                                break;
                            }

                            helper.SetTo(data.mapping);
                        }
                    }

                    if (leaf)
                    {
                        AddKeyToResult(data.mapping, false);
                        stack.Pop();
                    }
                }
            } while (rounds < DictionaryAttacker.maxIterations);
        }

        #endregion

        #region Helper Methods

        private Candidate[] GetCandidates(Word w)
        {
            List<byte[]> candidates = lDic.GetWordsFromPattern(w.Pattern);
            Candidate[] res = new Candidate[candidates.Count];

            for (int i = 0; i < res.Length; i++)
            {
                res[i] = new Candidate(candidates[i], CalculateWordCandidateFitness(candidates[i]));
            }

            return res;
        }

        private double CalculateWordCandidateFitness(byte[] candidate)
        {
            double fitness = 0.0;

            if (grams.GramSize() == 3)
            {
                if (candidate.Length == 1)
                {
                    int count = 0;
                    for (int i = 0; i < grams.GramSize(); i++)
                    {
                        for (int j = 0; j < grams.GramSize(); j++)
                        {
                            fitness += grams.CalculateCost(new int[] { candidate[0], i, j });
                            count++;
                        }
                    }
                    fitness = fitness / count;
                }
                else if (candidate.Length == 2)
                {
                    int count = 0;
                    for (int i = 0; i < grams.GramSize(); i++)
                    {
                        fitness += grams.CalculateCost(new int[] { candidate[0], candidate[1], i });
                        count++;
                    }
                    fitness = fitness / count;
                }
                else
                {
                    int l1 = candidate[0];
                    int l2 = candidate[1];
                    int l3 = candidate[2];

                    for (int i = 3; i < candidate.Length; i++)
                    {
                        fitness += grams.CalculateCost(new int[] { l1, l2, l3 });

                        l1 = l2;
                        l2 = l3;
                        l3 = candidate[i];
                    }
                }
            }
            else if (grams.GramSize() == 4)
            {
                if (candidate.Length == 1)
                {
                    int count = 0;
                    for (int i = 0; i < grams.GramSize(); i++)
                    {
                        for (int j = 0; j < grams.GramSize(); j++)
                        {
                            for (int t = 0; t < grams.GramSize(); t++)
                            {
                                fitness += grams.CalculateCost(new int[] { candidate[0], i, j, t });
                                count++;
                            }
                        }
                    }
                    fitness = fitness / count;
                }
                else if (candidate.Length == 2)
                {
                    int count = 0;
                    for (int i = 0; i < grams.GramSize(); i++)
                    {
                        for (int j = 0; j < grams.GramSize(); j++)
                        {
                            fitness += grams.CalculateCost(new int[] { candidate[0], candidate[1], i, j });
                            count++;
                        }
                    }
                    fitness = fitness / count;
                }
                else if (candidate.Length == 3)
                {
                    int count = 0;
                    for (int i = 0; i < grams.GramSize(); i++)
                    {
                        fitness += grams.CalculateCost(new int[] { candidate[0], candidate[1], candidate[2], i });
                        count++;
                    }
                    fitness = fitness / count;
                }
                else
                {
                    int l1 = candidate[0];
                    int l2 = candidate[1];
                    int l3 = candidate[2];
                    int l4 = candidate[3];

                    for (int i = 4; i < candidate.Length; i++)
                    {
                        fitness += grams.CalculateCost(new int[] { l1, l2, l3, l4 });

                        l1 = l2;
                        l2 = l3;
                        l3 = l4;
                        l4 = candidate[i];
                    }
                }
            }

            return fitness;
        }

        private void ReenableWords()
        {
            foreach (Word w in words)
            {
                if (w.Candidates.Length > 0)
                {
                    w.Enabled = true;
                }
            }
        }

        private Text DecryptCiphertext(int[] key, Text ciphertext, Alphabet ciphertext_alphabet)
        {
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

        private void AddKeyToResult(Mapping map, bool fullSolution)
        {
            if (fullSolution)
            {
                completeKey = true;
            }
            else
            {
                partialKey = true;
            }

            int[] key = MakeKeyComplete(map.DeriveKey());
            Text plaintext = DecryptCiphertext(key, ctext, calpha);
            string plain = plaintext.ToString(palpha);
            double fit = grams.CalculateCost(plaintext.ToIntArray());
            string key_string = CreateAlphabetOutput(key, palpha);
            KeyCandidate keyCan = new KeyCandidate(key, fit, plain, key_string)
            {
                DicAttack = true
            };
            updateKeyDisplay(keyCan);
        }

        private int[] MakeKeyComplete(int[] key)
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


            return key;
        }

        private int DetermineNextSubstituationRandomly(bool[] solvedWords, Mapping map, out int index, SolveData data)
        {
            int action = -1;

            int i = 0;
            while (i < solvedWords.Length * 10)
            {
                int cur_index = random.Next(solvedWords.Length);

                if (!(solvedWords[cur_index] || !words[cur_index].Enabled))
                {
                    index = cur_index;
                    return 1;
                }
                i++;
            }
            index = 0;
            return action;
        }

        private int DetermineNextSubstitutionDeterministic(bool[] solvedWords, Mapping map, out int index, SolveData data)
        {
            int score = int.MaxValue;
            int word = -1;

            for (int i = 0; i < solvedWords.Length; i++)
            {
                Word w = words[i];
                if (solvedWords[i] || !w.Enabled)
                {
                    continue;
                }

                int cur_score = (w.Candidates.Length - data.firstcand[i]);

                if (cur_score == 0)
                {
                    index = 0;
                    return -2;
                }

                if (cur_score < score)
                {
                    score = cur_score;
                    word = i;
                }
            }

            if (score > calpha.GetAlphabetQuantity())
            {
                for (int i = 0; i < historder.Length; i++)
                {
                    if (map.IsUniquelyMapped(historder[i]))
                    {
                        continue;
                    }
                    byte letter = historder[i];

                    if (histogram[letter] == 0)
                    {
                        continue;
                    }

                    index = letter;
                    return 2;
                }
            }

            index = word;
            if (index == -1)
            {
                return -2;
            }

            return 1;
        }

        private bool ReduceCandidates(Mapping set, bool[] solvedWords, SolveData data)
        {
            Mapping helper = new Mapping(calpha.GetAlphabetQuantity());

            int rounds = 0;

            bool dirty = true;

            while (dirty && rounds < DictionaryAttacker.maxReduceIterations)
            {
                dirty = false;

                for (int wordnum = 0; wordnum < words.Count; wordnum++)
                {
                    Word w = words[wordnum];
                    if (solvedWords[wordnum] || !w.Enabled)
                    {
                        continue;
                    }

                    helper.SetEmpty();

                    for (int c = data.firstcand[wordnum]; c < w.Candidates.Length; c++)
                    {
                        Candidate candidate = w.Candidates[c];
                        if (set.IsMappingOK(w.ByteValue, candidate.ByteValue))
                        {
                            helper.EnableMapping(w.ByteValue, candidate.ByteValue);
                        }
                        else
                        {
                            Candidate tmp = w.Candidates[data.firstcand[wordnum]];
                            w.Candidates[data.firstcand[wordnum]] = candidate;
                            w.Candidates[c] = tmp;
                            data.firstcand[wordnum]++;

                            dirty = true;
                        }
                    }

                    set.IntersectWith(helper);
                }

                long mappedTo = 0;
                for (int i = 0; i < calpha.GetAlphabetQuantity(); i++)
                {
                    if (!set.HasMapping((byte)i))
                    {
                        return true;
                    }

                    int mi = set.GetMapping((byte)i);
                    if (mi >= 0)
                    {
                        if ((mappedTo & (1 << mi)) > 0)
                        {
                            return true;
                        }
                        mappedTo |= (1 << mi);
                    }
                }

                rounds++;
            }

            return false;
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

    internal class SolveData
    {
        public Mapping mapping;
        public int numSolvedWords;
        public bool[] solvedWords;
        public int[] firstcand;

        public SolveData Copy()
        {
            SolveData res = new SolveData
            {
                mapping = mapping.Copy(),
                numSolvedWords = numSolvedWords,

                solvedWords = new bool[solvedWords.Length]
            };
            solvedWords.CopyTo(res.solvedWords, 0);

            res.firstcand = new int[firstcand.Length];
            firstcand.CopyTo(res.firstcand, 0);

            return res;
        }
    }
}
