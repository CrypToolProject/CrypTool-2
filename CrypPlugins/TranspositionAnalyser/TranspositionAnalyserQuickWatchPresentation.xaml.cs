using CrypTool.CrypAnalysisViewControl;
using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace TranspositionAnalyser
{
    [CrypTool.PluginBase.Attributes.Localization("TranspositionAnalyser.Properties.Resources")]
    public partial class TranspositionAnalyserQuickWatchPresentation : UserControl
    {
        public ObservableCollection<ResultEntry> Entries { get; } = new ObservableCollection<ResultEntry>();
        public event Action<ResultEntry> SelectedResultEntry;

        public TranspositionAnalyserQuickWatchPresentation()
        {
            InitializeComponent();
            DataContext = Entries;
        }

        private void HandleResultItemAction(ICrypAnalysisResultListEntry item)
        {
            if (item is ResultEntry resultItem)
            {
                SelectedResultEntry(resultItem);
            }
        }
    }
}
