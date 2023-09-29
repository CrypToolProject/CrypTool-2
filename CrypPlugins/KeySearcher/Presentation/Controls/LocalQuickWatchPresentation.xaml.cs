using CrypTool.CrypAnalysisViewControl;
using KeySearcher;
using System.Collections.ObjectModel;
using System.Windows;

namespace KeySearcherPresentation.Controls
{
    [CrypTool.PluginBase.Attributes.Localization("KeySearcher.Properties.Resources")]
    public partial class LocalQuickWatchPresentation
    {
        private KeySearcher.KeySearcher.UpdateOutput _updateOutputFromUserChoice;

        public KeySearcher.KeySearcher.UpdateOutput UpdateOutputFromUserChoice
        {
            get => _updateOutputFromUserChoice;
            set => _updateOutputFromUserChoice = value;
        }

        public ObservableCollection<ResultEntry> Entries { get; } = new ObservableCollection<ResultEntry>();
        
        public LocalQuickWatchPresentation()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void HandleResultItemAction(ICrypAnalysisResultListEntry item)
        {
            if (item is ResultEntry resultItem)
            {
                _updateOutputFromUserChoice(resultItem.Key, resultItem.FullText);
            }
        }
    }
}
