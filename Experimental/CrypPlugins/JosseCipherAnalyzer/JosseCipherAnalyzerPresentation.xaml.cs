using CrypTool.CrypAnalysisViewControl;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CrypTool.JosseCipherAnalyzer
{
    [PluginBase.Attributes.Localization("CrypTool.JosseCipherAnalyzer.Properties.Resources")]
    public partial class JosseCipherAnalyzerPresentation : UserControl
    {

        public ObservableCollection<ResultEntry> BestList { get; } = new ObservableCollection<ResultEntry>();
        private JosseCipherAnalyzer.UpdateOutput _updateOutputFromUserChoice;

        public JosseCipherAnalyzer.UpdateOutput UpdateOutputFromUserChoice
        {
            get => _updateOutputFromUserChoice;
            set => _updateOutputFromUserChoice = value;
        }

        public JosseCipherAnalyzerPresentation()
        {
            InitializeComponent();
            DataContext = BestList;
        }

        public void DisableGui()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                IsEnabled = false;
            }, null);
        }

        public void EnableGui()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                IsEnabled = true;
            }, null);
        }

        private void HandleResultItemAction(ICrypAnalysisResultListEntry item)
        {
            if (item is ResultEntry resultItem)
            {
                _updateOutputFromUserChoice(resultItem.Key, resultItem.Text);
            }
        }
    }
}
