using CrypTool.CrypAnalysisViewControl;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using static CrypTool.Plugins.Blockchain.Blockchain;

namespace CrypTool.Plugins.Blockchain
{
    [PluginBase.Attributes.Localization("CrypTool.Plugins.Blockchain.Properties.Resources")]
    public partial class BlockchainPresentation : UserControl
    {

        public ObservableCollection<Transaction> TransactionList { get; } = new ObservableCollection<Transaction>();
        public ObservableCollection<Balance> BalanceList { get; } = new ObservableCollection<Balance>();
        public BlockchainPresentation()
        {
            InitializeComponent();
        }

        private void HandleResultItemAction(ICrypAnalysisResultListEntry item)
        {
            /*
            if (item is ResultEntry resultItem)
            {
                _updateOutputFromUserChoice(resultItem.Key, resultItem.Text);
            }
            */
        }
    }
}
