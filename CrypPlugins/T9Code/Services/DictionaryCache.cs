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

namespace CrypTool.T9Code.Services
{
    internal class DictionaryCache
    {
        public Trie Trie { get; private set; }

        public DictionaryCache()
        {
            Trie = new Trie();
        }

        public void Clear()
        {
            Trie = null;
            Trie = new Trie();
        }

        public bool IsDataLoaded => Trie != null;
    }
}