using System.Collections.Generic;
using System.Linq;

namespace CrypTool.T9Code.DataStructure
{
    internal class Node
    {
        public string Value { get; }
        public List<Node> Children { get; }
        public Node Parent { get; }
        public int Depth { get; }
        public List<string> Words { get; }

        public Node(string value, Node parent, string word)
        {
            Value = value;
            Children = new List<Node>();
            Depth = parent?.Depth + 1 ?? 0;
            Parent = parent;
            Words = new List<string> { word };
        }

        public void AddChild(Node newChild)
        {
            Children.Add(newChild);
        }

        public Node FindChildNode(char c)
        {
            return Children.FirstOrDefault(child => child.Value == char.ToString(c));
        }

        public override string ToString()
        {
            return $"V: {Value}; D: {Depth}; N: {Words}";
        }
    }
}