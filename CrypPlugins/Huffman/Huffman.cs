/*
   Copyright 2011 CrypTool 2 Team <ct2contact@CrypTool.org>

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

using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using Huffman;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace CrypTool.Plugins.Huffman
{
    [Author("Marko Lazić", "marko.lazic.14@singimail.rs", "Singidunum University", "http://eng.singidunum.ac.rs/")]
    [PluginInfo("Huffman.Properties.Resources", "PluginCaption", "PluginTooltip",
        "Huffman/DetailedDescription/doc.xml", "Huffman/h.png")]
    [ComponentCategory(ComponentCategory.ToolsCodes)]
    public class Huffman : ICrypComponent
    {
        #region Private Variables

        private readonly HuffmanSettings settings;
        private byte[] inputBytes;
        private byte[] outputBytes;
        private byte[] inputTree;
        private byte[] outputTree;
        private readonly HuffmanPresentation presentation;

        #endregion

        #region Public interface

        public Huffman()
        {
            settings = new HuffmanSettings();
            presentation = new HuffmanPresentation();
        }

        public ISettings Settings => settings;

        public UserControl Presentation => presentation;

        [PropertyInfo(Direction.InputData, "InputBytesCaption", "InputBytesTooltip", true)]
        public byte[] InputBytes
        {
            get => inputBytes;
            set
            {
                if (inputBytes != value)
                {
                    inputBytes = value;
                    OnPropertyChanged("InputBytes");
                }
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputBytesCaption", "OutputBytesTooltip")]
        public byte[] OutputBytes => outputBytes;

        [PropertyInfo(Direction.InputData, "InputHuffmanTreeCaption", "InputHuffmanTreeTooltip", false)]
        public byte[] InputHuffmanTree
        {
            get => inputTree;
            set
            {
                if (inputTree != value)
                {
                    inputTree = value;
                    OnPropertyChanged("InputHuffmanTree");
                }
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputHuffmanTreeCaption", "OutputHuffmanTreeTooltip")]
        public byte[] OutputHuffmanTree => outputTree;

        #endregion

        #region IPlugin Members

        public void PreExecution()
        {
        }

        public void Execute()
        {
            if (InputBytes.Length == 0)
            {
                GuiLogMessage("No input bytes provided", NotificationLevel.Error);
                return;
            }
            if (settings.Action == HuffmanSettings.HuffmanMode.Compress)
            {
                try
                {
                    // Get encoding
                    Encoding en = getEncoding(settings.Presentation, settings.Encoding);
                    ProgressChanged(1, 8);

                    // Decode input bytes
                    string decoded = en.GetString(InputBytes);
                    ProgressChanged(2, 8);

                    // Get character frequencies
                    Dictionary<char, int> histogram = getCharFrequencies(decoded);
                    ProgressChanged(3, 8);

                    // Build Huffman tree
                    HuffmanTree tree = new HuffmanTree(histogram);
                    ProgressChanged(4, 8);

                    // Compress data
                    List<bool> compressed = tree.Compress(decoded);
                    ProgressChanged(5, 8);

                    // Encode tree
                    List<bool> encodedTree = new List<bool>();
                    encodeTree(tree.getRoot(), encodedTree, en);
                    ProgressChanged(6, 8);

                    // Pack and output data
                    outputBytes = packData(compressed);
                    outputTree = packData(encodedTree);
                    OnPropertyChanged("OutputBytes");
                    OnPropertyChanged("OutputHuffmanTree");
                    ProgressChanged(7, 8);

                    // Calculate compressed size and fill code table in presentation view
                    int compressedSize = outputBytes.Count() + outputTree.Count();
                    presentation.fillCodeTable(tree.getCodeTable(), histogram, decoded.Count(), compressedSize);
                    ProgressChanged(8, 8);
                }
                // Thrown when there's less than two characters in the input
                catch (ArgumentException ex)
                {
                    GuiLogMessage(ex.Message, NotificationLevel.Error);
                    return;
                }
            }
            else
            {
                if (InputHuffmanTree.Length == 0)
                {
                    GuiLogMessage("No Huffman tree provided", NotificationLevel.Error);
                    return;
                }
                // Get encoding
                Encoding en = getEncoding(settings.Presentation, settings.Encoding);
                ProgressChanged(1, 8);

                // Extract encoded tree                
                List<bool> encodedTree = extractData(InputHuffmanTree);
                ProgressChanged(2, 8);

                // Rebuild Huffman tree
                HuffmanTree tree = new HuffmanTree();
                tree.setRoot(decodeTree(ref encodedTree, en));
                ProgressChanged(3, 8);

                // Extract compressed data
                List<bool> compressed = extractData(InputBytes);
                ProgressChanged(4, 8);

                // Decompress data
                string decompressed = tree.Decompress(compressed);
                ProgressChanged(5, 8);

                // Encode and output data
                outputBytes = en.GetBytes(decompressed);
                OnPropertyChanged("OutputBytes");
                ProgressChanged(6, 8);

                // Get character frequencies for displaying in presentation view
                Dictionary<char, int> histogram = getCharFrequencies(decompressed);
                ProgressChanged(7, 8);

                // Calculate compressed size and fill code table in presentation view 
                int compressedSize = InputBytes.Count() + InputHuffmanTree.Count();
                tree.CreateCodeTable(histogram);
                presentation.fillCodeTable(tree.getCodeTable(), histogram, decompressed.Count(), compressedSize);
                ProgressChanged(8, 8);
            }
        }

        public void PostExecution()
        {
        }

        public void Stop()
        {
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        #endregion

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion

        #region Private Methods        

        private static Encoding getEncoding(HuffmanSettings.PresentationFormat presentation,
            HuffmanSettings.EncodingTypes encoding)
        {
            Encoding en = null;

            switch (presentation)
            {
                case HuffmanSettings.PresentationFormat.Text:
                    switch (encoding)
                    {
                        case HuffmanSettings.EncodingTypes.UTF8:
                            en = Encoding.UTF8;
                            break;
                        case HuffmanSettings.EncodingTypes.UTF7:
                            en = Encoding.UTF7;
                            break;
                        case HuffmanSettings.EncodingTypes.UTF16:
                            en = Encoding.Unicode;
                            break;
                        case HuffmanSettings.EncodingTypes.UTF32:
                            en = Encoding.UTF32;
                            break;
                        case HuffmanSettings.EncodingTypes.ASCII:
                            en = Encoding.ASCII;
                            break;
                        case HuffmanSettings.EncodingTypes.ISO8859_15:
                            en = Encoding.GetEncoding("iso-8859-15");
                            break;
                        case HuffmanSettings.EncodingTypes.Windows1252:
                            en = Encoding.GetEncoding(1252);
                            break;
                    }
                    break;
                case HuffmanSettings.PresentationFormat.Binary:
                    en = Encoding.GetEncoding("437");
                    break;
            }

            return en;
        }

        private static Dictionary<char, int> getCharFrequencies(string s)
        {
            if (s.Count() < 2)
            {
                throw new ArgumentException("Input must contain at least two characters");
            }

            Dictionary<char, int> histogram = new Dictionary<char, int>();

            foreach (char character in s)
            {
                if (!histogram.ContainsKey(character))
                {
                    histogram.Add(character, 1);
                }
                else
                {
                    int frequency = histogram[character];
                    histogram[character] = frequency + 1;
                }
            }

            return histogram;
        }

        private static void encodeTree(Node node, List<bool> encodedTree, Encoding en)
        {
            // If node is a leaf, add 1 and its character to the list - otherwise add 0 and continue
            // encoding its children
            if (node.getLeftChild() == null && node.getRightChild() == null)
            {
                encodedTree.Add(true);
                encodedTree.AddRange(toBits(encodeChar(node.getCharacter(), en)));
            }
            else
            {
                encodedTree.Add(false);
                encodeTree(node.getLeftChild(), encodedTree, en);
                encodeTree(node.getRightChild(), encodedTree, en);
            }
        }

        private static Node decodeTree(ref List<bool> encodedTree, Encoding en)
        {
            // If the first bit is 1, add a leaf node containing the character encoded by the maximum 
            // bits per character for a given encoding to its parent and remove those bits from the
            // list - otherwise remove the first bit and continue decoding left and right subtrees
            if (encodedTree.First())
            {
                encodedTree.Remove(true);
                char c = decodeChar(ref encodedTree, en);
                return new Node(c, 0);
            }
            else
            {
                encodedTree.Remove(false);
                Node leftChild = decodeTree(ref encodedTree, en);
                Node rightChild = decodeTree(ref encodedTree, en);
                return new Node(0, leftChild, rightChild);
            }
        }

        private static List<byte> encodeChar(char c, Encoding en)
        {
            // This method of encoding characters is used because we don't know how
            // much space will character from a variable-length character set take up

            // Write character to memory stream            
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms, en);
            bw.Write(c);

            return new List<byte>(ms.ToArray());
        }

        private static char decodeChar(ref List<bool> charBits, Encoding en)
        {
            // This method of decoding characters is used because we don't know how
            // many bytes does a character from a variable-length character set take up

            // Get maximum bits per character for a given encoding and write them to the memory stream            
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write(getCharBytes(charBits, en));

            // Reset memory stream position
            ms.Position = 0;

            // Read written character
            BinaryReader br = new BinaryReader(ms, en);
            char c = br.ReadChar();

            // For some reason, when encoding to UTF-7, binary writer writes five bytes, but when decoding,
            // binary reader reads only four

            // Remove bits used to encode a character
            if (en == Encoding.UTF7 && ms.Position == 4)
            {
                removeBits(ref charBits, ms.Position + 1);
            }
            else
            {
                removeBits(ref charBits, ms.Position);
            }

            return c;
        }

        private static byte[] packData(List<bool> data)
        {
            List<bool> packed = new List<bool>();

            // Calculate the offset of data from the closest bigger multiple of 8 and
            // pad it with that number of 0's
            int offset = 8 - data.Count % 8;

            for (int i = 0; i < offset; i++)
            {
                data.Add(false);
            }

            // Add offset and data          
            packed.AddRange(toBits((byte)offset));
            packed.AddRange(data);

            return toBytes(packed);
        }

        private static List<bool> extractData(byte[] packed)
        {
            List<bool> data = toBits(new List<byte>(packed));

            // Get data offset and remove it from data
            int offset = toByte(data.GetRange(0, 8));
            data.RemoveRange(0, 8);

            // Remove padding from data
            for (int i = 0; i < offset; i++)
            {
                data.RemoveAt(data.Count - 1);
            }

            return data;
        }

        private static byte[] getCharBytes(List<bool> charBits, Encoding en)
        {
            int maxEncodingBits = getMaxBitsPerChar(en);

            if (charBits.Count >= maxEncodingBits)
            {
                return toBytes(charBits.GetRange(0, maxEncodingBits));
            }
            else
            {
                return toBytes(charBits.GetRange(0, charBits.Count));
            }
        }

        private static int getMaxBitsPerChar(Encoding en)
        {
            if (en == Encoding.ASCII || en == Encoding.GetEncoding("iso-8859-15")
                || en == Encoding.GetEncoding(1252) || en == Encoding.GetEncoding("437"))
            {
                return 8;
            }
            else if (en == Encoding.UTF8 || en == Encoding.Unicode || en == Encoding.UTF32)
            {
                return 32;
            }
            else
            {
                return 64;
            }
        }

        private static void removeBits(ref List<bool> bits, long bytesRead)
        {
            if (bytesRead * 8 > bits.Count)
            {
                bits.RemoveRange(0, bits.Count);
            }
            else
            {
                bits.RemoveRange(0, (int)bytesRead * 8);
            }
        }

        private static List<bool> toBits(byte b)
        {
            List<bool> bits = new List<bool>();

            for (int i = 0; i < 8; i++)
            {
                // If b is even, add 0 to the list - otherwise add 1 and bitshift to the right
                if (b % 2 == 0)
                {
                    bits.Add(false);
                }
                else
                {
                    bits.Add(true);
                }

                b = (byte)(b >> 1);
            }

            return bits;
        }

        private static List<bool> toBits(List<byte> bytes)
        {
            List<bool> bits = new List<bool>();

            // Convert each byte into bits and add it to the list
            for (int i = 0; i < bytes.Count; i++)
            {
                bits.AddRange(toBits(bytes[i]));
            }

            return bits;
        }

        private static byte[] toBytes(List<bool> bits)
        {
            byte[] bytes = new byte[(int)Math.Ceiling((double)bits.Count / 8)];

            int bitIndex = 0;
            int byteIndex = 0;

            for (int i = 0; i < bits.Count; i++)
            {
                if (bits[i])
                {
                    bytes[byteIndex] |= (byte)(1 << bitIndex);
                }

                bitIndex++;

                if (bitIndex == 8)
                {
                    bitIndex = 0;
                    byteIndex++;
                }
            }

            return bytes;
        }

        private static byte toByte(List<bool> bits)
        {
            byte b = 0;

            int bitIndex = 0;

            for (int i = 0; i < 8; i++)
            {
                if (bits[i])
                {
                    b |= (byte)(1 << bitIndex);
                }

                bitIndex++;
            }

            return b;
        }

        #endregion

        #region Huffman Tree And Node Classes

        public class HuffmanTree
        {
            private readonly List<Node> nodes = new List<Node>();
            private Node root;
            private Dictionary<char, List<bool>> codeTable;

            public HuffmanTree(Dictionary<char, int> histogram)
            {
                // Add nodes
                foreach (KeyValuePair<char, int> entry in histogram)
                {
                    nodes.Add(new Node(entry.Key, entry.Value));
                }

                // Sort nodes by frequency
                nodes.Sort((node1, node2) => node1.getFrequency().CompareTo(node2.getFrequency()));

                while (nodes.Count > 1)
                {
                    // Create a parent node by combining frequencies from the first two nodes in the list
                    int frequency = nodes[0].getFrequency() + nodes[1].getFrequency();
                    Node parent = new Node(frequency, nodes[0], nodes[1]);

                    // Remove the first two nodes from the list and add the parent node
                    nodes.Remove(nodes[1]);
                    nodes.Remove(nodes[0]);
                    nodes.Add(parent);

                    // Keep sorting nodes by frequency after adding each parent node
                    nodes.Sort((node1, node2) => node1.getFrequency().CompareTo(node2.getFrequency()));
                }

                // Set the final parent to be the root
                root = nodes[0];

                // Create code table
                CreateCodeTable(histogram);
            }

            public HuffmanTree()
            {

            }

            public List<bool> Compress(string inputString)
            {
                List<bool> compressed = new List<bool>();

                // Find the code for each character in the input string and add it to the output
                for (int i = 0; i < inputString.Length; i++)
                {
                    compressed.AddRange(codeTable[inputString[i]]);
                }

                return compressed;
            }

            public string Decompress(List<bool> compressed)
            {
                StringBuilder decompressed = new StringBuilder();

                // Start from the root
                Node current = root;

                // For each bit in the compressed data, go right down the tree if it's 1 - otherwise go left
                foreach (bool bit in compressed)
                {
                    if (bit)
                    {
                        if (current.getRightChild() != null)
                        {
                            current = current.getRightChild();
                        }
                    }
                    else
                    {
                        if (current.getLeftChild() != null)
                        {
                            current = current.getLeftChild();
                        }
                    }

                    // When you get to a leaf node, add its character to the output and reset the current
                    // node back to the root
                    if (current.getLeftChild() == null && current.getRightChild() == null)
                    {
                        decompressed.Append(current.getCharacter());
                        current = root;
                    }
                }

                return decompressed.ToString();
            }

            public void CreateCodeTable(Dictionary<char, int> histogram)
            {
                Dictionary<char, List<bool>> codeTable = new Dictionary<char, List<bool>>();

                // Traverse to each node in the tree and add the paths taken
                foreach (KeyValuePair<char, int> entry in histogram)
                {
                    codeTable.Add(entry.Key, root.GetPath(entry.Key, new List<bool>()));
                }

                this.codeTable = codeTable;
            }

            public Node getRoot()
            {
                return root;
            }

            public void setRoot(Node root)
            {
                this.root = root;
            }

            public Dictionary<char, List<bool>> getCodeTable()
            {
                return codeTable;
            }
        }

        public class Node
        {
            private readonly char character;
            private readonly int frequency;
            private readonly Node leftChild;
            private readonly Node rightChild;

            public List<bool> GetPath(char character, List<bool> currentPath)
            {
                // If node is a leaf and it contains the wanted character, return the path taken to it
                if (leftChild == null && rightChild == null)
                {
                    if (character.Equals(this.character))
                    {
                        return currentPath;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    // If node is a parent, create two lists for storing information about the paths taken
                    List<bool> left = null;
                    List<bool> right = null;

                    // Keep going left until you hit a leaf node
                    if (leftChild != null)
                    {
                        List<bool> leftPath = new List<bool>();
                        leftPath.AddRange(currentPath);
                        leftPath.Add(false);

                        left = leftChild.GetPath(character, leftPath);
                    }

                    // Keep going right until you hit a leaf node
                    if (rightChild != null)
                    {
                        List<bool> rightPath = new List<bool>();
                        rightPath.AddRange(currentPath);
                        rightPath.Add(true);

                        right = rightChild.GetPath(character, rightPath);
                    }

                    // Return the correct way to go
                    if (left != null)
                    {
                        return left;
                    }
                    else
                    {
                        return right;
                    }
                }
            }

            // Leaf constructor
            public Node(char character, int frequency)
            {
                this.character = character;
                this.frequency = frequency;
            }

            // Parent constructor
            public Node(int frequency, Node leftChild, Node rightChild)
            {
                this.frequency = frequency;
                this.leftChild = leftChild;
                this.rightChild = rightChild;
            }

            public char getCharacter()
            {
                return character;
            }

            public int getFrequency()
            {
                return frequency;
            }

            public Node getLeftChild()
            {
                return leftChild;
            }

            public Node getRightChild()
            {
                return rightChild;
            }
        }

        #endregion

    }
}