using CrypTool.CrypAnalysisViewControl;
using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace IDPAnalyser
{
    [CrypTool.PluginBase.Attributes.Localization("IDPAnalyser.Properties.Resources")]
    public partial class IDPAnalyserQuickWatchPresentation : UserControl
    {
        public ObservableCollection<ResultEntry> Entries { get; } = new ObservableCollection<ResultEntry>();
        public event Action<ResultEntry> SelectedResultEntry;

        public IDPAnalyserQuickWatchPresentation()
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
