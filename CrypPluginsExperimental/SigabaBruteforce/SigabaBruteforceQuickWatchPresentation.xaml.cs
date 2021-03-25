using CrypTool.CrypAnalysisViewControl;
using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;


namespace SigabaBruteforce
{
    [global::CrypTool.PluginBase.Attributes.Localization("SigabaBruteforce.Properties.Resources")]
    public partial class SigabaBruteforceQuickWatchPresentation : UserControl
    {
        public ObservableCollection<ResultEntry> Entries { get; } = new ObservableCollection<ResultEntry>();
        public event Action<ResultEntry> SelectedResultEntry;

        public SigabaBruteforceQuickWatchPresentation()
        {
            InitializeComponent();
            this.DataContext = Entries;
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
