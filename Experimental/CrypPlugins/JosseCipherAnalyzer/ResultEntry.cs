using CrypTool.CrypAnalysisViewControl;
using System;
using System.ComponentModel;

namespace CrypTool.JosseCipherAnalyzer
{
    public class ResultEntry : ICrypAnalysisResultListEntry, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int ranking;
        public int Ranking
        {
            get => ranking;
            set
            {
                ranking = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Ranking)));
            }
        }

        public double Value { get; set; }
        public string Key { get; set; }
        public string Text { get; set; }

        public string ClipboardValue => ExactValue.ToString();
        public string ClipboardKey => Key;
        public string ClipboardText => Text;
        public string ClipboardEntry =>
            "Rank: " + Ranking + Environment.NewLine +
            "Value: " + ExactValue + Environment.NewLine +
            "Key: " + Key + Environment.NewLine +
            "KeyLength: " + KeyLength + Environment.NewLine +
            "Text: " + Text;

        public double ExactValue => Math.Abs(Value);

        public int KeyLength => Key.Length;
    }
}