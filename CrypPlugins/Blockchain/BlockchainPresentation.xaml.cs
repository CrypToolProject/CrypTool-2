/*
   Copyright (C) CrypTool 2 Team

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
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
        }
    }
}
