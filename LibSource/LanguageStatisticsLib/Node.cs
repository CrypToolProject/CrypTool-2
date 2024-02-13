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

using System.Collections.Generic;

namespace LanguageStatisticsLib
{
    /// <summary>
    /// A Node of a word tree.
    /// </summary>
    public class Node
    {
        /// <summary>
        /// A symbol that indicates the end of a word.
        /// </summary>
        public const char WordEndSymbol = (char)1;

        /// <summary>
        /// A symbol that indicates an end of the tree.
        /// </summary>
        public const char TerminationSymbol = (char)0;

        /// <summary>
        /// The value of this node.
        /// </summary>
        public char Value { get; set; }

        /// <summary>
        /// A value indicating whether a word ends here.
        /// </summary>
        public bool WordEndsHere { get; set; }

        /// <summary>
        /// All child nodes of this node.
        /// </summary>
        public List<Node> ChildNodes { get; set; } = new List<Node>();

        /// <summary>
        /// Returns true if the given object is equal to this node.
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
            if (!(obj is Node))
            {
                return false;
            }
            Node node = (Node)obj;
            if (node.Value != Value)
            {
                return false;
            }
            if (node.WordEndsHere != WordEndsHere)
            {
                return false;
            }
            if (node.ChildNodes.Count != ChildNodes.Count)
            {
                return false;
            }
            for (int i = 0; i < ChildNodes.Count; i++)
            {
                if (!ChildNodes[i].Equals(node.ChildNodes[i]))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns the hash code of this node.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int hashCode = 1061660465;
            hashCode = hashCode * -1521134295 + Value.GetHashCode();
            hashCode = hashCode * -1521134295 + WordEndsHere.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<List<Node>>.Default.GetHashCode(ChildNodes);
            return hashCode;
        }
    }
}
