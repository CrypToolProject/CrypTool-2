using CrypTool.CrypAnalysisViewControl;
using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace CrypTool.Plugins.UbchiPermutationCryptanalysis
{
    [CrypTool.PluginBase.Attributes.Localization("CrypTool.Plugins.UbchiPermutationCryptanalysis.Properties.Resources")]
    public partial class UbchiPermutationAnalysisPresentation : UserControl
    {
        public ObservableCollection<UbchiPermutationResult> Entries { get; private set; }

        public event Action<UbchiPermutationResult> SelectedResultEntry;

        public UbchiPermutationAnalysisPresentation()
        {
            Entries = new ObservableCollection<UbchiPermutationResult>();
            InitializeComponent();
            DataContext = Entries;
        }

        private void HandleResultItemAction(ICrypAnalysisResultListEntry item)
        {
            UbchiPermutationResult resultItem = item as UbchiPermutationResult;
            if (resultItem != null && SelectedResultEntry != null)
            {
                SelectedResultEntry(resultItem);
            }
        }
    }
}

