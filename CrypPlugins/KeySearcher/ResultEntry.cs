using CrypTool.CrypAnalysisViewControl;
using System;
using System.ComponentModel;

namespace KeySearcher
{
    /// <summary>
    /// Represents one entry in our result list
    /// </summary>
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

        public string Value { get; set; }
        public string Key { get; set; }
        public string Text { get; set; }
        public string FullText { get; set; }

        public string ClipboardValue => Value;
        public string ClipboardKey => Key;
        public string ClipboardText => Text;
        public string ClipboardEntry =>
            "Rank: " + Ranking + Environment.NewLine +
            "Value: " + Value + Environment.NewLine +
            "Key: " + Key + Environment.NewLine +
            "Text: " + FullText;

        //-------
        public string User { get; set; }
        public DateTime Time { get; set; }
        public long Maschid { get; set; }
        public string Maschname { get; set; }
        //-------
    }
}