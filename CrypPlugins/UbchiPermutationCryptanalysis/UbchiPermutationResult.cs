using CrypTool.CrypAnalysisViewControl;
using System;
using System.ComponentModel;

namespace CrypTool.Plugins.UbchiPermutationCryptanalysis
{
    public class UbchiPermutationResult : ICrypAnalysisResultListEntry, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int _rank;

        public int Rank
        {
            get { return _rank; }
            set
            {
                _rank = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Rank"));
                }
            }
        }

        public double Score { get; set; }
        public int KeyLength { get; set; }
        public int Nulls { get; set; }
        public string KeyPermutation { get; set; }
        public string PlaintextPreview { get; set; }
        public string FullPlaintext { get; set; }
        public string AnalysisInfo { get; set; }

        public string ClipboardValue { get { return FullPlaintext; } }
        public string ClipboardKey { get { return KeyPermutation; } }
        public string ClipboardText { get { return FullPlaintext; } }

        public string ClipboardEntry
        {
            get
            {
                return "Rank: " + Rank + Environment.NewLine +
                       "Score: " + Score.ToString("F2") + Environment.NewLine +
                       "Key: [" + KeyPermutation + "]" + Environment.NewLine +
                       "Key Length: " + KeyLength + Environment.NewLine +
                       "Nulls: " + Nulls + Environment.NewLine +
                       "Analysis: " + AnalysisInfo + Environment.NewLine +
                       "Plaintext: " + FullPlaintext;
            }
        }
    }
}
