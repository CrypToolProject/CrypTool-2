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