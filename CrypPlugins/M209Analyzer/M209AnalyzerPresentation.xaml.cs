/*
   Copyright 2023 Josef Matwich josef.matwich@gmail.com

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

namespace CrypTool.Plugins.M209Analyzer
{
    /// <summary>
    /// Interaktionslogik für AssignmentPresentation.xaml
    /// </summary>
    [PluginBase.Attributes.Localization("CrypTool.Plugins.M209Analyzer.Properties.Resources")]
    public partial class M209AnalyzerPresentation : UserControl
    {
        public ObservableCollection<ResultEntry> BestList { get; } = new ObservableCollection<ResultEntry>();

        #region Variables

        private UpdateOutput _updateOutputFromUserChoice;

        #endregion

        #region Properties

        public UpdateOutput UpdateOutputFromUserChoice
        {
            get => _updateOutputFromUserChoice;
            set => _updateOutputFromUserChoice = value;
        }

        #endregion

        #region constructor

        public M209AnalyzerPresentation()
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
            if (item is ResultEntry resultItem)
            {
                _updateOutputFromUserChoice(resultItem.Key, resultItem.Text);
            }
        }

        #endregion
    }
}
