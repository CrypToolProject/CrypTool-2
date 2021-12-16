using CrypTool.T9Code.DataStructure;
using CrypTool.T9Code.Enums;
using CrypTool.T9Code.Properties;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CrypTool.T9Code.Services
{
    public class CryptoService
    {
        private readonly DictionaryCache _dictionaryCache;
        private readonly GramService _gramService;
        public InternalGramType GramSize { get; set; }

        public CryptoService()
        {
            _dictionaryCache = new DictionaryCache();
            _gramService = new GramService();
        }

        public void SetGramLanguage(int language)
        {
            _gramService.SetGramLanguage(language);
        }

        public string Encode(string text)
        {
            return string.IsNullOrEmpty(text) ? string.Empty : CharMapping.StringToDigit(text);
        }

        public IList<string> Decode(string numbers)
        {
            return !string.IsNullOrEmpty(numbers)
? numbers.Split('0').Select(DecodeNumbers).ToList()
: new List<string>();
        }

        private string DecodeNumbers(string numbers)
        {
            if (!_dictionaryCache.IsDataLoaded)
            {
                return string.Empty;
            }

            List<string> words = DecodeNumbersByTrie(numbers);
            return words.Count == 1 ? words.First() : StringWithHighestCost(words);
        }

        private List<string> DecodeNumbersByTrie(string word)
        {
            if (word.Length == 0)
            {
                return new List<string>();
            }

            Node terminalNode = _dictionaryCache.Trie.Prefix(word).Children
                .FirstOrDefault(c => c.Value == char.ToString(Trie.TerminalSymbol));

            return terminalNode == null ? new List<string> { Resources.UnknownWord } : terminalNode.Words;
        }

        private string StringWithHighestCost(IList<string> possibleStrings)
        {
            return !possibleStrings.Any() ? string.Empty : _gramService.FindWordWithHighestScore(possibleStrings, GramSize);
        }

        public void LoadDictionary(Array dictionaryContent)
        {
            foreach (string word in (string[])dictionaryContent)
            {
                _dictionaryCache.Trie.Insert(word);
            }
        }

        public void ClearDictionaryCache()
        {
            _dictionaryCache.Clear();
        }
    }
}