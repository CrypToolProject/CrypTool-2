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