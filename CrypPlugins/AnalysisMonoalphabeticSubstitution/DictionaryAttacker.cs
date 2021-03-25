using CrypTool.PluginBase.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrypTool.AnalysisMonoalphabeticSubstitution
{
    /**
     *  Dictionary attacker is based on the Decrypto project and "Robust Dictionary Attack of Short Simple
     *  Substitution Ciphers" of Edwin Olson
     **/

    delegate int DetermineNextSubstitution(bool[] solvedWords, Mapping map, out int index, SolveData data);

    class DictionaryAttacker
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
        private List<int[]> result;
        private List<Word> words;
        private byte[] historder;
        private int[] histogram;
        private Random random;
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
            this.words = new List<Word>();
            this.result = new List<int[]>();
            this.random = new Random();
        }

        #endregion

        #region Properties

        public Dictionary languageDictionary
        {
            get { return this.lDic; }
            set { this.lDic = value; }
        }

        public Grams Grams
        {
            get { return this.grams; }
            set { this.grams = value; }
        }

        public Text ciphertext
        {
            get { return this.ctext; }
            set { this.ctext = value; }
        }

        public Alphabet ciphertext_alphabet
        {
            get { return this.calpha; }
            set { this.calpha = value; }
        }

        public Alphabet plaintext_alphabet
        {
            get { return this.palpha; }
            set { this.palpha = value; }
        }

        public int NumberOfWords
        {
            get { return this.words.Count; }
            set { ; }
        }

        public bool CompleteKey
        {
            get { return this.completeKey; }
            set { ; }
        }

        public bool PartialKey
        {
            get { return this.partialKey; }
            set { ; }
        }

        public PluginProgress PluginProgressCallback
        {
            get { return this.pluginProgress; }
            set { this.pluginProgress = value; }
        }

        public UpdateKeyDisplay UpdateKeyDisplay
        {
            get { return this.updateKeyDisplay; }
            set { this.updateKeyDisplay = value; }
        }

        public Boolean StopFlag
        {
            get { return this.stopFlag; }
            set { this.stopFlag = value; }
        }

        #endregion

        #region Methods

        public void PrepareAttack()
        {
            // Test input
            if (this.calpha == null || this.ctext == null || this.grams == null || this.lDic == null)
            {
                return;
            }

            List<Byte[]> text_in_words = this.ctext.ToSingleWords(this.calpha);

            // Add only unique words
            WordComparer word_comparer = new WordComparer();
            for (int i = 0; i < text_in_words.Count; i++)
            {
                Word w = new Word(i, text_in_words[i]);
                bool vorhanden = false;
                for (int j = 0; j < this.words.Count; j++)
                {
                    if (word_comparer.Compare(w, this.words[j]) == 0)
                    {
                        vorhanden = true;
                        break;
                    }
                }

                if (!vorhanden)
                    this.words.Add(w);

                if (StopFlag) return;
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
            int[] letter_frequencies = new int[this.calpha.GetAlphabetQuantity()];

            foreach (Word w in this.words)
            {
                for (int i = 0; i < w.ByteValue.Length; i++)
                {
                    if (w.ByteValue[i] < letter_frequencies.Length)
                    {
                        letter_frequencies[w.ByteValue[i]]++;
                    }
                }
            }
            this.histogram = letter_frequencies;

            this.historder = new byte[letter_frequencies.Length];
            for (int i = 0; i < this.historder.Length; i++)
            {
                this.historder[i] = (byte)i;
            }

            for (int i = 0; i < letter_frequencies.Length; i++)
            {
                for (int j=i+1;j<letter_frequencies.Length;j++)
                {
                    if (letter_frequencies[j] > letter_frequencies[i])
                    {
                        int helper = letter_frequencies[i];
                        letter_frequencies[i] = letter_frequencies[j];
                        letter_frequencies[j] = helper;
                        helper = this.historder[i];
                        this.historder[i] = this.historder[j];
                        this.historder[j] = (byte)helper;
                    }
                }
            }

            // Calculate max progress
            int nr = this.words.Count;
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
            this.maxProgressIt = (1 + nr + v1 + v2 + 300); 
        }

        public void SolveDeterministicFull()
        {
            this.pruneEachNode = true;
            this.pruneRootNode = true;
            this.scrambleWordOrder = false;

            this.DetermineNextSubstitution = this.DetermineNextSubstitutionDeterministic;

            this.ReenableWords();
            this.Solve();

            // Set progress
            this.currentProgress++;
            double state = ((double)this.currentProgress) / this.maxProgressIt;
            this.PluginProgressCallback(state, 200.0);
        }

        public void SolveDeterministicWithDisabledWords()
        {
            this.pruneEachNode = true;
            this.pruneRootNode = true;
            this.scrambleWordOrder = false;

            this.DetermineNextSubstitution = this.DetermineNextSubstitutionDeterministic;
            // Max 3 disabled words
            // Disable all combinations of 1 word
            if (this.words.Count >= 2 && this.words.Count < 200)
            {
                for (int i = 0; i < this.words.Count; i++)
                {
                    if (StopFlag) break;

                    this.ReenableWords();
                    this.words[i].Enabled = false;
                    this.Solve();

                    // Set progress
                    this.currentProgress++;
                    double state = (((double)this.currentProgress) / this.maxProgressIt)*100;
                    this.PluginProgressCallback(state, 200.0);
                }
            }
            // Disable all combinations of 2 words
            if (this.words.Count >= 3 && this.words.Count < 70)
            {
                for (int i = 0; i < this.words.Count; i++)
                {
                    if (StopFlag) break;

                    for (int j = i + 1; j < this.words.Count; j++)
                    {
                        this.ReenableWords();
                        this.words[i].Enabled = false;
                        this.words[j].Enabled = false;
                        this.Solve();

                        // Set progress
                        this.currentProgress++;
                        double state = (((double)this.currentProgress) / this.maxProgressIt) * 100;
                        this.PluginProgressCallback(state, 200.0);
                    }
                }
            }
            // Disable all combinations of 3 words
            if (this.words.Count >= 4 && this.words.Count < 30)
            {
                for (int i = 0; i < this.words.Count; i++)
                {
                    if (StopFlag) break;

                    for (int j = i + 1; j < this.words.Count; j++)
                    {
                        for (int x = j + 1; x < this.words.Count; x++)
                        {
                            this.ReenableWords();
                            this.words[i].Enabled = false;
                            this.words[j].Enabled = false;
                            this.words[x].Enabled = false;
                            this.Solve();

                            // Set progress
                            this.currentProgress++;
                            double state = (((double)this.currentProgress) / this.maxProgressIt) * 100;
                            this.PluginProgressCallback(state, 200.0);
                        }
                    }
                }
            }
        }

        public void SolveRandomized()
        {
            this.pruneEachNode = false;
            this.pruneRootNode = false;
            this.scrambleWordOrder = true;

            this.DetermineNextSubstitution = this.DetermineNextSubstituationRandomly;

            for (int i = 0; i < DictionaryAttacker.maxRandomIterations; i++)
            {
                if (StopFlag) break;

                this.Solve();

                // Set progress
                this.currentProgress++;
                double state = (((double)this.currentProgress) / this.maxProgressIt) * 100;
                this.PluginProgressCallback(state, 200.0);
            }
        }

        private void Solve()
        {
            //First Mapping
            Mapping startMap = new Mapping(this.calpha.GetAlphabetQuantity());
            startMap.SetFull();

            // Prepare solution base data
            SolveData basis = new SolveData();
            basis.solvedWords = new bool[this.words.Count];
            basis.numSolvedWords = 0;
            basis.mapping = startMap;
            basis.firstcand = new int[this.words.Count];
            for (int i = 0; i < this.words.Count; i++)
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
            int index;

            do
            {
                if (StopFlag) break;

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
                action = this.DetermineNextSubstitution(data.solvedWords, data.mapping, out index, data);
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
                            int d = this.random.Next(w.Candidates.Length - data.firstcand[index]) + data.firstcand[index];
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
                            inconsistent = ReduceCandidates(helper,data.solvedWords, data);
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

                    for (int c = 0; c < this.calpha.GetAlphabetQuantity(); c++)
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

        private Double CalculateWordCandidateFitness(byte[] candidate)
        {
            double fitness = 0.0;

            if (this.grams.GramSize() == 3)
            {
                if (candidate.Length == 1)
                {
                    int count = 0;
                    for (int i = 0; i < this.grams.GramSize(); i++)
                    {
                        for (int j = 0; j < this.grams.GramSize(); j++)
                        {
                            fitness += this.grams.CalculateCost(new int[] { candidate[0], i, j });
                            count++;
                        }
                    }
                    fitness = fitness / count;
                }
                else if (candidate.Length == 2)
                {
                    int count = 0;
                    for (int i = 0; i < this.grams.GramSize(); i++)
                    {
                        fitness += this.grams.CalculateCost(new int[] { candidate[0], candidate[1], i });
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
                        fitness += this.grams.CalculateCost(new int[] { l1, l2, l3 });

                        l1 = l2;
                        l2 = l3;
                        l3 = candidate[i];
                    }
                }
            }
            else if (this.grams.GramSize() == 4)
            {
                if (candidate.Length == 1)
                {
                    int count = 0;
                    for (int i = 0; i < this.grams.GramSize(); i++)
                    {
                        for (int j = 0; j < this.grams.GramSize(); j++)
                        {
                            for (int t = 0; t < this.grams.GramSize(); t++)
                            {
                                fitness += this.grams.CalculateCost(new int[] { candidate[0], i, j, t });
                                count++;
                            }
                        }
                    }
                    fitness = fitness / count;
                }
                else if (candidate.Length == 2)
                {
                    int count = 0;
                    for (int i = 0; i < this.grams.GramSize(); i++)
                    {
                        for (int j = 0; j < this.grams.GramSize(); j++)
                        {
                            fitness += this.grams.CalculateCost(new int[] { candidate[0], candidate[1], i, j });
                            count++;
                        }
                    }
                    fitness = fitness / count;
                }
                else if (candidate.Length == 3)
                {
                    int count = 0;
                    for (int i = 0; i < this.grams.GramSize(); i++)
                    {
                        fitness += this.grams.CalculateCost(new int[] { candidate[0], candidate[1], candidate[2], i });
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
                        fitness += this.grams.CalculateCost(new int[] { l1, l2, l3, l4 });

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
            foreach (Word w in this.words)
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
                this.completeKey = true;       
            }
            else
            {
                this.partialKey = true;
            }

            int[] key = this.MakeKeyComplete(map.DeriveKey());
            Text plaintext = this.DecryptCiphertext(key, this.ctext, this.calpha);
            string plain = plaintext.ToString(this.palpha);
            double fit = this.grams.CalculateCost(plaintext.ToIntArray());
            String key_string = CreateAlphabetOutput(key,this.palpha);
            KeyCandidate keyCan = new KeyCandidate(key, fit, plain, key_string);
            keyCan.DicAttack = true;
            this.updateKeyDisplay(keyCan);
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
                int cur_index = this.random.Next(solvedWords.Length);

                if (!(solvedWords[cur_index] || !this.words[cur_index].Enabled))
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
            int score = Int32.MaxValue;
            int word = -1;

            for (int i = 0; i < solvedWords.Length; i++)
            {
                Word w = this.words[i];
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

            if (score > this.calpha.GetAlphabetQuantity())
            { 
                for (int i = 0; i < this.historder.Length; i++)
                {
                    if (map.IsUniquelyMapped(this.historder[i]))
                    {
                        continue;
                    }
                    byte letter = this.historder[i];

                    if (this.histogram[letter] == 0)
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
            Mapping helper = new Mapping(this.calpha.GetAlphabetQuantity());

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
                for (int i = 0; i < this.calpha.GetAlphabetQuantity(); i++)
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

    class SolveData
    {
        public Mapping mapping;
        public int numSolvedWords;
        public bool[] solvedWords;
        public int[] firstcand;

        public SolveData Copy()
        {
            SolveData res = new SolveData();
            res.mapping = this.mapping.Copy();
            res.numSolvedWords = this.numSolvedWords;

            res.solvedWords = new bool[this.solvedWords.Length];
            this.solvedWords.CopyTo(res.solvedWords, 0);

            res.firstcand = new int[this.firstcand.Length];
            this.firstcand.CopyTo(res.firstcand, 0);

            return res;
        }
    }
}
