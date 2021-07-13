using CrypTool.T9Code.Services;

namespace CrypTool.T9Code.DataStructure
{
    internal class Trie
    {
        private readonly Node _root;
        internal static char TerminalSymbol = '$';
        internal static string RootSymbol = "^";

        public Trie()
        {
            _root = new Node(RootSymbol, null, null);
        }

        public Node Prefix(string s)
        {
            var currentNode = _root;
            var result = currentNode;

            foreach (var c in s)
            {
                currentNode = currentNode.FindChildNode(c);
                if (currentNode == null)
                {
                    break;
                }

                result = currentNode;
            }

            return result;
        }

        public bool Contains(string s)
        {
            var prefix = Prefix(s);
            return prefix.Depth == s.Length && prefix.FindChildNode(TerminalSymbol) != null;
        }

        public void Insert(string stringToInsert)
        {
            var numberRepresentation = CharMapping.StringToDigit(stringToInsert);

            var commonPrefix = Prefix(numberRepresentation);
            var currentNode = commonPrefix;

            for (var i = currentNode.Depth; i < numberRepresentation.Length; i++)
            {
                var newNode = new Node(char.ToString(numberRepresentation[i]), currentNode, null);
                currentNode.Children.Add(newNode);
                currentNode = newNode;
            }

            var terminalSymbolNode = currentNode.FindChildNode(TerminalSymbol);
            terminalSymbolNode?.Words.Add(stringToInsert);
            currentNode.AddChild(new Node(TerminalSymbol.ToString(), currentNode, stringToInsert));
        }
    }
}