using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Huffman
{
    public partial class HuffmanPresentation : UserControl
    {
        public HuffmanPresentation()
        {
            InitializeComponent();
        }

        public void fillCodeTable(Dictionary<char, List<bool>> codeTable, Dictionary<char, int> histogram, int uncompressedSize, int compressedSize)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                List<Code> codes = new List<Code>();

                foreach (KeyValuePair<char, List<bool>> entry in codeTable)
                {
                    codes.Add(new Code() { character = entry.Key, frequency = histogram[entry.Key], code = toString(entry.Value) });
                }

                codes.Sort((entry1, entry2) => entry2.frequency.CompareTo(entry1.frequency));

                uncompressedSize_LB.Content = string.Format("{0:n0}", uncompressedSize) + " bytes";
                compressedSize_LB.Content = string.Format("{0:n0}", compressedSize) + " bytes";
                double compressionRate = Math.Round(1 - (double)compressedSize / uncompressedSize, 2);
                compressionRate_LB.Content = compressionRate * 100 + "%";
                codeTable_DG.ItemsSource = codes;
                codeTable_DG.CanUserAddRows = false;
            }, null);
        }

        public string toString(List<bool> bits)
        {
            StringBuilder bitString = new StringBuilder();

            foreach (bool bit in bits)
            {
                if (bit)
                {
                    bitString.Append("1");
                }
                else
                {
                    bitString.Append("0");
                }
            }

            return bitString.ToString();
        }

        public class Code
        {
            public char character { get; set; }

            public int frequency { get; set; }

            public string code { get; set; }
        }
    }
}
