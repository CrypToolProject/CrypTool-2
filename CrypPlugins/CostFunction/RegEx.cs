/*                              
   Copyright 2010 Sven Rech (svenrech at googlemail dot com), Uni Duisburg-Essen

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
using System.Linq;

namespace CrypTool.Plugins.CostFunction
{
    /// <summary>
    /// An implementation of fast regex matching algorithm.
    /// Note: Only the matching is fast. Compiling the regex isn't!
    /// 
    /// The matching algorithm works on byte arrays and not on string.
    /// This is because the cost function also works on byte[]. So we save a conversion.
    /// </summary>
    internal class RegEx
    {
        private const int NOTRANSITION = int.MaxValue;
        private readonly int[][] transitionMatrix = null;
        private readonly int startIndex;

        public string Regex
        {
            get;
            private set;
        }

        public RegEx(string regex, bool caseSensitive)
        {
            if (caseSensitive)
            {
                regex = regex.ToLower();
            }

            //convert regex to NFA:
            int index = -1;
            NFA nfa = RegexToNFA(regex, ref index);
            if (index < regex.Length)
            {
                throw new ParseException("Error occurred while parsing the regular expression!");
            }

            if (nfa != null)
            {
                //make epsilon transitions superfluous:
                nfa.RemoveEpsilonTransitions();

                //convert NFA to DFA:
                transitionMatrix = nfa.GetDFATransitionMatrix(out startIndex);

                //manipulate transition matrix to make it case sensitive:
                if (caseSensitive)
                {
                    for (int x = 0; x < transitionMatrix.Length; x++)
                    {
                        for (int y = 0; y <= ('Z' - 'A'); y++)
                        {
                            transitionMatrix[x][(byte)(y + 'A')] = transitionMatrix[x][(byte)(y + 'a')];
                        }

                        transitionMatrix[x][(byte)('Ä')] = transitionMatrix[x][(byte)('ä')];
                        transitionMatrix[x][(byte)('Ö')] = transitionMatrix[x][(byte)('ö')];
                        transitionMatrix[x][(byte)('Ü')] = transitionMatrix[x][(byte)('ü')];
                    }
                }
            }
        }      

        /// <summary>
        /// Matches input with the given regex and returns a value which indicates "how much" it matches.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public int MatchesValue(byte[] input)
        {
            if (transitionMatrix == null)
            {
                return int.MinValue;
            }

            int state = startIndex;
            int count = 0;

            foreach (byte i in input)
            {
                int absState = state >= 0 ? state : ~state;
                state = transitionMatrix[absState][i];
                if (state == NOTRANSITION)
                {
                    return (-input.Length + count);
                }

                count++;
            }

            return (state < 0) ? 1 : 0;
        }

        #region NFA

        /// <summary>
        /// Implements a nondeterministic finite automaton and some useful functions.
        /// </summary>
        private class NFA
        {
            /// <summary>
            /// Represents the connection to the node Node which is triggered by the TransitionBytes.
            /// Epsilon can also be a transition reason.
            /// </summary>
            private struct Connection
            {
                public List<Connection> Node;
                public byte[] TransitionBytes;
                public bool Epsilon;
            }

            //This list stores for every node a list of his connections to other nodes:
            private List<List<Connection>> nodeConnections = new List<List<Connection>>();

            //start and end nodes:
            private int startIndex;
            private readonly List<int> endIndices = new List<int>();

            /// <summary>
            /// Concatenates two NFAs
            /// </summary>
            /// <param name="nfa1">The first NFA</param>
            /// <param name="nfa2">The second NFA</param>
            /// <returns></returns>
            public static NFA ConcatNFAs(NFA nfa1, NFA nfa2)
            {
                if (nfa1 == null && nfa2 == null)
                {
                    return null;
                }
                else if (nfa1 == null)
                {
                    return nfa2;
                }
                else if (nfa2 == null)
                {
                    return nfa1;
                }

                NFA result = new NFA
                {
                    nodeConnections = new List<List<Connection>>()
                };
                result.nodeConnections.AddRange(nfa1.nodeConnections);
                result.nodeConnections.AddRange(nfa2.nodeConnections);

                //bridge nfa1 and nfa2 by an epsilon connection:
                Connection bridge = new Connection
                {
                    Epsilon = true,
                    Node = nfa2.nodeConnections[nfa2.startIndex]
                };
                result.nodeConnections[nfa1.endIndices[0]].Add(bridge);

                result.startIndex = nfa1.startIndex;
                result.endIndices.Add(nfa1.nodeConnections.Count + nfa2.endIndices[0]);

                return result;
            }

            public static NFA AlternateNFAs(NFA nfa1, NFA nfa2)
            {
                if (nfa1 == null && nfa2 == null)
                {
                    return null;
                }
                else if (nfa1 == null)
                {
                    return nfa2;
                }
                else if (nfa2 == null)
                {
                    return nfa1;
                }

                NFA result = new NFA
                {
                    nodeConnections = new List<List<Connection>>()
                };
                result.nodeConnections.AddRange(nfa1.nodeConnections);
                result.nodeConnections.AddRange(nfa2.nodeConnections);

                //create new start node:
                Connection newStartConnection1 = new Connection
                {
                    Epsilon = true,
                    Node = result.nodeConnections[nfa1.startIndex]
                };
                Connection newStartConnection2 = new Connection
                {
                    Epsilon = true,
                    Node = result.nodeConnections[nfa1.nodeConnections.Count + nfa2.startIndex]
                };
                List<Connection> newStart = new List<Connection>() { newStartConnection1, newStartConnection2 };
                result.nodeConnections.Add(newStart);
                result.startIndex = result.nodeConnections.Count - 1;

                //create new end node:
                List<Connection> newEnd = new List<Connection>();
                result.nodeConnections.Add(newEnd);
                result.endIndices.Add(result.nodeConnections.Count - 1);
                Connection newEndConnection1 = new Connection
                {
                    Epsilon = true,
                    Node = result.nodeConnections[result.endIndices[0]]
                };
                result.nodeConnections[nfa1.endIndices[0]].Add(newEndConnection1);
                Connection newEndConnection2 = new Connection
                {
                    Epsilon = true,
                    Node = result.nodeConnections[result.endIndices[0]]
                };
                result.nodeConnections[nfa1.nodeConnections.Count + nfa2.endIndices[0]].Add(newEndConnection1);

                return result;
            }

            public void KleeneStar()
            {
                //create new end connection:
                List<Connection> newEnd = new List<Connection>();
                nodeConnections.Add(newEnd);

                //create new start connection:
                Connection newStartConnection1 = new Connection
                {
                    Epsilon = true,
                    Node = nodeConnections[startIndex]
                };
                Connection newStartConnection2 = new Connection
                {
                    Epsilon = true,
                    Node = newEnd
                };
                List<Connection> newStart = new List<Connection>() { newStartConnection1, newStartConnection2 };
                nodeConnections.Add(newStart);
                startIndex = nodeConnections.Count - 1;

                //connects old end with start:
                Connection oldEndWithStartConnection = new Connection
                {
                    Epsilon = true,
                    Node = newStart
                };
                nodeConnections[endIndices[0]].Add(oldEndWithStartConnection);

                endIndices[0] = nodeConnections.IndexOf(newEnd);
            }

            public static NFA ByteTransitionNFA(byte[] transitionBytes)
            {
                NFA result = new NFA();

                List<Connection> newEnd = new List<Connection>();
                Connection transition = new Connection
                {
                    Epsilon = false,
                    Node = newEnd,
                    TransitionBytes = transitionBytes
                };
                List<Connection> newStart = new List<Connection> { transition };

                result.nodeConnections.Add(newStart);
                result.nodeConnections.Add(newEnd);
                result.startIndex = 0;
                result.endIndices.Add(1);

                return result;
            }

            public override string ToString()
            {
                int i = 0;
                Dictionary<List<Connection>, int> nodeToID = new Dictionary<List<Connection>, int>();
                string res = "";

                foreach (List<Connection> node in nodeConnections)
                {
                    nodeToID.Add(node, i++);
                }

                res += "Start: " + nodeToID[nodeConnections[startIndex]] + "\n";
                res += "End: (";
                foreach (int ei in endIndices)
                {
                    res += nodeToID[nodeConnections[ei]] + ", ";
                }

                res += ")\n";

                foreach (List<Connection> node in nodeConnections)
                {
                    res += nodeToID[node] + ": ";
                    foreach (Connection c in node)
                    {
                        res += " -" + ListArray(c.TransitionBytes) + "-" + (c.Epsilon ? "e" : "") + "-> " + nodeToID[c.Node] + " ";
                    }

                    res += "\n";
                }

                return res;
            }

            private string ListArray(byte[] p)
            {
                if (p == null)
                {
                    return "";
                }

                string res = "[";
                foreach (byte b in p)
                {
                    res += Convert.ToChar(b);
                }

                return res + "]";
            }

            public void RemoveEpsilonTransitions()
            {
                bool update = true;

                while (update)
                {
                    update = false;
                    //for all epsilon connections, we create a new connection that "jumps" over the intermediate node:
                    foreach (List<Connection> node in nodeConnections)
                    {
                        for (int i = 0; i < node.Count; i++)
                        {
                            Connection connection = node[i];
                            if (connection.Epsilon)
                            {
                                foreach (Connection c in connection.Node)
                                {
                                    if (!node.Contains(c))
                                    {
                                        node.Add(c);
                                        update = true;
                                    }
                                }

                                //if we have an epsilon connection to an end node:
                                if (endIndices.Contains(nodeConnections.IndexOf(connection.Node)))
                                {
                                    int index = nodeConnections.IndexOf(node);
                                    if (!endIndices.Contains(index))
                                    {
                                        endIndices.Add(index);
                                    }
                                }
                            }
                        }
                    }
                }

                //remove all epsilon connections:
                foreach (List<Connection> node in nodeConnections)
                {
                    for (int i = node.Count - 1; i >= 0; i--)
                    {
                        if (node[i].Epsilon)
                        {
                            if (node[i].TransitionBytes == null)
                            {
                                node.RemoveAt(i);
                            }
                            else
                            {
                                Connection c = node[i];
                                c.Epsilon = false;
                            }
                        }
                    }
                }

            }


            public int[][] GetDFATransitionMatrix(out int start)
            {
                List<int[]> transitions = new List<int[]>();
                Dictionary<HashSet<List<Connection>>, int> states = new Dictionary<HashSet<List<Connection>>, int>();   //mapping of node powersets to state id
                int idCounter = 0;

                start = idCounter++;
                HashSet<List<Connection>> startState = new HashSet<List<Connection>>(new List<Connection>[] { nodeConnections[startIndex] });
                if (endIndices.Contains(startIndex))
                {
                    start = ~start;
                }

                states.Add(startState, start);

                int[] newTransitions = new int[256];

                Queue<HashSet<List<Connection>>> newStates = new Queue<HashSet<List<Connection>>>();
                newStates.Enqueue(startState);

                while (newStates.Count != 0)
                {
                    HashSet<List<Connection>> state = newStates.Dequeue();

                    //create a new row in transition matrix for this state:
                    newTransitions = new int[256];
                    for (int i = 0; i < 256; i++)
                    {
                        newTransitions[i] = NOTRANSITION;
                    }

                    transitions.Add(newTransitions);

                    //we have to check the transistions for every possible transition byte.
                    for (int b = byte.MinValue; b <= byte.MaxValue; b++)
                    {
                        //check which nodes we would reach by transition over b. Make a state out of these node powerset.
                        HashSet<List<Connection>> transitionState = new HashSet<List<Connection>>();
                        foreach (List<Connection> node in state)
                        {
                            foreach (Connection connection in node)
                            {
                                if (connection.TransitionBytes.Contains((byte)b))
                                {
                                    transitionState.Add(connection.Node);
                                }
                            }
                        }
                        if (transitionState.Count == 0)
                        {
                            continue;
                        }

                        int transitionStateIndex = NOTRANSITION;

                        //check if this state already exists:
                        foreach (HashSet<List<Connection>> s in states.Keys)
                        {
                            if (s.SetEquals(transitionState))
                            {
                                transitionStateIndex = states[s];
                                break;
                            }
                        }

                        //if not, we have to create it:
                        if (transitionStateIndex == NOTRANSITION)
                        {
                            transitionStateIndex = idCounter++;

                            foreach (List<Connection> node in transitionState)   //check if this is an end state
                            {
                                if (endIndices.Contains(nodeConnections.IndexOf(node)))
                                {
                                    transitionStateIndex = ~transitionStateIndex;
                                    break;
                                }
                            }

                            states.Add(transitionState, transitionStateIndex);
                            newStates.Enqueue(transitionState);
                        }

                        //put transition into table:
                        int absState = states[state] >= 0 ? states[state] : ~states[state];
                        transitions[absState][b] = transitionStateIndex;
                    }


                }

                return transitions.ToArray();
            }
        }

        #endregion

        /// <summary>
        /// Parses the regex and converts it into an NFA.
        /// </summary>
        /// <param name="regex"></param>
        private NFA RegexToNFA(string regex, ref int index)
        {
            bool bracket = false;
            if (index >= 0 && index < regex.Length)
            {
                bracket = (regex[index] == '(');
            }

            if (bracket || (index < 0))
            {
                index++;
            }

            NFA result = null;

            while (index < regex.Length && regex[index] != ')')
            {
                NFA newNFA = null;
                int combineMode = -1;   //0 = concat, 1 = alternate
                switch (regex[index])
                {
                    case '(':
                        newNFA = RegexToNFA(regex, ref index);
                        combineMode = 0;
                        break;
                    case '|':
                        index++;
                        newNFA = RegexToNFA(regex, ref index);
                        combineMode = 1;
                        break;
                    case '*':
                        index++;
                        combineMode = -1;
                        break;
                    case '[':
                        byte[] transitionBytes = GetTransitionSet(regex, ref index);
                        newNFA = NFA.ByteTransitionNFA(transitionBytes);
                        combineMode = 0;
                        break;
                    case '.':
                        transitionBytes = new byte[256];
                        for (int b = byte.MinValue; b <= byte.MaxValue; b++)
                        {
                            transitionBytes[b] = (byte)b;
                        }

                        newNFA = NFA.ByteTransitionNFA(transitionBytes);
                        combineMode = 0;
                        index++;
                        break;
                    case ']':
                        throw new ParseException("Error at position " + index + "! No ']' expected. Escape it!");
                    case '\\':
                        index++;
                        transitionBytes = new byte[] { Convert.ToByte(regex[index++]) };
                        newNFA = NFA.ByteTransitionNFA(transitionBytes);
                        combineMode = 0;
                        break;
                    default:
                        transitionBytes = new byte[] { Convert.ToByte(regex[index++]) };
                        newNFA = NFA.ByteTransitionNFA(transitionBytes);
                        combineMode = 0;
                        break;
                }

                if (index < regex.Length && regex[index] == '*')
                {
                    newNFA.KleeneStar();
                    index++;
                }

                if (combineMode == 0)
                {
                    result = NFA.ConcatNFAs(result, newNFA);
                }
                else if (combineMode == 1)
                {
                    result = NFA.AlternateNFAs(result, newNFA);
                }
            }


            if (bracket && (index < regex.Length) && regex[index] == ')')
            {
                index++;
            }
            else if (bracket && ((index >= regex.Length) || regex[index] != ')'))
            {
                throw new ParseException("Error at position " + index + "! Closing bracket expected!");
            }

            return result;
        }

        private byte[] GetTransitionSet(string regex, ref int index)
        {
            List<byte> set = new List<byte>();
            index++;
            bool invert = false;
            if (regex[index] == '^')
            {
                index++;
                invert = true;
            }

            while (index < regex.Length && regex[index] != ']')
            {
                if (index + 2 < regex.Length && regex[index + 1] == '-' && regex[index + 1] != ']')     //group
                {
                    for (byte b = Convert.ToByte(regex[index]); b <= Convert.ToByte(regex[index + 2]); b++)
                    {
                        set.Add(b);
                    }

                    index += 3;
                }
                else if (index + 1 < regex.Length && regex[index + 1] == '-')   //misuse of '-' group sign
                {
                    throw new ParseException("Error at position " + (index + 1) + "! Misuse of '-' in group!");
                }
                else    //normal case
                {
                    //TODO: Maybe we should check here, if character is allowed at this position.
                    set.Add(Convert.ToByte(regex[index]));
                    index++;
                }
            }

            if (index >= regex.Length)
            {
                throw new ParseException("Error at the end of the expression! ']' expected!");
            }

            byte[] result;
            if (!invert)
            {
                result = set.ToArray();
            }
            else
            {
                List<byte> set2 = new List<byte>();
                for (int b = byte.MinValue; b <= byte.MaxValue; b++)
                {
                    if (!set.Contains((byte)b))
                    {
                        set2.Add((byte)b);
                    }
                }

                result = set2.ToArray();
            }

            index++;

            return result;
        }

    }

    public class ParseException : Exception
    {
        public ParseException(string message) : base(message)
        {
        }
    }


}
