using CrypTool.PluginBase.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace CrypTool.AnalysisMonoalphabeticSubstitution
{
    internal class Dictionary
    {
        #region Variables

        private bool stopFlag;
        private readonly Node root;
        private readonly int amountOfCharacters;

        #endregion

        #region Constructor

        public Dictionary(string filename, int amountOfCharacters)
        {
            this.amountOfCharacters = amountOfCharacters;
            root = new Node(amountOfCharacters);
            using (MemoryStream ms = new MemoryStream())
            {
                using (FileStream fs = new FileStream(Path.Combine(DirectoryHelper.DirectoryLanguageStatistics, filename), FileMode.Open, FileAccess.Read))
                {
                    using (GZipStream zs = new GZipStream(fs, CompressionMode.Decompress))
                    {
                        byte[] buffer = new byte[1024];
                        while (true)
                        {
                            int length = zs.Read(buffer, 0, 1024);
                            if (length == 0)
                            {
                                break;
                            }
                            ms.Write(buffer, 0, length);
                        }
                    }
                }
                ms.Position = 0;
                using (BinaryReader binReader = new BinaryReader(ms))
                {
                    while (ms.Position != ms.Length)
                    {
                        if (stopFlag)
                        {
                            break;
                        }

                        // Read length of pattern
                        int lenPattern = binReader.ReadInt32();
                        // Read pattern
                        byte[] pattern = binReader.ReadBytes(lenPattern);
                        // Read number of words with the same pattern
                        int number = binReader.ReadInt32();
                        // Read words for the pattern
                        List<byte[]> words = new List<byte[]>();
                        for (int i = 0; i < number; i++)
                        {
                            int len = binReader.ReadInt32();
                            words.Add(binReader.ReadBytes(len));
                        }
                        // Add pattern and words to dictionary
                        Add(pattern, words);
                    }
                }
            }
        }

        private void Add(byte[] pattern, List<byte[]> words)
        {
            Node actualNode = root;
            for (int i = 0; i < pattern.Length; i++)
            {
                if (pattern[i] > amountOfCharacters)
                {
                    throw new Exception("Symbol > " + amountOfCharacters + " not possible in dictionary");
                }
                if (actualNode.Nodes[pattern[i]] == null)
                {
                    actualNode.Nodes[pattern[i]] = new Node(amountOfCharacters);
                }
                actualNode = actualNode.Nodes[pattern[i]];
            }
            if (actualNode.Words != null)
            {
                throw new Exception("Already words for this pattern stored in dictionary!");
            }
            actualNode.Words = words;
        }

        #endregion

        #region Properties

        public bool StopFlag
        {
            get => stopFlag;
            set => stopFlag = value;
        }

        #endregion

        public List<byte[]> GetWordsFromPattern(byte[] pattern)
        {
            Node actualNode = root;
            for (int i = 0; i < pattern.Length; i++)
            {
                if (pattern[i] > amountOfCharacters)
                {
                    throw new Exception("Symbol > " + amountOfCharacters + " not possible in dictionary");
                }
                actualNode = actualNode.Nodes[pattern[i]];
                if (actualNode == null)
                {
                    return new List<byte[]>();
                }
            }
            return (actualNode.Words ?? new List<byte[]>());
        }
    }

    public class Node
    {
        private readonly int amountOfCharacters;
        public Node(int amountOfCharacters)
        {
            this.amountOfCharacters = amountOfCharacters;
            Nodes = new Node[this.amountOfCharacters];
        }

        public Node[] Nodes;
        public List<byte[]> Words { get; set; }
    }
}
