/*
   Copyright 2020 George Lasry, Nils Kopal, CrypTool project
   http://www.uni-kassel.de/eecs/fachgebiete/ais/mitarbeiter/nils-kopal-m-sc.html

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
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CrypTool.EnigmaAnalyzer
{
    [PluginBase.Attributes.Localization("CrypTool.EnigmaAnalyzer.Properties.Resources")]
    public partial class AssignmentPresentation : UserControl
    {

        public ObservableCollection<BestListEntry> BestList { get; } = new ObservableCollection<BestListEntry>();

        #region constructor

        public AssignmentPresentation()
        {
            InitializeComponent();
            DataContext = BestList;
        }

        #endregion

        #region Main Methods

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

        #endregion

        #region Helper Methods

        private void HandleResultItemAction(ICrypAnalysisResultListEntry item)
        {
            if (item is BestListEntry resultItem)
            {
                //_updateOutputFromUserChoice(resultItem.Key, resultItem.Text);
            }
        }

        #endregion
    }
}
