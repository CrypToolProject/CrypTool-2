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
            Node currentNode = _root;
            Node result = currentNode;

            foreach (char c in s)
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
            Node prefix = Prefix(s);
            return prefix.Depth == s.Length && prefix.FindChildNode(TerminalSymbol) != null;
        }

        public void Insert(string stringToInsert)
        {
            string numberRepresentation = CharMapping.StringToDigit(stringToInsert);

            Node commonPrefix = Prefix(numberRepresentation);
            Node currentNode = commonPrefix;

            for (int i = currentNode.Depth; i < numberRepresentation.Length; i++)
            {
                Node newNode = new Node(char.ToString(numberRepresentation[i]), currentNode, null);
                currentNode.Children.Add(newNode);
                currentNode = newNode;
            }

            Node terminalSymbolNode = currentNode.FindChildNode(TerminalSymbol);
            terminalSymbolNode?.Words.Add(stringToInsert);
            currentNode.AddChild(new Node(TerminalSymbol.ToString(), currentNode, stringToInsert));
        }
    }
}