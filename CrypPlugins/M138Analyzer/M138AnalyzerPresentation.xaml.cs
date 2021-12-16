using CrypTool.CrypAnalysisViewControl;
using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace CrypTool.M138Analyzer
{
    [PluginBase.Attributes.Localization("CrypTool.M138Analyzer.Properties.Resources")]
    public partial class M138AnalyzerPresentation : UserControl
    {
        public ObservableCollection<ResultEntry> BestList { get; } = new ObservableCollection<ResultEntry>();
        public event Action<ResultEntry> SelectedResultEntry;

        public M138AnalyzerPresentation()
        {
            InitializeComponent();
            DataContext = BestList;
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
