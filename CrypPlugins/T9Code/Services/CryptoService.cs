/*
   Copyright (C) CrypTool 2 Team

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