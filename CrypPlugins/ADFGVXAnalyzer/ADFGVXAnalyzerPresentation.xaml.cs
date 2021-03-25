using CrypTool.CrypAnalysisViewControl;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ADFGVXAnalyzer
{
    [CrypTool.PluginBase.Attributes.Localization("ADFGVXAnalyzer.Properties.Resources")]
    public partial class ADFGVXAnalyzerPresentation : UserControl
    {

        public ObservableCollection<ResultEntry> BestList { get; } = new ObservableCollection<ResultEntry>();
        public event Action<ResultEntry> getTranspositionResult;

        public ADFGVXAnalyzerPresentation()
        {
            InitializeComponent();
            DataContext = BestList;
        }

        public void DisableGUI()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                IsEnabled = false;
            }, null);
        }

        public void EnableGUI()
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
                getTranspositionResult(resultItem);
            }
        }
    }
}
