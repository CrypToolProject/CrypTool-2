/*                              
   Copyright 2024 Nils Kopal, CrypTool Team

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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LanguageStatisticsLib
{
    /// <summary>
    /// A tree of words -- a tree-based dictionary.
    /// </summary>
    public class WordTree : Node
    {
        public int StoredWords { get; set; } = 0;
        public string LanguageCode { get; set; } = string.Empty;
        public string Alphabet { get; set; } = string.Empty;

        /// <summary>
        /// Deserializes a tree from the given stream.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static WordTree Deserialize(BinaryReader reader)
        {
            char readChar;

            //Word Tree data format:
            // "CT2DIC" 6 bytes
            // languageCodeBytes 0-terminated string;
            // magicNo 4 byte integer;
            // alphabet = 0-terminated string;
            // numberOfWords = 4 byte integer

            WordTree tree = new WordTree();

            //1. load word tree header
            string magicNo = new string(reader.ReadChars(6));
            if (magicNo != "CT2DIC")
            {
                throw new Exception("File does not start with the expected magic number for word tree.");
            }

            //read chars of language code until 0-termination
            StringBuilder languageCodeBuilder = new StringBuilder();
            while ((readChar = reader.ReadChar()) != '\0')
            {
                languageCodeBuilder.Append(readChar);
            }
            tree.LanguageCode = languageCodeBuilder.ToString();

            //read alphabet until 0-termination
            StringBuilder alphabetBuilder = new StringBuilder();
            while ((readChar = reader.ReadChar()) != '\0')
            {
                alphabetBuilder.Append(readChar);
            }
            tree.Alphabet = alphabetBuilder.ToString();

            //read number of stored words                                 
            tree.StoredWords = reader.ReadInt32();

            //2. load word tree data structure
            Stack<Node> stack = new Stack<Node>();
            stack.Push(tree);
            int symbol;
            while ((symbol = reader.Read()) != -1)
            {
                readChar = (char)symbol;
                if (readChar == Node.WordEndSymbol)
                {
                    stack.Peek().WordEndsHere = true;
                    tree.StoredWords++;
                    continue;
                }
                if (readChar == Node.TerminationSymbol)
                {
                    stack.Pop();
                    continue;
                }
                Node newNode = new Node() { Value = readChar };
                stack.Peek().ChildNodes.Add(newNode);
                stack.Push(newNode);
            }
            return tree;
        }

        /// <summary>
        /// Returns true if the tree contains the given word.
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public bool ContainsWord(string word)
        {
            Node currentNode = this;
            for (int i = 0; i < word.Length; i++)
            {
                Node foundNode = null;
                foreach (Node childNode in currentNode.ChildNodes)
                {
                    if (childNode.Value == word[i])
                    {
                        currentNode = childNode;
                        break;
                    }
                }
                if (foundNode == null)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Converts the tree to a list of words.
        /// <param name="maxDepth"> is the maximum depth of the tree (=max word length) to be converted.</param>
        /// </summary>
        public List<string> ToList(int maxDepth = int.MaxValue)
        {
            List<string> list = new List<string>();
            Stack<char> stack = new Stack<char>();
            int depth = 0;
            foreach (Node node in ChildNodes)
            {
                AddNodeToList(node, stack, list, depth, maxDepth);
            }
            return list;
        }

        /// <summary>
        /// Adds a node and all its children to the list.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="stack"></param>
        /// <param name="depth"></param>
        /// <param name="maxDepth"></param>
        private void AddNodeToList(Node node, Stack<char> stack, List<string> list, int depth, int maxDepth)
        {
            depth++;
            stack.Push(node.Value);
            if (node.WordEndsHere)
            {
                list.Add(new string(stack.Reverse().ToArray()));
            }
            if (depth < maxDepth)
            {
                foreach (Node childNode in node.ChildNodes)
                {
                    AddNodeToList(childNode, stack, list, depth, maxDepth);
                }
            }
            stack.Pop();
        }

        /// <summary>
        /// Returns true if the given object is equal to this tree.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            //check if obj is null
            if (obj == null)
            {
                return false;
            }
            //check if obj is a WordTree
            if (!(obj is WordTree))
            {
                return false;
            }
            //check if stored number of words are equal
            if (StoredWords != ((WordTree)obj).StoredWords)
            {
                return false;
            }
            return base.Equals(obj);
        }       

        /// <summary>
        /// Returns the hash code of this tree.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int hashCode = -1514771638;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + Value.GetHashCode();
            hashCode = hashCode * -1521134295 + WordEndsHere.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<List<Node>>.Default.GetHashCode(ChildNodes);
            hashCode = hashCode * -1521134295 + StoredWords.GetHashCode();
            return hashCode;
        }
    }
}
